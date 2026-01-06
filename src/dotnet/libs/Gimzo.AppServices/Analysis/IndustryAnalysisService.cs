using Dapper;
using Gimzo.Analysis.Technical.Charts;
using Gimzo.Infrastructure.Database;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Npgsql;
using System.Collections.Frozen;
using System.Collections.Immutable;

namespace Gimzo.AppServices.Analysis;

public readonly record struct IndustryScore(string SicCode, decimal ValueBillions, int Rank);

public class IndustryAnalysisService(
    DbDefPair dbDefPair,
    IMemoryCache memoryCache,
    ILogger<IndustryAnalysisService> logger) : ServiceBase(dbDefPair, memoryCache)
{
    private readonly ILogger<IndustryAnalysisService> _logger = logger;

    private readonly record struct SicCodeCashFlow(string Code, decimal Value);

    public async Task SaveAllIndustryScoresAsync(Guid processId)
    {
        if (processId == Guid.Empty)
            processId = Common.Constants.SystemId;

        var scores = await GetAllIndustryScoresAsync();

        var now = DateOnly.FromDateTime(DateTime.UtcNow);
        using var cmdCtx = _dbDefPair.GetCommandConnection();
        foreach (var chunk in scores.Chunk(Common.Constants.DefaultChunkSize))
        {
            await cmdCtx.ExecuteAsync(SqlRepository.MergeSicMoneyFlow,
                chunk.Select(k => new Infrastructure.Database.DataAccessObjects.SicMoneyFlow(processId)
                {
                    SicCode = k.SicCode,
                    DateEval = now,
                    FlowBillions = k.ValueBillions,
                    Rank = k.Rank
                }));
        }
    }

    public async Task<IReadOnlyCollection<IndustryScore>> GetAllIndustryScoresAsync()
    {
        var coMetricsTask = GetAllMarketCapsAsync();
        var prices = (await GetAllOhlcAsync()).GroupBy(g => g.Symbol).ToFrozenDictionary(g => g.Key, g => g.ToImmutableArray());
        var metrics = (await coMetricsTask).ToFrozenDictionary(m => m.Symbol!);
        var symbols = metrics.Keys.Intersect(prices.Keys).ToImmutableArray();

        List<SicCodeCashFlow> sicCashFlows = new(5 * symbols.Length);

        foreach (var symbol in symbols)
        {
            var m = metrics[symbol];
            if (string.IsNullOrWhiteSpace(m.SicCode))
                continue;

            var p = prices.GetValueOrDefault(symbol, []);

            var lastPrice = p.LastOrDefault()?.Close;
            var marketCap = m.MarketCap;
            if (marketCap == null && m.SharesOutstanding != null)
                marketCap = (decimal)m.SharesOutstanding * (p.LastOrDefault()?.Close ?? 0M);

            for (int i = p.Length - 1; i > 0; i--)
                sicCashFlows.Add(new(m.SicCode, (p[i].Close - p[i - 1].Close) * (decimal)p[i].Volume * marketCap.GetValueOrDefault()));
        }

        var sicGroup = sicCashFlows.GroupBy(g => g.Code).ToFrozenDictionary(g => g.Key,
            g => g.Select(r => r.Value).Sum() / (decimal)Math.Pow(10.0, 9.0)); // billions

        var results = new List<IndustryScore>(sicGroup.Count);

        var scores = sicGroup.Select(k => (k.Key, k.Value));
        var sorted = scores.OrderByDescending(k => k.Value).ToImmutableArray();

        for (int i = 0; i < sorted.Length; i++)
            results.Add(new(sorted[i].Key, sorted[i].Value, i + 1));

        return [.. results.OrderBy(k => k.Rank)];
    }

    private readonly record struct CompanyMetrics(string Symbol, decimal? MarketCap, double? SharesOutstanding, string? SicCode);

    private async Task<IReadOnlyCollection<CompanyMetrics>> GetAllMarketCapsAsync()
    {
        const string Sql = @"
WITH ranked AS (
    SELECT mc.symbol, mc.value, mc.shares_outstanding, c.sic_code,
    ROW_NUMBER() OVER (PARTITION BY mc.symbol ORDER BY mc.fiscal_year DESC) AS rn
    FROM public.market_caps mc JOIN public.us_companies c on mc.symbol = c.symbol
)
SELECT symbol, value AS MarketCap, shares_outstanding AS SharesOutstanding, sic_code AS SicCode FROM ranked
WHERE RN = 1;
";

        await using var ds = NpgsqlDataSource.Create(_dbDefPair.Query.ConnectionString);
        await using var connection = await ds.OpenConnectionAsync();
        await using var command = new NpgsqlCommand(Sql, connection);
        await using var reader = await command.ExecuteReaderAsync();

        var daos = new List<CompanyMetrics>(10000);

        while (await reader.ReadAsync())
        {
            daos.Add(new CompanyMetrics
            {
                Symbol = reader.GetString(0),
                MarketCap = reader.IsDBNull(1) ? null : (decimal)reader.GetDouble(1),
                SharesOutstanding = reader.IsDBNull(2) ? null : reader.GetDouble(2),
                SicCode = reader.IsDBNull(3) ? null : reader.GetString(3)
            });
        }

        return [.. daos];
    }

    private async Task<IReadOnlyCollection<Ohlc>> GetAllOhlcAsync()
    {
        const string Sql = @"
WITH ranked AS (
    SELECT *,
    ROW_NUMBER() OVER (PARTITION BY symbol ORDER BY date_eod DESC) AS rn
    FROM public.eod_prices
)
SELECT symbol, date_eod, open, high, low, close, volume FROM ranked
WHERE RN <= 6;
";

        await using var ds = NpgsqlDataSource.Create(_dbDefPair.Query.ConnectionString);
        await using var connection = await ds.OpenConnectionAsync();
        await using var command = new NpgsqlCommand(Sql, connection);
        await using var reader = await command.ExecuteReaderAsync();

        var daos = new List<Infrastructure.Database.DataAccessObjects.EodPrice>(10000);

        while (await reader.ReadAsync())
        {
            daos.Add(new Infrastructure.Database.DataAccessObjects.EodPrice
            {
                Symbol = reader.GetString(0),
                Date = DateOnly.FromDateTime(reader.GetDateTime(1)),
                Open = reader.GetDecimal(2),
                High = reader.GetDecimal(3),
                Low = reader.GetDecimal(4),
                Close = reader.GetDecimal(5),
                Volume = reader.GetDouble(6)
            });
        }
        return [.. daos.Select(k => k.ToOhlc())];
    }
}
