using System;

namespace Gimzo.Analysis.Fundamental;

public sealed class Dividend
{
    public required string Symbol { get; init; }
    public required string Registrant { get; init; }
    public required string Type { get; init; }
    public decimal Amount { get; init; }
    public DateOnly DeclarationDate { get; init; }
    public DateOnly ExDate { get; init; }
    public DateOnly RecordDate { get; init; }
    public DateOnly PaymentDate { get; init; }
}