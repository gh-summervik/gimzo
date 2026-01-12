using Dapper;
using Gimzo.Analysis.Technical.Charts;
using Gimzo.Infrastructure.Database;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Npgsql;
using NpgsqlTypes;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Gimzo.AppServices.Tests")]
namespace Gimzo.AppServices.Backtests;

public sealed partial class BacktestingService(DbDefPair dbDefPair,
    IMemoryCache memoryCache,
    ILogger<BacktestingService> logger) : ServiceBase(dbDefPair, memoryCache)
{
    private readonly ILogger<BacktestingService> _logger = logger;
    private const int DefaultQty = 1;

    internal static class Keys
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
        int maxAbsoluteScore = 100,
        int minCompanyPercentileRank = 0,
        int maxCompanyPercentileRank = 100,
        int minIndustryRank = 0,
        int maxIndustryRank = 500)
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
WHERE lc.absolute_value >= @MinAv AND lc.absolute_value <= @MaxAv
AND lc.percentile_rank >= @MinPr AND lc.percentile_rank <= @MaxPr
AND ls.rank >= @MinR AND ls.rank <= @MaxR;";

        await using var ds = NpgsqlDataSource.Create(_dbDefPair.Query.ConnectionString);
        await using var connection = await ds.OpenConnectionAsync();
        await using var command = new NpgsqlCommand(Sql, connection);

        command.Parameters.Add(new NpgsqlParameter("@MinAv", NpgsqlDbType.Integer) { Value = minAbsoluteScore });
        command.Parameters.Add(new NpgsqlParameter("@MaxAv", NpgsqlDbType.Integer) { Value = maxAbsoluteScore });
        command.Parameters.Add(new NpgsqlParameter("@MinPr", NpgsqlDbType.Integer) { Value = minCompanyPercentileRank });
        command.Parameters.Add(new NpgsqlParameter("@MaxPr", NpgsqlDbType.Integer) { Value = maxCompanyPercentileRank });
        command.Parameters.Add(new NpgsqlParameter("@MinR", NpgsqlDbType.Integer) { Value = minIndustryRank });
        command.Parameters.Add(new NpgsqlParameter("@MaxR", NpgsqlDbType.Integer) { Value = maxIndustryRank });

        await using var reader = await command.ExecuteReaderAsync();

        var symbols = new List<string>(5_000);

        while (await reader.ReadAsync())
            symbols.Add(reader.GetString(0));

        return [.. symbols];
    }

    public async Task ExecuteAsync(BacktestConfiguration config, IEnumerable<string> symbols, Guid userId,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentNullException.ThrowIfNull(symbols);

        // use for easier debugging.
        //foreach (var symbol in symbols)
        //{
        //    await ExecuteBullishPriceExtremeFollowAsync(config, symbol, userId, cancellationToken);
        //}

        await Parallel.ForEachAsync(
            symbols,
            new ParallelOptions
            {
                CancellationToken = cancellationToken,
                MaxDegreeOfParallelism = 10
            },
            async (symbol, token) =>
            {
                if (config.Name.StartsWith(Keys.PriceLongExtremeFollow, StringComparison.OrdinalIgnoreCase))
                    await ExecuteBullishPriceExtremeFollowAsync(config, symbol, userId, token);
            });
    }

    public async Task<IEnumerable<Models.BacktestResult>> GetBacktestResultsAsync()
    {
        const string Sql = @"
SELECT 
    created_by AS ProcessId,
    COUNT(*) AS Trades,
    ROUND(AVG(pnl_percent)::numeric, 2) AS AveragePnlPercent,
    ROUND(AVG(CASE WHEN is_winner THEN 1.0 ELSE 0 END)::numeric, 3) AS WinRate,
    ROUND(
        (AVG(CASE WHEN is_winner THEN 1.0 ELSE 0 END)::numeric * 
         AVG(CASE WHEN is_winner THEN pnl_percent END)::numeric) -
        ((1 - AVG(CASE WHEN is_winner THEN 1.0 ELSE 0 END)::numeric) * 
         AVG(CASE WHEN NOT is_winner THEN -pnl_percent END)::numeric),
        2
    ) AS expectancy,
    ROUND(AVG(CASE WHEN is_winner THEN pnl_percent END)::numeric, 2) AS AverageWin,
    ROUND(AVG(CASE WHEN NOT is_winner THEN -pnl_percent END)::numeric, 2) AS AverageLoss,
    backtest_type AS BacktestType
FROM public.backtest_trades
GROUP BY created_by, backtest_type
ORDER BY MAX(created_at) DESC";

        using var conn = _dbDefPair.GetQueryConnection();
        return await conn.QueryAsync<Models.BacktestResult>(Sql);
    }

    public async Task DeleteAllBacktestsAsync()
    {
        using var conn = _dbDefPair.GetCommandConnection();
        await conn.ExecuteAsync("DELETE FROM public.backtest_trades");
    }

    public async Task DeleteBacktestAsync(Guid processId)
    {
        using var conn = _dbDefPair.GetCommandConnection();
        await conn.ExecuteAsync("DELETE FROM public.backtest_trades WHERE created_by = @ProcessId", new {processId});
    }

    private static CashLedgerEntry? FindExit(CashLedgerEntry? entry, Chart chart)
    {
        if (entry is null || chart is null || chart.PriceActions.Length == 0)
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

    private static CashLedgerEntry? FindExitOnLong(CashLedgerEntry entry, Chart chart)
    {
        if (entry is null || entry.IsShort || chart is null || chart.PriceActions.Length == 0)
            return null;

        int idx = chart.GetIndexOfDate(entry.Date);

        var targets = new List<decimal>();

        var prevHigh = chart.FindPreviousHigh(idx);
        if (prevHigh.HasValue)
        {
            var prevLow = chart.FindPreviousLow(prevHigh.Value.Index);
            if (prevLow.HasValue && prevLow.Value.Price > 0 && prevLow.Value.Price < prevHigh.Value.Price)
                targets.Add(prevHigh.Value.Price - prevLow.Value.Price + entry.TradePrice);
        }

        targets.Add(entry.TradePrice * 1.20m);
        targets.Add(entry.TradePrice * 1.25m);
        targets.Add(entry.TradePrice * 1.5m);
        targets.Add(entry.TradePrice * 2m);
        targets.Add(entry.TradePrice * 3m);
        targets.Add(entry.TradePrice * 4m);

        targets.Sort();

        int tgtIdx = 0;
        bool sellOnWeakness = false;
        decimal stopPrice = entry.TradePrice * 0.92m;

        for (int i = idx + 1; i < chart.PriceActions.Length - 1; i++)
        {
            var bar = chart.PriceActions[i];

            if (bar.Date >= entry.Date.AddDays(40))
                return new CashLedgerEntry(bar.Date, entry.Symbol, entry.Quantity * -1, bar.Close, "Time ran out", entry.Id);

            if (bar.Close <= stopPrice)
                return new CashLedgerEntry(bar.Date, entry.Symbol, entry.Quantity * -1,
                    bar.Contains(stopPrice) ? stopPrice : bar.Open < stopPrice ? bar.Open : bar.MidPoint,
                    "Stop Loss", entry.Id);

            if (sellOnWeakness && bar.Close < chart.PriceActions[i - 1].Low)
                return new CashLedgerEntry(bar.Date, entry.Symbol, entry.Quantity * -1, bar.Close,
                    $"Sold on weakness after target ({tgtIdx + 1}) reached", entry.Id);

            if (tgtIdx >= targets.Count)
                continue;

            decimal tgtPrice = targets[tgtIdx];

            if (bar.High >= tgtPrice)
            {
                if (tgtIdx == targets.Count - 1)
                    return new CashLedgerEntry(bar.Date, entry.Symbol, entry.Quantity * -1, tgtPrice,
                        $"Target ({tgtIdx + 1}) reached", entry.Id);

                tgtIdx++;
                stopPrice = Math.Max(stopPrice, chart.PriceActions[i - 1].MidPoint);
                sellOnWeakness = true;
            }
        }

        return null;
    }
}
