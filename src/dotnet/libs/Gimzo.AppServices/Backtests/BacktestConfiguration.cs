using Gimzo.Analysis.Technical.Charts;

namespace Gimzo.AppServices.Backtests;

public readonly struct BacktestConfiguration
{
    public string Name { get; init; }
    public ChartConfiguration Chart { get; init; }
    public SymbolConfiguration SymbolCriteria { get; init; }
    public PriceExtremeFollowConfiguration? PriceExtremeFollow { get; init; }
}

public readonly struct SymbolConfiguration
{
    public SymbolConfiguration() { }
    public int MinAbsoluteScore { get; init; } = 0;
    public int MaxAbsoluteScore { get; init; } = 100;
    public int MinCompanyPercentileRank { get; init; } = 0;
    public int MaxCompanyPercentileRank { get; init; } = 100;
    public int MinIndustryRank { get; init; } = 0;
    public int MaxIndustryRank { get; init; } = 100;
}

public readonly struct PriceExtremeFollowConfiguration
{
    public int? LookbackPeriod { get; init; }
    public EntryCriteriaConfiguration? EntryCriteria { get; init; }
    public int? MovingAveragePeriod { get; init; }

    //public bool IsValid(out string message)
    //{
    //    message = "";
    //    if (!EntryCriteria.HasValue)
    //        message = "Entry criteria required.";
    //    else
    //    {
    //        var ec = EntryCriteria.Value;
    //        if (!ec.MinPercentFromPrevHigh.HasValue ||
    //            !ec.MaxPercentFromPrevHigh.HasValue ||
    //            !ec.MinRsi.HasValue ||
    //            !ec.MaxRsi.HasValue ||
    //            !ec.MinRelativeVolume.HasValue ||
    //            !ec.MinPercentFromMovingAverage.HasValue ||
    //            !ec.MinAverageTrueRange.HasValue)
    //            message = "Missing entry criteria.";
    //    }
    //    if (!LookbackPeriod.HasValue)
    //        message = "Lookback period required.";

    //    if (!MovingAveragePeriod.HasValue)
    //        message = "Moving average period is required.";

    //    return string.IsNullOrEmpty(message);
    //}
}

public readonly struct EntryCriteriaConfiguration
{
    public double? MinPercentFromPrevHigh { get; init; }
    public double? MaxPercentFromPrevHigh { get; init; }
    public double? MinRsi { get; init; }
    public double? MaxRsi { get; init; }
    public double? MinRelativeVolume { get; init; }
    public double? MinPriorSlope { get; init; }
    public double? MinPercentFromMovingAverage { get; init; }
    public decimal? MinAverageTrueRange { get; init; }
}