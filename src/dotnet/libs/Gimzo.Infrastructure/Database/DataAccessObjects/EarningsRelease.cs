namespace Gimzo.Infrastructure.Database.DataAccessObjects;

internal sealed record EarningsRelease : DaoBase
{
    private long? _conferenceCallUnixMs;

    public EarningsRelease() : base()
    {
        CentralIndexKey = "";
        FiscalQuarterEndDate = "";
    }

    public EarningsRelease(Guid userId) : base(userId)
    {
        CentralIndexKey = "";
        FiscalQuarterEndDate = "";
    }

    public EarningsRelease(Analysis.Fundamental.EarningsRelease release, Guid userId) : base(userId)
    {
        Symbol = release.Symbol;
        CentralIndexKey = release.CentralIndexKey;
        RegistrantName = release.RegistrantName;
        MarketCap = release.MarketCap;
        FiscalQuarterEndDate = release.FiscalQuarterEndDate;
        EarningsPerShare = release.EarningsPerShare;
        EarningsPerShareForecast = release.EarningsPerShareForecast;
        PercentageSurprise = release.PercentageSurprise;
        NumberOfForecasts = release.NumberOfForecasts;
        ConferenceCallTime = release.ConferenceCallTime;
    }

    public string CentralIndexKey { get; init; }
    public string FiscalQuarterEndDate { get; init; }
    public string? Symbol { get; init; }
    public string? RegistrantName { get; init; }
    public decimal? MarketCap { get; init; }
    public decimal? EarningsPerShare { get; init; }
    public decimal? EarningsPerShareForecast { get; init; }
    public double? PercentageSurprise { get; init; }
    public int? NumberOfForecasts { get; init; }
    public DateTimeOffset? ConferenceCallTime
    {
        get => DateTimeOffset.FromUnixTimeMilliseconds(_conferenceCallUnixMs ?? 0);
        init => _conferenceCallUnixMs = value?.ToUnixTimeMilliseconds() ?? 0;
    }
    public long? ConferenceCallUnixMs
    {
        get => _conferenceCallUnixMs;
        init => _conferenceCallUnixMs = value;
    }
    public override bool IsValid() => base.IsValid() &&
        !string.IsNullOrWhiteSpace(CentralIndexKey) &&
        !string.IsNullOrWhiteSpace(FiscalQuarterEndDate);

    public Analysis.Fundamental.EarningsRelease ToDomain()
    {
        return new()
        {
            CentralIndexKey = CentralIndexKey,
            ConferenceCallTime = ConferenceCallTime?.DateTime,
            EarningsPerShare = EarningsPerShare,
            EarningsPerShareForecast = EarningsPerShareForecast,
            FiscalQuarterEndDate = FiscalQuarterEndDate,
            MarketCap = MarketCap,
            NumberOfForecasts = NumberOfForecasts,
            PercentageSurprise = PercentageSurprise,
            RegistrantName = RegistrantName,
            Symbol = Symbol
        };
    }
}
