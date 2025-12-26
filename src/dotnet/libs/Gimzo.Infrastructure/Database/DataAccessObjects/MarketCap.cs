namespace Gimzo.Infrastructure.Database.DataAccessObjects;

internal sealed record MarketCap : DaoBase
{
    public MarketCap() : base()
    {
        CentralIndexKey = "";
        FiscalYear = "";
    }

    public MarketCap(Guid userId) : base(userId)
    {
        CentralIndexKey = "";
        FiscalYear = "";
    }

    public MarketCap(Guid userId, DataProviders.FinancialDataNet.MarketCap cap) : base(userId)
    {
        Symbol = cap.Symbol;
        CentralIndexKey = cap.CentralIndexKey;
        Registrant = cap.Registrant;
        FiscalYear = cap.FiscalYear;
        Value = cap.Value;
        ChangeInMarketCap = cap.ChangeInMarketCap;
        PercentageChangeInMarketCap = cap.PercentageChangeInMarketCap;
        SharesOutstanding = cap.SharesOutstanding;
        ChangeInSharesOutstanding = cap.ChangeInSharesOutstanding;
        PercentageChangeInSharesOutstanding = cap.PercentageChangeInSharesOutstanding;
    }

    public string CentralIndexKey { get; init; }
    public string FiscalYear { get; init; }
    public string? Symbol { get; init; }
    public string? Registrant { get; init; }
    public decimal? Value { get; init; }
    public decimal? ChangeInMarketCap { get; init; }
    public double? PercentageChangeInMarketCap { get; init; }
    public long? SharesOutstanding { get; init; }
    public long? ChangeInSharesOutstanding { get; init; }
    public double? PercentageChangeInSharesOutstanding { get; init; }

    public override bool IsValid() => base.IsValid() &&
        !string.IsNullOrWhiteSpace(CentralIndexKey) &&
        !string.IsNullOrWhiteSpace(FiscalYear);
}
