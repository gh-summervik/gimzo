namespace Gimzo.Analysis.Fundamental;

public sealed class KeyMetrics
{
    public required string CentralIndexKey { get; init; }
    public required string FiscalYear { get; init; }
    public string? Symbol { get; init; }
    public string? Registrant { get; init; }
    public DateOnly? PeriodEndDate { get; init; }
    public decimal? EarningsPerShare { get; init; }
    public decimal? EarningsPerShareForecast { get; init; }
    public double? PriceToEarningsRatio { get; init; }
    public double? ForwardPriceToEarningsRatio { get; init; }
    public double? EarningsGrowthRate { get; init; }
    public double? PriceEarningsToGrowthRate { get; init; }
    public decimal? BookValuePerShare { get; init; }
    public double? PriceToBookRatio { get; init; }
    public decimal? Ebitda { get; init; }
    public decimal? EnterpriseValue { get; init; }
    public double? DividendYield { get; init; }
    public double? DividendPayoutRatio { get; init; }
    public double? DebtToEquityRatio { get; init; }
    public decimal? CapitalExpenditures { get; init; }
    public decimal? FreeCashFlow { get; init; }
    public double? ReturnOnEquity { get; init; }
    public double? OneYearBeta { get; init; }
    public double? ThreeYearBeta { get; init; }
    public double? FiveYearBeta { get; init; }
}