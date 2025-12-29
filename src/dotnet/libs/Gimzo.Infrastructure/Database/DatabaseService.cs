using Dapper;
using Gimzo.Analysis.Fundamental;
using Gimzo.Analysis.Technical.Charts;
using Gimzo.Common;
using Microsoft.Extensions.Logging;

namespace Gimzo.Infrastructure.Database;

internal sealed class DatabaseService
{
    private readonly DbDefPair _dbDefPair;
    private readonly ILogger _logger;
    private const int ChunkSize = 1_000;

    public DatabaseService(DbDefPair dbDefPair, ILogger logger)
    {
        _dbDefPair = dbDefPair;
        _logger = logger;

        SqlMapper.AddTypeHandler(new DateOnlyTypeHandler());
        SqlMapper.AddTypeHandler(new NullableDateOnlyTypeHandler());
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

    public async Task<DbMetaInfo> GetDbMetaInfoAsync(string[]? schemas = null)
    {
        if ((schemas?.Length ?? 0) == 0)
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

    public async Task SaveStockSymbolsAsync(IEnumerable<Security> securities, Guid processId)
    {
        if (processId.Equals(Guid.Empty))
            processId = Constants.SystemId;

        using var cmdCtx = _dbDefPair.GetCommandConnection();
        foreach (var chunk in securities.Select(k => new DataAccessObjects.StockSymbol()
        {
            Symbol = k.Symbol,
            Registrant = k.Registrant,
            CreatedBy = processId,
            UpdatedBy = processId
        }).Chunk(ChunkSize))
        {
            await cmdCtx.ExecuteAsync(SqlRepository.MergeStockSymbols, chunk);
        }
    }

    public async Task SaveSecuritiesAsync(IEnumerable<Security> securities, Guid processId)
    {
        if (processId.Equals(Guid.Empty))
            processId = Constants.SystemId;

        using var cmdCtx = _dbDefPair.GetCommandConnection();
        foreach (var chunk in securities.Select(k => new DataAccessObjects.SecurityInformation()
        {
            Symbol = k.Symbol,
            Cusip = k.Cusip,
            Figi = k.Figi,
            Isin = k.Isin,
            Issuer = k.Issuer,
            Type = k.SecurityType,
            CreatedBy = processId,
            UpdatedBy = processId
        }).Chunk(ChunkSize))
        {
            await cmdCtx.ExecuteAsync(SqlRepository.MergeSecurityInformation, chunk);
        }
    }

    public async Task SaveEodPricesAsync(IEnumerable<Ohlc> prices, Guid processId)
    {
        if (processId.Equals(Guid.Empty))
            processId = Constants.SystemId;

        using var cmdCtx = _dbDefPair.GetCommandConnection();

        foreach (var chunk in prices.Select(k =>
            new DataAccessObjects.EodPrice(processId)
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
            }).Chunk(ChunkSize))
        {
            await cmdCtx.ExecuteAsync(SqlRepository.MergeEodPrices, chunk);
        }
    }

    /// <summary>
    /// Finds symbols with most recent pricing data greater than 10 days older than
    /// max eod pricing data found and adds them to the ignored list.
    /// </summary>
    public async Task AddDelistedSymbolsToIgnoreListAsync(Guid processId)
    {
        LogHelper.LogInfo(_logger, "Adding delisted symbols to ignore list.");
        if (processId.Equals(Guid.Empty))
            processId = Constants.SystemId;

        const string MaxPriceDateSql = @"SELECT MAX(date_eod) FROM public.eod_prices;";
        using var queryCtx = _dbDefPair.GetQueryConnection();
        var maxDate = await queryCtx.QuerySingleOrDefaultAsync<DateOnly?>(MaxPriceDateSql);
        if (!maxDate.HasValue || maxDate.Equals(DateOnly.MinValue))
            throw new Exception("Could not find max date for eod prices.");

        const string FindDelistedSql = @"
SELECT symbol
FROM public.eod_prices
GROUP BY symbol
HAVING MAX(date_eod) <= @MaxDate - INTERVAL '10 days'";

        var symbols = (await queryCtx.QueryAsync<string>(FindDelistedSql, new { maxDate })).ToArray();

        if (symbols.Length > 0)
            await InsertIgnoredSymbolsAsync("Delisted", processId, expiration: null, symbols);
    }

    public async Task AddSymbolsWithPricesOutsideRangeToIgnoreListAsync(Guid processId, decimal min = 10M, decimal max = 2_000M)
    {
        LogHelper.LogInfo(_logger, "Adding symbols with prices outside range to ignore list.");
        
        if (processId.Equals(Guid.Empty))
            processId = Constants.SystemId;

        const string Sql = @"
WITH recent AS (
    SELECT symbol, close
    FROM (
        SELECT symbol, close, date_eod,
        ROW_NUMBER() OVER (PARTITION BY symbol ORDER BY date_eod DESC) AS rn
        FROM public.eod_prices
    ) sub
    WHERE rn <= 200
)
SELECT symbol
FROM recent
GROUP BY symbol
HAVING AVG(close) < @Min OR AVG(close) > @Max";
        using var queryCtx = _dbDefPair.GetQueryConnection();
        var symbols = (await queryCtx.QueryAsync<string>(Sql, new { Min = min, Max = max })).ToArray();
        if (symbols.Length > 0)
            await InsertIgnoredSymbolsAsync("Price outside supported range", processId, 
                expiration: TimeHelper.TodayEastern.AddWeekdays(100), symbols);
    }

    //    public async Task AddSymbolsWithPricesOutsideRangeToIgnoreListAsync(Guid processId, decimal min = 10M, decimal max = 2_000M)
    //    {
    //        LogHelper.LogInfo(_logger, "Adding symbols with prices outside range to ignore list.");
    //        if (processId.Equals(Guid.Empty))
    //            processId = Constants.SystemId;

    //        const string Sql = @"
    //SELECT symbol
    //FROM public.eod_prices
    //GROUP BY symbol
    //HAVING AVG(close) < @Min OR AVG(close) > @Max";

    //        using var queryCtx = _dbDefPair.GetQueryConnection();
    //        var symbols = (await queryCtx.QueryAsync<string>(Sql, new { Min = min, Max = max })).ToArray();


    //        if (symbols.Length > 0)
    //            await InsertIgnoredSymbolsAsync("Price outside supported range", processId, expiration: null, symbols);
    //    }

    public async Task AddSymbolsWithShortChartsToIgnoreListAsync(Guid processId, int minDaysOfData = 200)
    {
        LogHelper.LogInfo(_logger, "Adding symbols with short charts to ignore list");
        const string Sql = @"
SELECT symbol, COUNT(*) AS Count, @MinDays AS Min, @MinDays - COUNT(*) AS Delta
FROM public.eod_prices
GROUP BY symbol
HAVING COUNT(*) < @MinDays";

        using var queryCtx = _dbDefPair.GetQueryConnection();
        var results = (await queryCtx.QueryAsync<EodCount>(Sql, new { MinDays = minDaysOfData })).ToArray();

        foreach (var dt in results.Select(k => k.Expiration).Distinct())
            await InsertIgnoredSymbolsAsync("Insufficient data", processId, dt,
                [.. results.Where(k => k.Expiration.Equals(dt)).Select(k => k.Symbol)]);
    }

    private record struct EodCount(string Symbol, int Count, int Min, int Delta)
    {
        public readonly DateOnly Expiration => TimeHelper.TodayEastern.AddWeekdays(Delta);
    };

    public async Task InsertIgnoredSymbolsAsync(string reason, Guid processId,
        DateOnly? expiration = null, params string[] symbols)
    {
        if (symbols.Length > 0)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(reason);
            if (processId.Equals(Guid.Empty))
                processId = Constants.SystemId;

            var daos = symbols.Select(k => new DataAccessObjects.IgnoredSymbol(processId)
            {
                Reason = reason,
                Expiration = expiration,
                Symbol = k,
                CreatedBy = processId,
                UpdatedBy = processId
            }).ToArray();
            using var cmdCtx = _dbDefPair.GetCommandConnection();
            foreach (var chunk in daos.Chunk(ChunkSize))
                await cmdCtx.ExecuteAsync(SqlRepository.MergeIgnoredSymbol, chunk);
        }
    }

