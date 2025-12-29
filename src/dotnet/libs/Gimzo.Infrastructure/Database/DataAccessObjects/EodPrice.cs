using Gimzo.Analysis.Technical.Charts;

namespace Gimzo.Infrastructure.Database.DataAccessObjects;

internal sealed record EodPrice : DaoBase
{
    private decimal _open;
    private decimal _high;
    private decimal _low;
    private decimal _close;

    public EodPrice() : base()
    {
        Symbol = "";
    }

    public EodPrice(Guid userId) : base(userId)
    {
        Symbol = "";
    }

    public string Symbol { get; init; }
    public DateOnly Date { get; init; }
    public decimal Open { get => _open; init => _open = Math.Round(value, Common.Constants.MoneyPrecision); }
    public decimal High { get => _high; init => _high = Math.Round(value, Common.Constants.MoneyPrecision); }
    public decimal Low { get => _low; init => _low = Math.Round(value, Common.Constants.MoneyPrecision); }
    public decimal Close { get => _close; init => _close = Math.Round(value, Common.Constants.MoneyPrecision); }
    public double Volume { get; init; }
    public Ohlc ToOhlc() => new(Symbol, Date, Open, High, Low, Close, Volume);
    public override bool IsValid() => base.IsValid() &&
        !string.IsNullOrWhiteSpace(Symbol) &&
        Date > new DateOnly(1900, 1, 1);
}

