using Gimzo.Analysis.Technical.Charts;

namespace Gimzo.AppServices.Backtesting;

public class TradeDetails
{
    public TradeDetail? Entry { get; internal set; }
    public TradeDetail? Exit { get; internal set; }
    public decimal? Profit => Entry?.Ledger is null || Exit?.Ledger is null 
        ? null 
        : Exit.Ledger.TradePrice - Entry.Ledger.TradePrice;

    public decimal? MaxFavorableExcursionPrice { get; internal set; }
    public decimal? MaxAdverseExcursionPrice { get; internal set; }
    public double? MaxFavorableExcursionPercent => Entry?.Ledger is null || MaxFavorableExcursionPrice is null
        ? null
        : Convert.ToDouble(MaxFavorableExcursionPrice / Entry.Ledger.TradePrice);

    public double? MaxAdverseExcursionPercent => Exit?.Ledger is null || MaxAdverseExcursionPrice is null
        ? null
        : Convert.ToDouble(MaxAdverseExcursionPrice / Exit.Ledger.TradePrice);

    public int? DurationDays => Entry?.Ledger is null || Exit?.Ledger is null
        ? null
        : Exit.Ledger.Date.DayNumber - Entry.Ledger.Date.DayNumber;
}

public class TradeDetail
{
    private readonly Dictionary<int, decimal> _distanceToMovingAverage = [];

    public TradeDetail()
    {
    }

    public CashLedgerEntry? Ledger { get; internal set; }
    public PriceExtreme? PreviousHigh { get; internal set; }
    public PriceExtreme? PreviousLow { get; internal set; }

    public double? PercentFromPreviousHigh => Ledger is null || PreviousHigh is null
        ? null
        : Convert.ToDouble((Ledger.TradePrice - PreviousHigh.Value.Price) / PreviousHigh.Value.Price * 100);

    public double? PercentFromPreviousLow => Ledger is null || PreviousLow is null
        ? null
        : Convert.ToDouble((Ledger.TradePrice - PreviousLow.Value.Price) / PreviousLow.Value.Price * 100);

    public ValueRange<decimal>? AverageTrueRange { get; internal set; }
    public ValueRange<double>? AverageVolume { get; internal set; }
    public double? RelativeVolume { get; internal set; }
    public ValueRange<int>? NumberDownDays { get; internal set; }
    public ValueRange<int>? NumberUpDays { get; internal set; }
    public ValueRange<int>? NumberRedDays { get; internal set; }
    public ValueRange<int>? NumberGreenDays { get; internal set; }
    public ValueRange<double>? PriorSlope { get; internal set; }
    public ValueRange<double>? RelativeStrengthIndex { get; internal set; }
    public string? Reason { get; internal set; }

    /// <summary>
    /// Key = moving average period.
    /// Value = trade price - moving average value.
    /// </summary>
    public IReadOnlyDictionary<int, decimal> DistanceToMovingAverage => _distanceToMovingAverage;
    internal void SetDistanceToMovingAverage(int period, decimal distance) =>
        _distanceToMovingAverage[period] = distance;
}

public readonly record struct ValueRange<T>
{
    public ValueRange(T value, int period)
    {
        Value = value;
        Period = period; 
    }

    public T Value { get; init; }
    public int Period { get; init; }
}
