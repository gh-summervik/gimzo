using Gimzo.AppServices.Data;
using Gimzo.Infrastructure.Database;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Gimzo.Analysis.Fundamental;

namespace Gimzo.AppServices.Analysis;

public sealed class CompanyAnalysisService(
    DbDefPair dbDefPair,
    IMemoryCache memoryCache,
    ILogger<CompanyAnalysisService> logger)
{
    private readonly DbDefPair _dbDefPair = dbDefPair;
    private readonly IMemoryCache _memoryCache = memoryCache;
    private readonly ILogger<CompanyAnalysisService> _logger = logger;
    private readonly DataService _dataService = new(dbDefPair, memoryCache, logger);

    private readonly BalanceSheetHealthAnalyzer _balanceAnalyzer = new();
    private readonly IncomeStatementHealthAnalyzer _incomeAnalyzer = new();
    private readonly CashFlowHealthAnalyzer _cashFlowAnalyzer = new();
    private readonly EarningsHealthAnalyzer _earningsAnalyzer = new();
    private readonly ValuationHealthAnalyzer _valuationAnalyzer = new();
    private readonly EfficiencyHealthAnalyzer _efficiencyAnalyzer = new();
    private readonly RiskSentimentHealthAnalyzer _riskAnalyzer = new();
    private readonly CompanyFundamentalAnalyzer _compositeAnalyzer = new();

    public async Task<CompanySiloScore?> GetSiloScoreAsync(string centralIndexKey)
    {
        LiquidityRatios? liquidity = await _dataService.GetLatestLiquidityRatiosAsync(centralIndexKey);
        SolvencyRatios? solvency = await _dataService.GetLatestSolvencyRatiosAsync(centralIndexKey);
        ProfitabilityRatios? profitability = await _dataService.GetLatestProfitabilityRatiosAsync(centralIndexKey);
        KeyMetrics? keyMetrics = await _dataService.GetLatestKeyMetricsAsync(centralIndexKey);
        ValuationRatios? valuation = await _dataService.GetLatestValuationRatiosAsync(centralIndexKey);
        EfficiencyRatios? efficiency = await _dataService.GetLatestEfficiencyRatiosAsync(centralIndexKey);
        ShortInterest? shortInterest = await _dataService.GetLatestShortInterestsAsync(centralIndexKey);

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
}