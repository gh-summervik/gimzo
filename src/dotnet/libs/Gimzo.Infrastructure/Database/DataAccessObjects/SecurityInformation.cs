namespace Gimzo.Infrastructure.Database.DataAccessObjects;

internal sealed record SecurityInformation : DaoBase
{
    public SecurityInformation() : base()
    {
        Symbol = "";
    }

    public SecurityInformation(Guid userId) : base(userId)
    {
        Symbol = "";
    }

    public SecurityInformation(Guid userId, DataProviders.FinancialDataNet.SecurityInformation info) : base(userId)
    {
        Symbol = info.Symbol;
        Issuer = info.Issuer;
        Cusip = info.Cusip;
        Isin = info.Isin;
        Figi = info.Figi;
        Type = info.Type;
    }

    public string Symbol { get; init; }
    public string? Issuer { get; init; }
    public string? Cusip { get; init; }
    public string? Isin { get; init; }
    public string? Figi { get; init; }
    public string? Type { get; init; }

    public override bool IsValid() => base.IsValid() &&
        !string.IsNullOrWhiteSpace(Symbol);
}
