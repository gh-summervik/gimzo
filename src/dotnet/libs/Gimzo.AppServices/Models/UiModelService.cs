using Gimzo.Analysis.Technical;
using Gimzo.Analysis.Technical.Charts;
using Gimzo.Analysis.Technical.Trends;
using Gimzo.Infrastructure.Database;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace Gimzo.AppServices.Models;

public sealed class UiModelService(DbDefPair dbDefPair, IMemoryCache memoryCache, ILogger<UiModelService> logger)
{
    private readonly DatabaseService _dbService = new(dbDefPair, logger);
    private readonly IMemoryCache _memoryCache = memoryCache;
    private const int ChartLookback = 60;
    private const int AverageTrueRangePeriod = 14;

    private static int GetChartCacheKey(string symbol, int lookback = ChartLookback, ChartInterval interval = ChartInterval.Daily) =>
        HashCode.Combine(symbol.ToUpperInvariant(), lookback, interval);

    private async Task<Chart?> GetChartAsync(string symbol, int lookback = ChartLookback, ChartInterval interval = ChartInterval.Daily)
    {
        var cacheKey = GetChartCacheKey(symbol, lookback, interval);
        Chart? chart;
        if (_memoryCache.TryGetValue(cacheKey, out var cachedValue) && cachedValue != null)
            chart = cachedValue as Chart;
        else
        {
            var ohlcs = (await _dbService.GetOhlcAsync(symbol)).ToArray();
            if (ohlcs.Length == 0)
                return null;
            chart = new Chart(symbol.ToUpperInvariant(), lookbackLength: 60)
                .WithCandles(ohlcs)
                .WithTrend(new GimzoTrend(ohlcs))
                .WithMovingAverage(21, MovingAverageType.Exponential)
                .WithMovingAverage(50, MovingAverageType.Exponential)
                .WithMovingAverage(200, MovingAverageType.Exponential)
                .WithAverageTrueRange(AverageTrueRangePeriod)
                .Build();
            _memoryCache.Set(cacheKey, chart);
        }
        return chart;
    }

    public async Task<CompanyInfo?> GetCompanyInfoAsync(string symbol)
    {
        var coInfo = await _dbService.GetCompanyInformationAsync(symbol);
        if (coInfo == null)
            return null;

        Chart? chart = await GetChartAsync(symbol, ChartLookback, ChartInterval.Daily);

        decimal? fiftyTwoWeekLow = null;
        decimal? fiftyTwoWeekHigh = null;
        long? averageVolume = null;

        if (chart != null && chart.PriceActions.Length > 0)
        {
            var lastDate = chart.PriceActions[^1].Date;
            var yearPrior = chart.GetNearestDate(lastDate.AddYears(-1)).GetValueOrDefault();

            var start = Math.Max(0, chart.GetIndexOfDate(yearPrior));
            var range = chart.GetSpan(start, chart.Length - 1).GetPriceRange();
            fiftyTwoWeekLow = range.Low;
            fiftyTwoWeekHigh = range.High;

            var monthPrior = chart.GetNearestDate(lastDate.AddMonths(-1)).GetValueOrDefault();
            start = Math.Max(0, chart.GetIndexOfDate(monthPrior));
            var x = chart.GetSpan(start, chart.Length - 1);
            averageVolume = Convert.ToInt64(x.PriceActions.Average(k => k.Volume));
        }

        return new()
        {
            BusinessAddress = coInfo.BusinessAddress,
            CentralIndexKey = coInfo.CentralIndexKey,
            ChiefExecutiveOfficer = coInfo.ChiefExecutiveOfficer,
            DateFounding = coInfo.DateFounding,
            Description = coInfo.Description,
            Ein = coInfo.Ein,
            Exchange = coInfo.Exchange,
            FiscalYearEnd = coInfo.FiscalYearEnd,
            FormerName = coInfo.FormerName,
            Industry = coInfo.Industry,
            Isin = coInfo.Isin,
            Lei = coInfo.Lei,
            MailingAddress = coInfo.MailingAddress,
            MarketCap = coInfo.MarketCap,
            NumberEmployees = coInfo.NumberEmployees,
            PhoneNumber = coInfo.PhoneNumber,
            Registrant = coInfo.Registrant,
            SharesIssued = coInfo.SharesIssued,
            SharesOutstanding = coInfo.SharesOutstanding,
            Sic = coInfo.SicCode == null ? null : $"{coInfo.SicCode} ({coInfo.SicDescription})",
            StateOfIncorporation = coInfo.StateOfIncorporation,
            Symbol = coInfo.Symbol,
            WebSite = coInfo.WebSite,
            LastOhlc = chart?.Candlesticks.LastOrDefault(),
            CurrentAverageTrueRange = chart?.GetAverageTrueRangeForPeriod(AverageTrueRangePeriod),
            FiftyTwoWeekLow = fiftyTwoWeekLow,
            FiftyTwoWeekHigh = fiftyTwoWeekHigh,
            TwentyDayAverageVolume = averageVolume
        };
    }
}
