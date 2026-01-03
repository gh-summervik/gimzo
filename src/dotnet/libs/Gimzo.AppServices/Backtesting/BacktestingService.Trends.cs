namespace Gimzo.AppServices.Backtesting;

public partial class BacktestingService
{
    private async Task<Ledger?> ExecuteGimzoTrendDivergenceAsync(string symbol, int period)
    {
        var chart = await GetChartAsync(symbol);
        if (chart == null)
            return null;

        var ledger = new Ledger();
        var closes = chart.PriceActions.Select(k => k.Close).Select(k => (double)k).ToArray();
        var opens = chart.PriceActions.Select(k => k.Open).ToArray();
        var volumes = chart.PriceActions.Select(k => k.Volume).ToArray();

        var trends = chart.TrendValues;

        for (int i = period; i < closes.Length - 1; i++)
        {
            var priceSlope = CalculateSlope(closes[(i - period)..i]);
            var trendSlope = CalculateSlope(trends[(i - period)..i]);

            var avgVolume = volumes[Math.Max(0, i - 20)..i].Average();

            const double MinTrendSlope = 0.07;
            if (trendSlope >= MinTrendSlope && priceSlope < MinTrendSlope && volumes[i] > avgVolume)
            {
                // buy
                var entry = new CashLedgerEntry(chart.PriceActions[i + 1].Date, symbol, DefaultQty,
                    opens[i + 1], "");
                ledger.Add(entry);
                ledger.Add(FindExit(entry, chart));
            }

            if (trendSlope <= -MinTrendSlope && priceSlope > MinTrendSlope && volumes[i] > avgVolume)
            {
                // sell
                var entry = new CashLedgerEntry(chart.PriceActions[i + 1].Date, symbol, -DefaultQty,
                    opens[i + 1], "");
                ledger.Add(entry);
                ledger.Add(FindExit(entry, chart));
            }
        }

        return ledger;
    }

    private async Task<Ledger?> ExecuteGimzoTrendFollowAsync(string symbol, int period)
    {
        
        var chart = await GetChartAsync(symbol);
        if (chart == null)
            return null;

        var ledger = new Ledger();
        var closes = chart.PriceActions.Select(k => k.Close).Select(k => (double)k).ToArray();
        var opens = chart.PriceActions.Select(k => k.Open).ToArray();
        var volumes = chart.PriceActions.Select(k => k.Volume).ToArray();

        var trends = chart.TrendValues;

        for (int i = period; i < closes.Length - 1; i++)
        {
            var priceSlope = CalculateSlope(closes[(i - period)..i]);
            var trendSlope = CalculateSlope(trends[(i - period)..i]);

            var avgVolume = volumes[Math.Max(0, i - 20)..i].Average();

            const double MinTrendSlope = 0.04;
            if (trends[i] < 0 && trendSlope >= MinTrendSlope && priceSlope > 0)
            {
                // buy
                var entry = new CashLedgerEntry(chart.PriceActions[i + 1].Date, symbol, DefaultQty,
                    opens[i + 1], "");
                ledger.Add(entry);
                ledger.Add(FindExit(entry, chart));
            }

            if (trends[i] > 0 && trendSlope <= -MinTrendSlope && priceSlope <= -MinTrendSlope)  
            {
                // sell
                var entry = new CashLedgerEntry(chart.PriceActions[i + 1].Date, symbol, -DefaultQty,
                    opens[i + 1], "");
                ledger.Add(entry);
                ledger.Add(FindExit(entry, chart));
            }
        }

        return ledger;
    }
}
