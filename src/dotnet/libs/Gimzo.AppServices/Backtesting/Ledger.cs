using System.Text;

namespace Gimzo.AppServices.Backtesting;

public sealed class Ledger
{
    private readonly List<CashLedgerEntry> _entries = new(100);

    public void Add(CashLedgerEntry? entry)
    {
        if (entry != null)
            _entries.Add(entry);
    }

    public IEnumerable<LedgerEntryPair> GetPairs()
    {
        var closes = _entries.Where(e => e.RelatesTo.HasValue)
                             .ToDictionary(e => e.RelatesTo!.Value);
        foreach (var open in _entries.Where(e => !e.RelatesTo.HasValue))
        {
            closes.TryGetValue(open.Id, out var close);
            yield return new LedgerEntryPair(open, close);
        }
    }

    public record PerformanceSummary(
        decimal TotalProfit,
        double WinRate,
        decimal AverageProfit,
        decimal AverageWin,
        decimal AverageLoss,
        decimal ProfitFactor,
        int TotalTrades)
    { 
        public string ToCliOutput()
        {
            StringBuilder sb = new();
            sb.AppendLine($"Total Trades  : {TotalTrades}");
            sb.AppendLine($"Win Rate      : {WinRate.ToString("#0.00")}");
            sb.AppendLine($"Avg Profit    : {AverageProfit.ToString("C")}");
            sb.AppendLine($"Avg Win       : {AverageWin.ToString("C")}");
            sb.AppendLine($"Avg Loss      : {AverageLoss.ToString("C")}");
            sb.AppendLine($"Total Profit  : {TotalProfit.ToString("C")}");
            sb.AppendLine($"Profit Factor : {ProfitFactor.ToString("#0.00")}");
            return sb.ToString();
        }
    }

    public PerformanceSummary GetPerformance()
    {
        var pairs = GetPairs().ToArray();
        int wins = 0;
        decimal grossWin = 0M;
        decimal grossLoss = 0M;
        decimal totalProfit = 0M;
        int trades = 0;
        foreach (var pair in pairs.Where(k => k.Close != null))
        {
            trades++;
            decimal p = pair.Profit;
            totalProfit += p;
            if (p > 0M)
            {
                wins++;
                grossWin += p;
            }
            else if (p < 0M)
                grossLoss -= p;
        }

        double winRate = trades > 0 ? (double)wins / trades : 0.0;
        decimal avgProfit = trades > 0 ? totalProfit / trades : 0M;
        decimal avgWin = wins > 0 ? grossWin / wins : 0M;
        decimal avgLoss = trades - wins > 0 ? grossLoss / (trades - wins) : 0M;
        decimal profitFactor = grossLoss == 0M ? 0M : grossWin / grossLoss;

        return new PerformanceSummary(totalProfit, winRate, avgProfit, avgWin, avgLoss, profitFactor, trades);
    }
}
