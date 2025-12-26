namespace Gimzo.Analysis.Fundamental;

public sealed class ValuationRatios
{
    public required string CentralIndexKey { get; init; }
    public required string FiscalYear { get; init; }
    public required string FiscalPeriod { get; init; }
    public string? Symbol { get; init; }
    public string? Registrant { get; init; }
    public DateOnly? PeriodEndDate { get; init; }
    public decimal? DividendsPerShare { get; init; }
    public double? DividendPayoutRatio { get; init; }
    public decimal? BookValuePerShare { get; init; }
    public double? RetentionRatio { get; init; }
    public decimal? NetFixedAssets { get; init; }
}