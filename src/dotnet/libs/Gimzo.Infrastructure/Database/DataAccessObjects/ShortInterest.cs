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

    public ShortInterest(Analysis.Fundamental.ShortInterest interest, Guid userId) : base(userId)
    {
        Symbol = interest.Symbol;
        Title = interest.Title;
        MarketCode = interest.MarketCode;
        SettlementDate = interest.SettlementDate.GetValueOrDefault();
        ShortedSecurities = interest.ShortedSecurities;
        PreviousShortedSecurities = interest.PreviousShortedSecurities;
        ChangeInShortedSecurities = interest.ChangeInShortedSecurities;
        PercentageChangeInShortedSecurities = interest.PercentageChangeInShortedSecurities;
        AverageDailyVolume = interest.AverageDailyVolume;
        DaysToCover = interest.DaysToCover;
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
    public double? DaysToCover { get; init; }
    public bool? IsStockSplit { get; init; }

    public override bool IsValid() => base.IsValid() &&
        !string.IsNullOrWhiteSpace(Symbol) &&
        SettlementDate > new DateOnly(1900, 1, 1);

    public Analysis.Fundamental.ShortInterest ToDomain()
    {
        return new()
        {
            Symbol = Symbol,
            AverageDailyVolume = AverageDailyVolume,
            ChangeInShortedSecurities = ChangeInShortedSecurities,
            DaysToCover = DaysToCover,
            IsStockSplit = IsStockSplit,
            MarketCode = MarketCode,
            PercentageChangeInShortedSecurities = PercentageChangeInShortedSecurities,
            PreviousShortedSecurities = PreviousShortedSecurities,
            SettlementDate = SettlementDate,
            ShortedSecurities = ShortedSecurities,
            Title = Title
        };
    }
}
