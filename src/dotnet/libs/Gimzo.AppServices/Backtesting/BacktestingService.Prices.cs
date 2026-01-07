using Dapper;
using Gimzo.Analysis.Technical;
using Gimzo.Analysis.Technical.Charts;
using Gimzo.Infrastructure.Database;
using Gimzo.Infrastructure.Database.DataAccessObjects;
using System.Data;

namespace Gimzo.AppServices.Backtesting;

public partial class BacktestingService
{
    private async Task ExecuteBullishPriceExtremeFollowAsync(string symbol, Guid userId,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentException.ThrowIfNullOrWhiteSpace(symbol);
        if (userId == Guid.Empty)
            userId = Common.Constants.SystemId;

        var chart = await GetChartAsync(symbol);

        if (chart is null)
            return;

        const int LookbackPeriod = 14;

        var trades = new List<BacktestTrade>();

        for (int i = 1; i < chart.Extremes.Length - 1; i++)
        {
            var current = chart.Extremes[i];

            if (current.Index < LookbackPeriod)
                continue;

            var prevHigh = chart.FindPreviousHigh(i);
            var prevLow = chart.FindPreviousLow(i);

            if (prevHigh is null)
                continue;

            if (!(current.IsLow && current.IsBullish))
                continue;

            var entryBarIndex = current.Index + 1;
            var entryPrice = chart.PriceActions[entryBarIndex].MidPoint;
            var entryDate = chart.PriceActions[entryBarIndex].Date;

            var entryLedger = new CashLedgerEntry(entryDate, symbol, DefaultQty, entryPrice, "");

            var exitLedger = FindExitOnLong(entryLedger, chart);
            if (exitLedger is null)
                continue;

            var exitBarIndex = chart.GetIndexOfDate(exitLedger.Date);

            var tradeBars = chart.Candlesticks[entryBarIndex..(exitBarIndex + 1)];

            var tradeDetails = new TradeDetails
            {
                Entry = CreateTradeDetail(chart, entryBarIndex, LookbackPeriod, entryLedger, prevHigh, prevLow),
                Exit = CreateTradeDetail(chart, exitBarIndex, LookbackPeriod, exitLedger,
                    chart.FindPreviousHigh(exitBarIndex), chart.FindPreviousLow(exitBarIndex)),
                MaxFavorableExcursionPrice = tradeBars.Max(c => c.High),
                MaxAdverseExcursionPrice = tradeBars.Min(c => c.Low)
            };

            var bt = new BacktestTrade(userId)
            {
                TradeId = Guid.NewGuid(),
                BacktestType = Keys.PriceLongExtremeFollow,
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
                MfePercent = CalculateExcursionPercent(tradeDetails.MaxFavorableExcursionPrice, entryPrice, favorable: true),
                MaePercent = CalculateExcursionPercent(tradeDetails.MaxAdverseExcursionPrice, entryPrice, favorable: false),

                // Entry context
                EntryPrevHighPrice = prevHigh?.Price,
                EntryPrevLowPrice = prevLow?.Price,
                EntryPercentFromPrevHigh = CalculatePercentFromPrev(entryPrice, prevHigh?.Price),
                EntryPercentFromPrevLow = CalculatePercentFromPrev(entryPrice, prevLow?.Price),
                EntryAtr = tradeDetails.Entry.AverageTrueRange?.Value,
                EntryAvgVolume = tradeDetails.Entry.AverageVolume?.Value,
                EntryRelativeVolume = tradeDetails.Entry.RelativeVolume,
                EntryNumUpDays = tradeDetails.Entry.NumberUpDays?.Value,
                EntryNumDownDays = tradeDetails.Entry.NumberDownDays?.Value,
                EntryNumGreenDays = tradeDetails.Entry.NumberGreenDays?.Value,
                EntryNumRedDays = tradeDetails.Entry.NumberRedDays?.Value,
                EntryPriorSlope = tradeDetails.Entry.PriorSlope?.Value,
                EntryRsi = tradeDetails.Entry.RelativeStrengthIndex?.Value,
                EntryMaDistances = tradeDetails.Entry.DistanceToMovingAverage is null
                    ? new Dictionary<int, decimal>()
                    : new Dictionary<int, decimal>(tradeDetails.Entry.DistanceToMovingAverage),

                // Exit context
                ExitPrevHighPrice = tradeDetails.Exit.PreviousHigh?.Price,
                ExitPrevLowPrice = tradeDetails.Exit.PreviousLow?.Price,
                ExitPercentFromPrevHigh = CalculatePercentFromPrev(exitLedger.TradePrice, tradeDetails.Exit.PreviousHigh?.Price),
                ExitPercentFromPrevLow = CalculatePercentFromPrev(exitLedger.TradePrice, tradeDetails.Exit.PreviousLow?.Price),
                ExitAtr = tradeDetails.Exit.AverageTrueRange?.Value,
                ExitAvgVolume = tradeDetails.Exit.AverageVolume?.Value,
                ExitRelativeVolume = tradeDetails.Exit.RelativeVolume,
                ExitNumUpDays = tradeDetails.Exit.NumberUpDays?.Value,
                ExitNumDownDays = tradeDetails.Exit.NumberDownDays?.Value,
                ExitNumGreenDays = tradeDetails.Exit.NumberGreenDays?.Value,
                ExitNumRedDays = tradeDetails.Exit.NumberRedDays?.Value,
                ExitPriorSlope = tradeDetails.Exit.PriorSlope?.Value,
                ExitRsi = tradeDetails.Exit.RelativeStrengthIndex?.Value,
                ExitMaDistances = tradeDetails.Exit.DistanceToMovingAverage is null
                    ? new Dictionary<int, decimal>()
                    : new Dictionary<int, decimal>(tradeDetails.Exit.DistanceToMovingAverage),

                ExitReason = exitLedger.Description ?? "Unknown"
            };

            trades.Add(bt);
        }

        var cmdCtx = _dbDefPair.GetCommandConnection();
        foreach (var chunk in trades.Chunk(Common.Constants.DefaultChunkSize))
            await cmdCtx.ExecuteAsync(SqlRepository.InsertBacktestTrades, chunk);
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
        PriceExtreme? prevLow)
    {
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

        var slope = Common.Maths.CalculateSlope(
            lookbackBars.Select(c => c.MidPoint).ToArray(),
            lookbackBars.Length);

        var detail = new TradeDetail
        {
            Ledger = ledgerEntry,
            PreviousHigh = prevHigh,
            PreviousLow = prevLow,
            AverageTrueRange = new ValueRange<decimal>(atr.Values[^1], lookbackPeriod),
            AverageVolume = new ValueRange<double>(avgVolume, lookbackPeriod),
            RelativeVolume = chart.PriceActions[barIndex].Volume / avgVolume,
            NumberUpDays = new ValueRange<int>(numUp, lookbackPeriod),
            NumberDownDays = new ValueRange<int>(numDown, lookbackPeriod),
            NumberGreenDays = new ValueRange<int>(numGreen, lookbackPeriod),
            NumberRedDays = new ValueRange<int>(numRed, lookbackPeriod),
            PriorSlope = new ValueRange<double>(slope, lookbackPeriod),
            RelativeStrengthIndex = new ValueRange<double>(chart.TrendValues[barIndex], lookbackPeriod)
        };

        foreach (var ma in chart.MovingAverages)
            detail.SetDistanceToMovingAverage(ma.Key.Period, ledgerEntry.TradePrice - ma.Values[barIndex]);

        return detail;
    }
}