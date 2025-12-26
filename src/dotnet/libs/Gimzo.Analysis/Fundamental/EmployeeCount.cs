namespace Gimzo.Analysis.Fundamental;

public sealed class EmployeeCount
{
    public required string CentralIndexKey { get; init; }
    public required string FiscalYear { get; init; }
    public string? Symbol { get; init; }
    public string? Registrant { get; init; }
    public int? Count { get; init; }
}