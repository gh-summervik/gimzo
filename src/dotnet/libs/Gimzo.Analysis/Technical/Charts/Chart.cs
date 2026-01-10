using System.Collections.Immutable;

namespace Gimzo.Analysis.Technical.Charts;

public class Chart : IEquatable<Chart?>
{
    private readonly List<MovingAverage> _movingaverages = new(3);
    private readonly HashSet<MovingAverageKey> _movingAverageKeys = new(3);
    private BollingerBandConfiguration? _bollingerBandConfig;
    private readonly ChartInterval _interval = ChartInterval.Daily;
    private readonly string _symbol = "None";

    private decimal[] _averageHeights = [];
    private decimal[] _averageBodyHeights = [];
    private double[] _averageVolumes = [];

    private int? _averageTrueRangePeriod;
    private int? _relativeStrengthPeriod;

    public ChartInfo Info { get; private set; }
    //public double[] RelativeStrengthValues { get; private set; } = [];
    public Ohlc[] PriceActions { get; private set; } = [];
    public Candlestick[] Candlesticks { get; private set; } = [];
    public int Length => PriceActions.Length;
    public DateOnly Start => PriceActions[0].Date;
    public DateOnly End => PriceActions[^1].Date;
    public MovingAverage[] MovingAverages => [.. _movingaverages];
    public MovingAverageKey[] MovingAverageKeys => [.. _movingaverages.Select(k => k.Key)];
    public MovingAverage? GetMovingAverage(MovingAverageKey key) => _movingaverages.FirstOrDefault(k => k.Key.Equals(key));
    public PriceExtreme[] Extremes { get; private set; } = [];
    public AverageTrueRange? AverageTrueRange { get; private set; }
    public double[] AverageVolumes => _averageVolumes;
    public BollingerBand? BollingerBand { get; private set; }
    public RelativeStrengthIndex? RelativeStrengthIndex { get; private set; }

    public Chart(string symbol, ChartInterval interval = ChartInterval.Daily)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(symbol);
        _symbol = symbol;
        _interval = interval;
    }

    public Chart WithConfiguration(ChartConfiguration config)
    {
        if ((config.MovingAverages?.Length ?? 0) > 0)
            foreach (var code in config.MovingAverages!.Distinct())
            {
                var key = MovingAverageKey.Create(code) ?? throw new ArgumentException($"Invalid moving average shorthand: {code}");
                _movingAverageKeys.Add(key);
            }

        if (config.BollingerBand.HasValue)
            _bollingerBandConfig = config.BollingerBand.Value;

        if (config.AverageTrueRangePeriod.HasValue)
            _averageTrueRangePeriod = config.AverageTrueRangePeriod.Value;

        if (config.RelativeStrengthPeriod.HasValue)
            _relativeStrengthPeriod = config.RelativeStrengthPeriod.Value;

        return this;
    }

    public Chart WithMovingAverage(MovingAverageKey key)
    {
        _movingAverageKeys.Add(key);
        return this;
    }

    public Chart WithMovingAverage(int period, MovingAverageType type, PricePoint pricePoint = PricePoint.Close)
    {
        if (period < 1)
            throw new ArgumentException($"{nameof(period)} must be greater than zero.");

        var key = new MovingAverageKey(period, pricePoint, type);
        return WithMovingAverage(key);
    }

    public Chart WithMovingAverages(params MovingAverageKey[] keys)
    {
        foreach (var key in keys)
            _movingAverageKeys.Add(key);
        return this;
    }

    public Chart WithPriceActions(IEnumerable<Ohlc> priceActions)
    {
        PriceActions = [.. priceActions];
        if (PriceActions.Length < 1)
            throw new ArgumentException($"{nameof(priceActions)} must have at least one {nameof(Ohlc)}");
        return this;
    }

    public Chart WithAverageTrueRangePeriod(int period = 14)
    {
        if (period < 1)
            throw new ArgumentException($"Average True Range requires a period of ast least 1.");
        _averageTrueRangePeriod = period;
        return this;
    }

    public Chart WithBollingerBand(int period = 21,
        MovingAverageType movingAverageType = MovingAverageType.Simple,
        PricePoint pricePoint = PricePoint.Close, double stdDevMultiplier = 2.0)
    {
        if (period < 1)
            throw new ArgumentException("Bollinger bands require a period of at least 1.");
        if (stdDevMultiplier < 1D || stdDevMultiplier > 3D)
            throw new ArgumentException("Standard deviation multipler requires a value between 1 and 3");

        var key = new MovingAverageKey(period, pricePoint, movingAverageType);
        _bollingerBandConfig = new()
        {
            MovingAverageCode = key.CreateShorthand(),
            StdDevMultipler = stdDevMultiplier,
        };

        return this;
    }

    public Chart WithBollingerBand(MovingAverageKey key, double stdDevMultiplier = 2.0)
    {
        if (key.Period < 1)
            throw new ArgumentException("Moving average key requires a period of at least 1.");

        _bollingerBandConfig = new()
        {
            MovingAverageCode = key.CreateShorthand(),
            StdDevMultipler = stdDevMultiplier,
        };
        return this;
    }

    public Chart WithRelativeStrengthPeriod(int period = 14)
    {
        if (period < 1)
            throw new ArgumentException("Relative strength index requires a period of at least 1.");

        _relativeStrengthPeriod = period;
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

        if (_averageTrueRangePeriod.HasValue)
            AverageTrueRange = new(PriceActions, _averageTrueRangePeriod.Value);

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

                highs.Add(new PriceExtreme(PriceActions[p].High, isHigh: true, isLow: false, sentiment, p, PriceActions[p].Date));
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

                lows.Add(new PriceExtreme(PriceActions[p].Low, isHigh: false, isLow: true, sentiment, p, PriceActions[p].Date));
                lastLow = low;
                lastLowSentiment = sentiment;
            }
        }

        Extremes = [.. highs.Union(lows).OrderBy(k => k.Index)];

        if (_bollingerBandConfig.HasValue)
        {
            var bb = _bollingerBandConfig.Value;
            var key = MovingAverageKey.Create(bb.MovingAverageCode).GetValueOrDefault();
            var values = PriceActions.Select(p => p.GetPricePoint(key.PricePoint)).ToArray();
            BollingerBand = new BollingerBand(bb, values);
        }

        if (_relativeStrengthPeriod.HasValue && _relativeStrengthPeriod.Value > 0)
            RelativeStrengthIndex = new(PriceActions, _relativeStrengthPeriod.Value,
                [.. PriceActions.Select(k => k.Volume)]);

        Info = new ChartInfo()
        {
            Symbol = _symbol,
            Interval = _interval,
            Start = PriceActions[0].Date,
            Finish = PriceActions[^1].Date,
            Length = PriceActions.Length
        };

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

    public int GetIndexOfDate(DateOnly date)
    {
        var ohlc = PriceActions.FirstOrDefault(p => p.Date.Equals(date));
        if (ohlc is not null)
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

        if (left is null)
            return right;
        if (right is null)
            return left;

        int distLeft = Math.Abs(date.DayNumber - left.Value.DayNumber);
        int distRight = Math.Abs(date.DayNumber - right.Value.DayNumber);

        return distLeft <= distRight ? left : right;
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

    public override bool Equals(object? obj) => Equals(obj as Chart);

    public bool Equals(Chart? other) => other is not null &&
        Info.Equals(other.Info) &&
        Length == other.Length &&
        Start.Equals(other.Start) &&
        End.Equals(other.End);

    public override int GetHashCode() => Info.GetHashCode();
}