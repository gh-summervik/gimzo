namespace Gimzo.Analysis.Fundamental;

public record struct BalanceSheet
{
    public required string Symbol { get; init; }
    public required string CentralIndexKey { get; init; }
    public required string FiscalYear { get; init; }
    public required string FiscalPeriod { get; init; }
    public string? Registrant { get; init; }
    public DateOnly? PeriodEndDate { get; init; }
    public decimal? Cash { get; init; }
    public decimal? MarketableSecuritiesCurrent { get; init; }
    public decimal? AccountsReceivable { get; init; }
    public decimal? Inventories { get; init; }
    public decimal? NonTradeReceivables { get; init; }
    public decimal? OtherAssetsCurrent { get; init; }
    public decimal? TotalAssetsCurrent { get; init; }
    public decimal? MarketableSecuritiesNonCurrent { get; init; }
    public decimal? PropertyPlantEquipment { get; init; }
    public decimal? OtherAssetsNonCurrent { get; init; }
    public decimal? TotalAssetsNonCurrent { get; init; }
    public decimal? TotalAssets { get; init; }
    public decimal? AccountsPayable { get; init; }
    public decimal? DeferredRevenue { get; init; }
    public decimal? ShortTermDebt { get; init; }
    public decimal? OtherLiabilitiesCurrent { get; init; }
    public decimal? TotalLiabilitiesCurrent { get; init; }
    public decimal? LongTermDebt { get; init; }
    public decimal? OtherLiabilitiesNonCurrent { get; init; }
    public decimal? TotalLiabilitiesNonCurrent { get; init; }
    public decimal? TotalLiabilities { get; init; }
    public decimal? CommonStock { get; init; }
    public decimal? RetainedEarnings { get; init; }
    public decimal? AccumulatedOtherComprehensiveIncome { get; init; }
    public decimal? TotalShareholdersEquity { get; init; }
}