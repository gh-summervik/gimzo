using Dapper;
using Gimzo.Analysis.Fundamental;
using Gimzo.Analysis.Technical;
using Gimzo.Analysis.Technical.Charts;
using Gimzo.Analysis.Technical.Trends;
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
    }

    internal async Task SaveProcess(Infrastructure.Database.DataAccessObjects.Process process)
    {
        using var cmdCtx = _dbDefPair.GetCommandConnection();
        await cmdCtx.ExecuteAsync(SqlRepository.MergeProcess, process);
    }

    public Task<IEnumerable<string>> GetSymbolsAsync() =>
        _dbDefPair.GetQueryConnection().QueryAsync<string>("SELECT symbol FROM public.stock_symbols");

    public async Task<CompanyInformation?> GetCompanyInformationAsync(string symbol)
    {
        ArgumentNullException.ThrowIfNullOrWhiteSpace(symbol, nameof(symbol));

        using var queryCtx = _dbDefPair.GetQueryConnection();
        string sql = $"{SqlRepository.SelectCompanyInfo} WHERE symbol = @Symbol LIMIT 1";
        var dao = await queryCtx.QueryFirstOrDefaultAsync<Infrastructure.Database.DataAccessObjects.CompanyInformation>(sql, new { Symbol = symbol.ToUpperInvariant() });
        return dao?.ToDomain();
    }

    public async Task<IReadOnlyCollection<Ohlc>> GetOhlcAsync(string symbol)
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
        int lookback = Common.Constants.DefaultChartLookback,
        ChartInterval interval = ChartInterval.Daily) =>
            HashCode.Combine(symbol.ToUpperInvariant(), lookback, interval);

    public async Task<Chart?> GetChartAsync(string symbol,
        int lookback = Common.Constants.DefaultChartLookback,
        ChartInterval interval = ChartInterval.Daily)
    {
        var cacheKey = GetChartCacheKey(symbol, lookback, interval);
        Chart? chart;
        if (_memoryCache.TryGetValue(cacheKey, out var cachedValue) && cachedValue != null)
            chart = cachedValue as Chart;
        else
        {
            var ohlcs = (await GetOhlcAsync(symbol)).ToArray();
            if (ohlcs.Length == 0)
                return null;
            chart = new Chart(symbol.ToUpperInvariant())
                .WithCandles(ohlcs)
                .WithTrend(new GimzoTrend(ohlcs))
                .WithMovingAverage(21, MovingAverageType.Exponential)
                .WithMovingAverage(50, MovingAverageType.Exponential)
                .WithMovingAverage(200, MovingAverageType.Exponential)
                .WithAverageTrueRange(Common.Constants.DefaultAverageTrueRangePeriod)
                .WithBollingerBand(21, MovingAverageType.Exponential)
                .Build();
            _memoryCache.Set(cacheKey, chart);
        }
        return chart;
    }
}
