using Gimzo.Analysis.Technical.Charts;

namespace Gimzo.Analysis.Technical.Trends;

public class MacdTrend(Ohlc[] prices, int fastPeriod = 12, int slowPeriod = 26, int signalPeriod = 9, double[]? precomputedAvgVolumes = null) : PriceTrendBase(prices, precomputedAvgVolumes), ITrend
{
    private readonly int _fastPeriod = fastPeriod;
    private readonly int _slowPeriod = slowPeriod;
    private readonly int _signalPeriod = signalPeriod;

    public void Calculate()
    {
        var closes = _prices.Select(p => (double)p.Close).ToArray();
        var emaFast = CalculateEma(closes, _fastPeriod);
        var emaSlow = CalculateEma(closes, _slowPeriod);
        var macdLine = new double[_prices.Length];
        for (int i = 0; i < _prices.Length; i++)
            macdLine[i] = i < _slowPeriod - 1 ? 0 : emaFast[i] - emaSlow[i];
        var signalLine = CalculateEma(macdLine, _signalPeriod);
        for (int i = 0; i < _prices.Length; i++)
        {
            if (i < _slowPeriod + _signalPeriod - 2)
            {
                TrendValues[i] = 0;
                continue;
            }
            double histogram = macdLine[i] - signalLine[i];
            double baseScore = Math.Tanh(histogram / 100); // Normalize to [-1,1]; adjust divisor per asset
            double avgVolume = _precomputedAvgVolumes?[i] ?? 0;
            double currentVolume = (double)_prices[i].Volume;
            double volumeFactor = avgVolume > 0 ? currentVolume / avgVolume : 1.0;
            volumeFactor = Math.Max(0.5, Math.Min(2.0, volumeFactor));
            TrendValues[i] = Math.Max(-1.0, Math.Min(1.0, baseScore * volumeFactor));
        }
    }

    private static double[] CalculateEma(double[] values, int period)
    {
        double[] ema = new double[values.Length];
        double multiplier = 2.0 / (period + 1);
        ema[0] = values[0];
        for (int i = 1; i < values.Length; i++)
            ema[i] = (values[i] - ema[i - 1]) * multiplier + ema[i - 1];
        return ema;
    }
}