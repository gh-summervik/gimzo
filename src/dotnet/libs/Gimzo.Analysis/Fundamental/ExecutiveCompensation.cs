namespace Gimzo.Analysis.Fundamental;

public sealed class ExecutiveCompensation
{
    public required string CentralIndexKey { get; init; }
    public required string Name { get; init; }
    public required string Position { get; init; }
    public required string FiscalYear { get; init; }
    public string? Symbol { get; init; }
    public string? Registrant { get; init; }
    public decimal? Salary { get; init; }
    public decimal? Bonus { get; init; }
    public decimal? StockAwards { get; init; }
    public decimal? IncentivePlanCompensation { get; init; }
    public decimal? OtherCompensation { get; init; }
    public decimal? TotalCompensation { get; init; }
}