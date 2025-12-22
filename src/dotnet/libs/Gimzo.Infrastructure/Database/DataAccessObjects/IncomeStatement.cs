namespace Gimzo.Infrastructure.Database.DataAccessObjects;

internal sealed record IncomeStatement : DaoBase
{
    public IncomeStatement() : base()
    {
        Symbol = "";
        CentralIndexKey = "";
        Registrant = "";
        FiscalYear = "";
        FiscalPeriod = "";
    }

    public IncomeStatement(Guid userId) : base(userId)
    {
        Symbol = "";
        CentralIndexKey = "";
        Registrant = "";
        FiscalYear = "";
        FiscalPeriod = "";
    }

    public IncomeStatement(Guid userId, Gimzo.Infrastructure.DataProviders.FinancialDataNet.IncomeStatement stmt) : base(userId)
    {
        Symbol = stmt.Symbol;
        CentralIndexKey = stmt.CentralIndexKey;
        Registrant = stmt.Registrant;
        FiscalYear = stmt.FiscalYear;
        FiscalPeriod = stmt.FiscalPeriod;
        PeriodEndDate = stmt.PeriodEndDate;
        Revenue = stmt.Revenue;
        CostOfRevenue = stmt.CostOfRevenue;
        GrossProfit = stmt.GrossProfit;
        ResearchDevelopmentExpenses = stmt.ResearchDevelopmentExpenses;
        GeneralAdminExpenses = stmt.GeneralAdminExpenses;
        OperatingExpenses = stmt.OperatingExpenses;
        OperatingIncome = stmt.OperatingIncome;
        InterestExpense = stmt.InterestExpense;
        InterestIncome = stmt.InterestIncome;
        NetIncome = stmt.NetIncome;
        EarningsPerShareBasic = stmt.EarningsPerShareBasic;
        EarningsPerShareDiluted = stmt.EarningsPerShareDiluted;
        WeightedAverageSharesOutstandingBasic = stmt.WeightedAverageSharesOutstandingBasic;
        WeightedAverageSharesOutstandingDiluted = stmt.WeightedAverageSharesOutstandingDiluted;
    }

    public string Symbol { get; init; }
    public string CentralIndexKey { get; init; }
    public string Registrant { get; init; }
    public string FiscalYear { get; init; }
    public string FiscalPeriod { get; init; }
    public DateOnly PeriodEndDate { get; init; }
    public decimal Revenue { get; init; }
    public decimal CostOfRevenue { get; init; }
    public decimal GrossProfit { get; init; }
    public decimal ResearchDevelopmentExpenses { get; init; }
    public decimal GeneralAdminExpenses { get; init; }
    public decimal OperatingExpenses { get; init; }
    public decimal OperatingIncome { get; init; }
    public decimal InterestExpense { get; init; }
    public decimal InterestIncome { get; init; }
    public decimal NetIncome { get; init; }
    public decimal EarningsPerShareBasic { get; init; }
    public decimal EarningsPerShareDiluted { get; init; }
    public long WeightedAverageSharesOutstandingBasic { get; init; }
    public long WeightedAverageSharesOutstandingDiluted { get; init; }

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
