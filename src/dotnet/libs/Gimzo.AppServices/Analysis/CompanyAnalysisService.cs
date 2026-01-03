using Dapper;
using Gimzo.Analysis.Fundamental;
using Gimzo.Infrastructure.Database;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System.Collections.Frozen;
using System.Collections.Immutable;

namespace Gimzo.AppServices.Analysis;

public sealed class CompanyAnalysisService(
    DbDefPair dbDefPair,
    IMemoryCache memoryCache,
    ILogger<CompanyAnalysisService> logger) : ServiceBase(dbDefPair, memoryCache)
{
    private readonly ILogger<CompanyAnalysisService> _logger = logger;

    private readonly BalanceSheetHealthAnalyzer _balanceAnalyzer = new();
    private readonly IncomeStatementHealthAnalyzer _incomeAnalyzer = new();
    private readonly CashFlowHealthAnalyzer _cashFlowAnalyzer = new();
    private readonly EarningsHealthAnalyzer _earningsAnalyzer = new();
    private readonly ValuationHealthAnalyzer _valuationAnalyzer = new();
    private readonly EfficiencyHealthAnalyzer _efficiencyAnalyzer = new();
    private readonly RiskSentimentHealthAnalyzer _riskAnalyzer = new();
    private readonly GrowthHealthAnalyzer _growthAnalyzer = new();
    private readonly CompanyFundamentalAnalyzer _compositeAnalyzer = new();

    public async Task<CompanySiloScore?> GetSiloScoreForSymbolAsync(string symbol)
    {
        LiquidityRatios? liquidity = await GetLatestLiquidityRatiosAsync(symbol);
        SolvencyRatios? solvency = await GetLatestSolvencyRatiosAsync(symbol);
        ProfitabilityRatios? profitability = await GetLatestProfitabilityRatiosAsync(symbol);
        KeyMetrics? keyMetrics = await GetLatestKeyMetricsAsync(symbol);
        ValuationRatios? valuation = await GetLatestValuationRatiosAsync(symbol);
        EfficiencyRatios? efficiency = await GetLatestEfficiencyRatiosAsync(symbol);
        ShortInterest? shortInterest = await GetLatestShortInterestAsync(symbol);
        IReadOnlyCollection<QuarterlyGrowthInput> growthInputs = await GetLatestFiveQuarterlyGrowthInputsAsync(symbol);

        if (liquidity is null || solvency is null || profitability is null || keyMetrics is null || efficiency is null)
            return null;

        BalanceSheetAssessment balance = _balanceAnalyzer.Assess(liquidity, solvency);
        IncomeStatementAssessment income = _incomeAnalyzer.Assess(profitability, efficiency);
        CashFlowAssessment cashFlow = _cashFlowAnalyzer.Assess(profitability);
        EarningsAssessment earnings = _earningsAnalyzer.Assess(keyMetrics);
        ValuationAssessment val = _valuationAnalyzer.Assess(keyMetrics, valuation);
        EfficiencyAssessment eff = _efficiencyAnalyzer.Assess(efficiency);
        GrowthAssessment growth = GrowthHealthAnalyzer.Assess(growthInputs);

        double? latestDaysToCover = shortInterest?.DaysToCover;
        RiskSentimentAssessment risk = _riskAnalyzer.Assess(keyMetrics, latestDaysToCover);

        return _compositeAnalyzer.ComputeSiloScore(
            balance,
            income,
            cashFlow,
            earnings,
            val,
            eff,
            risk,
            growth);
    }

    public async Task<IReadOnlyCollection<(string Symbol, CompanySiloScore Score)>> GetAllSiloScoresAsync()
    {
        var liquidityTask = GetAllLatestLiquidityRatiosAsync();
        var solvencyTask = GetAllLatestSolvencyRatiosAsync();
        var profitabilityTask = GetAllLatestProfitabilityRatiosAsync();
        var keyMetricsTask = GetAllLatestKeyMetricsAsync();
        var valuationTask = GetAllLatestValuationRatiosAsync();
        var efficiencyTask = GetAllLatestEfficiencyRatiosAsync();
        var shortInterestTask = GetAllLatestShortInterestAsync();
        var growthTask = GetAllLatestFiveQuarterlyGrowthInputsAsync();

        await Task.WhenAll(
            liquidityTask,
            solvencyTask,
            profitabilityTask,
            keyMetricsTask,
            valuationTask,
            efficiencyTask,
            shortInterestTask,
            growthTask);
        var liquidity = liquidityTask.Result.ToFrozenDictionary(r => r.Symbol!);
        var solvency = solvencyTask.Result.ToFrozenDictionary(r => r.Symbol!);
        var profitability = profitabilityTask.Result.ToFrozenDictionary(r => r.Symbol!);
        var keyMetrics = keyMetricsTask.Result.ToFrozenDictionary(r => r.Symbol!);
        var valuation = valuationTask.Result.ToFrozenDictionary(r => r!.Symbol!, r => r);
        var efficiency = efficiencyTask.Result.ToFrozenDictionary(r => r.Symbol!);
        var shortInterest = shortInterestTask.Result.ToFrozenDictionary(r => r.Symbol!, r => r);
        var growthGroups = growthTask.Result
            .GroupBy(g => g.Symbol!)
            .ToFrozenDictionary(g => g.Key, g => g.ToImmutableArray());

        var symbols = liquidity.Keys
            .Intersect(solvency.Keys)
            .Intersect(profitability.Keys)
            .Intersect(keyMetrics.Keys)
            .Intersect(efficiency.Keys);

        var results = new List<(string Symbol, CompanySiloScore Score)>(symbols.Count());

        foreach (var symbol in symbols)
        {
            var l = liquidity[symbol];
            var s = solvency[symbol];
            var p = profitability[symbol];
            var k = keyMetrics[symbol];
            var v = valuation.GetValueOrDefault(symbol);
            var e = efficiency[symbol];
            var si = shortInterest.GetValueOrDefault(symbol);
            var gi = growthGroups.GetValueOrDefault(symbol, []);

            BalanceSheetAssessment balance = _balanceAnalyzer.Assess(l, s);
            IncomeStatementAssessment income = _incomeAnalyzer.Assess(p, e);
            CashFlowAssessment cashFlow = _cashFlowAnalyzer.Assess(p);
            EarningsAssessment earnings = _earningsAnalyzer.Assess(k);
            ValuationAssessment val = _valuationAnalyzer.Assess(k, v);
            EfficiencyAssessment eff = _efficiencyAnalyzer.Assess(e);
            GrowthAssessment growth = GrowthHealthAnalyzer.Assess(gi);

            double? daysToCover = si?.DaysToCover;
            RiskSentimentAssessment risk = _riskAnalyzer.Assess(k, daysToCover);

            var score = _compositeAnalyzer.ComputeSiloScore(balance, income, cashFlow, earnings, val, eff, risk, growth);

            results.Add((symbol, score));
        }

        return results.ToFrozenSet();
    }

    private async Task<IReadOnlyCollection<LiquidityRatios>> GetAllLatestLiquidityRatiosAsync()
    {
        const string Sql = @"
WITH ranked AS (
    SELECT *, ROW_NUMBER() OVER (PARTITION BY symbol ORDER BY period_end_date DESC) AS rn
    FROM public.liquidity_ratios
    WHERE fiscal_period IN ('Q1','Q2','Q3','Q4')
)
SELECT * FROM ranked WHERE rn = 1";

        using var queryCtx = _dbDefPair.GetQueryConnection();
        var daos = await queryCtx.QueryAsync<Infrastructure.Database.DataAccessObjects.LiquidityRatios>(Sql);
        return [.. daos.Select(d => d.ToDomain())];
    }

    private async Task<IReadOnlyCollection<SolvencyRatios>> GetAllLatestSolvencyRatiosAsync()
    {
        const string Sql = @"
WITH ranked AS (
    SELECT *, ROW_NUMBER() OVER (PARTITION BY symbol ORDER BY period_end_date DESC) AS rn
    FROM public.solvency_ratios
    WHERE fiscal_period IN ('Q1','Q2','Q3','Q4')
)
SELECT * FROM ranked WHERE rn = 1";

        using var queryCtx = _dbDefPair.GetQueryConnection();
        var daos = await queryCtx.QueryAsync<Infrastructure.Database.DataAccessObjects.SolvencyRatios>(Sql);
        return [.. daos.Select(d => d.ToDomain())];
    }

    private async Task<IReadOnlyCollection<ProfitabilityRatios>> GetAllLatestProfitabilityRatiosAsync()
    {
        const string Sql = @"
WITH ranked AS (
    SELECT *, ROW_NUMBER() OVER (PARTITION BY symbol ORDER BY period_end_date DESC) AS rn
    FROM public.profitability_ratios
    WHERE fiscal_period IN ('Q1','Q2','Q3','Q4')
)
SELECT * FROM ranked WHERE rn = 1";

        using var queryCtx = _dbDefPair.GetQueryConnection();
        var daos = await queryCtx.QueryAsync<Infrastructure.Database.DataAccessObjects.ProfitabilityRatios>(Sql);
        return [.. daos.Select(d => d.ToDomain())];
    }

    private async Task<IReadOnlyCollection<KeyMetrics>> GetAllLatestKeyMetricsAsync()
    {
        const string Sql = @"
WITH ranked AS (
    SELECT *, ROW_NUMBER() OVER (PARTITION BY symbol ORDER BY period_end_date DESC) AS rn
    FROM public.key_metrics
)
SELECT * FROM ranked WHERE rn = 1";

        using var queryCtx = _dbDefPair.GetQueryConnection();
        var daos = await queryCtx.QueryAsync<Infrastructure.Database.DataAccessObjects.KeyMetrics>(Sql);
        return [.. daos.Select(d => d.ToDomain())];
    }

    private async Task<IReadOnlyCollection<ValuationRatios?>> GetAllLatestValuationRatiosAsync()
    {
        const string Sql = @"
WITH ranked AS (
    SELECT *, ROW_NUMBER() OVER (PARTITION BY symbol ORDER BY period_end_date DESC) AS rn
    FROM public.valuation_ratios
    WHERE fiscal_period IN ('Q1','Q2','Q3','Q4')
)
SELECT * FROM ranked WHERE rn = 1";

        using var queryCtx = _dbDefPair.GetQueryConnection();
        var daos = await queryCtx.QueryAsync<Infrastructure.Database.DataAccessObjects.ValuationRatios?>(Sql);
        return [.. daos.Select(d => d?.ToDomain())];
    }

    private async Task<IReadOnlyCollection<EfficiencyRatios>> GetAllLatestEfficiencyRatiosAsync()
    {
        const string Sql = @"
WITH ranked AS (
    SELECT *, ROW_NUMBER() OVER (PARTITION BY symbol ORDER BY period_end_date DESC) AS rn
    FROM public.efficiency_ratios
    WHERE fiscal_period IN ('Q1','Q2','Q3','Q4')
)
SELECT * FROM ranked WHERE rn = 1";

        using var queryCtx = _dbDefPair.GetQueryConnection();
        var daos = await queryCtx.QueryAsync<Infrastructure.Database.DataAccessObjects.EfficiencyRatios>(Sql);
        return [.. daos.Select(d => d.ToDomain())];
    }

    private async Task<IReadOnlyCollection<ShortInterest>> GetAllLatestShortInterestAsync()
    {
        const string Sql = @"
WITH ranked AS (
    SELECT *, ROW_NUMBER() OVER (PARTITION BY symbol ORDER BY settlement_date DESC) AS rn
    FROM public.short_interests
)
SELECT * FROM ranked WHERE rn = 1";

        using var queryCtx = _dbDefPair.GetQueryConnection();
        var daos = await queryCtx.QueryAsync<Infrastructure.Database.DataAccessObjects.ShortInterest>(Sql);
        return [.. daos.Select(d => d.ToDomain())];
    }

    private async Task<IReadOnlyCollection<QuarterlyGrowthInput>> GetAllLatestFiveQuarterlyGrowthInputsAsync()
    {
        const string Sql = @"
WITH ranked AS (
    SELECT symbol,
           period_end_date,
           revenue,
           earnings_per_share_diluted,
           ROW_NUMBER() OVER (PARTITION BY symbol ORDER BY period_end_date DESC) AS rn
    FROM public.income_statements
    WHERE fiscal_period IN ('Q1','Q2','Q3','Q4')
)
SELECT symbol AS Symbol,
       period_end_date AS PeriodEndDate,
       revenue AS Revenue,
       earnings_per_share_diluted AS EarningsPerShareDiluted
FROM ranked
WHERE rn <= 5";

        using var queryCtx = _dbDefPair.GetQueryConnection();
        var daos = await queryCtx.QueryAsync<Infrastructure.Database.DataAccessObjects.QuarterlyGrowthInput>(Sql);
        return [.. daos.Select(d => d.ToDomain())];
    }

    internal async Task<LiquidityRatios?> GetLatestLiquidityRatiosAsync(string symbol)
    {
        string sql = $@"{SqlRepository.SelectLiquidityRatios}
WHERE symbol = @Symbol AND fiscal_period IN ('Q1','Q2','Q3','Q4')
ORDER BY period_end_date DESC LIMIT 1";

        using var queryCtx = _dbDefPair.GetQueryConnection();
        var result = await queryCtx.QueryFirstOrDefaultAsync<Infrastructure.Database.DataAccessObjects.LiquidityRatios>(sql,
            new { Symbol = symbol });

        return result?.ToDomain();
    }

    internal async Task<SolvencyRatios?> GetLatestSolvencyRatiosAsync(string symbol)
    {
        string sql = $@"{SqlRepository.SelectSolvencyRatios}
WHERE symbol = @Symbol AND fiscal_period IN ('Q1','Q2','Q3','Q4')
ORDER BY period_end_date DESC LIMIT 1";

        using var queryCtx = _dbDefPair.GetQueryConnection();
        var result = await queryCtx.QueryFirstOrDefaultAsync<Infrastructure.Database.DataAccessObjects.SolvencyRatios>(sql,
            new { Symbol = symbol });

        return result?.ToDomain();
    }

    internal async Task<ProfitabilityRatios?> GetLatestProfitabilityRatiosAsync(string symbol)
    {
        string sql = $@"{SqlRepository.SelectProfitabilityRatios}
WHERE symbol = @Symbol AND fiscal_period IN ('Q1','Q2','Q3','Q4')
ORDER BY period_end_date DESC LIMIT 1";

        using var queryCtx = _dbDefPair.GetQueryConnection();
        var result = await queryCtx.QueryFirstOrDefaultAsync<Infrastructure.Database.DataAccessObjects.ProfitabilityRatios>(sql,
            new { Symbol = symbol });

        return result?.ToDomain();
    }

    internal async Task<KeyMetrics?> GetLatestKeyMetricsAsync(string symbol)
    {
        string sql = $@"{SqlRepository.SelectKeyMetrics}
WHERE symbol = @Symbol ORDER BY period_end_date DESC LIMIT 1";

        using var queryCtx = _dbDefPair.GetQueryConnection();
        var result = await queryCtx.QueryFirstOrDefaultAsync<Infrastructure.Database.DataAccessObjects.KeyMetrics>(sql,
            new { Symbol = symbol });

        return result?.ToDomain();
    }

    internal async Task<ValuationRatios?> GetLatestValuationRatiosAsync(string symbol)
    {
        string sql = $@"{SqlRepository.SelectValuationRatios}
WHERE symbol = @Symbol AND fiscal_period IN ('Q1','Q2','Q3','Q4')
ORDER BY period_end_date DESC LIMIT 1";

        using var queryCtx = _dbDefPair.GetQueryConnection();
        var result = await queryCtx.QueryFirstOrDefaultAsync<Infrastructure.Database.DataAccessObjects.ValuationRatios>(sql,
            new { Symbol = symbol });

        return result?.ToDomain();
    }

    internal async Task<EfficiencyRatios?> GetLatestEfficiencyRatiosAsync(string symbol)
    {
        string sql = $@"{SqlRepository.SelectEfficiencyRatios}
WHERE symbol = @Symbol AND fiscal_period IN ('Q1','Q2','Q3','Q4')
ORDER BY period_end_date DESC LIMIT 1";

        using var queryCtx = _dbDefPair.GetQueryConnection();
        var result = await queryCtx.QueryFirstOrDefaultAsync<Infrastructure.Database.DataAccessObjects.EfficiencyRatios>(sql,
            new { Symbol = symbol });

        return result?.ToDomain();
    }

    internal async Task<ShortInterest?> GetLatestShortInterestAsync(string symbol)
    {
        string sql = $@"{SqlRepository.SelectShortInterests}
WHERE symbol = @Symbol
ORDER BY settlement_date DESC LIMIT 1";

        using var queryCtx = _dbDefPair.GetQueryConnection();
        var result = await queryCtx.QueryFirstOrDefaultAsync<Infrastructure.Database.DataAccessObjects.ShortInterest>(sql,
            new { Symbol = symbol });

        return result?.ToDomain();
    }

    internal async Task<IReadOnlyCollection<QuarterlyGrowthInput>> GetLatestFiveQuarterlyGrowthInputsAsync(string symbol)
    {
        const string Sql = @"
SELECT
    period_end_date AS PeriodEndDate,
    revenue AS Revenue,
    earnings_per_share_diluted AS EarningsPerShareDiluted
FROM public.income_statements
WHERE symbol = @Symbol
  AND fiscal_period IN ('Q1','Q2','Q3','Q4')
ORDER BY period_end_date DESC
LIMIT 5";

        using var queryCtx = _dbDefPair.GetQueryConnection();
        var daos = await queryCtx.QueryAsync<Infrastructure.Database.DataAccessObjects.QuarterlyGrowthInput>(Sql,
            new { Symbol = symbol.ToUpperInvariant() });

        return [.. daos.Select(d => d.ToDomain())];
    }
}


