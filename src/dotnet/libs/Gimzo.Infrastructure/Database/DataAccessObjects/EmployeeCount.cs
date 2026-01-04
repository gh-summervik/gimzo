namespace Gimzo.Infrastructure.Database.DataAccessObjects;

internal sealed record EmployeeCount : DaoBase
{
    public EmployeeCount() : base()
    {
        Symbol = "";
        CentralIndexKey = "";
        FiscalYear = "";
    }

    public EmployeeCount(Guid userId) : base(userId)
    {
        Symbol = "";
        CentralIndexKey = "";
        FiscalYear = "";
    }

    public EmployeeCount(Analysis.Fundamental.EmployeeCount count, Guid userId) : base(userId)
    {
        Symbol = count.Symbol;
        CentralIndexKey = count.CentralIndexKey;
        Registrant = count.Registrant;
        FiscalYear = count.FiscalYear;
        Count = count.Count;
    }

    public string Symbol { get; init; }
    public string CentralIndexKey { get; init; }
    public string FiscalYear { get; init; }
    public string? Registrant { get; init; }
    public int? Count { get; init; }

    public override bool IsValid() => base.IsValid() &&
        !string.IsNullOrWhiteSpace(CentralIndexKey) &&
        !string.IsNullOrWhiteSpace(FiscalYear);

    public Analysis.Fundamental.EmployeeCount ToDomain()
    {
        return new()
        {
            Symbol = Symbol,
            CentralIndexKey = CentralIndexKey,
            Registrant = Registrant,
            FiscalYear = FiscalYear,
            Count = Count
        };
    }
}
