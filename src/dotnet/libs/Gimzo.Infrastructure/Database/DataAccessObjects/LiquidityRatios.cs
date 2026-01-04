namespace Gimzo.Infrastructure.Database.DataAccessObjects;

internal sealed record LiquidityRatios : DaoBase
{
    public LiquidityRatios() : base()
    {
        Symbol = "";
        CentralIndexKey = "";
        FiscalYear = "";
        FiscalPeriod = "";
    }

    public LiquidityRatios(Guid userId) : base(userId)
    {
        Symbol = "";
        CentralIndexKey = "";
        FiscalYear = "";
        FiscalPeriod = "";
    }

    public LiquidityRatios(Analysis.Fundamental.LiquidityRatios ratios, Guid userId) : base(userId)
    {
        Symbol = ratios.Symbol;
        CentralIndexKey = ratios.CentralIndexKey;
        Registrant = ratios.Registrant;
        FiscalYear = ratios.FiscalYear;
        FiscalPeriod = ratios.FiscalPeriod;
        PeriodEndDate = ratios.PeriodEndDate;
        WorkingCapital = ratios.WorkingCapital;
        CurrentRatio = ratios.CurrentRatio;
        CashRatio = ratios.CashRatio;
        QuickRatio = ratios.QuickRatio;
        DaysOfInventoryOutstanding = ratios.DaysOfInventoryOutstanding;
        DaysOfSalesOutstanding = ratios.DaysOfSalesOutstanding;
        DaysPayableOutstanding = ratios.DaysPayableOutstanding;
        CashConversionCycle = ratios.CashConversionCycle;
        SalesToWorkingCapitalRatio = ratios.SalesToWorkingCapitalRatio;
        CashToCurrentLiabilitiesRatio = ratios.CashToCurrentLiabilitiesRatio;
        WorkingCapitalToDebtRatio = ratios.WorkingCapitalToDebtRatio;
        CashFlowAdequacyRatio = ratios.CashFlowAdequacyRatio;
        SalesToCurrentAssetsRatio = ratios.SalesToCurrentAssetsRatio;
        CashToCurrentAssetsRatio = ratios.CashToCurrentAssetsRatio;
        CashToWorkingCapitalRatio = ratios.CashToWorkingCapitalRatio;
        InventoryToWorkingCapitalRatio = ratios.InventoryToWorkingCapitalRatio;
        NetDebt = ratios.NetDebt;
    }

    public string Symbol { get; init; }
    public string CentralIndexKey { get; init; }
    public string FiscalYear { get; init; }
    public string FiscalPeriod { get; init; }
    public string? Registrant { get; init; }
    public DateOnly? PeriodEndDate { get; init; }
    public decimal? WorkingCapital { get; init; }
    public double? CurrentRatio { get; init; }
    public double? CashRatio { get; init; }
    public double? QuickRatio { get; init; }
    public double? DaysOfInventoryOutstanding { get; init; }
    public double? DaysOfSalesOutstanding { get; init; }
    public double? DaysPayableOutstanding { get; init; }
    public double? CashConversionCycle { get; init; }
    public double? SalesToWorkingCapitalRatio { get; init; }
    public double? CashToCurrentLiabilitiesRatio { get; init; }
    public double? WorkingCapitalToDebtRatio { get; init; }
    public double? CashFlowAdequacyRatio { get; init; }
    public double? SalesToCurrentAssetsRatio { get; init; }
    public double? CashToCurrentAssetsRatio { get; init; }
    public double? CashToWorkingCapitalRatio { get; init; }
    public double? InventoryToWorkingCapitalRatio { get; init; }
    public decimal? NetDebt { get; init; }

    public override bool IsValid() => base.IsValid() &&
        !string.IsNullOrWhiteSpace(CentralIndexKey) &&
        !string.IsNullOrWhiteSpace(FiscalYear) &&
        !string.IsNullOrWhiteSpace(FiscalPeriod);

    public Analysis.Fundamental.LiquidityRatios ToDomain()
    {
        return new()
        {
            CashConversionCycle = CashConversionCycle,
            CashFlowAdequacyRatio = CashFlowAdequacyRatio,
            CashRatio = CashRatio,
            CashToCurrentAssetsRatio = CashToCurrentAssetsRatio,
            CashToCurrentLiabilitiesRatio = CashToCurrentLiabilitiesRatio,
            CashToWorkingCapitalRatio = CashToWorkingCapitalRatio,
            CentralIndexKey = CentralIndexKey,
            FiscalYear = FiscalYear,
            FiscalPeriod = FiscalPeriod,
            CurrentRatio = CurrentRatio,
            DaysOfInventoryOutstanding = DaysOfInventoryOutstanding,
            DaysOfSalesOutstanding = DaysOfSalesOutstanding,
            DaysPayableOutstanding = DaysPayableOutstanding,
            InventoryToWorkingCapitalRatio = InventoryToWorkingCapitalRatio,
            NetDebt = NetDebt,
            PeriodEndDate = PeriodEndDate,
            QuickRatio = QuickRatio,
            Registrant = Registrant,
            SalesToCurrentAssetsRatio = SalesToCurrentAssetsRatio,
            SalesToWorkingCapitalRatio = SalesToWorkingCapitalRatio,
            Symbol = Symbol,
            WorkingCapital = WorkingCapital,
            WorkingCapitalToDebtRatio = WorkingCapitalToDebtRatio
        };
    }
}
