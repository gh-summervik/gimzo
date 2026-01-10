namespace Gimzo.Analysis.Technical.Charts;

public readonly struct BollingerBandConfiguration
{
    public BollingerBandConfiguration()
    {
        MovingAverageCode = "S21C";
        StdDevMultipler = 2;
    }
    public string MovingAverageCode { get; init; }
    public double StdDevMultipler { get; init; }
}

public struct BollingerBand
{
    public BollingerBand(BollingerBandConfiguration configuration, decimal[] values)
    {
        MovingAverageKey = MovingAverageKey.Create(configuration.MovingAverageCode)
            ?? throw new ArgumentException($"Could not parse '{configuration.MovingAverageCode}' as moving average.");
        StdDevMultiplier = configuration.StdDevMultipler;
        Calculate(values);
    }

    public BollingerBand(MovingAverageKey maKey, double stdDevMultiplier, decimal[] closes)
    {
        MovingAverageKey = maKey;
        StdDevMultiplier = stdDevMultiplier;
        Calculate(closes);
    }

    public MovingAverageKey MovingAverageKey { get; }
    public double StdDevMultiplier { get; } = 2;
    public decimal[] Middle { get; private set; } = [];
    public decimal[] Upper { get; private set; } = [];
    public decimal[] Lower { get; private set; } = [];

    private void Calculate(decimal[] closes)
    {
        int period = MovingAverageKey.Period;
        int length = closes.Length;
        Middle = new decimal[length];
        Upper = new decimal[length];
        Lower = new decimal[length];

        var ma = new MovingAverage(MovingAverageKey, closes);

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
            Upper[i] = mean + (decimal)StdDevMultiplier * stdDev;
            Lower[i] = mean - (decimal)StdDevMultiplier * stdDev;
        }
    }
}