namespace Gimzo.Infrastructure.Database.DataAccessObjects;

internal sealed record SicMoneyFlow : DaoBase
{
    public SicMoneyFlow() : base()
    {
        SicCode = "";
    }

    public SicMoneyFlow(Guid userId): base(userId)
    {
        SicCode = "";
    }

    public SicMoneyFlow(Analysis.Fundamental.SicMoneyFlow moneyFlow, Guid userId) : base(userId)
    {
        SicCode = moneyFlow.SicCode;
        DateEval = moneyFlow.DateEval;
        FlowBillions = moneyFlow.FlowBillions;
        Rank = moneyFlow.Rank;
    }

    public string SicCode { get; init; }
    public DateOnly DateEval { get; init; }
    public decimal FlowBillions { get; init; }
    public int Rank { get; init; }

    public Analysis.Fundamental.SicMoneyFlow ToDomain()
    {
        return new()
        {
            SicCode = SicCode,
            DateEval = DateEval,
            FlowBillions = FlowBillions,
            Rank = Rank
        };
    }
}
