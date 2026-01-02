using System.Collections.ObjectModel;

namespace Gimzo.Analysis.Fundamental;

public sealed class CompanyAnalysisOrchestrator
{
    private readonly BalanceSheetHealthAnalyzer _balanceAnalyzer = new();
    private readonly IncomeStatementHealthAnalyzer _incomeAnalyzer = new();
    private readonly CashFlowHealthAnalyzer _cashFlowAnalyzer = new();
    private readonly EarningsHealthAnalyzer _earningsAnalyzer = new();
    private readonly ValuationHealthAnalyzer _valuationAnalyzer = new();
    private readonly EfficiencyHealthAnalyzer _efficiencyAnalyzer = new();
    private readonly RiskSentimentHealthAnalyzer _riskAnalyzer = new();
    private readonly CompanyFundamentalAnalyzer _compositeAnalyzer = new();

    public CompanySiloScore AnalyzeCompany(
        LiquidityRatios latestLiquidity,
        SolvencyRatios latestSolvency,
        ProfitabilityRatios latestProfitability,
        KeyMetrics latestKeyMetrics,
        ValuationRatios? latestValuation,
        EfficiencyRatios latestEfficiency,
        IReadOnlyCollection<ShortInterest> shortInterests)  // from DB query
    {
        var balance = _balanceAnalyzer.Assess(latestLiquidity, latestSolvency);
        var income = _incomeAnalyzer.Assess(latestProfitability);
        var cashFlow = _cashFlowAnalyzer.Assess(latestProfitability);
        var earnings = _earningsAnalyzer.Assess(latestKeyMetrics);
        var valuation = _valuationAnalyzer.Assess(latestKeyMetrics, latestValuation);
        var efficiency = _efficiencyAnalyzer.Assess(latestEfficiency);

        double? latestDaysToCover = shortInterests
            .OrderByDescending(si => si.SettlementDate)
            .FirstOrDefault()?.DaysToCover;

        var risk = _riskAnalyzer.Assess(latestKeyMetrics, latestDaysToCover);

        return _compositeAnalyzer.ComputeSiloScore(
            balance,
            income,
            cashFlow,
            earnings,
            valuation,
            efficiency,
            risk);
    }
}