using Dapper;
using Gimzo.Analysis.Fundamental;
using Gimzo.Analysis.Technical.Charts;
using Gimzo.Common;
using Gimzo.Infrastructure;
using Gimzo.Infrastructure.Database;
using Gimzo.Infrastructure.DataProviders.FinancialDataNet;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using OoplesFinance.YahooFinanceAPI;
using OoplesFinance.YahooFinanceAPI.Enums;
using OoplesFinance.YahooFinanceAPI.Models;
using System.Collections.Immutable;

namespace Gimzo.AppServices.Data;

public sealed class FinancialDataImporter(FinancialDataApiClient apiClient,
    DbDefPair dbDefPair,
    IMemoryCache memoryCache,
    ILogger<FinancialDataImporter> logger) : ServiceBase(dbDefPair, memoryCache)
{
    private readonly FinancialDataApiClient _fdnApiClient = apiClient;
    private readonly YahooClient _yahooClient = new();
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

        await SaveProcess(_process.ToDao());

        // this must happen before the fetch of meta info; meta info includes ignored symbols.
        await RemovedExpiredIgnoredSymbolsAsync();

        _metaInfo = await GetDbMetaInfoAsync();
        _initialized = true;
        LogHelper.LogInfo(_logger, "Import initialized");
    }

    public async Task ImportAsync(bool cleanupOnly = false)
    {
        if (!_initialized)
            throw new Exception($"Import not initialized; call {nameof(InitializeImportAsync)} first.");

        if (!cleanupOnly)
        {
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
        }
        var ignoreTasks = new[]
        {
            AddDelistedSymbolsToIgnoreListAsync(_process!.ProcessId),
            AddSymbolsWithPricesOutsideRangeToIgnoreListAsync(_process!.ProcessId, 10M, 2_000M),
            AddSymbolsWithShortChartsToIgnoreListAsync(_process!.ProcessId, 200)
        };
        await Task.WhenAll(ignoreTasks);

        await DeleteDataForIgnoredSymbolsAsync();
        
        await SaveProcess(_process!.ToDao(DateTimeOffset.UtcNow));
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

        await SaveStockSymbolsAsync(stockSymbols, _process!.ProcessId);

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
                    new Ohlc(k.Symbol, k.Date.GetValueOrDefault(),
                        k.Open.GetValueOrDefault(),
                        k.High.GetValueOrDefault(),
                        k.Low.GetValueOrDefault(),
                        k.Close.GetValueOrDefault(),
                        k.Volume.GetValueOrDefault()))
                    .Union(yahooEodPrices.Select(k =>
                        new Ohlc(k.Meta.Symbol,
                            DateOnly.FromDateTime(k.Date),
                            (decimal)k.Open, (decimal)k.High,
                            (decimal)k.Low, (decimal)k.Close, k.Volume))).ToImmutableArray();

                LogHelper.LogInfo(_logger, "Saving price data for {ticker} - {i}/{count}", ticker, i, count);

                await SaveEodPricesAsync(allPrices, _process!.ProcessId);
            }
        }

        LogHelper.LogInfo(_logger, "Saving security info for {count} symbols.", securityList.Count);

        await SaveSecuritiesAsync(securityList, _process!.ProcessId);
    }

    internal async Task DoSaturdayWorkAsync()
    {
        LogHelper.LogInfo(_logger, "Saturday import in {Name} started import at {DateTime} with process id {ProcessId}", nameof(FinancialDataImporter),
                DateTimeOffset.Now, _process!.ProcessId);

        var symbols = await GetSymbolsFromDatabaseAsync();

        int count = symbols.Count;
        int i = 0;

        foreach (var symbol in symbols)
        {
            i++;
            LogHelper.LogInfo(_logger, "Processing {symbol} - {i}/{count}", symbol, i, count);
            var coInfoModel = await _fdnApiClient.GetCompanyInformationAsync(symbol);
            if (coInfoModel.HasValue)
                await SaveCompanyInfoAsync(coInfoModel.Value.ToDomain(), _process!.ProcessId);

            var incStmtsModels = await _fdnApiClient.GetIncomeStatementsAsync(symbol);
            if (incStmtsModels.Length > 0)
                await SaveIncomeStatementsAsync(incStmtsModels.Select(k => k.ToDomain()).ToImmutableArray(), _process!.ProcessId);

            var bsStmtsModels = await _fdnApiClient.GetBalanceSheetStatementsAsync(symbol);
            if (bsStmtsModels.Length > 0)
                await SaveBalanceSheetsAsync(bsStmtsModels.Select(k => k.ToDomain()).ToImmutableArray(), _process!.ProcessId);

            var cfStmtsModels = await _fdnApiClient.GetCashFlowStatementsAsync(symbol);
            if (cfStmtsModels.Length > 0)
                await SaveCashFlowStatementsAsync(cfStmtsModels.Select(k => k.ToDomain()).ToImmutableArray(), _process!.ProcessId);

            var divsModels = await _fdnApiClient.GetDividendsAsync(symbol);
            if (divsModels.Length > 0)
                await SaveDividendsAsync(divsModels.Select(k => k.ToDomain()).ToImmutableArray(), _process!.ProcessId);

            var splitsModels = await _fdnApiClient.GetStockSplitsAsync(symbol);
            if (splitsModels.Length > 0)
                await SaveSplitsAsync(splitsModels.Select(k => k.ToDomain()).ToImmutableArray(), _process!.ProcessId);

            var earningsReleasesModels = await _fdnApiClient.GetEarningsReleasesAsync(symbol);
            if (earningsReleasesModels.Length > 0)
                await SaveEarningReleasesAsync(earningsReleasesModels.Select(k => k.ToDomain()).ToImmutableArray(), _process!.ProcessId);

            var shortsModels = await _fdnApiClient.GetShortInterestAsync(symbol);
            if (shortsModels.Length > 0)
                await SaveShortInterestsAsync(shortsModels.Select(k => k.ToDomain()).ToImmutableArray(), _process!.ProcessId);
        }
    }

    internal async Task DoSundayWorkAsync()
    {
        LogHelper.LogInfo(_logger, "Sunday import in {Name} started import at {DateTime} with process id {ProcessId}", nameof(FinancialDataImporter),
                DateTimeOffset.Now, _process!.ProcessId);

        var symbols = await GetSymbolsFromDatabaseAsync();

        int count = symbols.Count;
        int i = 0;

        foreach (var symbol in symbols)
        {
            i++;

            LogHelper.LogInfo(_logger, "Processing {symbol} - {i}/{count}", symbol, i, count);

            var metricsModel = await _fdnApiClient.GetKeyMetricsAsync(symbol);
            if (metricsModel.Length > 0)
                await SaveKeyMetricsAsync(metricsModel.Select(k => k.ToDomain()).ToImmutableArray(), _process!.ProcessId);

            var marketCapsModel = await _fdnApiClient.GetMarketCapAsync(symbol);
            if (marketCapsModel.Length > 0)
                await SaveMarketCapsAsync(marketCapsModel.Select(k => k.ToDomain()).ToImmutableArray(), _process!.ProcessId);

            var empCountsModel = await _fdnApiClient.GetEmployeeCountAsync(symbol);
            if (empCountsModel.Length > 0)
                await SaveEmployeeCountsAsync(empCountsModel.Select(k => k.ToDomain()).ToImmutableArray(), _process!.ProcessId);

            var compsModel = await _fdnApiClient.GetExecutiveCompensationAsync(symbol);
            if (compsModel.Length > 0)
                await SaveExecCompensationAsync(compsModel.Select(k => k.ToDomain()).ToImmutableArray(), _process!.ProcessId);

            var efficiencyRatiosModel = await _fdnApiClient.GetEfficiencyRatiosAsync(symbol);
            if (efficiencyRatiosModel.Length > 0)
                await SaveEfficiencyRatiosAsync(efficiencyRatiosModel.Select(k => k.ToDomain()).ToImmutableArray(), _process!.ProcessId);

            var liquidityModel = await _fdnApiClient.GetLiquidityRatiosAsync(symbol);
            if (liquidityModel.Length > 0)
                await SaveLiquidityRatiosAsync(liquidityModel.Select(k => k.ToDomain()).ToImmutableArray(), _process!.ProcessId);

            var profitabilityModel = await _fdnApiClient.GetProfitabilityRatiosAsync(symbol);
            if (profitabilityModel.Length > 0)
                await SaveProfitabilityRatiosAsync(profitabilityModel.Select(k => k.ToDomain()).ToImmutableArray(), _process!.ProcessId);

            var solvencyModel = await _fdnApiClient.GetSolvencyRatiosAsync(symbol);
            if (solvencyModel.Length > 0)
                await SaveSolvencyRatiosAsync(solvencyModel.Select(k => k.ToDomain()).ToImmutableArray(), _process!.ProcessId);

            var valuationsModel = await _fdnApiClient.GetValuationRatiosAsync(symbol);
            if (valuationsModel.Length > 0)
                await SaveValuationRatiosAsync(valuationsModel.Select(k => k.ToDomain()).ToImmutableArray(), _process!.ProcessId);
        }
    }

    private async Task<IReadOnlyCollection<string>> GetSymbolsFromDatabaseAsync()
    {
        const string FetchSymbolsSql = @"SELECT symbol from public.stock_symbols";
        using var queryCtx = _dbDefPair.GetQueryConnection();
        return [.. (await queryCtx.QueryAsync<string>(FetchSymbolsSql))];
    }

    public async Task RemovedExpiredIgnoredSymbolsAsync()
    {
        const string SelectSymbolsToRemove = @"SELECT symbol FROM public.ignored_symbols WHERE expiration <= @Now";
        const string RemoveExpiredIgnoreRecordsSql = @"DELETE FROM public.ignored_symbols WHERE symbol = @Symbol";

        using var queryCtx = _dbDefPair.GetQueryConnection();

        var symbolList = (await queryCtx.QueryAsync<string>(SelectSymbolsToRemove, new { Now = TimeHelper.TodayEastern })).ToArray();

        if (symbolList.Length == 0)
            LogHelper.LogInfo(_logger, "No ignored symbols to remove.");
        else
        {
            LogHelper.LogInfo(_logger, "Removing {count} symbol(s) from ignore list.", symbolList.Length);
            using var cmdCtx = _dbDefPair.GetCommandConnection();
            await cmdCtx.ExecuteAsync(RemoveExpiredIgnoreRecordsSql, symbolList.Select(k => new { Symbol = k }));
        }
    }

    internal async Task<DbMetaInfo> GetDbMetaInfoAsync(IReadOnlyCollection<string>? schemas = null)
    {
        if ((schemas?.Count ?? 0) == 0)
            schemas = ["public"];

        const string TcSql = @"
SELECT
table_name as Table,
(xpath('/row/count/text()', query_to_xml(format('SELECT COUNT(*) FROM ONLY %I.%I', table_schema, table_name), TRUE, TRUE, '')))[1]::text::bigint AS count
FROM
information_schema.tables
WHERE
table_schema = @schema
AND table_type = 'BASE TABLE'";
        const string IgSql = "SELECT symbol FROM public.ignored_symbols";

        using var queryCtx = _dbDefPair.GetQueryConnection();
        var ignoredSymbols = (await queryCtx.QueryAsync<string>(IgSql)).ToArray();

        var tableCounts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        foreach (var schema in schemas!.Distinct(StringComparer.OrdinalIgnoreCase))
        {
            var tableResults = await queryCtx.QueryAsync<(string Table, long Count)>(TcSql, new { schema = schema.ToLowerInvariant() });
            foreach (var (Table, Count) in tableResults)
                tableCounts[$"{schema}.{Table}"] = (int)Count;
        }

        return new DbMetaInfo
        {
            TableCounts = tableCounts,
            IgnoredSymbols = ignoredSymbols
        };
    }

    public async Task SaveStockSymbolsAsync(IReadOnlyCollection<Security> securities, Guid processId)
    {
        if (processId.Equals(Guid.Empty))
            processId = Constants.SystemId;

        using var cmdCtx = _dbDefPair.GetCommandConnection();
        foreach (var chunk in securities.Select(k => new Infrastructure.Database.DataAccessObjects.StockSymbol()
        {
            Symbol = k.Symbol,
            Registrant = k.Registrant,
            CreatedBy = processId,
            UpdatedBy = processId
        }).Chunk(Constants.DefaultChunkSize))
        {
            await cmdCtx.ExecuteAsync(SqlRepository.MergeStockSymbols, chunk);
        }
    }

    public async Task SaveSecuritiesAsync(IReadOnlyCollection<Security> securities, Guid processId)
    {
        if (processId.Equals(Guid.Empty))
            processId = Constants.SystemId;

        using var cmdCtx = _dbDefPair.GetCommandConnection();
        foreach (var chunk in securities.Select(k => new Infrastructure.Database.DataAccessObjects.SecurityInformation()
        {
            Symbol = k.Symbol,
            Cusip = k.Cusip,
            Figi = k.Figi,
            Isin = k.Isin,
            Issuer = k.Issuer,
            Type = k.SecurityType,
            CreatedBy = processId,
            UpdatedBy = processId
        }).Chunk(Constants.DefaultChunkSize))
        {
            await cmdCtx.ExecuteAsync(SqlRepository.MergeSecurityInformation, chunk);
        }
    }

    public async Task SaveEodPricesAsync(IReadOnlyCollection<Ohlc> prices, Guid processId)
    {
        if (processId.Equals(Guid.Empty))
            processId = Constants.SystemId;

        using var cmdCtx = _dbDefPair.GetCommandConnection();

        foreach (var chunk in prices.Select(k =>
            new Infrastructure.Database.DataAccessObjects.EodPrice(processId)
            {
                Date = k.Date,
                Open = k.Open,
                High = k.High,
                Low = k.Low,
                Close = k.Close,
                Volume = k.Volume,
                Symbol = k.Symbol,
                CreatedBy = processId,
                UpdatedBy = processId
            }).Chunk(Constants.DefaultChunkSize))
        {
            await cmdCtx.ExecuteAsync(SqlRepository.MergeEodPrices, chunk);
        }
    }

    public async Task SaveCompanyInfoAsync(Gimzo.Analysis.Fundamental.CompanyInformation company, Guid processId)
    {
        if (processId.Equals(Guid.Empty))
            processId = Constants.SystemId;

        using var cmdCtx = _dbDefPair.GetCommandConnection();

        await cmdCtx.ExecuteAsync(SqlRepository.MergeCompanyInfo,
            new Infrastructure.Database.DataAccessObjects.CompanyInformation(company, processId));
    }

    public async Task SaveIncomeStatementsAsync(IReadOnlyCollection<Gimzo.Analysis.Fundamental.IncomeStatement> statements, Guid processId)
    {
        if (processId.Equals(Guid.Empty))
            processId = Constants.SystemId;

        using var cmdCtx = _dbDefPair.GetCommandConnection();

        foreach (var chunk in statements.Select(k => new Infrastructure.Database.DataAccessObjects.IncomeStatement(k, processId)).Chunk(Constants.DefaultChunkSize))
            await cmdCtx.ExecuteAsync(SqlRepository.MergeIncomeStatements, chunk);
    }

    public async Task SaveBalanceSheetsAsync(IReadOnlyCollection<Gimzo.Analysis.Fundamental.BalanceSheet> statements, Guid processId)
    {
        if (processId.Equals(Guid.Empty))
            processId = Constants.SystemId;

        using var cmdCtx = _dbDefPair.GetCommandConnection();

        foreach (var chunk in statements.Select(k => new Infrastructure.Database.DataAccessObjects.BalanceSheet(k, processId)).Chunk(Constants.DefaultChunkSize))
            await cmdCtx.ExecuteAsync(SqlRepository.MergeBalanceSheets, chunk);
    }

    public async Task SaveCashFlowStatementsAsync(IReadOnlyCollection<Gimzo.Analysis.Fundamental.CashFlowStatement> statements, Guid processId)
    {
        if (processId.Equals(Guid.Empty))
            processId = Constants.SystemId;

        using var cmdCtx = _dbDefPair.GetCommandConnection();

        foreach (var chunk in statements.Select(k => new Infrastructure.Database.DataAccessObjects.CashFlowStatement(k, processId)).Chunk(Constants.DefaultChunkSize))
            await cmdCtx.ExecuteAsync(SqlRepository.MergeCashFlowStatements, chunk);
    }

    public async Task SaveDividendsAsync(IReadOnlyCollection<Gimzo.Analysis.Fundamental.Dividend> dividends, Guid processId)
    {
        if (processId.Equals(Guid.Empty))
            processId = Constants.SystemId;

        using var cmdCtx = _dbDefPair.GetCommandConnection();

        foreach (var chunk in dividends.Select(k => new Infrastructure.Database.DataAccessObjects.Dividend(k, processId)).Chunk(Constants.DefaultChunkSize))
            await cmdCtx.ExecuteAsync(SqlRepository.MergeDividends, chunk);
    }

    public async Task SaveSplitsAsync(IReadOnlyCollection<Gimzo.Analysis.Fundamental.StockSplit> splits, Guid processId)
    {
        if (processId.Equals(Guid.Empty))
            processId = Constants.SystemId;

        using var cmdCtx = _dbDefPair.GetCommandConnection();

        foreach (var chunk in splits.Select(k => new Infrastructure.Database.DataAccessObjects.StockSplit(k, processId)).Chunk(Constants.DefaultChunkSize))
            await cmdCtx.ExecuteAsync(SqlRepository.MergeStockSplits, chunk);
    }

    public async Task SaveEarningReleasesAsync(IReadOnlyCollection<Gimzo.Analysis.Fundamental.EarningsRelease> releases, Guid processId)
    {
        if (processId.Equals(Guid.Empty))
            processId = Constants.SystemId;

        using var cmdCtx = _dbDefPair.GetCommandConnection();

        foreach (var chunk in releases.Select(k => new Infrastructure.Database.DataAccessObjects.EarningsRelease(k, processId)).Chunk(Constants.DefaultChunkSize))
            await cmdCtx.ExecuteAsync(SqlRepository.MergeEarningsReleases, chunk);
    }

    public async Task SaveShortInterestsAsync(IReadOnlyCollection<Gimzo.Analysis.Fundamental.ShortInterest> shortInterests, Guid processId)
    {
        if (processId.Equals(Guid.Empty))
            processId = Constants.SystemId;

        using var cmdCtx = _dbDefPair.GetCommandConnection();

        foreach (var chunk in shortInterests.Select(k => new Infrastructure.Database.DataAccessObjects.ShortInterest(k, processId)).Chunk(Constants.DefaultChunkSize))
            await cmdCtx.ExecuteAsync(SqlRepository.MergeShortInterests, chunk);
    }

    /// <summary>
    /// Finds symbols with most recent pricing data greater than 10 days older than
    /// max eod pricing data found and adds them to the ignored list.
    /// </summary>
    public async Task AddDelistedSymbolsToIgnoreListAsync(Guid processId)
    {
        LogHelper.LogInfo(_logger, "Adding to ignore list delisted symbols.");
        if (processId.Equals(Guid.Empty))
            processId = Constants.SystemId;

        const string MaxPriceDateSql = @"SELECT MAX(date_eod) FROM public.eod_prices";
        using var queryCtx = _dbDefPair.GetQueryConnection();
        var maxDate = await queryCtx.QuerySingleOrDefaultAsync<DateOnly?>(MaxPriceDateSql);
        if (!maxDate.HasValue || maxDate.Equals(DateOnly.MinValue))
            throw new Exception("Could not find max date for eod prices.");

        const string FindDelistedSql = @"
SELECT symbol
FROM public.eod_prices
GROUP BY symbol
HAVING MAX(date_eod) <= @MaxDate - INTERVAL '10 days'";

        var symbols = (await queryCtx.QueryAsync<string>(FindDelistedSql, new { maxDate })).ToImmutableArray();

        if (symbols.Length > 0)
            await InsertIgnoredSymbolsAsync("Delisted", processId, symbols, expiration: null);

        const string FindEmptyPricesSql = @"
SELECT s.symbol
FROM public.stock_symbols s
LEFT JOIN public.eod_prices p ON s.symbol = p.symbol
WHERE p.symbol IS NULL";
        
        symbols = (await queryCtx.QueryAsync<string>(FindEmptyPricesSql)).ToImmutableArray();
        if (symbols.Length > 0)
            await InsertIgnoredSymbolsAsync("No price data", processId, symbols, expiration: null);
    }

    public async Task AddSymbolsWithPricesOutsideRangeToIgnoreListAsync(Guid processId, decimal min = 10M, decimal max = 2_000M)
    {
        LogHelper.LogInfo(_logger, "Adding to ignore list symbols with prices outside range.");
        if (processId.Equals(Guid.Empty))
            processId = Constants.SystemId;

        const string Sql = @"
SELECT symbol
FROM public.eod_prices
WHERE date_eod >= CURRENT_DATE - INTERVAL '6 months'
GROUP BY symbol
HAVING AVG(close) < @Min OR AVG(close) > @Max";

        using var queryCtx = _dbDefPair.GetQueryConnection();
        var symbols = (await queryCtx.QueryAsync<string>(Sql, new { Min = min, Max = max })).ToArray();

        if (symbols.Length > 0)
            await InsertIgnoredSymbolsAsync("Price outside supported range", processId, symbols,
                expiration: TimeHelper.TodayEastern.AddWeekdays(100));
    }

    public async Task AddSymbolsWithShortChartsToIgnoreListAsync(Guid processId, int minDaysOfData = 200)
    {
        LogHelper.LogInfo(_logger, "Adding to ignore list symbols with short charts.");
        const string Sql = @"
SELECT symbol, COUNT(*) AS Count, @MinDays AS Min, @MinDays - COUNT(*) AS Delta
FROM public.eod_prices
GROUP BY symbol
HAVING COUNT(*) < @MinDays";

        using var queryCtx = _dbDefPair.GetQueryConnection();
        var results = (await queryCtx.QueryAsync<EodCount>(Sql, new { MinDays = minDaysOfData })).ToArray();

        foreach (var dt in results.Select(k => k.Expiration).Distinct())
            await InsertIgnoredSymbolsAsync("Insufficient data", processId,
                [.. results.Where(k => k.Expiration.Equals(dt)).Select(k => k.Symbol)],
                dt);
    }

    private record struct EodCount(string Symbol, int Count, int Min, int Delta)
    {
        public readonly DateOnly Expiration => TimeHelper.TodayEastern.AddWeekdays(Delta);
    };

    /// <summary>
    /// Insert into the database a collection of symbols to ignore.
    /// The symbols share the same reason and the same expiration.
    /// </summary>
    public async Task InsertIgnoredSymbolsAsync(string reason, Guid processId,
        IReadOnlyCollection<string> symbols, DateOnly? expiration = null)
    {
        if (symbols.Count > 0)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(reason);
            if (processId.Equals(Guid.Empty))
                processId = Constants.SystemId;

            var daos = symbols.Select(k => new Infrastructure.Database.DataAccessObjects.IgnoredSymbol(processId)
            {
                Reason = reason,
                Expiration = expiration,
                Symbol = k,
                CreatedBy = processId,
                UpdatedBy = processId
            }).ToImmutableArray();
            using var cmdCtx = _dbDefPair.GetCommandConnection();
            foreach (var chunk in daos.Chunk(Constants.DefaultChunkSize))
                await cmdCtx.ExecuteAsync(SqlRepository.MergeIgnoredSymbol, chunk);
        }
    }

    /// <summary>
    /// Removes from the database all data related to ignored symbols.
    /// </summary>
    public async Task DeleteDataForIgnoredSymbolsAsync()
    {
        LogHelper.LogInfo(_logger, "Deleting data for ignored symbols");

        const string GetIgnoredSymbolsSql = @"SELECT symbol FROM public.ignored_symbols";
        using var queryCtx = _dbDefPair.GetQueryConnection();
        var symbols = await queryCtx.QueryAsync<string>(GetIgnoredSymbolsSql);
        using var cmdCtx = _dbDefPair.GetCommandConnection();

        foreach (var table in new string[] {
            "public.balance_sheets",
            "public.cash_flow_statements",
            "public.dividends",
            "public.earnings_releases",
            "public.efficiency_ratios",
            "public.employee_counts",
            "public.eod_prices",
            "public.executive_compensations",
            "public.income_statements",
            "public.key_metrics",
            "public.liquidity_ratios",
            "public.market_caps",
            "public.profitability_ratios",
            "public.security_information",
            "public.short_interests",
            "public.solvency_ratios",
            "public.stock_splits",
            "public.stock_symbols",
            "public.us_companies",
            "public.valuation_ratios"
        })
        {
            string sql = $"DELETE FROM {table} WHERE symbol = @Symbol";
            foreach (var symbol in symbols)
                await cmdCtx.ExecuteAsync(sql, new { symbol });
        }
    }

    public async Task SaveKeyMetricsAsync(IReadOnlyCollection<Gimzo.Analysis.Fundamental.KeyMetrics> metrics, Guid processId)
    {
        if (processId.Equals(Guid.Empty))
            processId = Constants.SystemId;

        using var cmdCtx = _dbDefPair.GetCommandConnection();

        foreach (var chunk in metrics.Select(k => new Infrastructure.Database.DataAccessObjects.KeyMetrics(k, processId)).Chunk(Constants.DefaultChunkSize))
            await cmdCtx.ExecuteAsync(SqlRepository.MergeKeyMetrics, chunk);
    }

    public async Task SaveMarketCapsAsync(IReadOnlyCollection<Gimzo.Analysis.Fundamental.MarketCap> marketCaps, Guid processId)
    {
        if (processId.Equals(Guid.Empty))
            processId = Constants.SystemId;

        using var cmdCtx = _dbDefPair.GetCommandConnection();

        foreach (var chunk in marketCaps.Select(k => new Infrastructure.Database.DataAccessObjects.MarketCap(k, processId)).Chunk(Constants.DefaultChunkSize))
            await cmdCtx.ExecuteAsync(SqlRepository.MergeMarketCaps, chunk);
    }

    public async Task SaveEmployeeCountsAsync(IReadOnlyCollection<Gimzo.Analysis.Fundamental.EmployeeCount> empCounts, Guid processId)
    {
        if (processId.Equals(Guid.Empty))
            processId = Constants.SystemId;

        using var cmdCtx = _dbDefPair.GetCommandConnection();

        foreach (var chunk in empCounts.Select(k => new Infrastructure.Database.DataAccessObjects.EmployeeCount(k, processId)).Chunk(Constants.DefaultChunkSize))
            await cmdCtx.ExecuteAsync(SqlRepository.MergeEmployeeCounts, chunk);
    }

    public async Task SaveExecCompensationAsync(IReadOnlyCollection<Gimzo.Analysis.Fundamental.ExecutiveCompensation> comps, Guid processId)
    {
        if (processId.Equals(Guid.Empty))
            processId = Constants.SystemId;

        using var cmdCtx = _dbDefPair.GetCommandConnection();

        foreach (var chunk in comps.Select(k => new Infrastructure.Database.DataAccessObjects.ExecutiveCompensation(k, processId)).Chunk(Constants.DefaultChunkSize))
            await cmdCtx.ExecuteAsync(SqlRepository.MergeExecutiveCompensations, chunk);
    }

    public async Task SaveEfficiencyRatiosAsync(IReadOnlyCollection<Gimzo.Analysis.Fundamental.EfficiencyRatios> ratios, Guid processId)
    {
        if (processId.Equals(Guid.Empty))
            processId = Constants.SystemId;

        using var cmdCtx = _dbDefPair.GetCommandConnection();

        foreach (var chunk in ratios.Select(k => new Infrastructure.Database.DataAccessObjects.EfficiencyRatios(k, processId)).Chunk(Constants.DefaultChunkSize))
            await cmdCtx.ExecuteAsync(SqlRepository.MergeEfficiencyRatios, chunk);
    }

    public async Task SaveLiquidityRatiosAsync(IReadOnlyCollection<Gimzo.Analysis.Fundamental.LiquidityRatios> ratios, Guid processId)
    {
        if (processId.Equals(Guid.Empty))
            processId = Constants.SystemId;

        using var cmdCtx = _dbDefPair.GetCommandConnection();

        foreach (var chunk in ratios.Select(k => new Infrastructure.Database.DataAccessObjects.LiquidityRatios(k, processId)).Chunk(Constants.DefaultChunkSize))
            await cmdCtx.ExecuteAsync(SqlRepository.MergeLiquidityRatios, chunk);
    }

    public async Task SaveProfitabilityRatiosAsync(IReadOnlyCollection<Gimzo.Analysis.Fundamental.ProfitabilityRatios> ratios, Guid processId)
    {
        if (processId.Equals(Guid.Empty))
            processId = Constants.SystemId;

        using var cmdCtx = _dbDefPair.GetCommandConnection();

        foreach (var chunk in ratios.Select(k => new Infrastructure.Database.DataAccessObjects.ProfitabilityRatios(k, processId)).Chunk(Constants.DefaultChunkSize))
            await cmdCtx.ExecuteAsync(SqlRepository.MergeProfitabilityRatios, chunk);
    }

    public async Task SaveSolvencyRatiosAsync(IReadOnlyCollection<Gimzo.Analysis.Fundamental.SolvencyRatios> ratios, Guid processId)
    {
        if (processId.Equals(Guid.Empty))
            processId = Constants.SystemId;

        using var cmdCtx = _dbDefPair.GetCommandConnection();

        foreach (var chunk in ratios.Select(k => new Infrastructure.Database.DataAccessObjects.SolvencyRatios(k, processId)).Chunk(Constants.DefaultChunkSize))
            await cmdCtx.ExecuteAsync(SqlRepository.MergeSolvencyRatios, chunk);
    }

    public async Task SaveValuationRatiosAsync(IReadOnlyCollection<Gimzo.Analysis.Fundamental.ValuationRatios> ratios, Guid processId)
    {
        if (processId.Equals(Guid.Empty))
            processId = Constants.SystemId;

        using var cmdCtx = _dbDefPair.GetCommandConnection();

        foreach (var chunk in ratios.Select(k => new Infrastructure.Database.DataAccessObjects.ValuationRatios(k, processId)).Chunk(Constants.DefaultChunkSize))
            await cmdCtx.ExecuteAsync(SqlRepository.MergeValuationRatios, chunk);
    }

}
