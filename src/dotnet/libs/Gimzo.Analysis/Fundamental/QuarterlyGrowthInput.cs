namespace Gimzo.Analysis.Fundamental;

public sealed record QuarterlyGrowthInput(
    string Symbol,
    DateOnly PeriodEndDate,
    decimal? Revenue,
    decimal? EarningsPerShareDiluted);