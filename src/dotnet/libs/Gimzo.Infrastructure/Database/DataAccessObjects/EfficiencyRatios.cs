namespace Gimzo.Infrastructure.Database.DataAccessObjects;

internal sealed record EfficiencyRatios : DaoBase
{
    public EfficiencyRatios() : base()
    {
        Symbol = "";
        CentralIndexKey = "";
        FiscalYear = "";
        FiscalPeriod = "";
    }

    public EfficiencyRatios(Guid userId) : base(userId)
    {
        Symbol = "";
        CentralIndexKey = "";
        FiscalYear = "";
        FiscalPeriod = "";
    }

    public EfficiencyRatios(Analysis.Fundamental.EfficiencyRatios ratios, Guid userId) : base(userId)
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

    public string Symbol { get; init; }
    public string CentralIndexKey { get; init; }
    public string FiscalYear { get; init; }
    public string FiscalPeriod { get; init; }
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

    public Analysis.Fundamental.EfficiencyRatios ToDomain()
    {
        return new()
        {
            AccountsPayableTurnoverRatio = AccountsPayableTurnoverRatio,
            AccountsReceivableTurnoverRatio = AccountsReceivableTurnoverRatio,
            AssetTurnoverRatio = AssetTurnoverRatio,
            CapitalIntensityRatio = CapitalIntensityRatio,
            CentralIndexKey = CentralIndexKey,
            DaysCashOnHand = DaysCashOnHand,
            DaysSalesInInventory = DaysSalesInInventory,
            DaysWorkingCapital = DaysWorkingCapital,
            EquityMultiplier = EquityMultiplier,
            FiscalPeriod = FiscalPeriod,
            FiscalYear = FiscalYear,
            FixedAssetTurnoverRatio = FixedAssetTurnoverRatio,
            InventoryToSalesRatio = InventoryToSalesRatio,
            InventoryTurnoverRatio = InventoryTurnoverRatio,
            InvestmentTurnoverRatio = InvestmentTurnoverRatio,
            PeriodEndDate = PeriodEndDate,
            Registrant = Registrant,
            SalesToEquityRatio = SalesToEquityRatio,
            SalesToOperatingIncomeRatio = SalesToOperatingIncomeRatio,
            Symbol = Symbol,
            WorkingCapitalTurnoverRatio = WorkingCapitalTurnoverRatio
        };
    }
}
