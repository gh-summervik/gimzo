namespace Gimzo.Infrastructure.Database.DataAccessObjects;

internal sealed record BalanceSheet : DaoBase
{
    public BalanceSheet() : base()
    {
        CentralIndexKey = "";
        FiscalYear = "";
        FiscalPeriod = "";
    }

    public BalanceSheet(Guid userId) : base(userId)
    {
        CentralIndexKey = "";
        FiscalYear = "";
        FiscalPeriod = "";
    }

    public BalanceSheet(Guid userId, DataProviders.FinancialDataNet.BalanceSheet sheet) : base(userId)
    {
        Symbol = sheet.Symbol;
        CentralIndexKey = sheet.CentralIndexKey;
        Registrant = sheet.Registrant;
        FiscalYear = sheet.FiscalYear;
        FiscalPeriod = sheet.FiscalPeriod;
        PeriodEndDate = sheet.PeriodEndDate;
        Cash = sheet.Cash;
        MarketableSecuritiesCurrent = sheet.MarketableSecuritiesCurrent;
        AccountsReceivable = sheet.AccountsReceivable;
        Inventories = sheet.Inventories;
        NonTradeReceivables = sheet.NonTradeReceivables;
        OtherAssetsCurrent = sheet.OtherAssetsCurrent;
        TotalAssetsCurrent = sheet.TotalAssetsCurrent;
        MarketableSecuritiesNonCurrent = sheet.MarketableSecuritiesNonCurrent;
        PropertyPlantEquipment = sheet.PropertyPlantEquipment;
        OtherAssetsNonCurrent = sheet.OtherAssetsNonCurrent;
        TotalAssetsNonCurrent = sheet.TotalAssetsNonCurrent;
        TotalAssets = sheet.TotalAssets;
        AccountsPayable = sheet.AccountsPayable;
        DeferredRevenue = sheet.DeferredRevenue;
        ShortTermDebt = sheet.ShortTermDebt;
        OtherLiabilitiesCurrent = sheet.OtherLiabilitiesCurrent;
        TotalLiabilitiesCurrent = sheet.TotalLiabilitiesCurrent;
        LongTermDebt = sheet.LongTermDebt;
        OtherLiabilitiesNonCurrent = sheet.OtherLiabilitiesNonCurrent;
        TotalLiabilitiesNonCurrent = sheet.TotalLiabilitiesNonCurrent;
        TotalLiabilities = sheet.TotalLiabilities;
        CommonStock = sheet.CommonStock;
        RetainedEarnings = sheet.RetainedEarnings;
        AccumulatedOtherComprehensiveIncome = sheet.AccumulatedOtherComprehensiveIncome;
        TotalShareholdersEquity = sheet.TotalShareholdersEquity;
    }

    public string CentralIndexKey { get; init; }
    public string FiscalYear { get; init; }
    public string FiscalPeriod { get; init; }
    public string? Symbol { get; init; }
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

    public override bool IsValid() => base.IsValid() &&
        !string.IsNullOrWhiteSpace(CentralIndexKey) &&
        !string.IsNullOrWhiteSpace(FiscalYear) &&
        !string.IsNullOrWhiteSpace(FiscalPeriod);
}
