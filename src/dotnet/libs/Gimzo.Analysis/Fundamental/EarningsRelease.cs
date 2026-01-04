namespace Gimzo.Analysis.Fundamental;

public record struct EarningsRelease
{
    public required string Symbol { get; init; }
    public required string CentralIndexKey { get; init; }
    public required string FiscalQuarterEndDate { get; init; }
    public string? RegistrantName { get; init; }
    public decimal? MarketCap { get; init; }
    public decimal? EarningsPerShare { get; init; }
    public decimal? EarningsPerShareForecast { get; init; }
    public double? PercentageSurprise { get; init; }
    public int? NumberOfForecasts { get; init; }
    public DateTime? ConferenceCallTime { get; init; }
}