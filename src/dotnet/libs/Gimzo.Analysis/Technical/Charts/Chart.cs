using Gimzo.Analysis.Technical.Trends;

namespace Gimzo.Analysis.Technical.Charts;

public class Chart : IEquatable<Chart?>
{
    private readonly List<MovingAverage> _movingaverages = new(3);
    private readonly HashSet<MovingAverageKey> _movingAverageKeys = new(3);
    private decimal[] _averageHeights = [];
    private decimal[] _averageBodyHeights = [];
    private double[] _averageVolumes = [];
    private TrendSentiment[] _lookbackSentiment = [];
    private readonly int _lookbackLength = 15;
    private readonly List<AverageTrueRange> _atrs = [];
    private readonly HashSet<int> _atrPeriods = [];

    public ChartInfo Info { get; init; }
    public Chart(string name, ChartInterval interval = ChartInterval.Daily,
        int lookbackLength = 15)
    {
        Info = new()
        {
            Symbol = name,
            Interval = interval
        };

        _lookbackLength = Math.Max(lookbackLength, 0);
    }

    private ITrend? Trend { get; set; } = null;
    public double[] TrendValues => Trend?.TrendValues ??
        [.. Enumerable.Repeat(0D, PriceActions.Length)];
    public Ohlc[] PriceActions { get; private set; } = [];
    public Candlestick[] Candlesticks { get; private set; } = [];
    public int Length => PriceActions.Length;
    public int Duration => End.DayNumber - Start.DayNumber;
    public DateOnly Start => PriceActions[0].Date;
    public DateOnly End => PriceActions[^1].Date;
    public MovingAverage[] MovingAverages => [.. _movingaverages];
    public AverageTrueRange[] ATRs => [.. _atrs];

    public Chart WithMovingAverage(MovingAverageKey key)
    {
        _movingAverageKeys.Add(key);
        return this;
    }

    public Chart WithMovingAverage(int period, MovingAverageType type, PricePoint pricePoint = PricePoint.Close)
    {
        var key = new MovingAverageKey(period, pricePoint, type);
        return WithMovingAverage(key);
    }

    public Chart WithMovingAverages(params MovingAverageKey[] keys)
    {
        foreach (var key in keys)
        {
            _movingAverageKeys.Add(key);
        }
        return this;
    }

    public Chart WithPriceActions(IEnumerable<Ohlc> priceActions)
    {
        PriceActions = [.. priceActions];
        return this;
    }

    public Chart WithCandles(IEnumerable<Ohlc> priceActions)
    {
        return WithPriceActions(priceActions);
    }

    public Chart WithTrend(ITrend trend)
    {
        Trend = trend;
        return this;
    }

    public Chart WithAverageTrueRange(int period = 14)
    {
        _atrPeriods.Add(period);
        return this;
    }

    public Chart WithAverageTrueRanges(params int[] periods)
    {
        foreach (var period in periods)
            _atrPeriods.Add(period);
        return this;
    }

    public Chart Build()
    {
        if (PriceActions.Length < 1)
            throw new Exception($"Cannot construct a chart with {PriceActions.Length} price actions.");

        Candlesticks = [.. PriceActions.Select(p => new Candlestick(p))];

        _movingaverages.Clear();
        foreach (var key in _movingAverageKeys)
            _movingaverages.Add(new MovingAverage(key, PriceActions));

        Trend?.Calculate();

        _averageHeights = new decimal[PriceActions.Length];
        _averageBodyHeights = new decimal[PriceActions.Length];
        _averageVolumes = new double[PriceActions.Length];
        _lookbackSentiment = new TrendSentiment[PriceActions.Length];

        for (int p = 0; p < PriceActions.Length; p++)
        {
            if (_lookbackLength > 0 && p > _lookbackLength)
            {
                var lookback = Candlesticks[(p - _lookbackLength - 1)..(p)];
                _lookbackSentiment[p] = lookback.All(pr => pr.High < Candlesticks[p].High)
                    ? TrendSentiment.Bullish
                    : lookback.All(pr => pr.Low > Candlesticks[p].Low)
                    ? TrendSentiment.Bearish
                    : TrendSentiment.Neutral;
            }
            else
                _lookbackSentiment[p] = TrendSentiment.Neutral;

            if (p == 0)
            {
                _averageHeights[p] = Candlesticks[p].Length;
                _averageBodyHeights[p] = Candlesticks[p].Body.Length;
                _averageVolumes[p] = Candlesticks[p].Volume;
            }
            else
            {
                _averageHeights[p] = _averageHeights[p - 1] + (Candlesticks[p].Length - _averageHeights[p - 1]) / (p + 1);
                _averageBodyHeights[p] = _averageBodyHeights[p - 1] + (Candlesticks[p].Body.Length - _averageBodyHeights[p - 1]) / (p + 1);
                _averageVolumes[p] = Math.Ceiling(_averageVolumes[p - 1] + (Candlesticks[p].Volume - _averageVolumes[p - 1]) / (p + 1));
            }
        }

        _atrs.Clear();
        foreach (var period in _atrPeriods)
            _atrs.Add(new AverageTrueRange(PriceActions, period));

        return this;
    }

