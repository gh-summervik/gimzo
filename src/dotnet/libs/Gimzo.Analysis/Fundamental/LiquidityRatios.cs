namespace Gimzo.Analysis.Fundamental;

public sealed class LiquidityRatios
{
    public required string Symbol { get; init; }
    public required string CentralIndexKey { get; init; }
    public required string Registrant { get; init; }
    public required string FiscalYear { get; init; }
    public required string FiscalPeriod { get; init; }
    public DateOnly PeriodEndDate { get; init; }
    public decimal WorkingCapital { get; init; }
    public double CurrentRatio { get; init; }
    public double CashRatio { get; init; }
    public double QuickRatio { get; init; }
    public double DaysOfInventoryOutstanding { get; init; }
    public double DaysOfSalesOutstanding { get; init; }
    public double DaysPayableOutstanding { get; init; }
    public double CashConversionCycle { get; init; }
    public double SalesToWorkingCapitalRatio { get; init; }
    public double CashToCurrentLiabilitiesRatio { get; init; }
    public double WorkingCapitalToDebtRatio { get; init; }
    public double CashFlowAdequacyRatio { get; init; }
    public double SalesToCurrentAssetsRatio { get; init; }
    public double CashToCurrentAssetsRatio { get; init; }
    public double CashToWorkingCapitalRatio { get; init; }
    public double InventoryToWorkingCapitalRatio { get; init; }
    public decimal NetDebt { get; init; }
}