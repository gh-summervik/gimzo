using Dapper;
using Gimzo.Analysis.Fundamental;
using Gimzo.Infrastructure.Database;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

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

        if (liquidity is null || solvency is null || profitability is null || keyMetrics is null || efficiency is null)
            return null;

        BalanceSheetAssessment balance = _balanceAnalyzer.Assess(liquidity, solvency);
        IncomeStatementAssessment income = _incomeAnalyzer.Assess(profitability);
        CashFlowAssessment cashFlow = _cashFlowAnalyzer.Assess(profitability);
        EarningsAssessment earnings = _earningsAnalyzer.Assess(keyMetrics);
        ValuationAssessment val = _valuationAnalyzer.Assess(keyMetrics, valuation);
        EfficiencyAssessment eff = _efficiencyAnalyzer.Assess(efficiency);

        double? latestDaysToCover = shortInterest?.DaysToCover;
        RiskSentimentAssessment risk = _riskAnalyzer.Assess(keyMetrics, latestDaysToCover);

        return _compositeAnalyzer.ComputeSiloScore(balance, income, cashFlow, earnings, val, eff, risk);
    }

    internal async Task<LiquidityRatios?> GetLatestLiquidityRatiosAsync(string symbol)
    {
        string sql = $@"{SqlRepository.SelectLiquidityRatios}
WHERE symbol = @Symbol AND fiscal_period IN ('Q1','Q2','Q3','Q4')
ORDER BY period_end_date DESC LIMIT 1";

        using var queryCtx = _dbDefPair.GetQueryConnection();
        var result = await queryCtx.QueryFirstOrDefaultAsync<Infrastructure.Database.DataAccessObjects.LiquidityRatios>(sql,
            new { symbol });

        return result?.ToDomain();
    }

    internal async Task<SolvencyRatios?> GetLatestSolvencyRatiosAsync(string symbol)
    {
        string sql = $@"{SqlRepository.SelectSolvencyRatios}
WHERE symbol = @Symbol AND fiscal_period IN ('Q1','Q2','Q3','Q4')
ORDER BY period_end_date DESC LIMIT 1";

        using var queryCtx = _dbDefPair.GetQueryConnection();
        var result = await queryCtx.QueryFirstOrDefaultAsync<Infrastructure.Database.DataAccessObjects.SolvencyRatios>(sql, new { symbol });

        return result?.ToDomain();
    }

    internal async Task<ProfitabilityRatios?> GetLatestProfitabilityRatiosAsync(string symbol)
    {
        string sql = $@"{SqlRepository.SelectProfitabilityRatios}
WHERE symbol = @Symbol AND fiscal_period IN ('Q1','Q2','Q3','Q4')
ORDER BY period_end_date DESC LIMIT 1";

        using var queryCtx = _dbDefPair.GetQueryConnection();
        var result = await queryCtx.QueryFirstOrDefaultAsync<Infrastructure.Database.DataAccessObjects.ProfitabilityRatios>(sql, new { symbol });

        return result?.ToDomain();
    }

    internal async Task<KeyMetrics?> GetLatestKeyMetricsAsync(string symbol)
    {
        string sql = $@"{SqlRepository.SelectKeyMetrics}
WHERE symbol = @Symbol ORDER BY period_end_date DESC LIMIT 1";

        using var queryCtx = _dbDefPair.GetQueryConnection();
        var result = await queryCtx.QueryFirstOrDefaultAsync<Infrastructure.Database.DataAccessObjects.KeyMetrics>(sql, new { symbol });

        return result?.ToDomain();
    }

    internal async Task<ValuationRatios?> GetLatestValuationRatiosAsync(string symbol)
    {
        string sql = $@"{SqlRepository.SelectValuationRatios}
WHERE symbol = @Symbol AND fiscal_period IN ('Q1','Q2','Q3','Q4')
ORDER BY period_end_date DESC LIMIT 1";

        using var queryCtx = _dbDefPair.GetQueryConnection();
        var result = await queryCtx.QueryFirstOrDefaultAsync<Infrastructure.Database.DataAccessObjects.ValuationRatios>(sql, new { symbol });

        return result?.ToDomain();
    }

    internal async Task<EfficiencyRatios?> GetLatestEfficiencyRatiosAsync(string symbol)
    {
        string sql = $@"{SqlRepository.SelectEfficiencyRatios}
WHERE symbol = @Symbol AND fiscal_period IN ('Q1','Q2','Q3','Q4')
ORDER BY period_end_date DESC LIMIT 1";

        using var queryCtx = _dbDefPair.GetQueryConnection();
        var result = await queryCtx.QueryFirstOrDefaultAsync<Infrastructure.Database.DataAccessObjects.EfficiencyRatios>(sql, new { symbol });

        return result?.ToDomain();
    }

    internal async Task<ShortInterest?> GetLatestShortInterestAsync(string symbol)
    {
        string sql = $@"{SqlRepository.SelectShortInterests} WHERE symbol = @Symbol
ORDER BY settlement_date DESC LIMIT 1";

        using var queryCtx = _dbDefPair.GetQueryConnection();
        var result = await queryCtx.QueryFirstOrDefaultAsync<Infrastructure.Database.DataAccessObjects.ShortInterest>(sql, new { symbol });

        return result?.ToDomain();
    }

}