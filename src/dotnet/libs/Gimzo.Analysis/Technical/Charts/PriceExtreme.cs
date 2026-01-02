namespace Gimzo.Analysis.Technical.Charts;

public readonly struct PriceExtreme(decimal price, bool isHigh, bool isLow, TrendSentiment sentiment, int index)
{
    public decimal Price { get; } = price;
    public bool IsHigh { get; } = isHigh;
    public bool IsLow { get; } = isLow;
    public TrendSentiment Sentiment { get; } = sentiment;
    public int Index { get; } = index;
    public bool IsBullish => Sentiment == TrendSentiment.Bullish;
    public bool IsBearish => Sentiment == TrendSentiment.Bearish;

    public bool IsDefault => Price == 0M && IsHigh == IsLow && Sentiment == TrendSentiment.None && Index == 0;
}
