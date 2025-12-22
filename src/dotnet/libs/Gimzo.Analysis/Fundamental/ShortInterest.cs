using System;

namespace Gimzo.Analysis.Fundamental;

public sealed class ShortInterest
{
    public required string Symbol { get; init; }
    public required string Title { get; init; }
    public required string MarketCode { get; init; }
    public DateOnly SettlementDate { get; init; }
    public long ShortedSecurities { get; init; }
    public long PreviousShortedSecurities { get; init; }
    public long ChangeInShortedSecurities { get; init; }
    public double PercentageChangeInShortedSecurities { get; init; }
    public long AverageDailyVolume { get; init; }
    public double DaysToConvert { get; init; }
    public bool IsStockSplit { get; init; }
}