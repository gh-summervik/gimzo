namespace Gimzo.Analysis.Fundamental;

public readonly record struct IncomeStatementAssessment(
    int Score1To99,
    string Assessment,
    double? GrossMargin,
    double? OperatingMargin,
    double? ProfitMargin,
    double? ReturnOnEquity,
    double? ReturnOnAssets);

public sealed class IncomeStatementHealthAnalyzer
{
    private static double NormalizeMargin(double value)   // 0-1 fraction or percent? assuming fraction
        => value >= 0.40 ? 1.0 :
           value >= 0.30 ? 0.85 :
           value >= 0.20 ? 0.65 :
           value >= 0.10 ? 0.4 :
           value > 0 ? 0.2 : 0.0;

    private static double NormalizeROE(double value)
        => value >= 0.20 ? 1.0 :
           value >= 0.15 ? 0.85 :
           value >= 0.10 ? 0.65 :
           value >= 0.05 ? 0.4 :
           value > 0 ? 0.2 : 0.0;

    private static double NormalizeROA(double value)
        => value >= 0.12 ? 1.0 :
           value >= 0.08 ? 0.85 :
           value >= 0.05 ? 0.65 :
           value >= 0.02 ? 0.4 :
           value > 0 ? 0.2 : 0.0;

    public IncomeStatementAssessment Assess(ProfitabilityRatios profitability)
    {
        double sGross = profitability.GrossMargin.HasValue ? NormalizeMargin(profitability.GrossMargin.Value) : 0.0;
        double sOp = profitability.OperatingMargin.HasValue ? NormalizeMargin(profitability.OperatingMargin.Value) : 0.0;
        double sProfit = profitability.ProfitMargin.HasValue ? NormalizeMargin(profitability.ProfitMargin.Value) : 0.0;
        double sRoe = profitability.ReturnOnEquity.HasValue ? NormalizeROE(profitability.ReturnOnEquity.Value) : 0.0;
        double sRoa = profitability.ReturnOnAssets.HasValue ? NormalizeROA(profitability.ReturnOnAssets.Value) : 0.0;

        double composite = (sGross + sOp + sProfit + sRoe + sRoa) / 5.0;
        int score = (int)Math.Round(composite * 98 + 1);

        string assessment = score >= 85 ? "Exceptional profitability" :
                            score >= 70 ? "Strong" :
                            score >= 55 ? "Good" :
                            score >= 40 ? "Average" :
                            score >= 25 ? "Weak" :
                            "Loss-making/Poor";

        return new IncomeStatementAssessment(
            score,
            assessment,
            profitability.GrossMargin,
            profitability.OperatingMargin,
            profitability.ProfitMargin,
            profitability.ReturnOnEquity,
            profitability.ReturnOnAssets);
    }
}