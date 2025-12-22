using System;

namespace Gimzo.Analysis.Fundamental;

public sealed class EmployeeCount
{
    public required string Symbol { get; init; }
    public required string CentralIndexKey { get; init; }
    public required string Registrant { get; init; }
    public required string FiscalYear { get; init; }
    public int Count { get; init; }
}