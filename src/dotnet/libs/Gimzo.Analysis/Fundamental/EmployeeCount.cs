namespace Gimzo.Analysis.Fundamental;

public record struct EmployeeCount
{
    public required string Symbol { get; init; }
    public required string CentralIndexKey { get; init; }
    public required string FiscalYear { get; init; }
    public string? Registrant { get; init; }
    public int? Count { get; init; }
}