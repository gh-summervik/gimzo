namespace Gimzo.Infrastructure.Database.DataAccessObjects;

internal sealed record ProfitabilityRatio : DaoBase
{
    public ProfitabilityRatio() : base()
    {
        CentralIndexKey = "";
        FiscalYear = "";
        FiscalPeriod = "";
    }

    public ProfitabilityRatio(Guid userId) : base(userId)
    {
        CentralIndexKey = "";
        FiscalYear = "";
        FiscalPeriod = "";
    }

    public ProfitabilityRatio(Analysis.Fundamental.ProfitabilityRatios ratios, Guid userId) : base(userId)
    {
        Symbol = ratios.Symbol;
        CentralIndexKey = ratios.CentralIndexKey;
        Registrant = ratios.Registrant;
        FiscalYear = ratios.FiscalYear;
        FiscalPeriod = ratios.FiscalPeriod;
        PeriodEndDate = ratios.PeriodEndDate;
        Ebit = ratios.Ebit;
        Ebitda = ratios.Ebitda;
        ProfitMargin = ratios.ProfitMargin;
        GrossMargin = ratios.GrossMargin;
        OperatingMargin = ratios.OperatingMargin;
        OperatingCashFlowMargin = ratios.OperatingCashFlowMargin;
        ReturnOnEquity = ratios.ReturnOnEquity;
        ReturnOnAssets = ratios.ReturnOnAssets;
        ReturnOnDebt = ratios.ReturnOnDebt;
        CashReturnOnAssets = ratios.CashReturnOnAssets;
        CashTurnoverRatio = ratios.CashTurnoverRatio;
    }

    
    public string CentralIndexKey { get; init; }
    public string FiscalYear { get; init; }
    public string FiscalPeriod { get; init; }
    public string? Symbol { get; init; }
    public string? Registrant { get; init; }
    public DateOnly? PeriodEndDate { get; init; }
    public decimal? Ebit { get; init; }
    public decimal? Ebitda { get; init; }
    public double? ProfitMargin { get; init; }
    public double? GrossMargin { get; init; }
    public double? OperatingMargin { get; init; }
    public double? OperatingCashFlowMargin { get; init; }
    public double? ReturnOnEquity { get; init; }
    public double? ReturnOnAssets { get; init; }
    public double? ReturnOnDebt { get; init; }
    public double? CashReturnOnAssets { get; init; }
    public double? CashTurnoverRatio { get; init; }

    public override bool IsValid() => base.IsValid() &&
        !string.IsNullOrWhiteSpace(CentralIndexKey) &&
        !string.IsNullOrWhiteSpace(FiscalYear) &&
        !string.IsNullOrWhiteSpace(FiscalPeriod);
}
