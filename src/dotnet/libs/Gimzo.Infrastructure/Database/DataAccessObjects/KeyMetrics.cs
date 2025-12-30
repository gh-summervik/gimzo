namespace Gimzo.Infrastructure.Database.DataAccessObjects;

internal sealed record KeyMetrics : DaoBase
{
    public KeyMetrics() : base()
    {
        CentralIndexKey = "";
        FiscalYear = "";
    }

    public KeyMetrics(Guid userId) : base(userId)
    {
        CentralIndexKey = "";
        FiscalYear = "";
    }

    public KeyMetrics(Analysis.Fundamental.KeyMetrics metrics, Guid userId) : base(userId)
    {
        Symbol = metrics.Symbol;
        CentralIndexKey = metrics.CentralIndexKey;
        Registrant = metrics.Registrant;
        FiscalYear = metrics.FiscalYear;
        PeriodEndDate = metrics.PeriodEndDate;
        EarningsPerShare = metrics.EarningsPerShare;
        EarningsPerShareForecast = metrics.EarningsPerShareForecast;
        PriceToEarningsRatio = metrics.PriceToEarningsRatio;
        ForwardPriceToEarningsRatio = metrics.ForwardPriceToEarningsRatio;
        EarningsGrowthRate = metrics.EarningsGrowthRate;
        PriceEarningsToGrowthRate = metrics.PriceEarningsToGrowthRate;
        BookValuePerShare = metrics.BookValuePerShare;
        PriceToBookRatio = metrics.PriceToBookRatio;
        Ebitda = metrics.Ebitda;
        EnterpriseValue = metrics.EnterpriseValue;
        DividendYield = metrics.DividendYield;
        DividendPayoutRatio = metrics.DividendPayoutRatio;
        DebtToEquityRatio = metrics.DebtToEquityRatio;
        CapitalExpenditures = metrics.CapitalExpenditures;
        FreeCashFlow = metrics.FreeCashFlow;
        ReturnOnEquity = metrics.ReturnOnEquity;
        OneYearBeta = metrics.OneYearBeta;
        ThreeYearBeta = metrics.ThreeYearBeta;
        FiveYearBeta = metrics.FiveYearBeta;
    }

    public string CentralIndexKey { get; init; }
    public string FiscalYear { get; init; }
    public string? Symbol { get; init; }
    public string? Registrant { get; init; }
    public DateOnly? PeriodEndDate { get; init; }
    public decimal? EarningsPerShare { get; init; }
    public decimal? EarningsPerShareForecast { get; init; }
    public double? PriceToEarningsRatio { get; init; }
    public double? ForwardPriceToEarningsRatio { get; init; }
    public double? EarningsGrowthRate { get; init; }
    public double? PriceEarningsToGrowthRate { get; init; }
    public decimal? BookValuePerShare { get; init; }
    public double? PriceToBookRatio { get; init; }
    public double? Ebitda { get; init; }
    public decimal? EnterpriseValue { get; init; }
    public double? DividendYield { get; init; }
    public double? DividendPayoutRatio { get; init; }
    public double? DebtToEquityRatio { get; init; }
    public decimal? CapitalExpenditures { get; init; }
    public decimal? FreeCashFlow { get; init; }
    public decimal? ReturnOnEquity { get; init; }
    public double? OneYearBeta { get; init; }
    public double? ThreeYearBeta { get; init; }
    public double? FiveYearBeta { get; init; }

    public override bool IsValid() => base.IsValid() &&
        !string.IsNullOrWhiteSpace(CentralIndexKey) &&
        !string.IsNullOrWhiteSpace(FiscalYear);
}
