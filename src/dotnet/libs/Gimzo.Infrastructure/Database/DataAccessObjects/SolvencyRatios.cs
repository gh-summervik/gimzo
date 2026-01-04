namespace Gimzo.Infrastructure.Database.DataAccessObjects;

internal sealed record SolvencyRatios : DaoBase
{
    public SolvencyRatios() : base()
    {
        Symbol = "";
        CentralIndexKey = "";
        FiscalYear = "";
        FiscalPeriod = "";
    }

    public SolvencyRatios(Guid userId) : base(userId)
    {
        Symbol = "";
        CentralIndexKey = "";
        FiscalYear = "";
        FiscalPeriod = "";
    }

    public SolvencyRatios(Analysis.Fundamental.SolvencyRatios ratios, Guid userId) : base(userId)
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

    public string Symbol { get; init; }
    public string CentralIndexKey { get; init; }
    public string FiscalYear { get; init; }
    public string FiscalPeriod { get; init; }
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

    public Analysis.Fundamental.SolvencyRatios ToDomain()
    {
        return new()
        {
            AssetCoverageRatio = AssetCoverageRatio,
            InterestCoverageRatio = InterestCoverageRatio,
            DebtCoverageRatio = DebtCoverageRatio,
            CashFlowToDebtRatio = CashFlowToDebtRatio,
            CentralIndexKey = CentralIndexKey,
            FiscalYear = FiscalYear,
            FiscalPeriod = FiscalPeriod,
            DebtToAssetsRatio = DebtToAssetsRatio,
            DebtToCapitalRatio = DebtToCapitalRatio,
            DebtToEquityRatio = DebtToEquityRatio,
            DebtToIncomeRatio = DebtToIncomeRatio,
            EquityRatio = EquityRatio,
            PeriodEndDate = PeriodEndDate,
            Registrant = Registrant,
            Symbol = Symbol
        };
    }
}
