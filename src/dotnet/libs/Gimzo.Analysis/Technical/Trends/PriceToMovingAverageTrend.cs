using Gimzo.Analysis.Technical.Charts;

namespace Gimzo.Analysis.Technical.Trends;

public class PriceToMovingAverageTrend : PriceTrendBase, ITrend
{
    private readonly MovingAverage _movingAverage;
    private readonly int _lookbackPeriod;
    private readonly double _alpha;
    private readonly double _beta;
    private readonly double _gamma;

    public override string Name => _movingAverage.Key.ToString();

    public PriceToMovingAverageTrend(MovingAverageKey movingAverageKey, Ohlc[] prices,
        int? lookbackPeriod = null, double alpha = 0.5, double beta = 1.0, double gamma = 1.0, double[]? precomputedAvgVolumes = null)
        : base(prices, precomputedAvgVolumes)
    {
        if (alpha < 0 || alpha > 1)
            throw new ArgumentOutOfRangeException(nameof(alpha), "Alpha must be between 0 and 1.");
        _movingAverage = new MovingAverage(movingAverageKey,
            prices.Select(p => p.GetPricePoint(movingAverageKey.PricePoint)).ToArray());
        _lookbackPeriod = lookbackPeriod ?? movingAverageKey.Period;
        _alpha = alpha;
        _beta = beta;
        _gamma = gamma;
    }

    public void Calculate()
    {
        int period = _movingAverage.Key.Period;
        for (int t = 0; t < _prices.Length; t++)
        {
            if (t < period - 1)
            {
                TrendValues[t] = 0.0;
                continue;
            }
            int maStart = Math.Max(0, t - _lookbackPeriod + 1);
            int maLength = t - maStart + 1;
            var maSegment = _movingAverage.Values.Skip(maStart).Take(maLength)
                .Select(v => (double)v).ToArray();
            double stdDevMA = CalculateStandardDeviation(maSegment);
            double z = stdDevMA == 0 ? 0.0 : (double)(_prices[t].Close - _movingAverage.Values[t]) / stdDevMA;
            double normalizedZ = (2.0 / Math.PI) * Math.Atan(_gamma * z);
            double slope = CalculateRegressionSlope(maSegment, 0, maLength);
            double normalizedSlope = (2.0 / Math.PI) * Math.Atan(_beta * slope);
            double baseTrendScore = _alpha * normalizedSlope + (1 - _alpha) * normalizedZ;
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

    private static double CalculateRegressionSlope(double[] values, int startIndex, int length)
    {
        if (length <= 1)
            return 0.0;
        double sumX = 0, sumY = 0, sumXY = 0, sumX2 = 0;
        for (int i = 0; i < length; i++)
        {
            double x = i;
            double y = values[startIndex + i];
            sumX += x;
            sumY += y;
            sumXY += x * y;
            sumX2 += x * x;
        }
        double denominator = length * sumX2 - sumX * sumX;
        if (denominator == 0)
            return 0.0;
        return (length * sumXY - sumX * sumY) / denominator;
    }
}