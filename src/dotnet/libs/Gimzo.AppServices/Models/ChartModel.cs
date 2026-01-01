using Gimzo.Analysis.Technical.Charts;

namespace Gimzo.AppServices.Models;

public class ChartModel
{
    public string? Symbol { get; set; }
    public Ohlc[]? Prices { get; set; }
    public string? SerializedData { get; set; }
    public string? SerializedTrend { get; set; }
}