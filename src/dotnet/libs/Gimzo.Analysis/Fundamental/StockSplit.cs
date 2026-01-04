namespace Gimzo.Analysis.Fundamental;

public readonly record struct StockSplit
{
    public required string Symbol { get; init; }
    public required string CentralIndexKey { get; init; }
    public DateOnly? ExecutionDate { get; init; }
    public string? Registrant { get; init; }
    public double? Multiplier { get; init; }
}