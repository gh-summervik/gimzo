using Dapper;
using Gimzo.Analysis.Technical;
using Gimzo.Analysis.Technical.Charts;
using Gimzo.Infrastructure.Database;
using Gimzo.Infrastructure.Database.DataAccessObjects;
using System.Data;

namespace Gimzo.AppServices.Backtests;

public partial class BacktestingService
{
    private async Task ExecuteBullishPriceExtremeFollowAsync(BacktestConfiguration config,
        string symbol, Guid userId,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentException.ThrowIfNullOrWhiteSpace(symbol);

        var pef = config.PriceExtremeFollow ?? throw new ArgumentException($"{nameof(config.PriceExtremeFollow)} is required.");

        if (userId == Guid.Empty)
            userId = Common.Constants.SystemId;

        var chart = await GetChartAsync(symbol, config.Chart);
        if (chart is null)
            return;

        decimal[] midPoints = [.. chart.Candlesticks.Select(c => c.MidPoint)];

        var trades = new List<BacktestTrade>(5_000);

        for (int i = 1; i < chart.Extremes.Length - 1; i++)
        {
            var current = chart.Extremes[i];

            if (current.Index < config.PriceExtremeFollow.GetValueOrDefault().LookbackPeriod.GetValueOrDefault())
                continue;

            var prevHigh = chart.FindPreviousHigh(i);
            var prevLow = chart.FindPreviousLow(i);

            if (prevHigh is null || !(current.IsLow && current.IsBullish))
                continue;

            int entryBarIndex = current.Index + 1;
            if (entryBarIndex >= chart.Length)
                continue;

            decimal entryPrice = chart.PriceActions[entryBarIndex].MidPoint;
            DateOnly entryDate = chart.PriceActions[entryBarIndex].Date;

            var entryLedger = new CashLedgerEntry(entryDate, symbol, DefaultQty, entryPrice, "");

            var entryDetail = CreateTradeDetail(chart, entryBarIndex,
                config.PriceExtremeFollow.GetValueOrDefault().LookbackPeriod.GetValueOrDefault(),
                entryLedger, prevHigh, prevLow, midPoints);

            decimal prevHighPrice = prevHigh?.Price ?? 0m;
            double? percentFromHigh = prevHighPrice > 0 ? (double)((entryPrice - prevHighPrice) / prevHighPrice * 100) : null;

            double? rsi = entryDetail.RelativeStrengthIndex?.Value;

            double relativeVolume = entryDetail.RelativeVolume ?? 0;

            double priorSlope = entryDetail.PriorSlope?.Value ?? 0;

            var maPeriod = config.PriceExtremeFollow.GetValueOrDefault().MovingAveragePeriod.GetValueOrDefault();
            if (maPeriod == 0)
                maPeriod = 21;
            decimal maValue = chart.MovingAverages.FirstOrDefault(m => m.Key.Period == maPeriod).Values[entryBarIndex];
            double? maPercent = maValue > 0 ? (double)((entryPrice - maValue) / maValue * 100) : null;

            decimal? atr = entryDetail.AverageTrueRange?.Value;

            if (percentFromHigh < config.PriceExtremeFollow.GetValueOrDefault().EntryCriteria!.Value.MinPercentFromPrevHigh ||
                percentFromHigh > config.PriceExtremeFollow.GetValueOrDefault().EntryCriteria!.Value.MaxPercentFromPrevHigh ||
                rsi < config.PriceExtremeFollow.GetValueOrDefault().EntryCriteria!.Value.MinRsi || 
                rsi > config.PriceExtremeFollow.GetValueOrDefault().EntryCriteria!.Value.MaxRsi ||
                relativeVolume < config.PriceExtremeFollow.GetValueOrDefault().EntryCriteria!.Value.MinRelativeVolume ||
                priorSlope <= config.PriceExtremeFollow.GetValueOrDefault().EntryCriteria!.Value.MinPriorSlope ||
                maPercent < config.PriceExtremeFollow.GetValueOrDefault().EntryCriteria!.Value.MinPercentFromMovingAverage ||
                atr < config.PriceExtremeFollow.GetValueOrDefault().EntryCriteria!.Value.MinAverageTrueRange)
                continue;

            var exitLedger = FindExitOnLong(entryLedger, chart);
            if (exitLedger is null)
                continue;

            int exitBarIndex = chart.GetIndexOfDate(exitLedger.Date);
            if (exitBarIndex < 0)
                continue;

            var tradeBars = chart.Candlesticks[entryBarIndex..(exitBarIndex + 1)];

            var exitDetail = CreateTradeDetail(chart, exitBarIndex,
                config.PriceExtremeFollow.GetValueOrDefault().LookbackPeriod.GetValueOrDefault(),
                exitLedger,
                chart.FindPreviousHigh(exitBarIndex),
                chart.FindPreviousLow(exitBarIndex),
                midPoints);

            var tradeDetails = new TradeDetails
            {
                Entry = entryDetail,
                Exit = exitDetail,
                MaxFavorableExcursionPrice = tradeBars.Max(c => c.High),
                MaxAdverseExcursionPrice = tradeBars.Min(c => c.Low)
            };

            var bt = new BacktestTrade(userId)
            {
                TradeId = Guid.NewGuid(),
                BacktestType = config.Name,
                Symbol = symbol,
                EntryDate = entryDate,
                ExitDate = exitLedger.Date,
                EntryPrice = entryPrice,
                ExitPrice = exitLedger.TradePrice,
                PnlPercent = CalculatePnlPercent(entryPrice, exitLedger.TradePrice),
                IsWinner = CalculatePnlPercent(entryPrice, exitLedger.TradePrice) > 0,
                DurationDays = tradeDetails.DurationDays ?? (exitLedger.Date.DayNumber - entryDate.DayNumber),
                MfePrice = tradeDetails.MaxFavorableExcursionPrice,
                MaePrice = tradeDetails.MaxAdverseExcursionPrice,
                MfePercent = CalculateExcursionPercent(tradeDetails.MaxFavorableExcursionPrice, entryPrice, true),
                MaePercent = CalculateExcursionPercent(tradeDetails.MaxAdverseExcursionPrice, entryPrice, false),
                EntryPrevHighPrice = prevHigh?.Price,
                EntryPrevLowPrice = prevLow?.Price,
                EntryPercentFromPrevHigh = percentFromHigh,
                EntryPercentFromPrevLow = CalculatePercentFromPrev(entryPrice, prevLow?.Price),
                EntryAtr = atr,
                EntryAvgVolume = entryDetail.AverageVolume?.Value,
                EntryRelativeVolume = relativeVolume,
                EntryNumUpDays = entryDetail.NumberUpDays?.Value,
                EntryNumDownDays = entryDetail.NumberDownDays?.Value,
                EntryNumGreenDays = entryDetail.NumberGreenDays?.Value,
                EntryNumRedDays = entryDetail.NumberRedDays?.Value,
                EntryPriorSlope = priorSlope,
                EntryRsi = rsi,
                EntryMaDistances = new Dictionary<int, decimal>(entryDetail.DistanceToMovingAverage),
                ExitPrevHighPrice = exitDetail.PreviousHigh?.Price,
                ExitPrevLowPrice = exitDetail.PreviousLow?.Price,
                ExitPercentFromPrevHigh = CalculatePercentFromPrev(exitLedger.TradePrice, exitDetail.PreviousHigh?.Price),
                ExitPercentFromPrevLow = CalculatePercentFromPrev(exitLedger.TradePrice, exitDetail.PreviousLow?.Price),
                ExitAtr = exitDetail.AverageTrueRange?.Value,
                ExitAvgVolume = exitDetail.AverageVolume?.Value,
                ExitRelativeVolume = exitDetail.RelativeVolume,
                ExitNumUpDays = exitDetail.NumberUpDays?.Value,
                ExitNumDownDays = exitDetail.NumberDownDays?.Value,
                ExitNumGreenDays = exitDetail.NumberGreenDays?.Value,
                ExitNumRedDays = exitDetail.NumberRedDays?.Value,
                ExitPriorSlope = exitDetail.PriorSlope?.Value,
                ExitRsi = exitDetail.RelativeStrengthIndex?.Value,
                ExitMaDistances = new Dictionary<int, decimal>(exitDetail.DistanceToMovingAverage),
                ExitReason = exitLedger.Description ?? "Unknown"
            };

            trades.Add(bt);
        }

        if (trades.Count == 0)
            return;

        using var conn = _dbDefPair.GetCommandConnection();

        foreach (var chunk in trades.Chunk(Common.Constants.DefaultChunkSize))
            await conn.ExecuteAsync(SqlRepository.InsertBacktestTrades, chunk);
    }

