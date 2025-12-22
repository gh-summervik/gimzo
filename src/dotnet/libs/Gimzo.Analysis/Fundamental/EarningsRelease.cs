using System;

namespace Gimzo.Analysis.Fundamental;

public sealed class EarningsRelease
{
    public required string TradingSymbol { get; init; }
    public required string CentralIndexKey { get; init; }
    public required string RegistrantName { get; init; }
    public decimal MarketCap { get; init; }
    public required string FiscalQuarterEndDate { get; init; }
    public decimal EarningsPerShare { get; init; }
    public decimal EarningsPerShareForecast { get; init; }
    public double PercentageSurprise { get; init; }
    public int NumberOfForecasts { get; init; }
    public DateTime ConferenceCallTime { get; init; }
}