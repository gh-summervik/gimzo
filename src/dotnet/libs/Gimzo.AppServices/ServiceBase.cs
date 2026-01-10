using Dapper;
using Gimzo.Analysis.Fundamental;
using Gimzo.Analysis.Technical;
using Gimzo.Analysis.Technical.Charts;
using Gimzo.AppServices.Backtests;
using Gimzo.Infrastructure;
using Gimzo.Infrastructure.Database;
using Microsoft.Extensions.Caching.Memory;

namespace Gimzo.AppServices;

public abstract class ServiceBase
{
    protected readonly DbDefPair _dbDefPair;
    protected readonly IMemoryCache _memoryCache;

    public ServiceBase(DbDefPair dbDefPair, IMemoryCache memoryCache)
    {
        _dbDefPair = dbDefPair;
        _memoryCache = memoryCache;
        SqlMapper.AddTypeHandler(new DateOnlyTypeHandler());
        SqlMapper.AddTypeHandler(new NullableDateOnlyTypeHandler());
        SqlMapper.AddTypeHandler(new DictionaryIntDecimalJsonbHandler());
    }

    public async Task<DbMetaInfo> GetDbMetaInfoAsync(IReadOnlyCollection<string>? schemas = null)
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

    internal async Task SaveProcess(Infrastructure.Database.DataAccessObjects.Process process)
    {
        using var cmdCtx = _dbDefPair.GetCommandConnection();
        await cmdCtx.ExecuteAsync(SqlRepository.MergeProcess, process);
    }

    internal Task<IEnumerable<string>> GetSymbolsAsync() =>
        _dbDefPair.GetQueryConnection().QueryAsync<string>("SELECT symbol FROM public.stock_symbols");

    protected async Task<CompanyInformation?> GetCompanyInformationAsync(string symbol)
    {
        ArgumentNullException.ThrowIfNullOrWhiteSpace(symbol, nameof(symbol));

        using var queryCtx = _dbDefPair.GetQueryConnection();
        string sql = $"{SqlRepository.SelectCompanyInfo} WHERE symbol = @Symbol LIMIT 1";
        var dao = await queryCtx.QueryFirstOrDefaultAsync<Infrastructure.Database.DataAccessObjects.CompanyInformation>(sql, new { Symbol = symbol.ToUpperInvariant() });
        return dao?.ToDomain();
    }

    internal async Task<IReadOnlyCollection<Ohlc>> GetOhlcAsync(string symbol)
    {
        ArgumentNullException.ThrowIfNullOrWhiteSpace(symbol, nameof(symbol));
        using var queryCtx = _dbDefPair.GetQueryConnection();
        string sql = $"{SqlRepository.SelectEodPrices} WHERE symbol = @Symbol";
        var daos = (await queryCtx.QueryAsync<Infrastructure.Database.DataAccessObjects.EodPrice>(sql,
            new { Symbol = symbol.ToUpperInvariant() })).OrderBy(k => k.Date).ToArray();

        if (daos.Length == 0)
            return [];

        return [.. daos.Select(k => k.ToOhlc())];
    }

    private static int GetChartCacheKey(string symbol,
        ChartInterval interval = ChartInterval.Daily) =>
            HashCode.Combine(symbol.ToUpperInvariant(), interval);

    internal async Task<Chart?> GetChartAsync(string symbol, ChartConfiguration config)
    {
        var cacheKey = GetChartCacheKey(symbol, config.Interval);
        Chart? chart;
        if (_memoryCache.TryGetValue(cacheKey, out var cachedValue) && cachedValue is not null)
            chart = cachedValue as Chart;
        else
        {
            var ohlcs = (await GetOhlcAsync(symbol)).ToArray();
            if (ohlcs.Length == 0)
                return null;
            chart = new Chart(symbol.ToUpperInvariant())
                .WithPriceActions(ohlcs)
                .WithConfiguration(config)
                .Build();
            _memoryCache.Set(cacheKey, chart);
        }
        return chart;
    }

    internal async Task<Chart?> GetChartAsync(string symbol,
        ChartInterval interval = ChartInterval.Daily)
    {
        var cacheKey = GetChartCacheKey(symbol, interval);
        Chart? chart;
        if (_memoryCache.TryGetValue(cacheKey, out var cachedValue) && cachedValue is not null)
            chart = cachedValue as Chart;
        else
        {
            var ohlcs = (await GetOhlcAsync(symbol)).ToArray();
            if (ohlcs.Length == 0)
                return null;
            chart = new Chart(symbol.ToUpperInvariant())
                .WithPriceActions(ohlcs)
                //.WithTrend(new RelativeStrengthIndex(ohlcs))
                .WithMovingAverage(21, MovingAverageType.Exponential)
                .WithMovingAverage(50, MovingAverageType.Exponential)
                .WithMovingAverage(200, MovingAverageType.Exponential)
                .WithAverageTrueRangePeriod(Common.Constants.DefaultAverageTrueRangePeriod)
                .WithBollingerBand(21, MovingAverageType.Exponential)
                .Build();
            _memoryCache.Set(cacheKey, chart);
        }
        return chart;
    }
}
