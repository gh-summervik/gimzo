namespace Gimzo.Infrastructure.Database.DataAccessObjects;

internal sealed record CompanyValuation : DaoBase
{
    public CompanyValuation() : base()
    {
        Symbol = "";
    }

    public CompanyValuation(Guid userId) : base(userId)
    {
        Symbol = "";
    }

    public CompanyValuation(Analysis.Fundamental.CompanyValuation valuation, Guid userId) : base(userId)
    {
        Symbol = valuation.Symbol;
        DateEval = valuation.DateEval;
        Absolute = valuation.Absolute;
        Percentile = valuation.Percentile;
    }

    public string Symbol { get; init; }
    public DateOnly DateEval { get; init; }
    public int Absolute { get; init; }
    public int Percentile { get; init; }

    public Analysis.Fundamental.CompanyValuation ToDomain()
    {
        return new()
        {
            Symbol = Symbol,
            DateEval = DateEval,
            Absolute = Absolute,
            Percentile = Percentile,
        };
    }
}
