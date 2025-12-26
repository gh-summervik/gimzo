namespace Gimzo.Analysis.Fundamental;

public sealed class StockSplit
{
    public required string CentralIndexKey { get; init; }
    public DateOnly ExecutionDate { get; init; }
    public string? Symbol { get; init; }
    public string? Registrant { get; init; }
    public double? Multiplier { get; init; }
}