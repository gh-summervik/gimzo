using Gimzo.Analysis.Technical.Trends;

namespace Gimzo.Analysis.Technical.Charts;

public class Chart : IEquatable<Chart?>
{
    private readonly List<MovingAverage> _movingaverages = new(3);
    private readonly HashSet<MovingAverageKey> _movingAverageKeys = new(3);
    private decimal[] _averageHeights = [];
    private decimal[] _averageBodyHeights = [];
    private double[] _averageVolumes = [];
    private readonly List<AverageTrueRange> _atrs = [];
    private readonly HashSet<int> _atrPeriods = [];

    public ChartInfo Info { get; init; }
    public Chart(string name, ChartInterval interval = ChartInterval.Daily)
    {
        Info = new()
        {
            Symbol = name,
            Interval = interval
        };
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
    public MovingAverageKey[] MovingAverageKeys => [.. _movingaverages.Select(k => k.Key)];
    public MovingAverage? GetMovingAverage(MovingAverageKey key) =>
        _movingaverages.FirstOrDefault(k => k.Key.Equals(key));

    public PriceExtreme[] Extremes { get; private set; } = [];
    public AverageTrueRange[] ATRs => [.. _atrs];
    public double[] AverageVolumes => _averageVolumes;
    public BollingerBand? BollingerBand { get; private set; }    
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

    public Chart WithBollingerBand(int period = 21,
        MovingAverageType movingAverageType = MovingAverageType.Simple,
        PricePoint pricePoint = PricePoint.Close, double stdDevMultiplier = 2.0)
    {
        var key = new MovingAverageKey(period, pricePoint, movingAverageType);
        return WithBollingerBand(key, stdDevMultiplier);
    }

    public Chart WithBollingerBand(MovingAverageKey key, double stdDevMultiplier = 2.0)
    {
        BollingerBand = new BollingerBand(key, stdDevMultiplier, [.. PriceActions.Select(p => p.Close)]);
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

        for (int p = 0; p < PriceActions.Length; p++)
        {
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

        List<PriceExtreme> highs = new(100);
        List<PriceExtreme> lows = new(100);

        /*
         * Completing this loop twice because a given candle can be both a low and a high.
         * Mixing them together was just a PITA.
         */
        decimal lastHigh = 0M;
        decimal lastLow = 0M;
        TrendSentiment lastLowSentiment = TrendSentiment.None;
        TrendSentiment lastHighSentiment = TrendSentiment.None;

        for (int p = 1; p < PriceActions.Length - 1; p++)
        {
            decimal high = PriceActions[p].High;

            /* 
             * Address situation wherein we hit a new high and match it on 2 or more days.
             * We only want to count the final entry as the high.
             */
            decimal prev = PriceActions[p - 1].High;
            while (high > PriceActions[p - 1].High && high == PriceActions[p + 1].High && p < PriceActions.Length - 2)
                p++;

            if (high > prev && high > PriceActions[p + 1].High)
            {
                TrendSentiment sentiment = TrendSentiment.None;

                if (lastHighSentiment == TrendSentiment.None)
                    sentiment = TrendSentiment.Bullish;
                else if (high > lastHigh)
                    sentiment = TrendSentiment.Bullish;
                else if (high < lastHigh)
                    sentiment = TrendSentiment.Bearish;
                else
                    sentiment = lastHighSentiment;

                highs.Add(new PriceExtreme(PriceActions[p].High, isHigh: true, isLow: false, sentiment, p));
                lastHigh = high;
                lastHighSentiment = sentiment;
            }
        }

        for (int p = 1; p < PriceActions.Length - 1; p++)
        {

            decimal low = PriceActions[p].Low;

            /* 
             * See note in loop above.
             */
            decimal prev = PriceActions[p - 1].Low;
            while (low < PriceActions[p - 1].Low && low == PriceActions[p + 1].Low && p < PriceActions.Length - 2)
                p++;

            if (low < prev && low < PriceActions[p + 1].Low)
            {
                TrendSentiment sentiment = TrendSentiment.None;

                if (lastLowSentiment == TrendSentiment.None)
                    sentiment = TrendSentiment.Bearish;
                else if (low < lastLow)
                    sentiment = TrendSentiment.Bearish;
                else if (low > lastLow)
                    sentiment = TrendSentiment.Bullish;
                else
                    sentiment = lastLowSentiment;

                lows.Add(new PriceExtreme(PriceActions[p].Low, isHigh: false, isLow: true, sentiment, p));
                lastLow = low;
                lastLowSentiment = sentiment;
            }
        }

        Extremes = [.. highs.Union(lows).OrderBy(k => k.Index)];

        return this;
    }

    public PriceExtreme? FindPreviousHigh(int index, TrendSentiment sentiment = TrendSentiment.None)
    {
        var prev = Extremes.LastOrDefault(k => k.Index < index && k.IsHigh &&
            (sentiment == TrendSentiment.None || k.Sentiment == sentiment));
        return prev.IsDefault ? null : prev;
    }

    public PriceExtreme? FindPreviousLow(int index, TrendSentiment sentiment = TrendSentiment.None)
    {
        var prev = Extremes.LastOrDefault(k => k.Index < index && k.IsLow &&
            (sentiment == TrendSentiment.None || k.Sentiment == sentiment));
        return prev.IsDefault ? null : prev;
    }

    public PriceExtreme? FindPreviousHigh(PriceExtreme extreme, TrendSentiment sentiment = TrendSentiment.None)
    {
        int idx = Extremes.IndexOf(extreme);
        if (idx < 1)
            return null;
        for (int i = idx - 1; i >= 0; i--)
        {
            if (Extremes[i].IsHigh && (sentiment == TrendSentiment.None || Extremes[i].Sentiment == sentiment))
                return Extremes[i];
        }
        return null;
    }

    public PriceExtreme? FindPreviousLow(PriceExtreme extreme, TrendSentiment sentiment = TrendSentiment.None)
    {
        int idx = Extremes.IndexOf(extreme);
        if (idx < 1)
            return null;
        for (int i = idx - 1; i >= 0; i--)
        {
            if (Extremes[i].IsLow && (sentiment == TrendSentiment.None || Extremes[i].Sentiment == sentiment))
                return Extremes[i];
        }
        return null;
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

    public override bool Equals(object? obj)
    {
        return Equals(obj as Chart);
    }

    public bool Equals(Chart? other)
    {
        return other is not null &&
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
        return GetCacheKey(Info, Trend?.Name);
    }
}