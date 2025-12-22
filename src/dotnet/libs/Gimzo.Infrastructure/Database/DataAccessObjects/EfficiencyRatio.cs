namespace Gimzo.Infrastructure.Database.DataAccessObjects;

internal sealed record EfficiencyRatio : DaoBase
{
    public EfficiencyRatio() : base()
    {
        Symbol = "";
        CentralIndexKey = "";
        Registrant = "";
        FiscalYear = "";
        FiscalPeriod = "";
    }

    public EfficiencyRatio(Guid userId) : base(userId)
    {
        Symbol = "";
        CentralIndexKey = "";
        Registrant = "";
        FiscalYear = "";
        FiscalPeriod = "";
    }

    public EfficiencyRatio(Guid userId, Gimzo.Infrastructure.DataProviders.FinancialDataNet.EfficiencyRatios ratios) : base(userId)
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
    public string Registrant { get; init; }
    public string FiscalYear { get; init; }
    public string FiscalPeriod { get; init; }
    public DateOnly PeriodEndDate { get; init; }
    public double AssetTurnoverRatio { get; init; }
    public double InventoryTurnoverRatio { get; init; }
    public double AccountsReceivableTurnoverRatio { get; init; }
    public double AccountsPayableTurnoverRatio { get; init; }
    public double EquityMultiplier { get; init; }
    public double DaysSalesInInventory { get; init; }
    public double FixedAssetTurnoverRatio { get; init; }
    public double DaysWorkingCapital { get; init; }
    public double WorkingCapitalTurnoverRatio { get; init; }
    public double? DaysCashOnHand { get; init; }
    public double CapitalIntensityRatio { get; init; }
    public double SalesToEquityRatio { get; init; }
    public double InventoryToSalesRatio { get; init; }
    public double InvestmentTurnoverRatio { get; init; }
    public double SalesToOperatingIncomeRatio { get; init; }

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
