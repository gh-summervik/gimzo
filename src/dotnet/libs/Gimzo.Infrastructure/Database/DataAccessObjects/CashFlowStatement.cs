namespace Gimzo.Infrastructure.Database.DataAccessObjects;

internal sealed record CashFlowStatement : DaoBase
{
    public CashFlowStatement() : base()
    {
        Symbol = "";
        CentralIndexKey = "";
        Registrant = "";
        FiscalYear = "";
        FiscalPeriod = "";
    }

    public CashFlowStatement(Guid userId) : base(userId)
    {
        Symbol = "";
        CentralIndexKey = "";
        Registrant = "";
        FiscalYear = "";
        FiscalPeriod = "";
    }

    public CashFlowStatement(Guid userId, Gimzo.Infrastructure.DataProviders.FinancialDataNet.CashFlowStatement stmt) : base(userId)
    {
        Symbol = stmt.Symbol;
        CentralIndexKey = stmt.CentralIndexKey;
        Registrant = stmt.Registrant;
        FiscalYear = stmt.FiscalYear;
        FiscalPeriod = stmt.FiscalPeriod;
        PeriodEndDate = stmt.PeriodEndDate;
        Depreciation = stmt.Depreciation;
        ShareBasedCompensationExpense = stmt.ShareBasedCompensationExpense;
        DeferredIncomeTaxExpense = stmt.DeferredIncomeTaxExpense;
        OtherNonCashIncomeExpense = stmt.OtherNonCashIncomeExpense;
        ChangeInAccountsReceivable = stmt.ChangeInAccountsReceivable;
        ChangeInInventories = stmt.ChangeInInventories;
        ChangeInNonTradeReceivables = stmt.ChangeInNonTradeReceivables;
        ChangeInOtherAssets = stmt.ChangeInOtherAssets;
        ChangeInAccountsPayable = stmt.ChangeInAccountsPayable;
        ChangeInDeferredRevenue = stmt.ChangeInDeferredRevenue;
        ChangeInOtherLiabilities = stmt.ChangeInOtherLiabilities;
        CashFromOperatingActivities = stmt.CashFromOperatingActivities;
        PurchasesOfMarketableSecurities = stmt.PurchasesOfMarketableSecurities;
        SalesOfMarketableSecurities = stmt.SalesOfMarketableSecurities;
        AcquisitionOfProperty = stmt.AcquisitionOfProperty;
        AcquisitionOfBusiness = stmt.AcquisitionOfBusiness;
        OtherInvestingActivities = stmt.OtherInvestingActivities;
        CashFromInvestingActivities = stmt.CashFromInvestingActivities;
        TaxWithholdingforShareBasedCompensation = stmt.TaxWithholdingforShareBasedCompensation;
        PaymentsOfDividends = stmt.PaymentsOfDividends;
        IssuanceOfCommonStock = stmt.IssuanceOfCommonStock;
        RepurchaseOfCommonStock = stmt.RepurchaseOfCommonStock;
        IssuanceOfLongTermDebt = stmt.IssuanceOfLongTermDebt;
        RepaymentOfLongTermDebt = stmt.RepaymentOfLongTermDebt;
        OtherFinancingActivities = stmt.OtherFinancingActivities;
        CashFromFinancingActivities = stmt.CashFromFinancingActivities;
        ChangeInCash = stmt.ChangeInCash;
        CashAtEndOfPeriod = stmt.CashAtEndOfPeriod;
        IncomeTaxesPaid = stmt.IncomeTaxesPaid;
        InterestPaid = stmt.InterestPaid;
    }

    public string Symbol { get; init; }
    public string CentralIndexKey { get; init; }
    public string Registrant { get; init; }
    public string FiscalYear { get; init; }
    public string FiscalPeriod { get; init; }
    public DateOnly PeriodEndDate { get; init; }
    public decimal Depreciation { get; init; }
    public decimal ShareBasedCompensationExpense { get; init; }
    public decimal DeferredIncomeTaxExpense { get; init; }
    public decimal OtherNonCashIncomeExpense { get; init; }
    public decimal ChangeInAccountsReceivable { get; init; }
    public decimal ChangeInInventories { get; init; }
    public decimal ChangeInNonTradeReceivables { get; init; }
    public decimal ChangeInOtherAssets { get; init; }
    public decimal ChangeInAccountsPayable { get; init; }
    public decimal ChangeInDeferredRevenue { get; init; }
    public decimal ChangeInOtherLiabilities { get; init; }
    public decimal CashFromOperatingActivities { get; init; }
    public decimal PurchasesOfMarketableSecurities { get; init; }
    public decimal SalesOfMarketableSecurities { get; init; }
    public decimal AcquisitionOfProperty { get; init; }
    public decimal AcquisitionOfBusiness { get; init; }
    public decimal OtherInvestingActivities { get; init; }
    public decimal CashFromInvestingActivities { get; init; }
    public decimal TaxWithholdingforShareBasedCompensation { get; init; }
    public decimal PaymentsOfDividends { get; init; }
    public decimal IssuanceOfCommonStock { get; init; }
    public decimal RepurchaseOfCommonStock { get; init; }
    public decimal IssuanceOfLongTermDebt { get; init; }
    public decimal RepaymentOfLongTermDebt { get; init; }
    public decimal OtherFinancingActivities { get; init; }
    public decimal CashFromFinancingActivities { get; init; }
    public decimal ChangeInCash { get; init; }
    public decimal CashAtEndOfPeriod { get; init; }
    public decimal IncomeTaxesPaid { get; init; }
    public decimal InterestPaid { get; init; }

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
