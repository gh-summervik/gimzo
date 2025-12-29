namespace Gimzo.Infrastructure.Database.DataAccessObjects;

internal sealed record IgnoredSymbol : DaoBase
{
    public IgnoredSymbol() : base()
    {
        Symbol = "";
        Reason = "";
    }

    public IgnoredSymbol(Guid userId) : base(userId)
    {
        Symbol = "";
        Reason = "";
    }

    public string Symbol { get; init; }
    public string Reason { get; init; }
    public DateOnly? Expiration { get; init; }

    public override bool IsValid() => base.IsValid() &&
        !string.IsNullOrWhiteSpace(Symbol) &&
        !string.IsNullOrWhiteSpace(Reason);
}
