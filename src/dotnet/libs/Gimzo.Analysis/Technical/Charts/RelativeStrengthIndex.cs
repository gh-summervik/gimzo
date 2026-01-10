namespace Gimzo.Analysis.Technical.Charts;

public sealed class RelativeStrengthIndex(Ohlc[] prices, int period = 14, double[]? precomputedAvgVolumes = null)
{
    private readonly Ohlc[] _prices = prices;
    private readonly double[]? _precomputedAvgVolumes = precomputedAvgVolumes;
    private readonly int _period = period;

    public double[] Values { get; private set; } = [];
    public string Name => GetType().Name;

    public void Calculate()
    {
        var closes = _prices.Select(p => (double)p.Close).ToArray();
        var rsi = CalculateRsi(closes, _period);
        Values = new double[closes.Length];

        for (int i = 0; i < _prices.Length; i++)
        {
            double rsiValue = rsi[i];
            if (rsiValue == 0)
            {
                Values[i] = 0;
                continue;
            }

            double baseScore = rsiValue > 70 ? -(rsiValue - 70) / 30
                             : rsiValue < 30 ? (30 - rsiValue) / 30
                             : 0;

            double avgVolume = _precomputedAvgVolumes?[i] ?? 0;
            double currentVolume = (double)_prices[i].Volume;
            double volumeFactor = avgVolume > 0 ? currentVolume / avgVolume : 1.0;
            volumeFactor = Math.Max(0.5, Math.Min(2.0, volumeFactor));

            Values[i] = Math.Max(-1.0, Math.Min(1.0, baseScore * volumeFactor));
        }
    }

    private static double[] CalculateRsi(double[] closes, int period)
    {
        int length = closes.Length;
        double[] rsi = new double[length];

        if (length < period + 1)
            return rsi;

        double avgGain = 0;
        double avgLoss = 0;

        for (int i = 1; i <= period; i++)
        {
            double delta = closes[i] - closes[i - 1];
            avgGain += Math.Max(delta, 0);
            avgLoss += Math.Abs(Math.Min(delta, 0));
        }

        avgGain /= period;
        avgLoss /= period;

        rsi[period] = avgLoss == 0 ? 100 : 100 - 100 / (1 + avgGain / avgLoss);

        for (int i = period + 1; i < length; i++)
        {
            double delta = closes[i] - closes[i - 1];
            double gain = Math.Max(delta, 0);
            double loss = Math.Abs(Math.Min(delta, 0));

            avgGain = (avgGain * (period - 1) + gain) / period;
            avgLoss = (avgLoss * (period - 1) + loss) / period;

            rsi[i] = avgLoss == 0 ? 100 : 100 - 100 / (1 + avgGain / avgLoss);
        }

        return rsi;
    }

    private static double[] ComputeRollingAverageVolumes(Ohlc[] prices, int lookback)
    {
        int length = prices.Length;
        if (length == 0)
            return [];

        double[] avgVolumes = new double[length];
        double runningSum = 0.0;

        for (int i = 0; i < length; i++)
        {
            if (i > 0)
            {
                int prevCount = Math.Min(i, lookback);
                avgVolumes[i] = prevCount > 0 ? runningSum / prevCount : 0.0;
            }

            runningSum += (double)prices[i].Volume;

            if (i >= lookback)
                runningSum -= (double)prices[i - lookback].Volume;
        }

        return avgVolumes;
    }
}