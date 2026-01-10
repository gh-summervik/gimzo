namespace Gimzo.Analysis.Technical.Charts;

public readonly struct ChartConfiguration
{
    public string[]? MovingAverages { get; init; }
    public BollingerBandConfiguration? BollingerBand { get; init; }
    public int? AverageTrueRangePeriod { get; init; }
    public int? RelativeStrengthPeriod { get; init; }
    public ChartInterval Interval { get; init; }
}
