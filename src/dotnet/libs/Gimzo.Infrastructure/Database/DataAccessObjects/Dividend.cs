namespace Gimzo.Infrastructure.Database.DataAccessObjects;

internal sealed record Dividend : DaoBase
{
    public Dividend() : base()
    {
        Symbol = "";
    }

    public Dividend(Guid userId) : base(userId)
    {
        Symbol = "";
    }

    public Dividend(Guid userId, DataProviders.FinancialDataNet.Dividend div) : base(userId)
    {
        Symbol = div.Symbol;
        Registrant = div.Registrant;
        Type = div.Type;
        Amount = div.Amount;
        DeclarationDate = div.DeclarationDate;
        ExDate = div.ExDate;
        RecordDate = div.RecordDate;
        PaymentDate = div.PaymentDate;
    }

    public string Symbol { get; init; }
    public DateOnly? ExDate { get; init; }
    public string? Registrant { get; init; }
    public string? Type { get; init; }
    public decimal? Amount { get; init; }
    public DateOnly? DeclarationDate { get; init; }
    public DateOnly? RecordDate { get; init; }
    public DateOnly? PaymentDate { get; init; }

    public override bool IsValid() => base.IsValid() &&
        !string.IsNullOrWhiteSpace(Symbol) &&
        ExDate > new DateOnly(1900, 1, 1);
}
