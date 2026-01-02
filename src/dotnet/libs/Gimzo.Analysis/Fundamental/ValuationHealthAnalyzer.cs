using System;

namespace Gimzo.Analysis.Fundamental;

public readonly record struct ValuationAssessment(
    int Score1To99,
    string Assessment,
    double? PriceToBookRatio,
    double? DividendYield,
    double? DividendPayoutRatio,
    double? RetentionRatio);

public sealed class ValuationHealthAnalyzer
{
    private static double NormalizePb(double value)  // lower better
        => value <= 1.0 ? 1.0 :
           value <= 1.5 ? 0.85 :
           value <= 2.0 ? 0.65 :
           value <= 3.0 ? 0.4 :
           0.2;

    private static double NormalizeYield(double value)
        => value >= 0.05 ? 1.0 :
           value >= 0.04 ? 0.85 :
           value >= 0.03 ? 0.65 :
           value >= 0.02 ? 0.4 :
           value > 0 ? 0.2 : 0.0;

    private static double NormalizePayout(double value)  // reasonable range
        => value <= 0.3 ? 1.0 :
           value <= 0.5 ? 0.85 :
           value <= 0.7 ? 0.65 :
           value <= 0.9 ? 0.4 :
           0.2;

    private static double NormalizeRetention(double value)
        => value >= 0.7 ? 1.0 :
           value >= 0.5 ? 0.85 :
           value >= 0.3 ? 0.65 :
           value > 0 ? 0.4 :
           0.2;

    public ValuationAssessment Assess(KeyMetrics key, ValuationRatios? valuation = null)
    {
        double sPb = key.PriceToBookRatio.HasValue ? NormalizePb(key.PriceToBookRatio.Value) : 0.0;
        double sYield = key.DividendYield.HasValue ? NormalizeYield(key.DividendYield.Value) : 0.0;
        double sPayout = key.DividendPayoutRatio.HasValue ? NormalizePayout(key.DividendPayoutRatio.Value) : 0.0;
        double sRetention = valuation?.RetentionRatio.HasValue == true ? NormalizeRetention(valuation.RetentionRatio.Value) : 0.0;

        double composite = (sPb + sYield + sPayout + sRetention) / (sRetention > 0 ? 4.0 : 3.0);
        int score = (int)Math.Round(composite * 98 + 1);

        string assessment = score >= 85 ? "Deep value" :
                            score >= 70 ? "Attractive" :
                            score >= 55 ? "Fairly valued" :
                            score >= 40 ? "Fully valued" :
                            "Expensive";

        return new ValuationAssessment(
            score,
            assessment,
            key.PriceToBookRatio,
            key.DividendYield,
            key.DividendPayoutRatio,
            valuation?.RetentionRatio);
    }
}