namespace Gimzo.Infrastructure.Database.DataAccessObjects;

internal sealed record ShortInterest : DaoBase
{
    public ShortInterest() : base()
    {
        Symbol = "";
        Title = "";
        MarketCode = "";
    }

    public ShortInterest(Guid userId) : base(userId)
    {
        Symbol = "";
        Title = "";
        MarketCode = "";
    }

    public ShortInterest(Guid userId, Gimzo.Infrastructure.DataProviders.FinancialDataNet.ShortInterest interest) : base(userId)
    {
        Symbol = interest.Symbol;
        Title = interest.Title;
        MarketCode = interest.MarketCode;
        SettlementDate = interest.SettlementDate;
        ShortedSecurities = interest.ShortedSecurities;
        PreviousShortedSecurities = interest.PreviousShortedSecurities;
        ChangeInShortedSecurities = interest.ChangeInShortedSecurities;
        PercentageChangeInShortedSecurities = interest.PercentageChangeInShortedSecurities;
        AverageDailyVolume = interest.AverageDailyVolume;
        DaysToConvert = interest.DaysToConvert;
        IsStockSplit = interest.IsStockSplit;
    }

    public string Symbol { get; init; }
    public string Title { get; init; }
    public string MarketCode { get; init; }
    public DateOnly SettlementDate { get; init; }
    public long ShortedSecurities { get; init; }
    public long PreviousShortedSecurities { get; init; }
    public long ChangeInShortedSecurities { get; init; }
    public double PercentageChangeInShortedSecurities { get; init; }
    public long AverageDailyVolume { get; init; }
    public double DaysToConvert { get; init; }
    public bool IsStockSplit { get; init; }

    public override bool IsValid() => base.IsValid() &&
        !string.IsNullOrWhiteSpace(Symbol) &&
        !string.IsNullOrWhiteSpace(Title) &&
        !string.IsNullOrWhiteSpace(MarketCode) &&
        SettlementDate > new DateOnly(1900, 1, 1);
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