    private static double CalculatePnlPercent(decimal entry, decimal exit) =>
        (double)((exit - entry) / entry * 100);

    private static double? CalculatePercentFromPrev(decimal price, decimal? extreme) =>
        extreme is null ? null : (double)((price - extreme.Value) / extreme.Value * 100);

    private static double? CalculateExcursionPercent(decimal? extreme, decimal basePrice, bool favorable) =>
        extreme is null ? null : (double)((favorable ? extreme.Value - basePrice : basePrice - extreme.Value) / basePrice * 100);

    private TradeDetail CreateTradeDetail(
        Chart chart,
        int barIndex,
        int lookbackPeriod,
        CashLedgerEntry ledgerEntry,
        PriceExtreme? prevHigh,
        PriceExtreme? prevLow,
        IReadOnlyList<decimal> midPoints)
    {
        if (barIndex < 0 || barIndex >= chart.PriceActions.Length)
            throw new ArgumentOutOfRangeException(nameof(barIndex));

        var lookbackEnd = barIndex;
        var lookbackStart = Math.Max(0, lookbackEnd - lookbackPeriod);
        var lookbackBars = chart.Candlesticks[lookbackStart..lookbackEnd];

        var atr = new AverageTrueRange(lookbackBars, lookbackPeriod);

        int numUp = 0, numDown = 0, numGreen = 0, numRed = 0;
        for (int j = 1; j < lookbackBars.Length; j++)
        {
            if (lookbackBars[j].Close > lookbackBars[j - 1].Close)
                numUp++;
            if (lookbackBars[j].Close < lookbackBars[j - 1].Close)
                numDown++;
            if (lookbackBars[j].Color == CandlestickColor.Light)
                numGreen++;
            if (lookbackBars[j].Color == CandlestickColor.Dark)
                numRed++;
        }

        var avgVolume = lookbackBars.Average(c => c.Volume);

        int length = lookbackBars.Length;
        var slope = Common.Maths.CalculateSlope(midPoints, lookbackStart, length);

        double? rsiValue = barIndex < (chart.RelativeStrengthIndex?.Values.Length ?? 0)
            ? chart.RelativeStrengthIndex!.Values[barIndex]
            : null;

        var detail = new TradeDetail
        {
            Ledger = ledgerEntry,
            PreviousHigh = prevHigh,
            PreviousLow = prevLow,
            AverageTrueRange = atr.Values.Length > 0
                ? new ValueRange<decimal>(atr.Values[^1], lookbackPeriod)
                : null,
            AverageVolume = new ValueRange<double>(avgVolume, lookbackPeriod),
            RelativeVolume = chart.PriceActions[barIndex].Volume / avgVolume,
            NumberUpDays = new ValueRange<int>(numUp, lookbackPeriod),
            NumberDownDays = new ValueRange<int>(numDown, lookbackPeriod),
            NumberGreenDays = new ValueRange<int>(numGreen, lookbackPeriod),
            NumberRedDays = new ValueRange<int>(numRed, lookbackPeriod),
            PriorSlope = new ValueRange<double>(slope, lookbackPeriod),
            RelativeStrengthIndex = rsiValue is null
                ? null
                : new ValueRange<double>(rsiValue.Value, lookbackPeriod)
        };

        foreach (var ma in chart.MovingAverages)
        {
            if (barIndex < ma.Values.Length)
                detail.SetDistanceToMovingAverage(ma.Key.Period, ledgerEntry.TradePrice - ma.Values[barIndex]);
        }

        return detail;
    }

