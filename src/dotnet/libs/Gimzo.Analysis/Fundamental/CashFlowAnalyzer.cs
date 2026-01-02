using System;

namespace Gimzo.Analysis.Fundamental;

public readonly record struct CashFlowAssessment(
    int Score1To99,
    string Assessment,
    double? OperatingCashFlowMargin,
    double? CashReturnOnAssets,
    double? CashTurnoverRatio);

public sealed class CashFlowHealthAnalyzer
{
    private static double NormalizeOcfMargin(double value)
        => value >= 0.25 ? 1.0 :
           value >= 0.20 ? 0.85 :
           value >= 0.15 ? 0.65 :
           value >= 0.10 ? 0.4 :
           value > 0 ? 0.2 : 0.0;

    private static double NormalizeCashRoa(double value)
        => value >= 0.12 ? 1.0 :
           value >= 0.08 ? 0.85 :
           value >= 0.05 ? 0.65 :
           value >= 0.02 ? 0.4 :
           value > 0 ? 0.2 : 0.0;

    private static double NormalizeCashTurnover(double value)
        => value >= 8.0 ? 1.0 :
           value >= 6.0 ? 0.85 :
           value >= 4.0 ? 0.65 :
           value >= 2.0 ? 0.4 :
           value > 0 ? 0.2 : 0.0;

    public CashFlowAssessment Assess(ProfitabilityRatios profitability)
    {
        double sOcf = profitability.OperatingCashFlowMargin.HasValue ? NormalizeOcfMargin(profitability.OperatingCashFlowMargin.Value) : 0.0;
        double sCashRoa = profitability.CashReturnOnAssets.HasValue ? NormalizeCashRoa(profitability.CashReturnOnAssets.Value) : 0.0;
        double sTurnover = profitability.CashTurnoverRatio.HasValue ? NormalizeCashTurnover(profitability.CashTurnoverRatio.Value) : 0.0;

        double composite = (sOcf + sCashRoa + sTurnover) / 3.0;
        int score = (int)Math.Round(composite * 98 + 1);

        string assessment = score >= 85 ? "Exceptional cash generation" :
                            score >= 70 ? "Strong" :
                            score >= 55 ? "Solid" :
                            score >= 40 ? "Adequate" :
                            score >= 25 ? "Weak" :
                            "Poor/Negative cash flow";

        return new CashFlowAssessment(
            score,
            assessment,
            profitability.OperatingCashFlowMargin,
            profitability.CashReturnOnAssets,
            profitability.CashTurnoverRatio);
    }
}