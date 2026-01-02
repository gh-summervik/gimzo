namespace Gimzo.Analysis.Technical.Trends;

public class WeightedTrend
{
    public ITrend Trend { get; }
    public double Weight { get; }
    public double[] TrendValues { get; }

    public WeightedTrend(ITrend trend, double weight)
    {
        Trend = trend ?? throw new ArgumentNullException(nameof(trend));
        if (weight <= 0 || weight > 1D)
            throw new ArgumentOutOfRangeException(nameof(weight));
        Weight = weight;
        Trend.Calculate();
        TrendValues = new double[trend.TrendValues.Length];
        for (int t = 0; t < trend.TrendValues.Length; t++)
            TrendValues[t] = trend.TrendValues[t] * weight;
    }

    public string Name => $"{Trend.Name}:{Weight * 100:F2}%";
}