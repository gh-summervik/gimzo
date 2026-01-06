namespace Gimzo.Analysis.Fundamental;

public readonly record struct MarketCap
{
    public required string Symbol { get; init; }
    public required string CentralIndexKey { get; init; }
    public required string FiscalYear { get; init; }
    public string? Registrant { get; init; }
    public decimal? Value { get; init; }
    public decimal? ChangeInMarketCap { get; init; }
    public double? PercentageChangeInMarketCap { get; init; }
    public long? SharesOutstanding { get; init; }
    public long? ChangeInSharesOutstanding { get; init; }
    public double? PercentageChangeInSharesOutstanding { get; init; }
}