namespace Gimzo.Infrastructure.Database.DataAccessObjects;

internal sealed record SolvencyRatio : DaoBase
{
    public SolvencyRatio() : base()
    {
        CentralIndexKey = "";
        FiscalYear = "";
        FiscalPeriod = "";
    }

    public SolvencyRatio(Guid userId) : base(userId)
    {
        CentralIndexKey = "";
        FiscalYear = "";
        FiscalPeriod = "";
    }

    public SolvencyRatio(Analysis.Fundamental.SolvencyRatios ratios, Guid userId) : base(userId)
    {
        Symbol = ratios.Symbol;
        CentralIndexKey = ratios.CentralIndexKey;
        Registrant = ratios.Registrant;
        FiscalYear = ratios.FiscalYear;
        FiscalPeriod = ratios.FiscalPeriod;
        PeriodEndDate = ratios.PeriodEndDate;
        EquityRatio = ratios.EquityRatio;
        DebtCoverageRatio = ratios.DebtCoverageRatio;
        AssetCoverageRatio = ratios.AssetCoverageRatio;
        InterestCoverageRatio = ratios.InterestCoverageRatio;
        DebtToEquityRatio = ratios.DebtToEquityRatio;
        DebtToAssetsRatio = ratios.DebtToAssetsRatio;
        DebtToCapitalRatio = ratios.DebtToCapitalRatio;
        DebtToIncomeRatio = ratios.DebtToIncomeRatio;
        CashFlowToDebtRatio = ratios.CashFlowToDebtRatio;
    }

    public string CentralIndexKey { get; init; }
    public string FiscalYear { get; init; }
    public string FiscalPeriod { get; init; }
    public string? Symbol { get; init; }
    public string? Registrant { get; init; }
    public DateOnly? PeriodEndDate { get; init; }
    public double? EquityRatio { get; init; }
    public double? DebtCoverageRatio { get; init; }
    public double? AssetCoverageRatio { get; init; }
    public double? InterestCoverageRatio { get; init; }
    public double? DebtToEquityRatio { get; init; }
    public double? DebtToAssetsRatio { get; init; }
    public double? DebtToCapitalRatio { get; init; }
    public double? DebtToIncomeRatio { get; init; }
    public double? CashFlowToDebtRatio { get; init; }

    public override bool IsValid() => base.IsValid() &&
        !string.IsNullOrWhiteSpace(CentralIndexKey) &&
        !string.IsNullOrWhiteSpace(FiscalYear) &&
        !string.IsNullOrWhiteSpace(FiscalPeriod);
}
