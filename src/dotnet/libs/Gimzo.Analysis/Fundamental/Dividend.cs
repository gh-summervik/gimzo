namespace Gimzo.Analysis.Fundamental;

public record struct Dividend
{
    public required string Symbol { get; init; }
    public required DateOnly RecordDate { get; init; }
    public DateOnly? ExDate { get; init; }
    public string? Registrant { get; init; }
    public string? Type { get; init; }
    public decimal? Amount { get; init; }
    public DateOnly? DeclarationDate { get; init; }
    public DateOnly? PaymentDate { get; init; }
}