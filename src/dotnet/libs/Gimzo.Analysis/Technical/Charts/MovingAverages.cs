using Gimzo.Common;
using System.ComponentModel;
using System.Text.RegularExpressions;

namespace Gimzo.Analysis.Technical.Charts;

public enum MovingAverageType
{
    [Description("SMA")]
    Simple = 0,
    [Description("EMA")]
    Exponential = 1
}

public readonly struct MovingAverageKey : IEquatable<MovingAverageKey>
{
    public int Period { get; }
    public MovingAverageType Type { get; }
    public PricePoint PricePoint { get; }

    public MovingAverageKey(int period, PricePoint pricePoint = PricePoint.Close,
        MovingAverageType type = MovingAverageType.Simple)
    {
        if (period < 1)
            throw new ArgumentOutOfRangeException(nameof(period), $"{nameof(period)} cannot be less than 1.");
        Period = period;
        Type = type;
        PricePoint = pricePoint;
    }

    public override bool Equals(object? obj) => obj is MovingAverageKey key && Equals(key);

    public bool Equals(MovingAverageKey other) => Period == other.Period &&
               Type == other.Type &&
               PricePoint == other.PricePoint;

    public override int GetHashCode() => HashCode.Combine(Period, Type, PricePoint);

    public override readonly string ToString() =>
        $"{Type.GetEnumDescription()[0]}{Period}{PricePoint.GetEnumDescription()[0]}";

    public static bool operator ==(MovingAverageKey left, MovingAverageKey right) => left.Equals(right);

    public static bool operator !=(MovingAverageKey left, MovingAverageKey right) => !(left == right);

    public string CreateShorthand()
    {
        string type = Type switch
        {
            MovingAverageType.Simple => "S",
            MovingAverageType.Exponential => "E",
            _ => throw new Exception($"Logic error: Unknown moving average type: {Type.GetEnumDescription()}")
        };
        string pp = PricePoint switch
        {
            PricePoint.Open => "O",
            PricePoint.Close => "C",
            PricePoint.High => "H",
            PricePoint.Low => "L",
            PricePoint.MidPoint => "M",
            _ => throw new Exception($"Logic error: Unknown moving average type: {PricePoint.GetEnumDescription()}")
        };

        return $"{type}{Period}{pp}";
    }

    public static MovingAverageKey? Create(string shorthand)
    {
        string pattern = @"([SE])(\d+)([OHLCM])";

        MatchCollection matches = Regex.Matches(shorthand, pattern);
        if (matches.Count > 0)
        {
            var match = matches[0];
            var pricePoint = match.Groups[3].Value.ToUpperInvariant() switch
            {
                "O" => PricePoint.Open,
                "H" => PricePoint.High,
                "L" => PricePoint.Low,
                "C" => PricePoint.Close,
                "M" => PricePoint.MidPoint,
                _ => throw new Exception($"Could not parse '{match.Groups[3].Value}' into a price point.")
            };

            var type = match.Groups[1].Value.ToUpperInvariant() switch
            {
                "S" => MovingAverageType.Simple,
                "E" => MovingAverageType.Exponential,
                _ => throw new Exception($"Could not parse '{match.Groups[1].Value}' into a moving average type.")
            };

            return new MovingAverageKey(Convert.ToInt32(match.Groups[2].Value), pricePoint, type);
        }
        return null;
    }
}

public readonly struct MovingAverage
{
    public MovingAverageKey Key { get; }
    public decimal[] Values { get; }

    public MovingAverage(MovingAverageKey key, decimal[] values)
    {
        Key = key;

        Values = (values?.Length ?? 0) == 0
            ? []
            : new decimal[values!.Length];

        if (Values.Length < key.Period)
            return;

        ComputeAverage compute = key.Type switch
        {
            MovingAverageType.Simple => ComputeSma,
            MovingAverageType.Exponential => ComputeEma,
            _ => throw new ArgumentException($"Unsupported moving average type: {key.Type}", nameof(key))
        };

        compute(key.Period, values!);
    }

    public MovingAverage(MovingAverageKey key, Ohlc[] prices)
        : this(key, prices?.Length > 0
            ? prices.Select(p => p.GetPricePoint(key.PricePoint)).ToArray()
            : throw new ArgumentException("Prices array cannot be null or empty.", nameof(prices)))
    { }

    private delegate void ComputeAverage(int period, decimal[] values);

    private void ComputeSma(int period, decimal[] values)
    {
        decimal sum = 0M;
        for (int i = 0; i < period; i++)
            sum += values[i];

        Values[period - 1] = sum / period;

        for (int i = period; i < values.Length; i++)
        {
            sum += values[i] - values[i - period];
            Values[i] = sum / period;
        }
    }

    private void ComputeEma(int period, decimal[] values)
    {
        decimal factor = 2M / (period + 1);
        decimal sum = 0M;
        for (int i = 0; i < period; i++)
            sum += values[i];

        Values[period - 1] = sum / period; // Initial SMA

        for (int i = period; i < values.Length; i++)
            Values[i] = (values[i] - Values[i - 1]) * factor + Values[i - 1];
    }

    public KeyValuePair<MovingAverageKey, decimal[]> GetSpan(int start, int finish)
    {
        if (start > finish)
            (start, finish) = (finish, start);

        if (start < 0)
            throw new ArgumentOutOfRangeException(nameof(start), $"{nameof(start)} cannot be negative.");
        if (finish >= Values.Length)
            throw new ArgumentOutOfRangeException(nameof(finish), $"{nameof(finish)} cannot be greater than last index of chart.");

        return new KeyValuePair<MovingAverageKey, decimal[]>(Key, Values[start..(finish + 1)]);
    }
}