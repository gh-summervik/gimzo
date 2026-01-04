namespace Gimzo.Analysis.Fundamental;

public record struct ProfitabilityRatios
{
    public required string CentralIndexKey { get; init; }
    public required string FiscalYear { get; init; }
    public required string FiscalPeriod { get; init; }
    public string? Symbol { get; init; }
    public string? Registrant { get; init; }
    public DateOnly? PeriodEndDate { get; init; }
    public decimal? Ebit { get; init; }
    public decimal? Ebitda { get; init; }
    public double? ProfitMargin { get; init; }
    public double? GrossMargin { get; init; }
    public double? OperatingMargin { get; init; }
    public double? OperatingCashFlowMargin { get; init; }
    public double? ReturnOnEquity { get; init; }
    public double? ReturnOnAssets { get; init; }
    public double? ReturnOnDebt { get; init; }
    public double? CashReturnOnAssets { get; init; }
    public double? CashTurnoverRatio { get; init; }
}