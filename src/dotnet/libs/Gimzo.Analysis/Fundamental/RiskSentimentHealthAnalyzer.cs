using System;

namespace Gimzo.Analysis.Fundamental;

public readonly record struct RiskSentimentAssessment(
    int Score1To99,
    string Assessment,
    double? OneYearBeta,
    double? DaysToCover);  // from latest short interest

public sealed class RiskSentimentHealthAnalyzer
{
    private static double NormalizeBeta(double value)
        => value <= 0.8 ? 1.0 :
           value <= 1.0 ? 0.85 :
           value <= 1.2 ? 0.65 :
           value <= 1.5 ? 0.4 :
           0.2;

    private static double NormalizeDaysToCover(double value)
        => value <= 3 ? 1.0 :
           value <= 5 ? 0.85 :
           value <= 8 ? 0.65 :
           value <= 12 ? 0.4 :
           0.2;

    public RiskSentimentAssessment Assess(KeyMetrics key, double? latestDaysToCover = null)
    {
        double sBeta = key.OneYearBeta.HasValue ? NormalizeBeta(key.OneYearBeta.Value) : 0.0;
        double sShort = latestDaysToCover.HasValue ? NormalizeDaysToCover(latestDaysToCover.Value) : 0.0;

        double composite = (sBeta + sShort) / (sShort > 0 ? 2.0 : 1.0);
        int score = (int)Math.Round(composite * 98 + 1);

        string assessment = score >= 85 ? "Low risk, positive sentiment" :
                            score >= 70 ? "Stable" :
                            score >= 55 ? "Moderate risk" :
                            score >= 40 ? "Elevated risk" :
                            "High risk/bearish";

        return new RiskSentimentAssessment(
            score,
            assessment,
            key.OneYearBeta,
            latestDaysToCover);
    }
}