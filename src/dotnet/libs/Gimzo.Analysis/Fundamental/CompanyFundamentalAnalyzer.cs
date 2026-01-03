using System.Collections.Frozen;
using System.Collections.ObjectModel;

namespace Gimzo.Analysis.Fundamental;

public readonly record struct CompanySiloScore(
    int Score1To99,
    string OverallAssessment,
    IReadOnlyDictionary<string, int> SubScores);  // key = analyzer name, value = 1-99

public sealed class CompanyFundamentalAnalyzer
{
    private readonly FrozenDictionary<string, double> _weights;

    public CompanyFundamentalAnalyzer(IReadOnlyDictionary<string, double>? customWeights = null)
    {
        var defaultWeights = new Dictionary<string, double>
        {
            ["BalanceSheet"] = 0.20,
            ["IncomeStatement"] = 0.15,
            ["CashFlow"] = 0.10,
            ["Earnings"] = 0.10,
            ["Valuation"] = 0.10,
            ["Efficiency"] = 0.10,
            ["Growth"] = 0.15,
            ["RiskSentiment"] = 0.10
        };

        _weights = (customWeights ?? defaultWeights).ToFrozenDictionary();
    }

    public CompanySiloScore ComputeSiloScore(
        BalanceSheetAssessment balanceSheet,
        IncomeStatementAssessment income,
        CashFlowAssessment cashFlow,
        EarningsAssessment earnings,
        ValuationAssessment valuation,
        EfficiencyAssessment efficiency,
        RiskSentimentAssessment riskSentiment,
        GrowthAssessment growth)
    {
        var subScores = new Dictionary<string, int>
        {
            ["BalanceSheet"] = balanceSheet.Score1To99,
            ["IncomeStatement"] = income.Score1To99,
            ["CashFlow"] = cashFlow.Score1To99,
            ["Earnings"] = earnings.Score1To99,
            ["Valuation"] = valuation.Score1To99,
            ["Efficiency"] = efficiency.Score1To99,
            ["RiskSentiment"] = riskSentiment.Score1To99,
            ["Growth"] = growth.Score1To99
        };

        double weightedSum = 0.0;
        double totalWeight = 0.0;

        foreach (var kvp in _weights)
        {
            if (subScores.TryGetValue(kvp.Key, out int subScore))
            {
                weightedSum += subScore * kvp.Value;
                totalWeight += kvp.Value;
            }
        }

        double composite = totalWeight > 0 ? weightedSum / totalWeight : 0.0;
        int score = (int)Math.Round(composite);

        string assessment = score >= 85 ? "Elite" :
                            score >= 70 ? "Excellent" :
                            score >= 55 ? "Strong" :
                            score >= 40 ? "Solid" :
                            score >= 25 ? "Average" :
                            "Weak";

        return new CompanySiloScore(score, assessment, new ReadOnlyDictionary<string, int>(subScores));
    }
}