namespace Gimzo.Analysis.Fundamental;

public record struct QuarterlyGrowthInput(
    string Symbol,
    DateOnly? PeriodEndDate,
    decimal? Revenue,
    decimal? EarningsPerShareDiluted);