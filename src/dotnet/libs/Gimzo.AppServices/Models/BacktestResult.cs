using System.Text;

namespace Gimzo.AppServices.Models;

public record BacktestResult
{
    public Guid ProcessId { get; set; }
    public int Trades { get; set; }
    public double AveragePnlPercent { get; set; }
    public double WinRate { get; set; }
    public decimal Expectancy { get; set;  }
    public decimal AverageWin { get; set; }
    public decimal AverageLoss { get; set; }
    public string? BacktestType { get; set;  }
    public override string ToString()
    {
        List<string> items = new(7);
        items.Add(ProcessId.ToString("N"));
        items.Add(Trades.ToString().PadLeft(7,' '));
        items.Add(AveragePnlPercent.ToString("P2"));
        items.Add(WinRate.ToString("0.00"));
        items.Add(AverageWin.ToString("C2"));
        items.Add(AverageLoss.ToString("C2"));
        items.Add(BacktestType ?? "");
        return string.Join(" | ", items);
    }
}
