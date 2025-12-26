namespace Gimzo.Analysis.Fundamental;

public sealed class EfficiencyRatios
{
    public required string CentralIndexKey { get; init; }
    public required string FiscalYear { get; init; }
    public required string FiscalPeriod { get; init; }
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
}