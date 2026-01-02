using Gimzo.Analysis.Technical.Charts;

namespace Gimzo.Analysis.Technical.Trends;

public class MultipleMovingAverageTrend : PriceTrendBase, ITrend
{
    private readonly List<MovingAverage> _movingAverages;
    private readonly int _lookbackPeriod;
    private readonly double _gamma;

    public MultipleMovingAverageTrend(Ohlc[] prices, MovingAverageKey[] movingAverageKeys,
        int lookbackPeriod = 20, double gamma = 1.0, double[]? precomputedAvgVolumes = null) : base(prices, precomputedAvgVolumes)
    {
        if (movingAverageKeys.Length < 2)
            throw new ArgumentException("At least two moving average keys are required.");
        var uniqueKeys = movingAverageKeys.Distinct().OrderBy(k => k.Period).ToArray();
        if (uniqueKeys.Length < 2)
            throw new ArgumentException("At least two unique moving average keys are required.");
        _movingAverages = [];
        foreach (var key in uniqueKeys)
            _movingAverages.Add(new MovingAverage(key,
                prices.Select(p => p.GetPricePoint(key.PricePoint)).ToArray()));
        _lookbackPeriod = lookbackPeriod;
        _gamma = gamma;
    }

    public void Calculate()
    {
        int numPairs = _movingAverages.Count - 1;
        for (int t = 0; t < _prices.Length; t++)
        {
            if (t < _lookbackPeriod)
            {
                TrendValues[t] = 0.0;
                continue;
            }
            double totalScore = 0.0;
            for (int p = 0; p < numPairs; p++)
            {
                var maFast = _movingAverages[p].Values;
                var maSlow = _movingAverages[p + 1].Values;
                var differences = new double[_lookbackPeriod];
                for (int i = 0; i < _lookbackPeriod; i++)
                {
                    int index = t - _lookbackPeriod + 1 + i;
                    differences[i] = (double)(maFast[index] - maSlow[index]);
                }
                double stdDevDiff = CalculateStandardDeviation(differences);
                double diff_t = (double)(maFast[t] - maSlow[t]);
                double z_t = stdDevDiff == 0 ? 0.0 : diff_t / stdDevDiff;
                double scorePair = (2.0 / Math.PI) * Math.Atan(_gamma * z_t);
                totalScore += scorePair;
            }
            double baseTrendScore = totalScore / numPairs;
            double avgVolume = _precomputedAvgVolumes?[t] ?? 0;
            double currentVolume = (double)_prices[t].Volume;
            double volumeFactor = avgVolume > 0 ? currentVolume / avgVolume : 1.0;
            volumeFactor = Math.Max(0.5, Math.Min(2.0, volumeFactor));
            double adjustedTrendScore = baseTrendScore * volumeFactor;
            TrendValues[t] = Math.Max(-1.0, Math.Min(1.0, adjustedTrendScore));
        }
    }

    private static double CalculateStandardDeviation(double[] values)
    {
        if (values.Length <= 1)
            return 0.0;
        double mean = values.Average();
        double sumOfSquares = values.Sum(v => (v - mean) * (v - mean));
        return Math.Sqrt(sumOfSquares / values.Length);
    }
}