    //private TradeDetail CreateTradeDetail(
    //    Chart chart,
    //    int barIndex,
    //    int lookbackPeriod,
    //    CashLedgerEntry ledgerEntry,
    //    PriceExtreme? prevHigh,
    //    PriceExtreme? prevLow,
    //    IReadOnlyList<decimal> midPoints)
    //{
    //    if (barIndex < 0 || barIndex >= chart.PriceActions.Length)
    //        throw new ArgumentOutOfRangeException(nameof(barIndex));

    //    var lookbackEnd = barIndex;
    //    var lookbackStart = Math.Max(0, lookbackEnd - lookbackPeriod);
    //    var lookbackBars = chart.Candlesticks[lookbackStart..lookbackEnd];

    //    var atr = new AverageTrueRange(lookbackBars, lookbackPeriod);

    //    int numUp = 0, numDown = 0, numGreen = 0, numRed = 0;
    //    for (int j = 1; j < lookbackBars.Length; j++)
    //    {
    //        if (lookbackBars[j].Close > lookbackBars[j - 1].Close)
    //            numUp++;
    //        if (lookbackBars[j].Close < lookbackBars[j - 1].Close)
    //            numDown++;
    //        if (lookbackBars[j].Color == CandlestickColor.Light)
    //            numGreen++;
    //        if (lookbackBars[j].Color == CandlestickColor.Dark)
    //            numRed++;
    //    }

