namespace Gimzo.Analysis.Fundamental;

public sealed class IncomeStatement
{
    public required string CentralIndexKey { get; init; }
    public required string FiscalYear { get; init; }
    public required string FiscalPeriod { get; init; }
    public string? Symbol { get; init; }
    public string? Registrant { get; init; }
    public DateOnly? PeriodEndDate { get; init; }
    public decimal? Revenue { get; init; }
    public decimal? CostOfRevenue { get; init; }
    public decimal? GrossProfit { get; init; }
    public decimal? ResearchDevelopmentExpenses { get; init; }
    public decimal? GeneralAdminExpenses { get; init; }
    public decimal? OperatingExpenses { get; init; }
    public decimal? OperatingIncome { get; init; }
    public decimal? InterestExpense { get; init; }
    public decimal? InterestIncome { get; init; }
    public decimal? NetIncome { get; init; }
    public decimal? EarningsPerShareBasic { get; init; }
    public decimal? EarningsPerShareDiluted { get; init; }
    public long? WeightedAverageSharesOutstandingBasic { get; init; }
    public long? WeightedAverageSharesOutstandingDiluted { get; init; }
}