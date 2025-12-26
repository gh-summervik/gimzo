namespace Gimzo.Infrastructure.Database.DataAccessObjects;

internal sealed record StockSymbol : DaoBase
{
    public required string Symbol { get; init; }
    public string? Registrant { get; init; }

    public override bool IsValid() => base.IsValid() &&
        !string.IsNullOrWhiteSpace(Symbol);
}

internal sealed record EtfSymbol : DaoBase
{
    public required string Symbol { get; init; }
    public string? Description { get; init; }
    public override bool IsValid() => base.IsValid() &&
        !string.IsNullOrWhiteSpace(Symbol);
}

internal sealed record CommoditySymbol : DaoBase
{
    public required string Symbol { get; init; }
    public string? Description { get; init; }
    public override bool IsValid() => base.IsValid() &&
        !string.IsNullOrWhiteSpace(Symbol);
}

internal sealed record OtcSymbol : DaoBase
{
    public required string Symbol { get; init; }
    public string? Title { get; init; }
    public override bool IsValid() => base.IsValid() &&
        !string.IsNullOrWhiteSpace(Symbol);
}

internal sealed record CryptoSymbol : DaoBase
{
    public required string Symbol { get; init; }
    public string? BaseAsset { get; init; }
    public string? QuoteAsset { get; init; }
    public override bool IsValid() => base.IsValid() &&
        !string.IsNullOrWhiteSpace(Symbol);
}

internal sealed record IndexSymbol : DaoBase
{
    public required string Symbol { get; init; }
    public string? IndexName { get; init; }
    public override bool IsValid() => base.IsValid() &&
        !string.IsNullOrWhiteSpace(Symbol);
}
