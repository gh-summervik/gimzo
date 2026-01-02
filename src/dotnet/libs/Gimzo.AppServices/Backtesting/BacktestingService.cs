using Gimzo.Analysis.Technical.Charts;
using Gimzo.AppServices.Data;
using Gimzo.Infrastructure.Database;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

[assembly: InternalsVisibleTo("Gimzo.AppServices.Tests")]
namespace Gimzo.AppServices.Backtesting;

public sealed partial class BacktestingService(DbDefPair dbDefPair,
    IMemoryCache memoryCache,
    ILogger<BacktestingService> logger)
{
    private readonly ILogger<BacktestingService> _logger = logger;
    private readonly DataService _dataService = new(dbDefPair, memoryCache, logger);
    private const int DefaultQty = 1;

    private static class Scenarios
    {
        public const string TrendDivergence = "trend-divergence";
        public const string TrendFollow = "trend-follow";
        public const string MovingAverageCrossover = "ma-x";
        public const string PriceMovingAverageCrossover = "pma-x";
        public const string PriceExtremeFollow = "pxf";
    }

    public async Task<Ledger?> ExecuteAsync(string scenario, string symbol)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(scenario, nameof(scenario));
        ArgumentException.ThrowIfNullOrWhiteSpace(symbol, nameof(symbol));

        if (scenario.StartsWith(Scenarios.TrendDivergence, StringComparison.CurrentCultureIgnoreCase))
        {
            var pattern = $@"{Scenarios.TrendDivergence}\-(\d*)";
            int period = Convert.ToInt32(Regex.Matches(scenario, pattern)[0].Groups[1].Value);
            return await ExecuteGimzoTrendDivergenceAsync(symbol, period);
        }
        else if (scenario.StartsWith(Scenarios.TrendFollow, StringComparison.CurrentCultureIgnoreCase))
        {
            var pattern = $@"{Scenarios.TrendFollow}\-(\d*)";
            int period = Convert.ToInt32(Regex.Matches(scenario, pattern)[0].Groups[1].Value);
            return await ExecuteGimzoTrendFollowAsync(symbol, period);
        }
        else if (scenario.ToLowerInvariant().Equals(Scenarios.MovingAverageCrossover))
            return await ExecuteMovingAverageCrossoverAsync(symbol);
        else if (scenario.ToLowerInvariant().Equals(Scenarios.PriceMovingAverageCrossover))
            return await ExecutePriceMovingAverageCrossoverAsync(symbol);
        else if (scenario.ToLowerInvariant().Equals(Scenarios.PriceExtremeFollow))
            return await ExecutePriceExtremeFollowAsync(symbol);

        return null;
    }

    internal static double CalculateSlope(double[] values)
    {
        int n = values.Length;
        if (n < 2)
            return 0.0;

        double sumX = 0;
        double sumY = 0;
        double sumXY = 0;
        double sumX2 = 0;

        int i = 0;
        foreach (double y in values)
        {
            double x = i++;
            sumX += x;
            sumY += y;
            sumXY += x * y;
            sumX2 += x * x;
        }

        double denominator = n * sumX2 - sumX * sumX;
        return denominator == 0 ? 0.0 : (n * sumXY - sumX * sumY) / denominator;
    }

    private static CashLedgerEntry? FindExit(CashLedgerEntry? entry, Chart chart)
    {
        if (entry == null || chart == null || chart.PriceActions.Length == 0)
            return null;

        const int MinHoldTime = 1;
        const double MaxLossPercentage = 0.2;
        var idx = chart.GetIndexOfDate(entry.Date);
        idx += MinHoldTime; // must hold for at least X days

        if (idx >= chart.PriceActions.Length - 1)
            return null;

        decimal stopLoss = entry.IsLong ? Convert.ToDecimal(entry.TradePrice * (1M - (decimal)MaxLossPercentage))
            : Convert.ToDecimal(entry.TradePrice * (1M + (decimal)MaxLossPercentage));

        for (int i = idx; i < chart.PriceActions.Length; i++)
        {
            if ((entry.IsLong && chart.PriceActions[i].Close <= stopLoss) ||
                    (entry.IsShort && chart.PriceActions[i].Close >= stopLoss))
                return new CashLedgerEntry(chart.PriceActions[i].Date,
                    entry.Symbol,
                    entry.Quantity * -1,
                    chart.PriceActions[i].Close,
                    $"Down at least {MaxLossPercentage * 100}% after {MinHoldTime} days",
                    relatesTo: entry.Id);

            if (entry.IsLong)
            {
                // trailing candle
                if (chart.PriceActions[i].Close < chart.PriceActions[i - 1].Low)
                    return new CashLedgerEntry(chart.PriceActions[i].Date,
                        entry.Symbol,
                        entry.Quantity * -1,
                        chart.PriceActions[i].Close,
                        "Closed below previous low",
                        relatesTo: entry.Id);
            }
            else if (entry.IsShort)
            {
                // trailing candle
                if (chart.PriceActions[i].Close > chart.PriceActions[i - 1].High)
                    return new CashLedgerEntry(chart.PriceActions[i].Date,
                        entry.Symbol,
                        entry.Quantity * -1,
                        chart.PriceActions[i].Close,
                        "Closed above previous high",
                        relatesTo: entry.Id);
            }
        }

        return null;
    }

    private static CashLedgerEntry? FindExitOnLong(CashLedgerEntry? entry, Chart chart)
    {
        if (entry == null || entry.IsShort || chart == null || chart.PriceActions.Length == 0)
            return null;

        var idx = chart.GetIndexOfDate(entry.Date);

        List<decimal> targets = new(5);

        var prevHigh = chart.FindPreviousHigh(idx);
        if (prevHigh.HasValue)
            targets.Add(prevHigh.Value.Price * 1.05M);
        targets.Add(entry.TradePrice * 1.2M);
        targets.Add(entry.TradePrice * 1.5M);

        targets.Sort();

        int tgtIdx = 0;
        bool sellOnWeakness = false;
        decimal price;
        decimal stopPrice = entry.TradePrice * 0.92M;

        for (int i = idx + 1; i < chart.PriceActions.Length - 1; i++)
        {
            if (chart.PriceActions[i].Date >= entry.Date.AddDays(40))
                return new CashLedgerEntry(chart.PriceActions[i].Date,
                   entry.Symbol,
                   entry.Quantity * -1,
                   chart.PriceActions[i].Close,
                   "Time ran out",
                   relatesTo: entry.Id);

            if (chart.PriceActions[i].Close <= stopPrice)
                return new CashLedgerEntry(chart.PriceActions[i].Date,
                   entry.Symbol,
                   entry.Quantity * -1,
                   stopPrice,
                   "Stop Loss",
                   relatesTo: entry.Id);

            if (sellOnWeakness && chart.PriceActions[i].Close < chart.PriceActions[i - 1].Close)
            {
                return new CashLedgerEntry(chart.PriceActions[i].Date,
                   entry.Symbol,
                   entry.Quantity * -1,
                   chart.PriceActions[i].Close,
                   "Sold on weakness after target reached",
                   relatesTo: entry.Id);
            }
            else if (targets.Count < 2)
            {
                price = targets.First();
                if (chart.PriceActions[i].High >= price)
                    return new CashLedgerEntry(chart.PriceActions[i].Date,
                       entry.Symbol,
                       entry.Quantity * -1,
                       price,
                       "Target reached",
                       relatesTo: entry.Id);
            }
            else
            {
                price = targets.ElementAt(tgtIdx);
                if (tgtIdx < targets.Count - 1)
                {
                    if (chart.PriceActions[i].High >= price)
                    {
                        tgtIdx++;
                        sellOnWeakness = true;
                    }
                }
                
                if (tgtIdx == targets.Count - 1)
                {
                    return new CashLedgerEntry(chart.PriceActions[i].Date,
                       entry.Symbol,
                       entry.Quantity * -1,
                       price,
                       $"Target ({tgtIdx + 1}) reached",
                       relatesTo: entry.Id);
                }
            }
        }

        return null;
    }
}
