namespace Gimzo.Infrastructure.Database.DataAccessObjects;

internal sealed record StockSplit : DaoBase
{
    public StockSplit() : base()
    {
        CentralIndexKey = "";
    }

    public StockSplit(Guid userId) : base(userId)
    {
        CentralIndexKey = "";
    }

    public StockSplit(Analysis.Fundamental.StockSplit split, Guid userId) : base(userId)
    {
        Symbol = split.Symbol;
        CentralIndexKey = split.CentralIndexKey;
        Registrant = split.Registrant;
        ExecutionDate = split.ExecutionDate;
        Multiplier = split.Multiplier;
    }

    public string CentralIndexKey { get; init; }
    public DateOnly ExecutionDate { get; init; }
    public string? Symbol { get; init; }
    public string? Registrant { get; init; }
    public double? Multiplier { get; init; }

    public override bool IsValid() => base.IsValid() &&
        !string.IsNullOrWhiteSpace(CentralIndexKey) &&
        ExecutionDate > new DateOnly(1900, 1, 1);

    public Analysis.Fundamental.StockSplit ToDomain()
    {
        return new()
        {
            CentralIndexKey = CentralIndexKey,
            ExecutionDate = ExecutionDate,
            Multiplier = Multiplier,
            Registrant = Registrant,
            Symbol = Symbol
        };
    }
}