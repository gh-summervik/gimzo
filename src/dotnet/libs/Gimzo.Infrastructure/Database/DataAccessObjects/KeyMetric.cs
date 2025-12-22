namespace Gimzo.Infrastructure.Database.DataAccessObjects;

internal sealed record KeyMetric : DaoBase
{
    public KeyMetric() : base()
    {
        Symbol = "";
        CentralIndexKey = "";
        Registrant = "";
        FiscalYear = "";
    }

    public KeyMetric(Guid userId) : base(userId)
    {
        Symbol = "";
        CentralIndexKey = "";
        Registrant = "";
        FiscalYear = "";
    }

    public KeyMetric(Guid userId, Gimzo.Infrastructure.DataProviders.FinancialDataNet.KeyMetrics metrics) : base(userId)
    {
        Symbol = metrics.Symbol;
        CentralIndexKey = metrics.CentralIndexKey;
        Registrant = metrics.Registrant;
        FiscalYear = metrics.FiscalYear;
        PeriodEndDate = metrics.PeriodEndDate;
        EarningsPerShare = metrics.EarningsPerShare;
        EarningsPerShareForecast = metrics.EarningsPerShareForecast;
        PriceToEarningsRatio = metrics.PriceToEarningsRatio;
        ForwardPriceToEarningsRatio = metrics.ForwardPriceToEarningsRatio;
        EarningsGrowthRate = metrics.EarningsGrowthRate;
        PriceEarningsToGrowthRate = metrics.PriceEarningsToGrowthRate;
        BookValuePerShare = metrics.BookValuePerShare;
        PriceToBookRatio = metrics.PriceToBookRatio;
        Ebitda = metrics.Ebitda;
        EnterpriseValue = metrics.EnterpriseValue;
        DividendYield = metrics.DividendYield;
        DividendPayoutRatio = metrics.DividendPayoutRatio;
        DebtToEquityRatio = metrics.DebtToEquityRatio;
        CapitalExpenditures = metrics.CapitalExpenditures;
        FreeCashFlow = metrics.FreeCashFlow;
        ReturnOnEquity = metrics.ReturnOnEquity;
        OneYearBeta = metrics.OneYearBeta;
        ThreeYearBeta = metrics.ThreeYearBeta;
        FiveYearBeta = metrics.FiveYearBeta;
    }

    public string Symbol { get; init; }
    public string CentralIndexKey { get; init; }
    public string Registrant { get; init; }
    public string FiscalYear { get; init; }
    public DateOnly PeriodEndDate { get; init; }
    public decimal EarningsPerShare { get; init; }
    public decimal EarningsPerShareForecast { get; init; }
    public double PriceToEarningsRatio { get; init; }
    public double ForwardPriceToEarningsRatio { get; init; }
    public double EarningsGrowthRate { get; init; }
    public double PriceEarningsToGrowthRate { get; init; }
    public decimal BookValuePerShare { get; init; }
    public double PriceToBookRatio { get; init; }
    public double Ebitda { get; init; }
    public decimal EnterpriseValue { get; init; }
    public double DividendYield { get; init; }
    public double DividendPayoutRatio { get; init; }
    public double DebtToEquityRatio { get; init; }
    public decimal CapitalExpenditures { get; init; }
    public decimal FreeCashFlow { get; init; }
    public decimal ReturnOnEquity { get; init; }
    public double OneYearBeta { get; init; }
    public double ThreeYearBeta { get; init; }
    public double FiveYearBeta { get; init; }

    public override bool IsValid() => base.IsValid() &&
        !string.IsNullOrWhiteSpace(Symbol) &&
        !string.IsNullOrWhiteSpace(CentralIndexKey) &&
        !string.IsNullOrWhiteSpace(Registrant) &&
        !string.IsNullOrWhiteSpace(FiscalYear) &&
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
