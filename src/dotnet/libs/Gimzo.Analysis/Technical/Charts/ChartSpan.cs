namespace Gimzo.Analysis.Technical.Charts;

public readonly struct ChartSpan
{
    public ChartSpan(Chart chart, int start, int finish)
    {
        ArgumentNullException.ThrowIfNull(chart);
        if (start > finish)
            (start, finish) = (finish, start);

        if (start < 0)
            throw new ArgumentOutOfRangeException(nameof(start), $"{nameof(start)} cannot be negative.");
        if (finish >= chart.Length)
            throw new ArgumentOutOfRangeException(nameof(finish), $"{nameof(finish)} cannot be greater than last index of chart.");

        Length = finish - start + 1;

        Offset = start;
        ChartInfo = chart.Info;
        PriceActions = new ArraySegment<Ohlc>(chart.PriceActions, start, Length);
        Candlesticks = new ArraySegment<Candlestick>(chart.Candlesticks, start, Length);
        TrendValues = new ArraySegment<double>(chart.TrendValues, start, Length);
        MovingAverages = new(chart.MovingAverages.Length);
        foreach (var ma in chart.MovingAverages)
        {
            var sp = ma.GetSpan(start, finish);
            MovingAverages.Add(sp.Key, sp.Value);
        }
    }

    public ChartInfo ChartInfo { get; }
    public int Offset { get; }
    public ArraySegment<Ohlc> PriceActions { get; }
    public ArraySegment<Candlestick> Candlesticks { get; }
    public ArraySegment<double> TrendValues { get; }
    public Dictionary<MovingAverageKey, decimal[]> MovingAverages { get; }
    public int Length { get; }
    public PriceRange GetPriceRange() =>
        new(PriceActions.MaxBy(k => k.High)?.High ?? 0M,
            PriceActions.MinBy(k => k.Low)?.Low ?? 0M);
}
