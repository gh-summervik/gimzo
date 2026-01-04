namespace Gimzo.Infrastructure.Database.DataAccessObjects;

internal sealed record QuarterlyGrowthInput
{
    public string Symbol { get; init; } = "";
    public DateOnly? PeriodEndDate { get; init; }
    public decimal? Revenue { get; init; }
    public decimal? EarningsPerShareDiluted { get; init; }

    public Analysis.Fundamental.QuarterlyGrowthInput ToDomain() =>
        new(Symbol, PeriodEndDate, Revenue, EarningsPerShareDiluted);
}