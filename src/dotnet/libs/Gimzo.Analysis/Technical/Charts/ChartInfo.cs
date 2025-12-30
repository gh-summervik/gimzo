
using Gimzo.Common;

namespace Gimzo.Analysis.Technical.Charts;

public readonly struct ChartInfo : IEquatable<ChartInfo>
{
    public required string Symbol { get; init; }
    public ChartInterval Interval { get; init; }

    public override string ToString()
    {
        return $"{Symbol} {Interval.GetEnumDescription()}";
    }

    public override bool Equals(object? obj)
    {
        return obj is ChartInfo info && Equals(info);
    }

    public bool Equals(ChartInfo other)
    {
        return Symbol == other.Symbol &&
               Interval == other.Interval;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Symbol, Interval);
    }
    public static bool operator ==(ChartInfo left, ChartInfo right)
    {
        return left.Equals(right);
    }
    public static bool operator !=(ChartInfo left, ChartInfo right)
    {
        return !(left == right);
    }
}
