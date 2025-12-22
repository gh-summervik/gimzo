using System;

namespace Gimzo.Analysis.Fundamental;

public sealed class SolvencyRatios
{
    public required string Symbol { get; init; }
    public required string CentralIndexKey { get; init; }
    public required string Registrant { get; init; }
    public required string FiscalYear { get; init; }
    public required string FiscalPeriod { get; init; }
    public DateOnly PeriodEndDate { get; init; }
    public double EquityRatio { get; init; }
    public double? DebtCoverageRatio { get; init; }
    public double AssetCoverageRatio { get; init; }
    public double? InterestCoverageRatio { get; init; }
    public double DebtToEquityRatio { get; init; }
    public double DebtToAssetsRatio { get; init; }
    public double DebtToCapitalRatio { get; init; }
    public double? DebtToIncomeRatio { get; init; }
    public double CashFlowToDebtRatio { get; init; }
}