using Dapper;
using Gimzo.Analysis.Fundamental;
using Gimzo.Analysis.Technical;
using Gimzo.Analysis.Technical.Charts;
using Gimzo.Analysis.Technical.Trends;
using Gimzo.Infrastructure.Database;
using Microsoft.Extensions.Caching.Memory;

namespace Gimzo.AppServices.Data;

internal sealed partial class DataService
{
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

    public async Task<Ohlc[]> GetOhlcAsync(string symbol)
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

    private static int GetChartCacheKey(string symbol, int lookback = Common.Constants.DefaultChartLookback, ChartInterval interval = ChartInterval.Daily) =>
        HashCode.Combine(symbol.ToUpperInvariant(), lookback, interval);

    public async Task<Chart?> GetChartAsync(string symbol, int lookback = Common.Constants.DefaultChartLookback, ChartInterval interval = ChartInterval.Daily)
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

    internal async Task<LiquidityRatios?> GetLatestLiquidityRatiosAsync(string centralIndexKey)
    {
        string sql = $@"{SqlRepository.SelectLiquidityRatios}
WHERE central_index_key = @Cik AND fiscal_period IN ('Q1','Q2','Q3','Q4')
ORDER BY period_end_date DESC LIMIT 1";

        using var queryCtx = _dbDefPair.GetQueryConnection();
        var result = await queryCtx.QueryFirstOrDefaultAsync<Infrastructure.Database.DataAccessObjects.LiquidityRatios>(sql,
            new { Cik = centralIndexKey });

        return result?.ToDomain();
    }

    internal async Task<SolvencyRatios?> GetLatestSolvencyRatiosAsync(string centralIndexKey)
    {
        string sql = $@"{SqlRepository.SelectSolvencyRatios}
WHERE central_index_key = @Cik AND fiscal_period IN ('Q1','Q2','Q3','Q4')
ORDER BY period_end_date DESC LIMIT 1";

        using var queryCtx = _dbDefPair.GetQueryConnection();
        var result = await queryCtx.QueryFirstOrDefaultAsync<Infrastructure.Database.DataAccessObjects.SolvencyRatios>(sql, new { Cik = centralIndexKey });

        return result?.ToDomain();
    }

    internal async Task<ProfitabilityRatios?> GetLatestProfitabilityRatiosAsync(string centralIndexKey)
    {
        string sql = $@"{SqlRepository.SelectProfitabilityRatios}
WHERE central_index_key = @Cik AND fiscal_period IN ('Q1','Q2','Q3','Q4')
ORDER BY period_end_date DESC LIMIT 1";

        using var queryCtx = _dbDefPair.GetQueryConnection();
        var result = await queryCtx.QueryFirstOrDefaultAsync<Infrastructure.Database.DataAccessObjects.ProfitabilityRatios>(sql, new { Cik = centralIndexKey });

        return result?.ToDomain();
    }

    internal async Task<KeyMetrics?> GetLatestKeyMetricsAsync(string centralIndexKey)
    {
        string sql = $@"{SqlRepository.SelectKeyMetrics}
WHERE central_index_key = @Cik 
ORDER BY period_end_date DESC LIMIT 1";

        using var queryCtx = _dbDefPair.GetQueryConnection();
        var result = await queryCtx.QueryFirstOrDefaultAsync<Infrastructure.Database.DataAccessObjects.KeyMetrics>(sql, new { Cik = centralIndexKey });

        return result?.ToDomain();
    }

    internal async Task<ValuationRatios?> GetLatestValuationRatiosAsync(string centralIndexKey)
    {
        string sql = $@"{SqlRepository.SelectValuationRatios}
WHERE central_index_key = @Cik AND fiscal_period IN ('Q1','Q2','Q3','Q4')
ORDER BY period_end_date DESC LIMIT 1";

        using var queryCtx = _dbDefPair.GetQueryConnection();
        var result = await queryCtx.QueryFirstOrDefaultAsync<Infrastructure.Database.DataAccessObjects.ValuationRatios>(sql, new { Cik = centralIndexKey });

        return result?.ToDomain();
    }

    internal async Task<EfficiencyRatios?> GetLatestEfficiencyRatiosAsync(string centralIndexKey)
    {
        string sql = $@"{SqlRepository.SelectEfficiencyRatios}
WHERE central_index_key = @Cik AND fiscal_period IN ('Q1','Q2','Q3','Q4')
ORDER BY period_end_date DESC LIMIT 1";

        using var queryCtx = _dbDefPair.GetQueryConnection();
        var result = await queryCtx.QueryFirstOrDefaultAsync<Infrastructure.Database.DataAccessObjects.EfficiencyRatios>(sql, new { Cik = centralIndexKey });

        return result?.ToDomain();
    }

    internal async Task<ShortInterest?> GetLatestShortInterestsAsync(string centralIndexKey)
    {
        string sql = $@"{SqlRepository.SelectShortInterests} si
JOIN public.us_companies uc ON uc.symbol = si.symbol
WHERE uc.central_index_key = @Cik
ORDER BY si.settlement_date DESC LIMIT 1";

        using var queryCtx = _dbDefPair.GetQueryConnection();
        var result = await queryCtx.QueryFirstOrDefaultAsync<Infrastructure.Database.DataAccessObjects.ShortInterest>(sql, new { Cik = centralIndexKey });

        return result?.ToDomain();
    }

}