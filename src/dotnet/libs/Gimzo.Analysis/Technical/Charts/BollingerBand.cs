namespace Gimzo.Analysis.Technical.Charts;

public class BollingerBand
{
    public BollingerBand(MovingAverageKey maKey, double stdDevMultiplier, decimal[] closes)
    {
        MovingAverageKey = maKey;
        StdDevMultiplier = stdDevMultiplier;
        int period = maKey.Period;
        int length = closes.Length;
        Middle = new decimal[length];
        Upper = new decimal[length];
        Lower = new decimal[length];

        var ma = new MovingAverage(maKey, closes);

        for (int i = 0; i < length; i++)
        {
            if (i < period - 1)
                continue;

            decimal mean = ma.Values[i];
            decimal variance = 0M;
            for (int j = 0; j < period; j++)
            {
                decimal diff = closes[i - period + 1 + j] - mean;
                variance += diff * diff;
            }
            variance /= period;
            decimal stdDev = (decimal)Math.Sqrt((double)variance);

            Middle[i] = mean;
            Upper[i] = mean + (decimal)stdDevMultiplier * stdDev;
            Lower[i] = mean - (decimal)stdDevMultiplier * stdDev;
        }
    }

    public MovingAverageKey MovingAverageKey { get; }
    public double StdDevMultiplier { get; }
    public decimal[] Middle { get; }
    public decimal[] Upper { get; }
    public decimal[] Lower { get; }

}