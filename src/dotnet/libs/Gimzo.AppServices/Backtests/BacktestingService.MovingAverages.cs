namespace Gimzo.AppServices.Backtests;

public partial class BacktestingService
{
    private async Task<Ledger?> ExecuteMovingAverageCrossoverAsync(string symbol)
    {
        var chart = await GetChartAsync(symbol);
        if (chart is null || chart.MovingAverages.Length < 2)
            return null;

        var ledger = new Ledger();

        var sortedMa = chart.MovingAverageKeys.OrderBy(k => k.Period).ToArray();

        for (int i = 0; i < sortedMa.Length - 1; i++)
        {
            foreach (var match in FindIndexesOfBullishMaCrossovers(chart.GetMovingAverage(sortedMa[i]).GetValueOrDefault().Values,
                chart.GetMovingAverage(sortedMa[i + 1]).GetValueOrDefault().Values))
            {
                var entry = new CashLedgerEntry(chart.PriceActions[match + 1].Date, symbol, DefaultQty,
                    chart.PriceActions[match + 1].Open, "");
                ledger.Add(entry);
                ledger.Add(FindExit(entry, chart));
            }
            foreach (var match in FindIndexesOfBearishMaCrossovers(chart.GetMovingAverage(sortedMa[i]).GetValueOrDefault().Values,
                chart.GetMovingAverage(sortedMa[i + 1]).GetValueOrDefault().Values))
            {
                var entry = new CashLedgerEntry(chart.PriceActions[match + 1].Date, symbol, -DefaultQty,
                    chart.PriceActions[match + 1].Open, "");
                ledger.Add(entry);
                ledger.Add(FindExit(entry, chart));
            }
        }

        return ledger;
    }

    private static IEnumerable<int> FindIndexesOfBullishMaCrossovers(decimal[] fast, decimal[] slow)
    {
        if (fast.Length == 0 || fast.Length != slow.Length)
            yield break;

        HashSet<int> prevs = [];
        const int Lb = 5; // lookback

        for (int i = 0; i < fast.Length-1; i++)
        {
            if (slow[i] == 0)
                continue;

            if (fast[i] >= slow[i] && fast[i - Lb] < slow[i - Lb])
            {
                bool skip = false;
                for (int j = 1; j <= Lb; j++)
                    if (prevs.Contains(i - j))
                    {
                        skip = true;
                        break;
                    }

                if (!skip)
                {
                    prevs.Add(i);
                    yield return i;
                }
            }
        }
    }

    private static IEnumerable<int> FindIndexesOfBearishMaCrossovers(decimal[] fast, decimal[] slow)
    {
        if (fast.Length == 0 || fast.Length != slow.Length)
            yield break;

        HashSet<int> prevs = [];
        const int Lb = 5; // lookback

        for (int i = 0; i < fast.Length - 1; i++)
        {
            if (slow[i] == 0)
                continue;

            if (fast[i] <= slow[i] && fast[i - Lb] > slow[i - Lb])
            {
                bool skip = false;
                for (int j = 1; j <= Lb; j++)
                    if (prevs.Contains(i - j))
                    {
                        skip = true;
                        break;
                    }

                if (!skip)
                {
                    prevs.Add(i);
                    yield return i;
                }
            }
        }
    }

    private async Task<Ledger?> ExecutePriceMovingAverageCrossoverAsync(string symbol)
    {
        var chart = await GetChartAsync(symbol);
        if (chart is null || chart.MovingAverages.Length < 2)
            return null;

        var ledger = new Ledger();

        var sortedMa = chart.MovingAverageKeys.OrderBy(k => k.Period).ToArray();

        var prices = chart.PriceActions.Select(k => k.Close).ToArray();

        for (int i = 0; i < sortedMa.Length - 1; i++)
        {
            foreach (var match in FindIndexOfBullishPriceMaCrossover(prices,
                chart.GetMovingAverage(sortedMa[i]).GetValueOrDefault().Values))
            {
                var entry = new CashLedgerEntry(chart.PriceActions[match + 1].Date, symbol, DefaultQty,
                    chart.PriceActions[match + 1].Open, "");
                ledger.Add(entry);
                ledger.Add(FindExit(entry, chart));
            }
            foreach (var match in FindIndexOfBearishPriceMaCrossover(prices,
                chart.GetMovingAverage(sortedMa[i]).GetValueOrDefault().Values))
            {
                var entry = new CashLedgerEntry(chart.PriceActions[match + 1].Date, symbol, -DefaultQty,
                    chart.PriceActions[match + 1].Open, "");
                ledger.Add(entry);
                ledger.Add(FindExit(entry, chart));
            }
        }

        return ledger;
    }

    private static IEnumerable<int> FindIndexOfBullishPriceMaCrossover(decimal[] prices, decimal[] mas)
    {
        if (prices.Length == 0 || prices.Length != mas.Length)
            yield break;

        HashSet<int> prevs = [];
        const int Lb = 3; // lookback

        for (int i = 3; i < prices.Length - 1; i++)
        {
            if (mas[i] == 0)
                continue;

            if (prices[i] > mas[i] && prices[i - 1] > mas[i - 1] && prices[i - Lb] < mas[i - Lb])
            {
                bool skip = false;
                for (int j = 1; j <= Lb; j++)
                    if (prevs.Contains(i - j))
                    {
                        skip = true;
                        break;
                    }

                if (!skip)
                {
                    prevs.Add(i);
                    yield return i;
                }
            }
        }
    }

    private static IEnumerable<int> FindIndexOfBearishPriceMaCrossover(decimal[] prices, decimal[] mas)
    {
        if (prices.Length == 0 || prices.Length != mas.Length)
            yield break;

        HashSet<int> prevs = [];
        const int Lb = 3; // lookback

        for (int i = 3; i < prices.Length - 1; i++)
        {
            if (mas[i] == 0)
                continue;

            if (prices[i] < mas[i] && prices[i-1] < mas[i-1] && prices[i-Lb] > mas[i-Lb])
            {
                bool skip = false;
                for (int j = 1; j <= Lb; j++)
                    if (prevs.Contains(i - j))
                    {
                        skip = true;
                        break;
                    }

                if (!skip)
                {
                    prevs.Add(i);
                    yield return i;
                }
            }
        }
    }
}