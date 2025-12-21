using Gimzo.Analysis.Technical.Charts;

namespace Gimzo.Analysis.Technical.Patterns;

public readonly struct PatternMatch
{
    public ChartInfo ChartInfo { get; init; }
    public PatternInfo PatternInfo { get; init; }
    public ChartPositionRange Location { get; init; }
    public DateOnly Date { get; init; }
}

