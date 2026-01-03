namespace Gimzo.Analysis.Fundamental;

// New analyzer
public readonly record struct GrowthAssessment(
    int Score1To99,
    string Assessment,
    double? RevenueGrowthPct,
    double? EpsGrowthPct);

public sealed record QuarterlyIncome(
    DateOnly PeriodEndDate,
    decimal? Revenue,
    decimal? EarningsPerShareDiluted);

public sealed class GrowthHealthAnalyzer
{
    private static double NormalizeRevenueGrowth(double value)
        => value >= 30 ? 1.0 :
           value >= 20 ? 0.85 :
           value >= 10 ? 0.65 :
           value >= 0 ? 0.4 : 0.2;

    private static double NormalizeEpsGrowth(double value)
        => value >= 50 ? 1.0 :
           value >= 25 ? 0.95 :  // CANSLIM-style strong boost
           value >= 10 ? 0.7 :
           value >= 0 ? 0.4 : 0.2;

    public static GrowthAssessment Assess(IReadOnlyCollection<QuarterlyGrowthInput> history)
    {
        if (history.Count < 2)
            return new GrowthAssessment(1, "Insufficient data", null, null);

        var ordered = history.OrderByDescending(h => h.PeriodEndDate).ToArray();
        var latest = ordered[0];
        var prior = ordered[1];

        double revenueGrowth = prior.Revenue > 0
            ? (double)((latest.Revenue.GetValueOrDefault() - prior.Revenue.Value) / prior.Revenue.Value * 100)
            : 0;

        double epsGrowth = prior.EarningsPerShareDiluted > 0
            ? (double)((latest.EarningsPerShareDiluted.GetValueOrDefault() - prior.EarningsPerShareDiluted.Value) /
                       Math.Abs(prior.EarningsPerShareDiluted.Value) * 100)
            : 0;

        double sRev = NormalizeRevenueGrowth(revenueGrowth);
        double sEps = NormalizeEpsGrowth(epsGrowth);

        double composite = (sRev + sEps) / 2;
        int score = (int)Math.Round(composite * 98 + 1);

        string assessment = score >= 85 ? "Explosive growth" :
                            score >= 70 ? "Strong growth" :
                            score >= 55 ? "Moderate growth" :
                            score >= 40 ? "Stable" :
                            "Declining";

        return new GrowthAssessment(score, assessment, revenueGrowth, epsGrowth);
    }
}