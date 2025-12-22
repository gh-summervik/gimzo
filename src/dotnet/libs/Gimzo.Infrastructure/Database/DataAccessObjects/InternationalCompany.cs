namespace Gimzo.Infrastructure.Database.DataAccessObjects;

internal sealed record InternationalCompany : DaoBase
{
    public InternationalCompany() : base()
    {
        Symbol = "";
        Registrant = "";
        Exchange = "";
        Industry = "";
        YearFounding = "";
        ChiefExecutiveOfficer = "";
        WebSite = "";
        Description = "";
    }

    public InternationalCompany(Guid userId) : base(userId)
    {
        Symbol = "";
        Registrant = "";
        Exchange = "";
        Industry = "";
        YearFounding = "";
        ChiefExecutiveOfficer = "";
        WebSite = "";
        Description = "";
    }

    public InternationalCompany(Guid userId, Gimzo.Infrastructure.DataProviders.FinancialDataNet.InternationalCompanyInformation info) : base(userId)
    {
        Symbol = info.Symbol;
        Registrant = info.Registrant;
        Exchange = info.Exchange;
        Isin = info.Isin;
        Industry = info.Industry;
        YearFounding = info.YearFounding;
        ChiefExecutiveOfficer = info.ChiefExecutiveOfficer;
        NumberEmployees = info.NumberEmployees;
        WebSite = info.WebSite;
        Description = info.Description;
    }

    public string Symbol { get; init; }
    public string Registrant { get; init; }
    public string Exchange { get; init; }
    public string? Isin { get; init; }
    public string Industry { get; init; }
    public string YearFounding { get; init; }
    public string ChiefExecutiveOfficer { get; init; }
    public int? NumberEmployees { get; init; }
    public string WebSite { get; init; }
    public string Description { get; init; }

    public override bool IsValid() => base.IsValid() &&
        !string.IsNullOrWhiteSpace(Symbol) &&
        !string.IsNullOrWhiteSpace(Registrant) &&
        !string.IsNullOrWhiteSpace(Exchange) &&
        !string.IsNullOrWhiteSpace(Industry);
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
