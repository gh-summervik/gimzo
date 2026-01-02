using Dapper;
using Gimzo.Analysis.Fundamental;
using Gimzo.Common;
using Gimzo.Infrastructure;
using Gimzo.Infrastructure.Database;
using Gimzo.Infrastructure.DataProviders.FinancialDataNet;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using OoplesFinance.YahooFinanceAPI;
using OoplesFinance.YahooFinanceAPI.Enums;
using OoplesFinance.YahooFinanceAPI.Models;

namespace Gimzo.AppServices.Data;

public sealed class FinancialDataImporter(FinancialDataApiClient apiClient,
    DbDefPair dbDefPair,
    IMemoryCache memoryCache,
    ILogger<FinancialDataImporter> logger)
{
    private readonly FinancialDataApiClient _fdnApiClient = apiClient;
    private readonly YahooClient _yahooClient = new();
    private readonly DataService _dataService = new(dbDefPair, memoryCache, logger);
    private readonly DbDefPair _dbDefPair = dbDefPair;
    private readonly ILogger<FinancialDataImporter> _logger = logger;
    private readonly DateOnly _today = TimeHelper.TodayEastern;
    private DbMetaInfo _metaInfo = new();
    private Process? _process;
    private bool _forceWeekday;
    private bool _forceSaturday;
    private bool _forceSunday;
    private bool _initialized;

    /// <summary>
    /// Initialize the import process by hydrating meta info and determining
    /// which day to run.
    /// This method is intended to be called first.
    /// </summary>
    public async Task InitializeImportAsync(Process process, bool forceWeekday = false,
        bool forceSaturday = false, bool forceSunday = false)
    {
        _process = process ?? throw new ArgumentNullException(nameof(process));
        _forceWeekday = forceWeekday;
        _forceSaturday = forceSaturday;
        _forceSunday = forceSunday;

        await _dataService.SaveProcess(_process.ToDao());

        // this must happen before the fetch of meta info; meta info includes ignored symbols.
        await _dataService.RemovedExpiredIgnoredSymbolsAsync();

        _metaInfo = await _dataService.GetDbMetaInfoAsync();
        _initialized = true;
        LogHelper.LogInfo(_logger, "Import initialized");
    }

    public async Task ImportAsync()
    {
        if (!_initialized)
            throw new Exception($"Import not initialized; call {nameof(InitializeImportAsync)} first.");

        if (_forceSunday | _forceSaturday | _forceWeekday)
        {
            if (_forceWeekday)
                await DoWeekdayWorkAsync();
            if (_forceSaturday)
                await DoSaturdayWorkAsync();
            if (_forceSunday)
                await DoSundayWorkAsync();
        }
        else
        {
            var t = _today.DayOfWeek switch
            {
                DayOfWeek.Sunday => DoSundayWorkAsync(),
                DayOfWeek.Saturday => DoSaturdayWorkAsync(),
                _ => DoWeekdayWorkAsync()
            };
            await t;
        }

        var ignoreTasks = new[]
        {
            _dataService.AddDelistedSymbolsToIgnoreListAsync(_process!.ProcessId),
            _dataService.AddSymbolsWithPricesOutsideRangeToIgnoreListAsync(_process!.ProcessId, 10M, 2_000M),
            _dataService.AddSymbolsWithShortChartsToIgnoreListAsync(_process!.ProcessId, 200)
        };
        await Task.WhenAll(ignoreTasks);

        await _dataService.DeleteDataForIgnoredSymbolsAsync();

        await _dataService.SaveProcess(_process!.ToDao(DateTimeOffset.UtcNow));
    }

    internal async Task DoWeekdayWorkAsync()
    {
        LogHelper.LogInfo(_logger, "Weekday import in {Name} started import at {DateTime} with process id {ProcessId}", nameof(FinancialDataImporter),
                DateTimeOffset.Now, _process!.ProcessId);

        var stockSymbolModels = (await _fdnApiClient.GetStockSymbolsAsync())
            .Where(k => !_metaInfo.IgnoredSymbols.Contains(k.Symbol));

        var stockSymbols = stockSymbolModels
            .Select(k => new Security(k.Symbol, "Stock", registrant: k.Registrant)).ToArray();

        LogHelper.LogInfo(_logger, "Saving {count} symbols.", stockSymbols.Length);

        await _dataService.SaveStockSymbolsAsync(stockSymbols, _process!.ProcessId);

        List<Security> securityList = new(10_000);
        LogHelper.LogInfo(_logger, "Fetching security information.");

        int count = stockSymbols.Length;
        int i = 0;
        foreach (var ticker in stockSymbols.Select(k => k.Symbol))
        {
            i++;
            var secInfo = await _fdnApiClient.GetSecurityInformationAsync(ticker);
            if (secInfo.HasValue)
            {
                var model = secInfo.Value;

                securityList.Add(new Security(model.Symbol, model.Type ?? "Unknown")
                {
                    Issuer = model.Issuer,
                    Cusip = model.Cusip,
                    Isin = model.Isin,
                    Figi = model.Figi
                });
            }

            var fdnEodPrices = await _fdnApiClient.GetStockPricesAsync(ticker);

            if (fdnEodPrices.Length > 0)
            {
                /*
                 * YAHOO to the rescue.
                 * 
                 * We have to employ Yahoo here to capture the past few days because
                 * financialdata.net has a 2-day delay on EOD historical prices. Really.
                 * 
                 * We do all this date manipulation so that one set of values does not overwrite the other.
                 * FDN is the primary, and then we try to fill in the latest using Yahoo.
                 */
                var yahooStartTime = fdnEodPrices[^1].Date.GetValueOrDefault().AddWeekdays(1).ToDateTime(TimeOnly.MinValue);
                DateTime yahooFinishTime = TimeHelper.NowEastern.TimeOfDay.Hours > 17
                    ? DateTime.Now.EndOfDay() : DateTime.Now.AddWeekdays(-1).EndOfDay();
                IEnumerable<HistoricalChartInfo> yahooEodPrices;
                try
                {
                    yahooEodPrices = yahooStartTime < DateTime.Now && yahooStartTime < yahooFinishTime
                        ? await _yahooClient.GetHistoricalDataAsync(ticker, DataFrequency.Daily, yahooStartTime, yahooFinishTime)
                        : [];
                }
                catch (Exception ex)
                {
                    LogHelper.LogWarning(_logger, ex.Message);
                    // not all tickers are available via Yahoo.
                    yahooEodPrices = [];
                }

                var allPrices = fdnEodPrices.Select(k =>
                    new Analysis.Technical.Charts.Ohlc(k.Symbol, k.Date.GetValueOrDefault(),
                        k.Open.GetValueOrDefault(),
                        k.High.GetValueOrDefault(),
                        k.Low.GetValueOrDefault(),
                        k.Close.GetValueOrDefault(),
                        k.Volume.GetValueOrDefault()))
                    .Union(yahooEodPrices.Select(k =>
                        new Analysis.Technical.Charts.Ohlc(k.Meta.Symbol,
                            DateOnly.FromDateTime(k.Date),
                            (decimal)k.Open, (decimal)k.High,
                            (decimal)k.Low, (decimal)k.Close, k.Volume)));

                LogHelper.LogInfo(_logger, "Saving price data for {ticker} - {i}/{count}", ticker, i, count);

                await _dataService.SaveEodPricesAsync(allPrices, _process!.ProcessId);
            }
        }

        LogHelper.LogInfo(_logger, "Saving security info for {count} symbols.", securityList.Count);

        await _dataService.SaveSecuritiesAsync(securityList, _process!.ProcessId);
    }

    internal async Task DoSaturdayWorkAsync()
    {
        LogHelper.LogInfo(_logger, "Saturday import in {Name} started import at {DateTime} with process id {ProcessId}", nameof(FinancialDataImporter),
                DateTimeOffset.Now, _process!.ProcessId);

        var symbols = await GetSymbolsFromDatabaseAsync();

        int count = symbols.Length;
        int i = 0;

        foreach (var symbol in symbols)
        {
            i++;
            LogHelper.LogInfo(_logger, "Processing {symbol} - {i}/{count}", symbol, i, count);
            var coInfoModel = await _fdnApiClient.GetCompanyInformationAsync(symbol);
            if (coInfoModel.HasValue)
                await _dataService.SaveCompanyInfoAsync(coInfoModel.Value.ToDomain(), _process!.ProcessId);

            var incStmtsModels = await _fdnApiClient.GetIncomeStatementsAsync(symbol);
            if (incStmtsModels.Length > 0)
                await _dataService.SaveIncomeStatementsAsync(incStmtsModels.Select(k => k.ToDomain()), _process!.ProcessId);

            var bsStmtsModels = await _fdnApiClient.GetBalanceSheetStatementsAsync(symbol);
            if (bsStmtsModels.Length > 0)
                await _dataService.SaveBalanceSheetsAsync(bsStmtsModels.Select(k => k.ToDomain()), _process!.ProcessId);

            var cfStmtsModels = await _fdnApiClient.GetCashFlowStatementsAsync(symbol);
            if (cfStmtsModels.Length > 0)
                await _dataService.SaveCashFlowStatementsAsync(cfStmtsModels.Select(k => k.ToDomain()), _process!.ProcessId);

            var divsModels = await _fdnApiClient.GetDividendsAsync(symbol);
            if (divsModels.Length > 0)
                await _dataService.SaveDividendsAsync(divsModels.Select(k => k.ToDomain()), _process!.ProcessId);

            var splitsModels = await _fdnApiClient.GetStockSplitsAsync(symbol);
            if (splitsModels.Length > 0)
                await _dataService.SaveSplitsAsync(splitsModels.Select(k => k.ToDomain()), _process!.ProcessId);

            var earningsReleasesModels = await _fdnApiClient.GetEarningsReleasesAsync(symbol);
            if (earningsReleasesModels.Length > 0)
                await _dataService.SaveEarningReleasesAsync(earningsReleasesModels.Select(k => k.ToDomain()), _process!.ProcessId);

            var shortsModels = await _fdnApiClient.GetShortInterestAsync(symbol);
            if (shortsModels.Length > 0)
                await _dataService.SaveShortInterestsAsync(shortsModels.Select(k => k.ToDomain()), _process!.ProcessId);
        }
    }

    internal async Task DoSundayWorkAsync()
    {
        LogHelper.LogInfo(_logger, "Sunday import in {Name} started import at {DateTime} with process id {ProcessId}", nameof(FinancialDataImporter),
                DateTimeOffset.Now, _process!.ProcessId);

        var symbols = await GetSymbolsFromDatabaseAsync();

        int count = symbols.Length;
        int i = 0;

        foreach (var symbol in symbols)
        {
            i++;

            LogHelper.LogInfo(_logger, "Processing {symbol} - {i}/{count}", symbol, i, count);

            var metricsModel = await _fdnApiClient.GetKeyMetricsAsync(symbol);
            if (metricsModel.Length > 0)
                await _dataService.SaveKeyMetricsAsync(metricsModel.Select(k => k.ToDomain()), _process!.ProcessId);

            var marketCapsModel = await _fdnApiClient.GetMarketCapAsync(symbol);
            if (marketCapsModel.Length > 0)
                await _dataService.SaveMarketCapsAsync(marketCapsModel.Select(k => k.ToDomain()), _process!.ProcessId);

            var empCountsModel = await _fdnApiClient.GetEmployeeCountAsync(symbol);
            if (empCountsModel.Length > 0)
                await _dataService.SaveEmployeeCountsAsync(empCountsModel.Select(k => k.ToDomain()), _process!.ProcessId);

            var compsModel = await _fdnApiClient.GetExecutiveCompensationAsync(symbol);
            if (compsModel.Length > 0)
                await _dataService.SaveExecCompensationAsync(compsModel.Select(k => k.ToDomain()), _process!.ProcessId);

            var efficiencyRatiosModel = await _fdnApiClient.GetEfficiencyRatiosAsync(symbol);
            if (efficiencyRatiosModel.Length > 0)
                await _dataService.SaveEfficiencyRatiosAsync(efficiencyRatiosModel.Select(k => k.ToDomain()), _process!.ProcessId);

            var liquidityModel = await _fdnApiClient.GetLiquidityRatiosAsync(symbol);
            if (liquidityModel.Length > 0)
                await _dataService.SaveLiquidityRatiosAsync(liquidityModel.Select(k => k.ToDomain()), _process!.ProcessId);

            var profitabilityModel = await _fdnApiClient.GetProfitabilityRatiosAsync(symbol);
            if (profitabilityModel.Length > 0)
                await _dataService.SaveProfitabilityRatiosAsync(profitabilityModel.Select(k => k.ToDomain()), _process!.ProcessId);

            var solvencyModel = await _fdnApiClient.GetSolvencyRatiosAsync(symbol);
            if (solvencyModel.Length > 0)
                await _dataService.SaveSolvencyRatiosAsync(solvencyModel.Select(k => k.ToDomain()), _process!.ProcessId);

            var valuationsModel = await _fdnApiClient.GetValuationRatiosAsync(symbol);
            if (valuationsModel.Length > 0)
                await _dataService.SaveValuationRatiosAsync(valuationsModel.Select(k => k.ToDomain()), _process!.ProcessId);
        }
    }

    private async Task<string[]> GetSymbolsFromDatabaseAsync()
    {
        const string FetchSymbolsSql = @"SELECT symbol from public.stock_symbols";
        using var queryCtx = _dbDefPair.GetQueryConnection();
        return [.. (await queryCtx.QueryAsync<string>(FetchSymbolsSql))];
    }
}
