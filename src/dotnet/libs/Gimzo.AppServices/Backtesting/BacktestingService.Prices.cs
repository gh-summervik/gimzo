namespace Gimzo.AppServices.Backtesting;

public partial class BacktestingService
{
    private async Task<Ledger?> ExecutePriceExtremeFollowAsync(string symbol)
    {
        var chart = await _dataService.GetChartAsync(symbol);
        if (chart == null)
            return null;

        var ledger = new Ledger();

        for (int i = 1; i < chart.Extremes.Length && i < chart.PriceActions.Length - 1; i++)
        {
            var current = chart.Extremes[i];

            var v = Math.Max(0, current.Index - 21);
            var avgVol = chart.PriceActions[v..current.Index].Select(k => k.Volume).Average();

            if (chart.PriceActions[current.Index].Volume < 1.2 * avgVol)
                continue;

            if (current.IsLow && current.IsBullish && chart.TrendValues[current.Index] > 0.2)
            {
                var entry = new CashLedgerEntry(chart.PriceActions[current.Index + 1].Date, symbol, DefaultQty,
                    chart.PriceActions[current.Index + 1].Close, "");
                ledger.Add(entry);
                ledger.Add(FindExitOnLong(entry, chart));
            }
        }
        return ledger;
    }
}
