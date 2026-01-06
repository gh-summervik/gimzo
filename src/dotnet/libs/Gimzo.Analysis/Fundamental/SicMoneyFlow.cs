namespace Gimzo.Analysis.Fundamental;

public record struct SicMoneyFlow
{
    public required string SicCode { get; init; }
    public required DateOnly DateEval { get; init; }
    public required decimal FlowBillions { get; init; }
    public required int Rank { get; init; }
}
