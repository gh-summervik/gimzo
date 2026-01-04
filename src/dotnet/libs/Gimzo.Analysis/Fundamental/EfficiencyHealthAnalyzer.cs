namespace Gimzo.Analysis.Fundamental;

public readonly record struct EfficiencyAssessment(
    int Score,
    string Assessment,
    double? AssetTurnoverRatio,
    double? InventoryTurnoverRatio,
    double? AccountsReceivableTurnoverRatio,
    double? FixedAssetTurnoverRatio);

public sealed class EfficiencyHealthAnalyzer
{
    private static double NormalizeTurnover(double value)
        => value >= 2.0 ? 1.0 :
           value >= 1.5 ? 0.85 :
           value >= 1.0 ? 0.65 :
           value >= 0.5 ? 0.4 :
           value > 0 ? 0.2 : 0.0;

    public static EfficiencyAssessment Assess(EfficiencyRatios efficiency)
    {
        double sAsset = efficiency.AssetTurnoverRatio.HasValue ? NormalizeTurnover(efficiency.AssetTurnoverRatio.Value) : 0.0;
        double sInv = efficiency.InventoryTurnoverRatio.HasValue ? NormalizeTurnover(efficiency.InventoryTurnoverRatio.Value) : 0.0;
        double sAr = efficiency.AccountsReceivableTurnoverRatio.HasValue ? NormalizeTurnover(efficiency.AccountsReceivableTurnoverRatio.Value) : 0.0;
        double sFixed = efficiency.FixedAssetTurnoverRatio.HasValue ? NormalizeTurnover(efficiency.FixedAssetTurnoverRatio.Value) : 0.0;

        double composite = (sAsset + sInv + sAr + sFixed) / 4.0;
        int score = (int)Math.Round(composite * 98 + 1);

        string assessment = score >= 85 ? "Highly efficient" :
                            score >= 70 ? "Strong" :
                            score >= 55 ? "Good" :
                            score >= 40 ? "Average" :
                            "Inefficient";

        return new EfficiencyAssessment(
            score,
            assessment,
            efficiency.AssetTurnoverRatio,
            efficiency.InventoryTurnoverRatio,
            efficiency.AccountsReceivableTurnoverRatio,
            efficiency.FixedAssetTurnoverRatio);
    }
}