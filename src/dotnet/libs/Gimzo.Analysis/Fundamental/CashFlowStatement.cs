namespace Gimzo.Analysis.Fundamental;

public record struct CashFlowStatement
{
    public required string Symbol { get; init; }
    public required string CentralIndexKey { get; init; }
    public required string FiscalYear { get; init; }
    public required string FiscalPeriod { get; init; }
    public string? Registrant { get; init; }
    public DateOnly? PeriodEndDate { get; init; }
    public decimal? Depreciation { get; init; }
    public decimal? ShareBasedCompensationExpense { get; init; }
    public decimal? DeferredIncomeTaxExpense { get; init; }
    public decimal? OtherNonCashIncomeExpense { get; init; }
    public decimal? ChangeInAccountsReceivable { get; init; }
    public decimal? ChangeInInventories { get; init; }
    public decimal? ChangeInNonTradeReceivables { get; init; }
    public decimal? ChangeInOtherAssets { get; init; }
    public decimal? ChangeInAccountsPayable { get; init; }
    public decimal? ChangeInDeferredRevenue { get; init; }
    public decimal? ChangeInOtherLiabilities { get; init; }
    public decimal? CashFromOperatingActivities { get; init; }
    public decimal? PurchasesOfMarketableSecurities { get; init; }
    public decimal? SalesOfMarketableSecurities { get; init; }
    public decimal? AcquisitionOfProperty { get; init; }
    public decimal? AcquisitionOfBusiness { get; init; }
    public decimal? OtherInvestingActivities { get; init; }
    public decimal? CashFromInvestingActivities { get; init; }
    public decimal? TaxWithholdingForShareBasedCompensation { get; init; }
    public decimal? PaymentsOfDividends { get; init; }
    public decimal? IssuanceOfCommonStock { get; init; }
    public decimal? RepurchaseOfCommonStock { get; init; }
    public decimal? IssuanceOfLongTermDebt { get; init; }
    public decimal? RepaymentOfLongTermDebt { get; init; }
    public decimal? OtherFinancingActivities { get; init; }
    public decimal? CashFromFinancingActivities { get; init; }
    public decimal? ChangeInCash { get; init; }
    public decimal? CashAtEndOfPeriod { get; init; }
    public decimal? IncomeTaxesPaid { get; init; }
    public decimal? InterestPaid { get; init; }
}