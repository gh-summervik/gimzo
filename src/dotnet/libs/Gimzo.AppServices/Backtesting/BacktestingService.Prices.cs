using Gimzo.Analysis.Technical.Charts;

namespace Gimzo.AppServices.Backtesting;

public partial class BacktestingService
{
    private async Task<Ledger?> ExecuteBullishPriceExtremeFollowAsync(string symbol)
    {
        var chart = await GetChartAsync(symbol);
        if (chart == null)
            return null;

        var ledger = new Ledger();

        for (int i = 1; i < chart.Extremes.Length && i < chart.PriceActions.Length - 1; i++)
        {
            var current = chart.Extremes[i];

            var prevHigh = chart.FindPreviousHigh(i);
            if (prevHigh == null)
                continue;

            if (!(current.IsLow && current.IsBullish && chart.TrendValues[current.Index] > -0.2 &&
                chart.PriceActions[current.Index + 1].Open > chart.PriceActions[current.Index].Close &&
                chart.PriceActions[current.Index + 1].MidPoint < prevHigh.Value.Price))
                continue;

            var v = Math.Max(0, current.Index - 21);
            var avgVol = chart.PriceActions[v..current.Index].Select(k => k.Volume).Average();

            var entry = new CashLedgerEntry(chart.PriceActions[current.Index + 1].Date, symbol, DefaultQty,
                chart.PriceActions[current.Index + 1].MidPoint, "");
            var exitPoint = FindExitOnLong(entry, chart);
            if (exitPoint != null)
            {
                ledger.Add(entry);
                ledger.Add(exitPoint);
            }
        }
        return ledger;
    }
}
