namespace Gimzo.Analysis.Fundamental;

public readonly record struct BalanceSheetAssessment(
    int Score,
    double? CurrentRatio,
    double? QuickRatio,
    double? CashRatio,
    double? EquityRatio,
    double? DebtToEquityRatio,
    double? DebtToAssetsRatio,
    double? InterestCoverageRatio);

public static class BalanceSheetHealthAnalyzer
{
    private static double NormalizeHigher(double value, double excellent, double good, double fair, double poor)
        => value >= excellent ? 1.0 :
           value >= good ? 0.85 :
           value >= fair ? 0.65 :
           value >= poor ? 0.4 :
           value > 0 ? 0.2 : 0.0;

    private static double NormalizeDebtToEquity(double value)
        => value <= 0.3 ? 1.0 :
           value <= 0.6 ? 0.85 :
           value <= 1.0 ? 0.65 :
           value <= 2.0 ? 0.4 :
           0.2;

    private static double NormalizeDebtToAssets(double value)  // value is 0–1 fraction
        => value <= 0.2 ? 1.0 :
           value <= 0.3 ? 0.85 :
           value <= 0.4 ? 0.65 :
           value <= 0.5 ? 0.4 :
           0.2;

    private static double NormalizeInterestCoverage(double value)
        => value >= 10 ? 1.0 :
           value >= 5 ? 0.85 :
           value >= 3 ? 0.65 :
           value >= 1.5 ? 0.4 :
           value > 0 ? 0.2 : 0.0;

    public static BalanceSheetAssessment Assess(LiquidityRatios liquidity, SolvencyRatios solvency)
    {
        double sCr = liquidity.CurrentRatio.HasValue ? NormalizeHigher(liquidity.CurrentRatio.Value, 3.0, 2.0, 1.5, 1.0) : 0.0;
        double sQr = liquidity.QuickRatio.HasValue ? NormalizeHigher(liquidity.QuickRatio.Value, 2.0, 1.5, 1.0, 0.5) : 0.0;
        double sCash = liquidity.CashRatio.HasValue ? NormalizeHigher(liquidity.CashRatio.Value, 1.0, 0.7, 0.4, 0.2) : 0.0;
        double sEr = solvency.EquityRatio.HasValue ? NormalizeHigher(solvency.EquityRatio.Value, 0.7, 0.6, 0.5, 0.4) : 0.0;
        double sDe = solvency.DebtToEquityRatio.HasValue ? NormalizeDebtToEquity(solvency.DebtToEquityRatio.Value) : 0.0;
        double sDa = solvency.DebtToAssetsRatio.HasValue ? NormalizeDebtToAssets(solvency.DebtToAssetsRatio.Value) : 0.0;
        double sIc = solvency.InterestCoverageRatio.HasValue ? NormalizeInterestCoverage(solvency.InterestCoverageRatio.Value) : 0.0;

        double composite = (sCr + sQr + sCash + sEr + sDe + sDa + sIc) / 7.0;
        int score = (int)Math.Round(composite * 98 + 1);

        return new BalanceSheetAssessment(
            score,
            liquidity.CurrentRatio,
            liquidity.QuickRatio,
            liquidity.CashRatio,
            solvency.EquityRatio,
            solvency.DebtToEquityRatio,
            solvency.DebtToAssetsRatio,
            solvency.InterestCoverageRatio);
    }
}