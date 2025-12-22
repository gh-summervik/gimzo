using System;

namespace Gimzo.Analysis.Fundamental;

public sealed class StockSplit
{
    public required string Symbol { get; init; }
    public required string CentralIndexKey { get; init; }
    public required string Registrant { get; init; }
    public DateOnly ExecutionDate { get; init; }
    public double Multiplier { get; init; }
}