namespace Gimzo.AppServices.Backtests;

public record CashLedgerEntry
{
    public CashLedgerEntry(DateOnly date, string symbol, int quantity, 
        decimal tradePrice, string description = "", Guid? relatesTo = null)
    {
        Id = Guid.NewGuid();
        RelatesTo = relatesTo;
        Date = date;
        Symbol = symbol;
        Quantity = quantity;
        TradePrice = tradePrice;
        Description = description;
    }

    public Guid Id { get; }
    public Guid? RelatesTo { get; }
    public DateOnly Date { get; }
    public string Symbol { get; }
    public int Quantity { get; }
    public decimal TradePrice { get; }
    public string Description { get; }
    public decimal CashValue => Quantity * -1 * TradePrice;
    public bool IsLong => Quantity > 0;
    public bool IsShort => Quantity < 0;
}
