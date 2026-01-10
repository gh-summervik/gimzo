using System.Collections.Immutable;

namespace Gimzo.AppServices.Backtests;

public partial class BacktestingService
{
    private async Task<Ledger?> ExecuteGimzoTrendDivergenceAsync(string symbol, int period)
    {
        var chart = await GetChartAsync(symbol);
        if (chart == null)
            return null;

        var ledger = new Ledger();

        decimal[] closes = [.. chart.PriceActions.Select(k => k.Close)];
        decimal[] opens = [.. chart.PriceActions.Select(k => k.Open)];
        double[] volumes = [.. chart.PriceActions.Select(k => k.Volume)];
        decimal[] trendDecimals = chart.RelativeStrengthIndex == null ? [] :
            [.. chart.RelativeStrengthIndex.Values.Select(t => (decimal)t)];

        for (int i = period; i < closes.Length - 1; i++)
        {
            int start = i - period;

            var priceSlope = Common.Maths.CalculateSlope(closes, start, period);
            var trendSlope = Common.Maths.CalculateSlope(trendDecimals, start, period);

            var avgVolume = volumes[Math.Max(0, i - 20)..i].Average();

            const double MinTrendSlope = 0.07;
            if (trendSlope >= MinTrendSlope && priceSlope < MinTrendSlope && volumes[i] > avgVolume)
            {
                var entry = new CashLedgerEntry(chart.PriceActions[i + 1].Date, symbol, DefaultQty,
                    opens[i + 1], "");
                ledger.Add(entry);
                ledger.Add(FindExit(entry, chart));
            }

            if (trendSlope <= -MinTrendSlope && priceSlope > MinTrendSlope && volumes[i] > avgVolume)
            {
                var entry = new CashLedgerEntry(chart.PriceActions[i + 1].Date, symbol, -DefaultQty,
                    opens[i + 1], "");
                ledger.Add(entry);
                ledger.Add(FindExit(entry, chart));
            }
        }

        return ledger;
    }

    //private async Task<Ledger?> ExecuteGimzoTrendFollowAsync(string symbol, int period)
    //{
    //    var chart = await GetChartAsync(symbol);
    //    if (chart == null)
    //        return null;

    //    var ledger = new Ledger();

    //    decimal[] closes = [.. chart.PriceActions.Select(k => k.Close)];
    //    decimal[] opens = [.. chart.PriceActions.Select(k => k.Open)];
    //    double[] volumes = [.. chart.PriceActions.Select(k => k.Volume)];
    //    decimal[] trendDecimals = [.. chart.RelativeStrengthValues.Select(t => (decimal)t)];

    //    for (int i = period; i < closes.Length - 1; i++)
    //    {
    //        int start = i - period;

    //        var priceSlope = Common.Maths.CalculateSlope(closes, start, period);
    //        var trendSlope = Common.Maths.CalculateSlope(trendDecimals, start, period);

    //        var avgVolume = volumes[Math.Max(0, i - 20)..i].Average();

    //        const double MinTrendSlope = 0.04;
    //        if (trends[i] < 0 && trendSlope >= MinTrendSlope && priceSlope > 0)
    //        {
    //            var entry = new CashLedgerEntry(chart.PriceActions[i + 1].Date, symbol, DefaultQty,
    //                opens[i + 1], "");
    //            ledger.Add(entry);
    //            ledger.Add(FindExit(entry, chart));
    //        }

    //        if (trends[i] > 0 && trendSlope <= -MinTrendSlope && priceSlope <= -MinTrendSlope)
    //        {
    //            var entry = new CashLedgerEntry(chart.PriceActions[i + 1].Date, symbol, -DefaultQty,
    //                opens[i + 1], "");
    //            ledger.Add(entry);
    //            ledger.Add(FindExit(entry, chart));
    //        }
    //    }

    //    return ledger;
    //}
}