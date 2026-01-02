using Gimzo.Analysis.Technical.Charts;

namespace Gimzo.Analysis.Technical.Trends;

public class GimzoTrend(Ohlc[] prices) : PriceTrendBase(prices, PrecomputeAverageVolumes(prices, 20)), ITrend
{
    public void Calculate()
    {
        MovingAverageKey[] maKeys = [
            new MovingAverageKey(21, PricePoint.Close),
            new MovingAverageKey(50, PricePoint.Close),
            new MovingAverageKey(200, PricePoint.Close)
        ];

        MovingAverage[] mas = [.. maKeys.Select(k => new MovingAverage(k, _prices.Select(p => p.GetPricePoint(k.PricePoint)).ToArray()))];

        double[] weights = [0.20, 0.18, 0.15, 0.12, 0.10, 0.09, 0.08, 0.08];

        WeightedTrend[] trends = [
            new WeightedTrend(new MacdTrend(_prices, precomputedAvgVolumes: _precomputedAvgVolumes), weights[0]),
            new WeightedTrend(new RsiTrend(_prices, precomputedAvgVolumes: _precomputedAvgVolumes), weights[1]),
            new WeightedTrend(new ExtremeTrend(_prices, precomputedAvgVolumes: _precomputedAvgVolumes), weights[2]),
            new WeightedTrend(new PriceToMovingAverageTrend(maKeys[0], _prices, precomputedAvgVolumes: _precomputedAvgVolumes), weights[3]),
            new WeightedTrend(new PriceToMovingAverageTrend(maKeys[1], _prices, precomputedAvgVolumes: _precomputedAvgVolumes), weights[4]),
            new WeightedTrend(new MultipleMovingAverageTrend(_prices, maKeys, precomputedAvgVolumes: _precomputedAvgVolumes), weights[5]),
            new WeightedTrend(new PriceToMovingAverageTrend(maKeys[2], _prices, precomputedAvgVolumes: _precomputedAvgVolumes), weights[6]),
            new WeightedTrend(new CandlestickTrend(_prices, precomputedAvgVolumes: _precomputedAvgVolumes), weights[7])
        ];
        var combined = new CombinedWeightedTrend(trends);
        combined.Calculate();
        TrendValues = combined.TrendValues;
    }

    private static double[] PrecomputeAverageVolumes(Ohlc[] prices, int lookback)
    {
        if (prices.Length == 0)
            return [];

        double[] avgVolumes = new double[prices.Length];
        double runningSum = 0;
        for (int i = 0; i < prices.Length; i++)
        {
            runningSum += (double)prices[i].Volume;
            if (i >= lookback)
                runningSum -= (double)prices[i - lookback].Volume;
            avgVolumes[i] = i < lookback - 1 ? (i == 0 ? 0 : runningSum / (i + 1)) : runningSum / lookback;
        }
        return avgVolumes;
    }
}