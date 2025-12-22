namespace Gimzo.Infrastructure.Database.DataAccessObjects;

internal sealed record BalanceSheet : DaoBase
{
    public BalanceSheet() : base()
    {
        Symbol = "";
        CentralIndexKey = "";
        Registrant = "";
        FiscalYear = "";
        FiscalPeriod = "";
    }

    public BalanceSheet(Guid userId) : base(userId)
    {
        Symbol = "";
        CentralIndexKey = "";
        Registrant = "";
        FiscalYear = "";
        FiscalPeriod = "";
    }

    public BalanceSheet(Guid userId, Gimzo.Infrastructure.DataProviders.FinancialDataNet.BalanceSheet sheet) : base(userId)
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

    public string Symbol { get; init; }
    public string CentralIndexKey { get; init; }
    public string Registrant { get; init; }
    public string FiscalYear { get; init; }
    public string FiscalPeriod { get; init; }
    public DateOnly PeriodEndDate { get; init; }
    public decimal Cash { get; init; }
    public decimal MarketableSecuritiesCurrent { get; init; }
    public decimal AccountsReceivable { get; init; }
    public decimal Inventories { get; init; }
    public decimal NonTradeReceivables { get; init; }
    public decimal OtherAssetsCurrent { get; init; }
    public decimal TotalAssetsCurrent { get; init; }
    public decimal MarketableSecuritiesNonCurrent { get; init; }
    public decimal PropertyPlantEquipment { get; init; }
    public decimal OtherAssetsNonCurrent { get; init; }
    public decimal TotalAssetsNonCurrent { get; init; }
    public decimal TotalAssets { get; init; }
    public decimal AccountsPayable { get; init; }
    public decimal DeferredRevenue { get; init; }
    public decimal ShortTermDebt { get; init; }
    public decimal OtherLiabilitiesCurrent { get; init; }
    public decimal TotalLiabilitiesCurrent { get; init; }
    public decimal LongTermDebt { get; init; }
    public decimal OtherLiabilitiesNonCurrent { get; init; }
    public decimal TotalLiabilitiesNonCurrent { get; init; }
    public decimal TotalLiabilities { get; init; }
    public decimal CommonStock { get; init; }
    public decimal RetainedEarnings { get; init; }
    public decimal AccumulatedOtherComprehensiveIncome { get; init; }
    public decimal TotalShareholdersEquity { get; init; }

    public override bool IsValid() => base.IsValid() &&
        !string.IsNullOrWhiteSpace(Symbol) &&
        !string.IsNullOrWhiteSpace(CentralIndexKey) &&
        !string.IsNullOrWhiteSpace(Registrant) &&
        !string.IsNullOrWhiteSpace(FiscalYear) &&
        !string.IsNullOrWhiteSpace(FiscalPeriod) &&
        PeriodEndDate > new DateOnly(1900, 1, 1);
}

//namespace Gimzo.Infrastructure.Database.DataAccessObjects;

//internal sealed record Security : DaoBase
//{
//    public Security() : base()
//    {
//        Symbol = "";
//        Type = "";
//    }

//    public Security(Guid userId) : base(userId)
//    {
//        Symbol = "";
//        Type = "";
//    }

//    public Security(Guid userId, DataProviders.FinancialDataNet.Stock stock) : base(userId)
//    {
//        Symbol = stock.Symbol;
//        Type = stock.IsInternational
//            ? $"International {nameof(DataProviders.FinancialDataNet.Stock)}"
//            : nameof(DataProviders.FinancialDataNet.Stock);
//        Registrant = stock.Registrant;
//    }

//    public Security(Guid userId, DataProviders.FinancialDataNet.ExchangeTradedFund etf) : base(userId)
//    {
//        Symbol = etf.Symbol;
//        Type = nameof(DataProviders.FinancialDataNet.ExchangeTradedFund);
//        Description = etf.Description;
//    }

//    public Security(Guid userId, DataProviders.FinancialDataNet.Commodity commodity) : base(userId)
//    {
//        Symbol = commodity.Symbol;
//        Type = nameof(DataProviders.FinancialDataNet.Commodity);
//        Description = commodity.Description;
//    }

//    public Security(Guid userId, DataProviders.FinancialDataNet.OverTheCounter otc) : base(userId)
//    {
//        Symbol = otc.Symbol;
//        Type = nameof(DataProviders.FinancialDataNet.OverTheCounter);
//        Description = otc.Title;
//    }

//    public Security(Guid userId, DataProviders.FinancialDataNet.Index index) : base(userId)
//    {
//        Symbol = index.Symbol;
//        Type = nameof(DataProviders.FinancialDataNet.Index);
//        Description = index.Name;
//    }

//    public Security(Guid userId, DataProviders.FinancialDataNet.Future future) : base(userId)
//    {
//        Symbol = future.Symbol;
//        Type = nameof(DataProviders.FinancialDataNet.Future);
//        Description = future.Description;
//        SubType = future.Type;
//    }

//    public Security(Guid userId, DataProviders.FinancialDataNet.Crypto crypto) : base(userId)
//    {
//        Symbol = crypto.Symbol;
//        Type = nameof(DataProviders.FinancialDataNet.Crypto);
//        BaseAsset = crypto.BaseAsset;
//        QuoteAsset = crypto.QuoteAsset;
//    }

//    public string Symbol { get; init; }
//    public string Type { get; init; }
//    public string? Registrant { get; init; }
//    public string? Description { get; init; }
//    public string? SubType { get; init; }
//    public string? BaseAsset { get; init; }
//    public string? QuoteAsset { get; init; }

//    public override bool IsValid() => base.IsValid() && 
//        !string.IsNullOrWhiteSpace(Symbol) &&
//        !string.IsNullOrWhiteSpace(Type);
//}
