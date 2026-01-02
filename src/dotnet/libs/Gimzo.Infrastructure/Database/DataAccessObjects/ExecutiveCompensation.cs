namespace Gimzo.Infrastructure.Database.DataAccessObjects;

internal sealed record ExecutiveCompensation : DaoBase
{
    public ExecutiveCompensation() : base()
    {
        CentralIndexKey = "";
        Name = "";
        Position = "";
        FiscalYear = "";
    }

    public ExecutiveCompensation(Guid userId) : base(userId)
    {
        CentralIndexKey = "";
        Name = "";
        Position = "";
        FiscalYear = "";
    }

    public ExecutiveCompensation(Analysis.Fundamental.ExecutiveCompensation comp, Guid userId) : base(userId)
    {
        Symbol = comp.Symbol;
        CentralIndexKey = comp.CentralIndexKey;
        Registrant = comp.Registrant;
        Name = comp.Name;
        Position = comp.Position;
        FiscalYear = comp.FiscalYear;
        Salary = comp.Salary;
        Bonus = comp.Bonus;
        StockAwards = comp.StockAwards;
        IncentivePlanCompensation = comp.IncentivePlanCompensation;
        OtherCompensation = comp.OtherCompensation;
        TotalCompensation = comp.TotalCompensation;
    }

    public string CentralIndexKey { get; init; }
    public string Name { get; init; }
    public string Position { get; init; }
    public string FiscalYear { get; init; }
    public string? Symbol { get; init; }
    public string? Registrant { get; init; }
    public decimal? Salary { get; init; }
    public decimal? Bonus { get; init; }
    public decimal? StockAwards { get; init; }
    public decimal? IncentivePlanCompensation { get; init; }
    public decimal? OtherCompensation { get; init; }
    public decimal? TotalCompensation { get; init; }

    public override bool IsValid() => base.IsValid() &&
        !string.IsNullOrWhiteSpace(CentralIndexKey) &&
        !string.IsNullOrWhiteSpace(Name) &&
        !string.IsNullOrWhiteSpace(Position) &&
        !string.IsNullOrWhiteSpace(FiscalYear);

    public Analysis.Fundamental.ExecutiveCompensation ToDomain()
    {
        return new()
        {
            Symbol = Symbol,
            Bonus = Bonus,
            StockAwards = StockAwards,
            CentralIndexKey = CentralIndexKey,
            FiscalYear = FiscalYear,
            IncentivePlanCompensation = IncentivePlanCompensation,
            OtherCompensation = OtherCompensation,
            Name = Name,
            Position = Position,
            Registrant = Registrant,
            Salary = Salary,
            TotalCompensation = TotalCompensation
        };
    }
}
