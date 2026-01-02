using Gimzo.Analysis.Technical.Charts;

namespace Gimzo.Analysis.Technical.Trends;

public class RsiTrend(Ohlc[] prices, int period = 14, double[]? precomputedAvgVolumes = null) : PriceTrendBase(prices, precomputedAvgVolumes), ITrend
{
    private readonly int _period = period;

    public void Calculate()
    {
        var closes = _prices.Select(p => (double)p.Close).ToArray();
        var rsi = CalculateRsi(closes, _period);
        for (int i = 0; i < _prices.Length; i++)
        {
            if (i < _period)
            {
                TrendValues[i] = 0;
                continue;
            }
            // Normalize RSI to [-1,1]: >70 overbought (bearish), <30 oversold (bullish)
            double baseScore = rsi[i] > 70 ? -(rsi[i] - 70) / 30 : rsi[i] < 30 ? (30 - rsi[i]) / 30 : 0;
            double avgVolume = _precomputedAvgVolumes?[i] ?? 0;
            double currentVolume = (double)_prices[i].Volume;
            double volumeFactor = avgVolume > 0 ? currentVolume / avgVolume : 1.0;
            volumeFactor = Math.Max(0.5, Math.Min(2.0, volumeFactor));
            TrendValues[i] = Math.Max(-1.0, Math.Min(1.0, baseScore * volumeFactor));
        }
    }

    private static double[] CalculateRsi(double[] closes, int period)
    {
        double[] rsi = new double[closes.Length];
        double avgGain = 0, avgLoss = 0;
        for (int i = 1; i < period; i++)
        {
            double delta = closes[i] - closes[i - 1];
            if (delta > 0)
                avgGain += delta;
            else
                avgLoss -= delta;
        }
        avgGain /= period;
        avgLoss /= period;
        rsi[period - 1] = avgLoss == 0 ? 100 : 100 - (100 / (1 + avgGain / avgLoss));
        for (int i = period; i < closes.Length; i++)
        {
            double delta = closes[i] - closes[i - 1];
            double gain = Math.Max(delta, 0);
            double loss = Math.Abs(Math.Min(delta, 0));
            avgGain = (avgGain * (period - 1) + gain) / period;
            avgLoss = (avgLoss * (period - 1) + loss) / period;
            rsi[i] = avgLoss == 0 ? 100 : 100 - (100 / (1 + avgGain / avgLoss));
        }
        return rsi;
    }
}