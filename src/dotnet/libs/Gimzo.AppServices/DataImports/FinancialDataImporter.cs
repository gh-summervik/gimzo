using Dapper;
using Gimzo.Analysis.Fundamental;
using Gimzo.Common;
using Gimzo.Infrastructure;
using Gimzo.Infrastructure.Database;
using Gimzo.Infrastructure.DataProviders.FinancialDataNet;
using Microsoft.Extensions.Logging;

namespace Gimzo.AppServices.DataImports;

public sealed class FinancialDataImporter(FinancialDataApiClient apiClient,
    DbDefPair dbDefPair,
    ILogger<FinancialDataImporter> logger)
{
    private readonly FinancialDataApiClient _apiClient = apiClient;
    private readonly DatabaseService _dbService = new(dbDefPair, logger);
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

        await _dbService.SaveProcess(_process.ToDao());

        // this must happen before the fetch of meta info; meta info includes ignored symbols.
        await _dbService.RemovedExpiredIgnoredSymbolsAsync();

        _metaInfo = await _dbService.GetDbMetaInfoAsync();
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
            _dbService.AddDelistedSymbolsToIgnoreListAsync(_process!.ProcessId),
            _dbService.AddSymbolsWithPricesOutsideRangeToIgnoreListAsync(_process!.ProcessId, 10M, 2_000M),
            _dbService.AddSymbolsWithShortChartsToIgnoreListAsync(_process!.ProcessId, 200)
        };
        await Task.WhenAll(ignoreTasks);

        await _dbService.DeleteDataForIgnoredSymbolsAsync();

        await _dbService.SaveProcess(_process!.ToDao(DateTimeOffset.UtcNow));
    }

    internal async Task DoWeekdayWorkAsync()
    {
        LogHelper.LogInfo(_logger, "Weekday import in {Name} started import at {DateTime} with process id {ProcessId}", nameof(FinancialDataImporter),
                DateTimeOffset.Now, _process!.ProcessId);

        var stockSymbols = (await _apiClient.GetStockSymbolsAsync())
            .Select(k => new Security(k.Symbol, "Stock", registrant: k.Registrant)).ToArray();

        LogHelper.LogInfo(_logger, "Saving {count} symbols.", stockSymbols.Length);

        await _dbService.SaveStockSymbolsAsync(stockSymbols, _process!.ProcessId);

        List<Security> securityList = new(10_000);
        LogHelper.LogInfo(_logger, "Fetching security information.");

        int count = stockSymbols.Length;
        int i = 0;
        foreach (var ticker in stockSymbols.Select(k => k.Symbol))
        {
            i++;
            var secInfo = await _apiClient.GetSecurityInformationAsync(ticker);
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

            var eodPrices = await _apiClient.GetStockPricesAsync(ticker);

            LogHelper.LogInfo(_logger, "Saving price data for {ticker} - {i}/{count}", ticker, i, count);

            await _dbService.SaveEodPricesAsync(eodPrices.Select(k =>
                new Analysis.Technical.Charts.Ohlc(k.Symbol, k.Date.GetValueOrDefault(),
                    k.Open.GetValueOrDefault(),
                    k.High.GetValueOrDefault(),
                    k.Low.GetValueOrDefault(),
                    k.Close.GetValueOrDefault(),
                    k.Volume.GetValueOrDefault())), _process!.ProcessId);
        }

        LogHelper.LogInfo(_logger, "Saving security info for {count} symbols.", securityList.Count);

        await _dbService.SaveSecuritiesAsync(securityList, _process!.ProcessId);
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
            var coInfoModel = await _apiClient.GetCompanyInformationAsync(symbol);
            if (coInfoModel.HasValue)
                await _dbService.SaveCompanyInfoAsync(coInfoModel.Value.ToDomain(), _process!.ProcessId);

            var incStmtsModels = await _apiClient.GetIncomeStatementsAsync(symbol);
            if (incStmtsModels.Length > 0)
                await _dbService.SaveIncomeStatementsAsync(incStmtsModels.Select(k => k.ToDomain()), _process!.ProcessId);

            var bsStmtsModels = await _apiClient.GetBalanceSheetStatementsAsync(symbol);
            if (bsStmtsModels.Length > 0)
                await _dbService.SaveBalanceSheetsAsync(bsStmtsModels.Select(k => k.ToDomain()), _process!.ProcessId);

            var cfStmtsModels = await _apiClient.GetCashFlowStatementsAsync(symbol);
            if (cfStmtsModels.Length > 0)
                await _dbService.SaveCashFlowStatementsAsync(cfStmtsModels.Select(k => k.ToDomain()), _process!.ProcessId);

            var divsModels = await _apiClient.GetDividendsAsync(symbol);
            if (divsModels.Length > 0)
                await _dbService.SaveDividendsAsync(divsModels.Select(k => k.ToDomain()), _process!.ProcessId);

            var splitsModels = await _apiClient.GetStockSplitsAsync(symbol);
            if (splitsModels.Length > 0)
                await _dbService.SaveSplitsAsync(splitsModels.Select(k => k.ToDomain()), _process!.ProcessId);

            var earningsReleasesModels = await _apiClient.GetEarningsReleasesAsync(symbol);
            if (earningsReleasesModels.Length > 0)
                await _dbService.SaveEarningReleasesAsync(earningsReleasesModels.Select(k => k.ToDomain()), _process!.ProcessId);

            var shortsModels = await _apiClient.GetShortInterestAsync(symbol);
            if (shortsModels.Length > 0)
                await _dbService.SaveShortInterestsAsync(shortsModels.Select(k => k.ToDomain()), _process!.ProcessId);
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

            var metricsModel = await _apiClient.GetKeyMetricsAsync(symbol);
            if (metricsModel.Length > 0)
                await _dbService.SaveKeyMetricsAsync(metricsModel.Select(k => k.ToDomain()), _process!.ProcessId);

            var marketCapsModel = await _apiClient.GetMarketCapAsync(symbol);
            if (marketCapsModel.Length > 0)
                await _dbService.SaveMarketCapsAsync(marketCapsModel.Select(k => k.ToDomain()), _process!.ProcessId);

            var empCountsModel = await _apiClient.GetEmployeeCountAsync(symbol);
            if (empCountsModel.Length > 0)
                await _dbService.SaveEmployeeCountsAsync(empCountsModel.Select(k => k.ToDomain()), _process!.ProcessId);

            var compsModel = await _apiClient.GetExecutiveCompensationAsync(symbol);
            if (compsModel.Length > 0)
                await _dbService.SaveExecCompensationAsync(compsModel.Select(k => k.ToDomain()), _process!.ProcessId);

            var efficiencyRatiosModel = await _apiClient.GetEfficiencyRatiosAsync(symbol);
            if (efficiencyRatiosModel.Length > 0)
                await _dbService.SaveEfficiencyRatiosAsync(efficiencyRatiosModel.Select(k => k.ToDomain()), _process!.ProcessId);

            var liquidityModel = await _apiClient.GetLiquidityRatiosAsync(symbol);
            if (liquidityModel.Length > 0)
                await _dbService.SaveLiquidityRatiosAsync(liquidityModel.Select(k => k.ToDomain()), _process!.ProcessId);

            var profitabilityModel = await _apiClient.GetProfitabilityRatiosAsync(symbol);
            if (profitabilityModel.Length > 0)
                await _dbService.SaveProfitabilityRatiosAsync(profitabilityModel.Select(k => k.ToDomain()), _process!.ProcessId);

            var solvencyModel = await _apiClient.GetSolvencyRatiosAsync(symbol);
            if (solvencyModel.Length > 0)
                await _dbService.SaveSolvencyRatiosAsync(solvencyModel.Select(k => k.ToDomain()), _process!.ProcessId);

            var valuationsModel = await _apiClient.GetValuationRatiosAsync(symbol);
            if (valuationsModel.Length > 0)
                await _dbService.SaveValuationRatiosAsync(valuationsModel.Select(k => k.ToDomain()), _process!.ProcessId);
        }
    }

    private async Task<string[]> GetSymbolsFromDatabaseAsync()
    {
        const string FetchSymbolsSql = @"SELECT symbol from public.stock_symbols";
        using var queryCtx = _dbDefPair.GetQueryConnection();
        return [.. (await queryCtx.QueryAsync<string>(FetchSymbolsSql))];
    }
}