    //    var avgVolume = lookbackBars.Average(c => c.Volume);

    //    int length = lookbackBars.Length;
    //    var slope = Common.Maths.CalculateSlope(midPoints, lookbackStart, length);

    //    double? rsiValue = barIndex < chart.RelativeStrengthValues.Length
    //        ? chart.RelativeStrengthValues[barIndex]
    //        : null;

    //    var detail = new TradeDetail
    //    {
    //        Ledger = ledgerEntry,
    //        PreviousHigh = prevHigh,
    //        PreviousLow = prevLow,
    //        AverageTrueRange = new ValueRange<decimal>(atr.Values[^1], lookbackPeriod),
    //        AverageVolume = new ValueRange<double>(avgVolume, lookbackPeriod),
    //        RelativeVolume = chart.PriceActions[barIndex].Volume / avgVolume,
    //        NumberUpDays = new ValueRange<int>(numUp, lookbackPeriod),
    //        NumberDownDays = new ValueRange<int>(numDown, lookbackPeriod),
    //        NumberGreenDays = new ValueRange<int>(numGreen, lookbackPeriod),
    //        NumberRedDays = new ValueRange<int>(numRed, lookbackPeriod),
    //        PriorSlope = new ValueRange<double>(slope, lookbackPeriod),
    //        RelativeStrengthIndex = rsiValue is null
    //            ? null
    //            : new ValueRange<double>(rsiValue.Value, lookbackPeriod)
    //    };

    //    foreach (var ma in chart.MovingAverages)
    //    {
    //        if (barIndex < ma.Values.Length)
    //            detail.SetDistanceToMovingAverage(ma.Key.Period, ledgerEntry.TradePrice - ma.Values[barIndex]);
    //    }

    //    return detail;
    //}

    //private TradeDetail CreateTradeDetail(
    //    Chart chart,
    //    int barIndex,
    //    int lookbackPeriod,
    //    CashLedgerEntry ledgerEntry,
    //    PriceExtreme? prevHigh,
    //    PriceExtreme? prevLow,
    //    IReadOnlyList<decimal> midPoints)
    //{
    //    var lookbackEnd = barIndex;
    //    var lookbackStart = Math.Max(0, lookbackEnd - lookbackPeriod);
    //    var lookbackBars = chart.Candlesticks[lookbackStart..lookbackEnd];

    //    var atr = new AverageTrueRange(lookbackBars, lookbackPeriod);

    //    int numUp = 0, numDown = 0, numGreen = 0, numRed = 0;
    //    for (int j = 1; j < lookbackBars.Length; j++)
    //    {
    //        if (lookbackBars[j].Close > lookbackBars[j - 1].Close)
    //            numUp++;
    //        if (lookbackBars[j].Close < lookbackBars[j - 1].Close)
    //            numDown++;
    //        if (lookbackBars[j].Color == CandlestickColor.Light)
    //            numGreen++;
    //        if (lookbackBars[j].Color == CandlestickColor.Dark)
    //            numRed++;
    //    }

    //    var avgVolume = lookbackBars.Average(c => c.Volume);

    //    int length = lookbackBars.Length;
    //    var slope = Common.Maths.CalculateSlope(midPoints, lookbackStart, length);

    //    var detail = new TradeDetail
    //    {
    //        Ledger = ledgerEntry,
    //        PreviousHigh = prevHigh,
    //        PreviousLow = prevLow,
    //        AverageTrueRange = new ValueRange<decimal>(atr.Values[^1], lookbackPeriod),
    //        AverageVolume = new ValueRange<double>(avgVolume, lookbackPeriod),
    //        RelativeVolume = chart.PriceActions[barIndex].Volume / avgVolume,
    //        NumberUpDays = new ValueRange<int>(numUp, lookbackPeriod),
    //        NumberDownDays = new ValueRange<int>(numDown, lookbackPeriod),
    //        NumberGreenDays = new ValueRange<int>(numGreen, lookbackPeriod),
    //        NumberRedDays = new ValueRange<int>(numRed, lookbackPeriod),
    //        PriorSlope = new ValueRange<double>(slope, lookbackPeriod),
    //        RelativeStrengthIndex = new ValueRange<double>(chart.RelativeStrengthValues[barIndex], lookbackPeriod)
    //    };

    //    foreach (var ma in chart.MovingAverages)
    //        detail.SetDistanceToMovingAverage(ma.Key.Period, ledgerEntry.TradePrice - ma.Values[barIndex]);

    //    return detail;
    //}
}