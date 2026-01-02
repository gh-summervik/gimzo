using Gimzo.Analysis.Technical.Charts;

namespace Gimzo.Analysis.Technical.Trends;

public class CandlestickTrend : PriceTrendBase, ITrend
{
    private readonly int _lookbackPeriod;
    private readonly double _weightColor;
    private readonly double _weightWick;
    private readonly double _weightPattern;
    private readonly double _weightSlope;
    private readonly double _weightVolume;

    public CandlestickTrend(Ohlc[] prices, int lookbackPeriod = 20,
        double weightColor = 0.2, double weightWick = 0.2, double weightPattern = 0.2,
        double weightSlope = 0.2, double weightVolume = 0.2, double[]? precomputedAvgVolumes = null) : base(prices, precomputedAvgVolumes)
    {
        _lookbackPeriod = lookbackPeriod;
        _weightColor = weightColor;
        _weightWick = weightWick;
        _weightPattern = weightPattern;
        _weightSlope = weightSlope;
        _weightVolume = weightVolume;
        double totalWeight = _weightColor + _weightWick + _weightPattern + _weightSlope + _weightVolume;
        if (Math.Abs(totalWeight - 1.0) > 0.0001)
            throw new ArgumentException("Weights must sum to 1.");
    }

    public void Calculate()
    {
        for (int t = 0; t < _prices.Length; t++)
        {
            if (t < _lookbackPeriod)
            {
                TrendValues[t] = 0.0;
                continue;
            }
            var candle = new Candlestick(_prices[t]);
            double score = 0.0;
            score += _weightColor * (candle.Color == CandlestickColor.Light ? 1.0 :
                                     candle.Color == CandlestickColor.Dark ? -1.0 : 0.0);
            double wickScore = 0.0;
            if (candle.UpperShadow.Length > candle.LowerShadow.Length)
                wickScore -= 1.0;
            else if (candle.LowerShadow.Length > candle.UpperShadow.Length)
                wickScore += 1.0;
            score += _weightWick * wickScore;
            double patternScore = GetPatternScore(candle);
            score += _weightPattern * patternScore;
            double slope = CalculateSlope(t);
            score += _weightSlope * (slope > 0 ? 1.0 : slope < 0 ? -1.0 : 0.0);
            double avgVolume = _precomputedAvgVolumes?[t] ?? 0;
            double currentVolume = (double)_prices[t].Volume;
            double volumeRatio = avgVolume == 0 ? 1.0 : currentVolume / avgVolume;
            double volumeFactor = Math.Max(0.5, Math.Min(2.0, volumeRatio));
            score *= volumeFactor;
            TrendValues[t] = Math.Max(-1.0, Math.Min(1.0, score));
        }
    }

    private static double GetPatternScore(Candlestick candle)
    {
        if (candle.IsBullishMarubozu)
            return 1.0;
        if (candle.IsBearishMarubozu)
            return -1.0;
        if (candle.IsBullishBelthold)
            return 0.8;
        if (candle.IsBearishBelthold)
            return -0.8;
        if (candle.IsDoji)
            return 0.0;
        if (candle.IsDragonflyDoji)
            return 0.5;
        if (candle.IsGravestoneDoji)
            return -0.5;
        if (candle.IsSpinningTop)
            return 0.0;
        if (candle.IsUmbrella)
            return 0.6;
        if (candle.IsInvertedUmbrella)
            return -0.6;
        return 0.0;
    }

    private double CalculateSlope(int endIndex)
    {
        if (endIndex < _lookbackPeriod)
            return 0.0;
        double sumX = 0, sumY = 0, sumXY = 0, sumX2 = 0;
        for (int i = 0; i < _lookbackPeriod; i++)
        {
            int idx = endIndex - _lookbackPeriod + 1 + i;
            double x = i;
            double y = (double)_prices[idx].Close;
            sumX += x;
            sumY += y;
            sumXY += x * y;
            sumX2 += x * x;
        }
        double denominator = _lookbackPeriod * sumX2 - sumX * sumX;
        if (denominator == 0)
            return 0.0;
        return (_lookbackPeriod * sumXY - sumX * sumY) / denominator;
    }
}