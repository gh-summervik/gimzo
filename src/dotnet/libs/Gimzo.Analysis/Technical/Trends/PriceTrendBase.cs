using Gimzo.Analysis.Technical.Charts;

namespace Gimzo.Analysis.Technical.Trends;

public abstract class PriceTrendBase
{
    protected readonly Ohlc[] _prices = [];
    protected readonly double[]? _precomputedAvgVolumes;

    public PriceTrendBase(Ohlc[] prices, double[]? precomputedAvgVolumes = null)
    {
        _prices = prices ?? throw new ArgumentNullException(nameof(prices));
        _precomputedAvgVolumes = precomputedAvgVolumes;
        TrendValues = new double[_prices.Length];
    }

    public double[] TrendValues { get; protected set; }

    public virtual string Name => GetType().Name;
}