namespace Gimzo.Infrastructure.Database.DataAccessObjects;

internal sealed record EmployeeCount : DaoBase
{
    public EmployeeCount() : base()
    {
        CentralIndexKey = "";
        FiscalYear = "";
    }

    public EmployeeCount(Guid userId) : base(userId)
    {
        CentralIndexKey = "";
        FiscalYear = "";
    }

    public EmployeeCount(Guid userId, DataProviders.FinancialDataNet.EmployeeCount count) : base(userId)
    {
        Symbol = count.Symbol;
        CentralIndexKey = count.CentralIndexKey;
        Registrant = count.Registrant;
        FiscalYear = count.FiscalYear;
        Count = count.Count;
    }

    public string CentralIndexKey { get; init; }
    public string FiscalYear { get; init; }
    public string? Symbol { get; init; }
    public string? Registrant { get; init; }
    public int? Count { get; init; }

    public override bool IsValid() => base.IsValid() &&
        !string.IsNullOrWhiteSpace(CentralIndexKey) &&
        !string.IsNullOrWhiteSpace(FiscalYear);
}
