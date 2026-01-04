namespace Gimzo.Analysis.Fundamental;

public record struct ShortInterest
{
    public required string Symbol { get; init; }
    public DateOnly? SettlementDate { get; init; }
    public string? Title { get; init; }
    public string? MarketCode { get; init; }
    public long? ShortedSecurities { get; init; }
    public long? PreviousShortedSecurities { get; init; }
    public long? ChangeInShortedSecurities { get; init; }
    public double? PercentageChangeInShortedSecurities { get; init; }
    public long? AverageDailyVolume { get; init; }
    public double? DaysToCover { get; init; }
    public bool? IsStockSplit { get; init; }
}