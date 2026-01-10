
using Gimzo.Common;

namespace Gimzo.Analysis.Technical.Charts;

public readonly struct ChartInfo : IEquatable<ChartInfo>
{
    public required string Symbol { get; init; }
    public ChartInterval Interval { get; init; }
    public DateOnly Start { get; init; }
    public DateOnly Finish { get; init; }
    public int Length { get; init; }

    public override string ToString() => $"{Symbol} {Interval.GetEnumDescription()} {Start:yyyy-MM-dd} - {Finish:yyyy-MM-dd}";
    public override bool Equals(object? obj) => obj is ChartInfo info && Equals(info);

    public bool Equals(ChartInfo other) => Symbol == other.Symbol &&
        Interval == other.Interval &&
        Start.DayNumber == other.Start.DayNumber &&
        Finish.DayNumber == other.Finish.DayNumber &&
        Length == other.Length;

    public override int GetHashCode() => HashCode.Combine(Symbol, Interval, Start, Finish, Length);
    public static bool operator ==(ChartInfo left, ChartInfo right) => left.Equals(right);
    public static bool operator !=(ChartInfo left, ChartInfo right) => !(left == right);
}
