namespace Gimzo.Analysis.Technical.Trends;

public static class TrendExtensions
{
    public static TrendSentiment AsSentiment(this double dbl)
    {
        return dbl switch
        {
            var x when x < 0D => TrendSentiment.Bearish,
            var x when x <= 1.0D => TrendSentiment.Bullish,
            _ => TrendSentiment.None
        };
    }
}
