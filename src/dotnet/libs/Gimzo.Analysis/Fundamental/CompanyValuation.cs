namespace Gimzo.Analysis.Fundamental;

public record struct CompanyValuation
{
    public required string Symbol { get; init; }
    public required DateOnly DateEval { get; init; }
    public required int Absolute { get; init; }
    public required int Percentile { get; init; }
}