    public AverageTrueRange? GetAverageTrueRangeForPeriod(int period) => _atrs.FirstOrDefault(k => k.Period == period);

    public int[] GetAverageTrueRangePeriods() => [.. _atrs.Select(k => k.Period)];

    public int GetIndexOfDate(DateOnly date)
    {
        var ohlc = PriceActions.FirstOrDefault(p => p.Date.Equals(date));
        if (ohlc != null)
            return Array.IndexOf(PriceActions, ohlc);
        return -1;
    }

    public DateOnly? GetNearestDate(DateOnly date)
    {
        if (PriceActions.Length == 0)
            return null;

        var first = PriceActions[0].Date;
        var last = PriceActions[^1].Date;

        if (date < first)
            return first;
        if (date > last)
            return last;

        // Binary search for exact match or insertion point.
        int low = 0;
        int high = PriceActions.Length - 1;
        while (low <= high)
        {
            int mid = low + (high - low) / 2;
            var midDate = PriceActions[mid].Date;
            if (midDate == date)
                return midDate;
            if (midDate < date)
                low = mid + 1;
            else
                high = mid - 1;
        }

        // Check closest between high (left) and low (right).
        DateOnly? left = high >= 0 ? PriceActions[high].Date : null;
        DateOnly? right = low < PriceActions.Length ? PriceActions[low].Date : null;

        if (left == null)
            return right;
        if (right == null)
            return left;

        int distLeft = Math.Abs(date.DayNumber - left.Value.DayNumber);
        int distRight = Math.Abs(date.DayNumber - right.Value.DayNumber);

        return distLeft <= distRight ? left : right;
    }

    public bool IsTall(int position, int lookbackPeriod = 0, decimal tolerance = 1M)
    {
        if (position < 10)
        {
            return false;
        }

        tolerance = Math.Max(1M, tolerance);

        var lookbackPosition = lookbackPeriod == 0 ? 0 : Math.Max(position - lookbackPeriod, 0);

        if (lookbackPosition == 0)
        {
            return Candlesticks[position].Body.Length > tolerance * _averageBodyHeights[position - 1];
        }
        else
        {
            var avg = Candlesticks[lookbackPosition..(position - 1)].Average(c => c.Body.Length);
            return Candlesticks[position].Body.Length > tolerance * avg;
        }
    }

    public bool IsShort(int position, int lookbackPeriod = 0, decimal tolerance = 1M)
    {
        if (position < 10)
        {
            return false;
        }

        tolerance = Math.Min(1M, tolerance);

        var lookbackPosition = lookbackPeriod == 0 ? 0 : Math.Max(position - lookbackPeriod, 0);

        if (lookbackPeriod == 0)
        {
            return Candlesticks[position].Body.Length < tolerance * _averageBodyHeights[position - 1];
        }
        else
        {
            var avg = Candlesticks[lookbackPosition..(position - 1)].Select(c => c.Body.Length).Average();
            return Candlesticks[position].Body.Length < tolerance * avg;
        }
    }

    public ChartSpan GetSpan(int start, int finish)
    {
        if (start > finish)
            (start, finish) = (finish, start);

        if (start < 0 || start >= this.Length)
            throw new ArgumentOutOfRangeException(nameof(start));

        if (finish < 0 || finish >= this.Length)
            throw new ArgumentOutOfRangeException(nameof(finish));

        return new ChartSpan(this, start, finish);
    }

    public TrendSentiment LookbackSentiment(int position) => position > -1 && position < _lookbackSentiment.Length
        ? _lookbackSentiment[position]
        : TrendSentiment.Neutral;

    public override bool Equals(object? obj)
    {
        return Equals(obj as Chart);
    }

    public bool Equals(Chart? other)
    {
        return other is not null &&
               _lookbackLength == other._lookbackLength &&
               Info.Interval == other.Info.Interval &&
               Info.Symbol == other.Info.Symbol &&
               Length == other.Length &&
               Start.Equals(other.Start) &&
               End.Equals(other.End);
    }

    public static int GetCacheKey(ChartInfo chartInfo,
        string? trend = null, int lookbackLength = 15)
    {
        return HashCode.Combine(chartInfo, trend, lookbackLength);
    }

    public override int GetHashCode()
    {
        return GetCacheKey(Info, Trend?.Name, _lookbackLength);
    }
}