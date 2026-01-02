using Gimzo.Analysis.Technical.Charts;

namespace Gimzo.Analysis.Technical.Trends;

public class ExtremeTrend : PriceTrendBase, ITrend
{
    private readonly int _lookbackPeriod;
    private readonly double _alpha;
    private readonly double _beta;

    public ExtremeTrend(Ohlc[] prices, int lookbackPeriod = 20, double alpha = 0.5, double beta = 1.0, double[]? precomputedAvgVolumes = null)
        : base(prices, precomputedAvgVolumes)
    {
        if (alpha < 0 || alpha > 1D)
            throw new ArgumentOutOfRangeException(nameof(alpha));
        _lookbackPeriod = lookbackPeriod;
        _alpha = alpha;
        _beta = beta;
    }

    public void Calculate()
    {
        decimal[] highs = new decimal[_prices.Length];
        decimal[] lows = new decimal[_prices.Length];
        for (int i = 0; i < _prices.Length; i++)
        {
            if (i < _lookbackPeriod - 1)
            {
                highs[i] = _prices.Take(i + 1).Max(p => p.High);
                lows[i] = _prices.Take(i + 1).Min(p => p.Low);
            }
            else
            {
                highs[i] = _prices.Skip(i - _lookbackPeriod + 1).Take(_lookbackPeriod).Max(p => p.High);
                lows[i] = _prices.Skip(i - _lookbackPeriod + 1).Take(_lookbackPeriod).Min(p => p.Low);
            }
        }
        for (int i = 0; i < _prices.Length; i++)
        {
            if (i < _lookbackPeriod)
            {
                TrendValues[i] = 0D;
                continue;
            }
            var slope = CalculateRegressionSlope(i, _lookbackPeriod);
            var normalizedSlope = (2D / Math.PI) * Math.Atan(_beta * slope);
            var recentHigh = highs[i];
            var recentLow = lows[i];
            var range = recentHigh - recentLow;
            var position = range == 0M ? 0M : (_prices[i].Close - recentLow) / range;
            var scaledPosition = 2M * position - 1M;
            var baseTrendScore = _alpha * normalizedSlope + (1 - _alpha) * (double)scaledPosition;
            var avgVolume = _precomputedAvgVolumes?[i] ?? 0;
            var currentVolume = (double)_prices[i].Volume;
            var volumeFactor = avgVolume > 0 ? currentVolume / avgVolume : 1.0;
            volumeFactor = Math.Max(0.5, Math.Min(2.0, volumeFactor));
            TrendValues[i] = Math.Max(-1.0, Math.Min(1.0, baseTrendScore * volumeFactor));
        }
    }

    private double CalculateRegressionSlope(int endIndex, int lookbackPeriod)
    {
        decimal sumX = 0, sumY = 0, sumXY = 0, sumX2 = 0;
        int n = lookbackPeriod;
        for (int i = 0; i < n; i++)
        {
            int x = i;
            var y = _prices[endIndex - n + 1 + i].Close;
            sumX += x;
            sumY += y;
            sumXY += x * y;
            sumX2 += x * x;
        }
        var denominator = n * sumX2 - sumX * sumX;
        if (denominator == 0M)
            return 0.0;
        var slope = (n * sumXY - sumX * sumY) / denominator;
        return (double)slope;
    }
}