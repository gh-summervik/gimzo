namespace Gimzo.Infrastructure.Database.DataAccessObjects;

internal sealed record ValuationRatios : DaoBase
{
    public ValuationRatios() : base()
    {
        Symbol = "";
        CentralIndexKey = "";
        FiscalYear = "";
        FiscalPeriod = "";
    }

    public ValuationRatios(Guid userId) : base(userId)
    {
        Symbol = "";
        CentralIndexKey = "";
        FiscalYear = "";
        FiscalPeriod = "";
    }

    public ValuationRatios(Analysis.Fundamental.ValuationRatios ratios, Guid userId) : base(userId)
    {
        Symbol = ratios.Symbol;
        CentralIndexKey = ratios.CentralIndexKey;
        Registrant = ratios.Registrant;
        FiscalYear = ratios.FiscalYear;
        FiscalPeriod = ratios.FiscalPeriod;
        PeriodEndDate = ratios.PeriodEndDate;
        DividendsPerShare = ratios.DividendsPerShare;
        DividendPayoutRatio = ratios.DividendPayoutRatio;
        BookValuePerShare = ratios.BookValuePerShare;
        RetentionRatio = ratios.RetentionRatio;
        NetFixedAssets = ratios.NetFixedAssets;
    }

    public string Symbol { get; init; }
    public string CentralIndexKey { get; init; }
    public string FiscalYear { get; init; }
    public string FiscalPeriod { get; init; }
    public string? Registrant { get; init; }
    public DateOnly? PeriodEndDate { get; init; }
    public decimal? DividendsPerShare { get; init; }
    public double? DividendPayoutRatio { get; init; }
    public decimal? BookValuePerShare { get; init; }
    public double? RetentionRatio { get; init; }
    public decimal? NetFixedAssets { get; init; }

    public override bool IsValid() => base.IsValid() &&
        !string.IsNullOrWhiteSpace(CentralIndexKey) &&
        !string.IsNullOrWhiteSpace(FiscalYear) &&
        !string.IsNullOrWhiteSpace(FiscalPeriod);

    public Analysis.Fundamental.ValuationRatios ToDomain()
    {
        return new()
        {
            BookValuePerShare = BookValuePerShare,
            CentralIndexKey = CentralIndexKey,
            DividendPayoutRatio = DividendPayoutRatio,
            RetentionRatio = RetentionRatio,
            NetFixedAssets = NetFixedAssets,
            DividendsPerShare = DividendsPerShare,
            FiscalPeriod = FiscalPeriod,
            FiscalYear = FiscalYear,
            Registrant = Registrant,
            Symbol = Symbol,
            PeriodEndDate = PeriodEndDate
        };
    }
}
