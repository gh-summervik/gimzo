namespace Gimzo.AppServices.Backtesting;

public readonly struct LedgerEntryPair(CashLedgerEntry open, CashLedgerEntry? close)
{
    public CashLedgerEntry Open { get; } = open;
    public CashLedgerEntry? Close { get; } = close;
    public decimal Profit => Close is null ? 0M : Open.CashValue + Close.CashValue; 
    public bool IsWin => Profit > 0M;
    public bool IsLoss => Profit < 0M;
    public int? DurationDays => Close is null || Open is null ? null
        : Close.Date.DayNumber - Open.Date.DayNumber;

}
