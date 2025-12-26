namespace Gimzo.Infrastructure.Database.DataAccessObjects;

internal sealed record ShortInterest : DaoBase
{
    public ShortInterest() : base()
    {
        Symbol = "";
    }

    public ShortInterest(Guid userId) : base(userId)
    {
        Symbol = "";
    }

    public ShortInterest(Guid userId, DataProviders.FinancialDataNet.ShortInterest interest) : base(userId)
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
    public DateOnly SettlementDate { get; init; }
    public string? Title { get; init; }
    public string? MarketCode { get; init; }
    public long? ShortedSecurities { get; init; }
    public long? PreviousShortedSecurities { get; init; }
    public long? ChangeInShortedSecurities { get; init; }
    public double? PercentageChangeInShortedSecurities { get; init; }
    public long? AverageDailyVolume { get; init; }
    public double? DaysToConvert { get; init; }
    public bool? IsStockSplit { get; init; }

    public override bool IsValid() => base.IsValid() &&
        !string.IsNullOrWhiteSpace(Symbol) &&
        SettlementDate > new DateOnly(1900, 1, 1);
}
