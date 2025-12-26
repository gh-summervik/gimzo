namespace Gimzo.Infrastructure.Database.DataAccessObjects;

internal sealed record IncomeStatement : DaoBase
{
    public IncomeStatement() : base()
    {
        CentralIndexKey = "";
        FiscalYear = "";
        FiscalPeriod = "";
    }

    public IncomeStatement(Guid userId) : base(userId)
    {
        CentralIndexKey = "";
        FiscalYear = "";
        FiscalPeriod = "";
    }

    public IncomeStatement(Guid userId, DataProviders.FinancialDataNet.IncomeStatement stmt) : base(userId)
    {
        Symbol = stmt.Symbol;
        CentralIndexKey = stmt.CentralIndexKey;
        Registrant = stmt.Registrant;
        FiscalYear = stmt.FiscalYear;
        FiscalPeriod = stmt.FiscalPeriod;
        PeriodEndDate = stmt.PeriodEndDate;
        Revenue = stmt.Revenue;
        CostOfRevenue = stmt.CostOfRevenue;
        GrossProfit = stmt.GrossProfit;
        ResearchDevelopmentExpenses = stmt.ResearchDevelopmentExpenses;
        GeneralAdminExpenses = stmt.GeneralAdminExpenses;
        OperatingExpenses = stmt.OperatingExpenses;
        OperatingIncome = stmt.OperatingIncome;
        InterestExpense = stmt.InterestExpense;
        InterestIncome = stmt.InterestIncome;
        NetIncome = stmt.NetIncome;
        EarningsPerShareBasic = stmt.EarningsPerShareBasic;
        EarningsPerShareDiluted = stmt.EarningsPerShareDiluted;
        WeightedAverageSharesOutstandingBasic = stmt.WeightedAverageSharesOutstandingBasic;
        WeightedAverageSharesOutstandingDiluted = stmt.WeightedAverageSharesOutstandingDiluted;
    }

    public string CentralIndexKey { get; init; }
    public string FiscalYear { get; init; }
    public string FiscalPeriod { get; init; }
    public string? Symbol { get; init; }
    public string? Registrant { get; init; }
    public DateOnly? PeriodEndDate { get; init; }
    public decimal? Revenue { get; init; }
    public decimal? CostOfRevenue { get; init; }
    public decimal? GrossProfit { get; init; }
    public decimal? ResearchDevelopmentExpenses { get; init; }
    public decimal? GeneralAdminExpenses { get; init; }
    public decimal? OperatingExpenses { get; init; }
    public decimal? OperatingIncome { get; init; }
    public decimal? InterestExpense { get; init; }
    public decimal? InterestIncome { get; init; }
    public decimal? NetIncome { get; init; }
    public decimal? EarningsPerShareBasic { get; init; }
    public decimal? EarningsPerShareDiluted { get; init; }
    public long? WeightedAverageSharesOutstandingBasic { get; init; }
    public long? WeightedAverageSharesOutstandingDiluted { get; init; }

    public override bool IsValid() => base.IsValid() &&
        !string.IsNullOrWhiteSpace(CentralIndexKey) &&
        !string.IsNullOrWhiteSpace(FiscalYear) &&
        !string.IsNullOrWhiteSpace(FiscalPeriod);
}