    public async Task DeleteDataForIgnoredSymbolsAsync()
    {
        LogHelper.LogInfo(_logger, "Deleting data for ignored symbols");

        const string GetIgnoredSymbolsSql = @"SELECT symbol FROM public.ignored_symbols";
        using var queryCtx = _dbDefPair.GetQueryConnection();
        var symbols = await queryCtx.QueryAsync<string>(GetIgnoredSymbolsSql);
        using var cmdCtx = _dbDefPair.GetCommandConnection();
        const string DeleteSql = @"
DELETE FROM public.balance_sheets WHERE symbol = @Symbol;
DELETE FROM public.cash_flow_statements WHERE symbol = @Symbol;
DELETE FROM public.dividends WHERE symbol = @Symbol;
DELETE FROM public.earnings_releases WHERE symbol = @Symbol;
DELETE FROM public.efficiency_ratios WHERE symbol = @Symbol;
DELETE FROM public.employee_counts WHERE symbol = @Symbol;
DELETE FROM public.eod_prices WHERE symbol = @Symbol;
DELETE FROM public.executive_compensations WHERE symbol = @Symbol;
DELETE FROM public.income_statements WHERE symbol = @Symbol;
DELETE FROM public.key_metrics WHERE symbol = @Symbol;
DELETE FROM public.liquidity_ratios WHERE symbol = @Symbol;
DELETE FROM public.market_caps WHERE symbol = @Symbol;
DELETE FROM public.profitability_ratios WHERE symbol = @Symbol;
DELETE FROM public.security_information WHERE symbol = @Symbol;
DELETE FROM public.short_interests WHERE symbol = @Symbol;
DELETE FROM public.solvency_ratios WHERE symbol = @Symbol;
DELETE FROM public.stock_splits WHERE symbol = @Symbol;
DELETE FROM public.stock_symbols WHERE symbol = @Symbol;
DELETE FROM public.us_companies WHERE symbol = @Symbol;
DELETE FROM public.valuation_ratios WHERE symbol = @Symbol;";
        foreach (var symbol in symbols)
            await cmdCtx.ExecuteAsync(DeleteSql, new { symbol });
    }
}