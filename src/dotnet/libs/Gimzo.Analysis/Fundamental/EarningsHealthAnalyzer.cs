using System;

namespace Gimzo.Analysis.Fundamental;

public readonly record struct EarningsAssessment(
    int Score1To99,
    string Assessment,
    decimal? EarningsPerShare,
    decimal? EarningsPerShareForecast,
    double? EarningsGrowthRate,
    double? PriceEarningsToGrowthRate,
    double? ForwardPriceToEarningsRatio);

public sealed class EarningsHealthAnalyzer
{
    private static double NormalizeEps(decimal? value)
        => value >= 5m ? 1.0 :
           value >= 3m ? 0.85 :
           value >= 1m ? 0.65 :
           value > 0m ? 0.4 :
           value == 0m ? 0.2 : 0.0;

    private static double NormalizeGrowth(double value)
        => value >= 0.25 ? 1.0 :
           value >= 0.20 ? 0.85 :
           value >= 0.15 ? 0.65 :
           value >= 0.10 ? 0.4 :
           value > 0 ? 0.2 : 0.0;

    private static double NormalizePeg(double value)  // lower better
        => value <= 0.8 ? 1.0 :
           value <= 1.0 ? 0.85 :
           value <= 1.5 ? 0.65 :
           value <= 2.0 ? 0.4 :
           0.2;

    private static double NormalizeForwardPe(double value)  // lower better
        => value <= 12 ? 1.0 :
           value <= 15 ? 0.85 :
           value <= 20 ? 0.65 :
           value <= 30 ? 0.4 :
           0.2;

    public EarningsAssessment Assess(KeyMetrics metrics)
    {
        double sEps = metrics.EarningsPerShare.HasValue ? NormalizeEps(metrics.EarningsPerShare.Value) : 0.0;
        double sGrowth = metrics.EarningsGrowthRate.HasValue ? NormalizeGrowth(metrics.EarningsGrowthRate.Value) : 0.0;
        double sPeg = metrics.PriceEarningsToGrowthRate.HasValue ? NormalizePeg(metrics.PriceEarningsToGrowthRate.Value) : 0.0;
        double sFwdPe = metrics.ForwardPriceToEarningsRatio.HasValue ? NormalizeForwardPe(metrics.ForwardPriceToEarningsRatio.Value) : 0.0;

        double composite = (sEps + sGrowth + sPeg + sFwdPe) / 4.0;
        int score = (int)Math.Round(composite * 98 + 1);

        string assessment = score >= 85 ? "Exceptional earnings quality & growth" :
                            score >= 70 ? "Strong" :
                            score >= 55 ? "Solid" :
                            score >= 40 ? "Fair" :
                            score >= 25 ? "Weak" :
                            "Loss-making/Poor";

        return new EarningsAssessment(
            score,
            assessment,
            metrics.EarningsPerShare,
            metrics.EarningsPerShareForecast,
            metrics.EarningsGrowthRate,
            metrics.PriceEarningsToGrowthRate,
            metrics.ForwardPriceToEarningsRatio);
    }
}