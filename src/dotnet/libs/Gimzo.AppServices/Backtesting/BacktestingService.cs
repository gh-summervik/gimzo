using Gimzo.Analysis.Technical.Charts;
using Gimzo.Infrastructure.Database;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Npgsql;
using NpgsqlTypes;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

[assembly: InternalsVisibleTo("Gimzo.AppServices.Tests")]
namespace Gimzo.AppServices.Backtesting;

public sealed partial class BacktestingService(DbDefPair dbDefPair,
    IMemoryCache memoryCache,
    ILogger<BacktestingService> logger) : ServiceBase(dbDefPair, memoryCache)
{
    private readonly ILogger<BacktestingService> _logger = logger;
    private const int DefaultQty = 1;

    private static class Keys
    {
        public const string TrendDivergence = "trend-divergence";
        public const string TrendFollow = "trend-follow";
        public const string MovingAverageCrossover = "ma-x";
        public const string PriceMovingAverageCrossover = "pma-x";
        public const string PriceLongExtremeFollow = "pxfl";
    }

    public static Dictionary<string, string> Scenarios => new(StringComparer.OrdinalIgnoreCase) {
        { Keys.TrendDivergence, $"Buy/Sell when price diverges from trend. Use {Keys.TrendDivergence}-n where 'n' is the number of days in the divergence."},
        { Keys.TrendFollow, $"Buy/Sell when price follows the trend. Use {Keys.TrendFollow}-n where 'n' is the number of days in the divergence."},
        { Keys.MovingAverageCrossover, $"Buy/Sell when short moving average crosses over longer moving average."},
        { Keys.PriceMovingAverageCrossover, $"Buy/Sell when Price crosses over moving average."},
        { Keys.PriceLongExtremeFollow, $"Buy when price creates new bullish low."},
    };

    public async Task<IReadOnlyCollection<string>> GetSymbolsToTest(
        int minAbsoluteScore = 0,
        int minCompanyPercentileRank = 0,
        int minIndustryRank = 0)
    {
        const string Sql = @"
WITH latest_company AS (
     SELECT * FROM (
        SELECT *,
        ROW_NUMBER() OVER (PARTITION BY symbol ORDER BY date_eval DESC) AS rn
        FROM public.company_valuations
    ) sub
    WHERE rn = 1
),
latest_sic AS (
    SELECT *
    FROM (
        SELECT *,
        ROW_NUMBER() OVER (PARTITION BY sic_code ORDER BY date_eval DESC) AS rn
        FROM public.sic_money_flow
        ) sub
    WHERE rn = 1
)
SELECT lc.symbol FROM latest_company lc
    JOIN public.us_companies c ON lc.symbol = c.symbol
    JOIN latest_sic ls ON c.sic_code = ls.sic_code
WHERE lc.absolute_value >= @AV AND lc.percentile_rank >= @PR AND ls.rank >= @R;";

        await using var ds = NpgsqlDataSource.Create(_dbDefPair.Query.ConnectionString);
        await using var connection = await ds.OpenConnectionAsync();
        await using var command = new NpgsqlCommand(Sql, connection);

        command.Parameters.Add(new NpgsqlParameter("@AV", NpgsqlDbType.Integer) { Value = minAbsoluteScore });
        command.Parameters.Add(new NpgsqlParameter("@PR", NpgsqlDbType.Integer) { Value = minCompanyPercentileRank });
        command.Parameters.Add(new NpgsqlParameter("@R", NpgsqlDbType.Integer) { Value = minIndustryRank });

        await using var reader = await command.ExecuteReaderAsync();

        var symbols = new List<string>(5_000);

        while (await reader.ReadAsync())
            symbols.Add(reader.GetString(0));

        return [.. symbols];
    }

    public async Task<Ledger?> ExecuteAsync(string scenario, string symbol)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(scenario, nameof(scenario));
        ArgumentException.ThrowIfNullOrWhiteSpace(symbol, nameof(symbol));

        if (scenario.StartsWith(Keys.TrendDivergence, StringComparison.CurrentCultureIgnoreCase))
        {
            var pattern = $@"{Keys.TrendDivergence}\-(\d*)";
            int period = Convert.ToInt32(Regex.Matches(scenario, pattern)[0].Groups[1].Value);
            return await ExecuteGimzoTrendDivergenceAsync(symbol, period);
        }
        else if (scenario.StartsWith(Keys.TrendFollow, StringComparison.CurrentCultureIgnoreCase))
        {
            var pattern = $@"{Keys.TrendFollow}\-(\d*)";
            int period = Convert.ToInt32(Regex.Matches(scenario, pattern)[0].Groups[1].Value);
            return await ExecuteGimzoTrendFollowAsync(symbol, period);
        }
        else if (scenario.ToLowerInvariant().Equals(Keys.MovingAverageCrossover))
            return await ExecuteMovingAverageCrossoverAsync(symbol);
        else if (scenario.ToLowerInvariant().Equals(Keys.PriceMovingAverageCrossover))
            return await ExecutePriceMovingAverageCrossoverAsync(symbol);
        else if (scenario.ToLowerInvariant().Equals(Keys.PriceLongExtremeFollow))
            return await ExecuteBullishPriceExtremeFollowAsync(symbol);

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

        List<decimal> targets = new(10);

        var prevHigh = chart.FindPreviousHigh(idx);

        if (prevHigh.HasValue)
        {
            var prevLow = chart.FindPreviousLow(prevHigh.Value.Index);
            if (prevLow.HasValue && prevLow.Value.Price > 0 && prevLow.Value.Price < prevHigh.Value.Price)
                targets.Add(prevHigh.Value.Price - prevLow.Value.Price + entry.TradePrice);
        }

        targets.Add(entry.TradePrice * 1.20M);
        targets.Add(entry.TradePrice * 1.25M);
        targets.Add(entry.TradePrice * 1.5M);
        targets.Add(entry.TradePrice * 2M);
        targets.Add(entry.TradePrice * 3M);
        targets.Add(entry.TradePrice * 4M);

        targets.Sort();

        int tgtIdx = 0;
        bool sellOnWeakness = false;
        decimal tgtPrice;
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
                    chart.PriceActions[i].Contains(stopPrice) 
                        ? stopPrice
                        : chart.PriceActions[i].Open < stopPrice 
                            ? chart.PriceActions[i].Open 
                            : chart.PriceActions[i].MidPoint,
                    "Stop Loss",
                    relatesTo: entry.Id);

            if (sellOnWeakness && chart.PriceActions[i].Close < chart.PriceActions[i - 1].Low)
            {
                return new CashLedgerEntry(chart.PriceActions[i].Date,
                   entry.Symbol,
                   entry.Quantity * -1,
                   chart.PriceActions[i].Close,
                   $"Sold on weakness after target ({tgtIdx + 1}) reached",
                   relatesTo: entry.Id);
            }
            
            if (targets.Count < 2)
            {
                tgtPrice = targets.First();
                if (chart.PriceActions[i].High >= tgtPrice)
                    return new CashLedgerEntry(chart.PriceActions[i].Date,
                       entry.Symbol,
                       entry.Quantity * -1,
                       tgtPrice,
                       $"Target ({tgtIdx + 1}) reached",
                       relatesTo: entry.Id);
            }
            else
            {
                tgtPrice = targets.ElementAt(tgtIdx);

                if (tgtIdx == targets.Count - 1)
                {
                    return new CashLedgerEntry(chart.PriceActions[i].Date,
                       entry.Symbol,
                       entry.Quantity * -1,
                       tgtPrice,
                       $"Target ({tgtIdx + 1}) reached",
                       relatesTo: entry.Id);
                }

                if (chart.PriceActions[i].High >= tgtPrice)
                {
                    tgtIdx++;
                    //stopPrice = Math.Max(stopPrice, chart.PriceActions[i].High);
                    //stopPrice = Math.Max(stopPrice, chart.PriceActions[i - 1].High);
                    //stopPrice = Math.Max(stopPrice, chart.PriceActions[i].MidPoint);
                    //stopPrice = Math.Max(stopPrice, chart.PriceActions[i].Low);
                    stopPrice = Math.Max(stopPrice, chart.PriceActions[i - 1].MidPoint);
                    sellOnWeakness = true;
                }
            }
        }

        return null;
    }
}
