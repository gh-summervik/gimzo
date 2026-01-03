using Gimzo.Analysis.Technical;
using Gimzo.Analysis.Technical.Charts;
using Gimzo.Infrastructure.Database;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Gimzo.AppServices.Models;

public sealed class UiModelService(DbDefPair dbDefPair, IMemoryCache memoryCache, ILogger<UiModelService> logger)
    :ServiceBase(dbDefPair,memoryCache)
{
    private readonly ILogger<UiModelService> _logger = logger;

    public async Task<CompanyInfo?> GetCompanyInfoAsync(string symbol)
    {
        var coInfo = await GetCompanyInformationAsync(symbol);
        if (coInfo == null)
            return null;

        Chart? chart = await GetChartAsync(symbol, Common.Constants.DefaultChartLookback, ChartInterval.Daily);

        decimal? fiftyTwoWeekLow = null;
        decimal? fiftyTwoWeekHigh = null;
        long? averageVolume = null;
        double? lastTrendValue = null;

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
            lastTrendValue = chart.TrendValues[^1];
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
            CurrentAverageTrueRange = chart?.GetAverageTrueRangeForPeriod(Common.Constants.DefaultAverageTrueRangePeriod),
            FiftyTwoWeekLow = fiftyTwoWeekLow,
            FiftyTwoWeekHigh = fiftyTwoWeekHigh,
            TwentyDayAverageVolume = averageVolume,
            LastTrendValue = lastTrendValue
        };
    }

    public async Task<ChartModel?> GetChartModelAsync(string symbol,
        DateOnly start, DateOnly? finish = null,
        int lookback = Common.Constants.DefaultChartLookback,
        ChartInterval interval = ChartInterval.Daily)
    {
        var chart = await GetChartAsync(symbol, lookback, interval);

        if (chart == null || chart.PriceActions.Length == 0)
            return null;

        if (start < chart.PriceActions[0].Date)
            start = chart.PriceActions[0].Date;

        if (!finish.HasValue || finish > chart.PriceActions[^1].Date)
            finish = chart.PriceActions[^1].Date;

        start = chart.GetNearestDate(start) ?? chart.PriceActions[0].Date;
        finish = chart.GetNearestDate(finish.GetValueOrDefault()) ?? chart.PriceActions[^1].Date;

        var startIdx = chart.GetIndexOfDate(start);
        var finishIdx = chart.GetIndexOfDate(finish.Value);

        var span = chart.GetSpan(startIdx, finishIdx);

        var prices = span.PriceActions;

        var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        var candleData = prices.Select(p => new
        {
            x = (long)(p.Date.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc) - epoch).TotalMilliseconds,
            y = new[] { p.Open, p.High, p.Low, p.Close }
        }).ToArray();

        var series = new List<object> { new { name = "Candlestick", type = "candlestick", data = candleData } };

        var timestamps = candleData.Select(d => d.x).ToArray();

        foreach (var kvp in span.MovingAverages)
        {
            var name = $"{kvp.Key.Type}{kvp.Key.Period}";
            var color = kvp.Key.Period switch
            {
                21 => "#00FF00",  // Green for short-term
                50 => "#0000FF",  // Blue for medium
                200 => "#FF0000", // Red for long-term
                _ => "#808080"   // Gray fallback
            };
            var maData = timestamps.Select((ts, i) => new { x = ts, y = kvp.Value[i] }).ToArray();
            series.Add(new { name, type = "line", data = maData, color });
        }

        var serializedData = JsonSerializer.Serialize(series);

        // Serialize trend as separate line series
        var trendData = timestamps.Select((ts, i) => new { x = ts, y = span.TrendValues[i] }).ToArray();
        var trendSeries = new[] { new { name = "Trend", type = "line", data = trendData, color = "#FFA500" } };  // Orange example
        var serializedTrend = JsonSerializer.Serialize(trendSeries);

        return new ChartModel
        {
            Symbol = symbol.ToUpperInvariant(),
            Prices = [.. prices],
            SerializedData = serializedData,
            SerializedTrend = serializedTrend
        };
    }

    //public async Task<ChartModel?> GetChartModelAsync(string symbol,
    //DateOnly start, DateOnly? finish = null,
    //int lookback = ChartLookback,
    //ChartInterval interval = ChartInterval.Daily)
    //{
    //    var chart = await GetChartAsync(symbol, lookback, interval);

    //    if (chart == null || chart.PriceActions.Length == 0)
    //        return null;

    //    if (start < chart.PriceActions[0].Date)
    //        start = chart.PriceActions[0].Date;

    //    if (!finish.HasValue || finish > chart.PriceActions[^1].Date)
    //        finish = chart.PriceActions[^1].Date;

    //    start = chart.GetNearestDate(start) ?? chart.PriceActions[0].Date;
    //    finish = chart.GetNearestDate(finish.GetValueOrDefault()) ?? chart.PriceActions[^1].Date;

    //    var startIdx = chart.GetIndexOfDate(start);
    //    var finishIdx = chart.GetIndexOfDate(finish.Value);

    //    var span = chart.GetSpan(startIdx, finishIdx);

    //    var prices = span.PriceActions;

    //    var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    //    var candleData = prices.Select(p => new
    //    {
    //        x = (long)(p.Date.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc) - epoch).TotalMilliseconds,
    //        y = new[] { p.Open, p.High, p.Low, p.Close }
    //    }).ToArray();

    //    var series = new List<object> { new { name = "Candlestick", type = "candlestick", data = candleData } };

    //    var timestamps = candleData.Select(d => d.x).ToArray();

    //    foreach (var kvp in span.MovingAverages)
    //    {
    //        var name = $"{kvp.Key.Type}{kvp.Key.Period}";
    //        var color = kvp.Key.Period switch
    //        {
    //            21 => "#00FF00",  // Green for short-term
    //            50 => "#0000FF",  // Blue for medium
    //            200 => "#FF0000", // Red for long-term
    //            _ => "#808080"   // Gray fallback
    //        };
    //        var maData = timestamps.Select((ts, i) => new { x = ts, y = kvp.Value[i] }).ToArray();
    //        series.Add(new { name, type = "line", data = maData, color });
    //    }

    //    var serializedData = JsonSerializer.Serialize(series);

    //    return new ChartModel
    //    {
    //        Symbol = symbol.ToUpperInvariant(),
    //        Prices = prices.ToArray(),
    //        SerializedData = serializedData
    //    };
    //}
    //public async Task<ChartModel?> GetChartModelAsync(string symbol,
    //DateOnly start, DateOnly? finish = null,
    //int lookback = ChartLookback,
    //ChartInterval interval = ChartInterval.Daily)
    //{
    //    var chart = await GetChartAsync(symbol, lookback, interval);

    //    if (chart == null || chart.PriceActions.Length == 0)
    //        return null;

    //    if (start < chart.PriceActions[0].Date)
    //        start = chart.PriceActions[0].Date;

    //    if (!finish.HasValue || finish > chart.PriceActions[^1].Date)
    //        finish = chart.PriceActions[^1].Date;

    //    start = chart.GetNearestDate(start) ?? chart.PriceActions[0].Date;
    //    finish = chart.GetNearestDate(finish.GetValueOrDefault()) ?? chart.PriceActions[^1].Date;

    //    var startIdx = chart.GetIndexOfDate(start);
    //    var finishIdx = chart.GetIndexOfDate(finish.Value);

    //    var span = chart.GetSpan(startIdx, finishIdx);

    //    var prices = span.PriceActions;

    //    var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    //    var candleData = prices.Select(p => new
    //    {
    //        x = (long)(p.Date.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc) - epoch).TotalMilliseconds,
    //        y = new[] { p.Open, p.High, p.Low, p.Close }
    //    }).ToArray();

    //    var series = new List<object> { new { name = "Candlestick", type = "candlestick", data = candleData } };

    //    var timestamps = candleData.Select(d => d.x).ToArray();

    //    foreach (var kvp in span.MovingAverages)
    //    {
    //        var name = $"{kvp.Key.Type}{kvp.Key.Period}";
    //        var maData = timestamps.Select((ts, i) => new { x = ts, y = kvp.Value[i] }).ToArray();
    //        series.Add(new { name, type = "line", data = maData });
    //    }

    //    var serializedData = JsonSerializer.Serialize(series);

    //    return new ChartModel
    //    {
    //        Symbol = symbol.ToUpperInvariant(),
    //        Prices = prices.ToArray(),
    //        SerializedData = serializedData
    //    };
    //}

    //public async Task<ChartModel?> GetChartModelAsync(string symbol,
    //    DateOnly start, DateOnly? finish = null,
    //    int lookback = ChartLookback, 
    //    ChartInterval interval = ChartInterval.Daily)
    //{
    //    var chart = await GetChartAsync(symbol, lookback, interval);

    //    if (chart == null || chart.PriceActions.Length == 0)
    //        return null;

    //    if (start < chart.PriceActions[0].Date)
    //        start = chart.PriceActions[0].Date; 

    //    if (!finish.HasValue || finish > chart.PriceActions[^1].Date)
    //        finish = chart.PriceActions[^1].Date;

    //    start = chart.GetNearestDate(start) ?? chart.PriceActions[0].Date;
    //    finish = chart.GetNearestDate(finish.GetValueOrDefault()) ?? chart.PriceActions[^1].Date;

    //    var startIdx = chart.GetIndexOfDate(start);
    //    var finishIdx = chart.GetIndexOfDate(finish.Value);

    //    var span = chart.GetSpan(startIdx, finishIdx);

    //    var prices = span.PriceActions;

    //    var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    //    var candleData = prices.Select(p => new
    //    {
    //        x = (long)(p.Date.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc) - epoch).TotalMilliseconds,
    //        y = new[] { p.Open, p.High, p.Low, p.Close }
    //    });

    //    foreach (var kvp in span.MovingAverages)
    //    {
    //        var name = $"{kvp.Key.Type}{kvp.Key.Period}";
    //        var maData = timestamps.Select((ts, i) => new { x = ts, y = kvp.Value[i] }).ToArray();
    //        series.Add(new { name, type = "line", data = maData });
    //    }
    //    var serializedData = JsonSerializer.Serialize(candleData);

    //    return new ChartModel
    //    {
    //        Symbol = symbol.ToUpperInvariant(),
    //        Prices = prices.ToArray(),
    //        SerializedData = serializedData
    //    };
    //}
}
