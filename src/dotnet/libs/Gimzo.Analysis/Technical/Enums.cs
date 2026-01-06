namespace Gimzo.Analysis.Technical;

public enum ChartInterval
{
    Daily = 0,
    Weekly,
    Monthly,
    Quarterly,
    Annually
}

public enum CandlestickColor
{
    None = 0,
    Light,
    Dark
}

public enum PricePoint
{
    MidPoint = 0,
    Open,
    High,
    Low,
    Close
}

public enum TrendSentiment
{
    None = 0,
    Bullish,
    Bearish
}