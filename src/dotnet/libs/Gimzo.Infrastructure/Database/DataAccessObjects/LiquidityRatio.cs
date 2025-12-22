namespace Gimzo.Infrastructure.Database.DataAccessObjects;

internal sealed record LiquidityRatio : DaoBase
{
    public LiquidityRatio() : base()
    {
        Symbol = "";
        CentralIndexKey = "";
        Registrant = "";
        FiscalYear = "";
        FiscalPeriod = "";
    }

    public LiquidityRatio(Guid userId) : base(userId)
    {
        Symbol = "";
        CentralIndexKey = "";
        Registrant = "";
        FiscalYear = "";
        FiscalPeriod = "";
    }

    public LiquidityRatio(Guid userId, Gimzo.Infrastructure.DataProviders.FinancialDataNet.LiquidityRatios ratios) : base(userId)
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
    public string Registrant { get; init; }
    public string FiscalYear { get; init; }
    public string FiscalPeriod { get; init; }
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
