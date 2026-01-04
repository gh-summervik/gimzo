namespace Gimzo.Analysis.Fundamental;

public record struct MarketCap
{
    public required string CentralIndexKey { get; init; }
    public required string FiscalYear { get; init; }
    public string? Symbol { get; init; }
    public string? Registrant { get; init; }
    public double? Value { get; init; }
    public double? ChangeInMarketCap { get; init; }
    public double? PercentageChangeInMarketCap { get; init; }
    public long? SharesOutstanding { get; init; }
    public long? ChangeInSharesOutstanding { get; init; }
    public double? PercentageChangeInSharesOutstanding { get; init; }
}