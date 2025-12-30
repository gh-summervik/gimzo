namespace Gimzo.Infrastructure.Database.DataAccessObjects;

internal sealed record EfficiencyRatio : DaoBase
{
    public EfficiencyRatio() : base()
    {
        CentralIndexKey = "";
        FiscalYear = "";
        FiscalPeriod = "";
    }

    public EfficiencyRatio(Guid userId) : base(userId)
    {
        CentralIndexKey = "";
        FiscalYear = "";
        FiscalPeriod = "";
    }

    public EfficiencyRatio(Analysis.Fundamental.EfficiencyRatios ratios, Guid userId) : base(userId)
    {
        Symbol = ratios.Symbol;
        CentralIndexKey = ratios.CentralIndexKey;
        Registrant = ratios.Registrant;
        FiscalYear = ratios.FiscalYear;
        FiscalPeriod = ratios.FiscalPeriod;
        PeriodEndDate = ratios.PeriodEndDate;
        AssetTurnoverRatio = ratios.AssetTurnoverRatio;
        InventoryTurnoverRatio = ratios.InventoryTurnoverRatio;
        AccountsReceivableTurnoverRatio = ratios.AccountsReceivableTurnoverRatio;
        AccountsPayableTurnoverRatio = ratios.AccountsPayableTurnoverRatio;
        EquityMultiplier = ratios.EquityMultiplier;
        DaysSalesInInventory = ratios.DaysSalesInInventory;
        FixedAssetTurnoverRatio = ratios.FixedAssetTurnoverRatio;
        DaysWorkingCapital = ratios.DaysWorkingCapital;
        WorkingCapitalTurnoverRatio = ratios.WorkingCapitalTurnoverRatio;
        DaysCashOnHand = ratios.DaysCashOnHand;
        CapitalIntensityRatio = ratios.CapitalIntensityRatio;
        SalesToEquityRatio = ratios.SalesToEquityRatio;
        InventoryToSalesRatio = ratios.InventoryToSalesRatio;
        InvestmentTurnoverRatio = ratios.InvestmentTurnoverRatio;
        SalesToOperatingIncomeRatio = ratios.SalesToOperatingIncomeRatio;
    }

    public string CentralIndexKey { get; init; }
    public string FiscalYear { get; init; }
    public string FiscalPeriod { get; init; }
    public string? Symbol { get; init; }
    public string? Registrant { get; init; }
    public DateOnly? PeriodEndDate { get; init; }
    public double? AssetTurnoverRatio { get; init; }
    public double? InventoryTurnoverRatio { get; init; }
    public double? AccountsReceivableTurnoverRatio { get; init; }
    public double? AccountsPayableTurnoverRatio { get; init; }
    public double? EquityMultiplier { get; init; }
    public double? DaysSalesInInventory { get; init; }
    public double? FixedAssetTurnoverRatio { get; init; }
    public double? DaysWorkingCapital { get; init; }
    public double? WorkingCapitalTurnoverRatio { get; init; }
    public double? DaysCashOnHand { get; init; }
    public double? CapitalIntensityRatio { get; init; }
    public double? SalesToEquityRatio { get; init; }
    public double? InventoryToSalesRatio { get; init; }
    public double? InvestmentTurnoverRatio { get; init; }
    public double? SalesToOperatingIncomeRatio { get; init; }
    public override bool IsValid() => base.IsValid() &&
        !string.IsNullOrWhiteSpace(CentralIndexKey) &&
        !string.IsNullOrWhiteSpace(FiscalYear) &&
        !string.IsNullOrWhiteSpace(FiscalPeriod);
}
