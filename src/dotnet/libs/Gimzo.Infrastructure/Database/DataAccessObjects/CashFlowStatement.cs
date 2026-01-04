namespace Gimzo.Infrastructure.Database.DataAccessObjects;

internal sealed record CashFlowStatement : DaoBase
{
    public CashFlowStatement() : base()
    {
        Symbol = "";
        CentralIndexKey = "";
        FiscalYear = "";
        FiscalPeriod = "";
    }

    public CashFlowStatement(Guid userId) : base(userId)
    {
        Symbol = "";
        CentralIndexKey = "";
        FiscalYear = "";
        FiscalPeriod = "";
    }

    public CashFlowStatement(Analysis.Fundamental.CashFlowStatement stmt, Guid userId) : base(userId)
    {
        Symbol = stmt.Symbol;
        CentralIndexKey = stmt.CentralIndexKey;
        Registrant = stmt.Registrant;
        FiscalYear = stmt.FiscalYear;
        FiscalPeriod = stmt.FiscalPeriod;
        PeriodEndDate = stmt.PeriodEndDate;
        Depreciation = stmt.Depreciation;
        ShareBasedCompensationExpense = stmt.ShareBasedCompensationExpense;
        DeferredIncomeTaxExpense = stmt.DeferredIncomeTaxExpense;
        OtherNonCashIncomeExpense = stmt.OtherNonCashIncomeExpense;
        ChangeInAccountsReceivable = stmt.ChangeInAccountsReceivable;
        ChangeInInventories = stmt.ChangeInInventories;
        ChangeInNonTradeReceivables = stmt.ChangeInNonTradeReceivables;
        ChangeInOtherAssets = stmt.ChangeInOtherAssets;
        ChangeInAccountsPayable = stmt.ChangeInAccountsPayable;
        ChangeInDeferredRevenue = stmt.ChangeInDeferredRevenue;
        ChangeInOtherLiabilities = stmt.ChangeInOtherLiabilities;
        CashFromOperatingActivities = stmt.CashFromOperatingActivities;
        PurchasesOfMarketableSecurities = stmt.PurchasesOfMarketableSecurities;
        SalesOfMarketableSecurities = stmt.SalesOfMarketableSecurities;
        AcquisitionOfProperty = stmt.AcquisitionOfProperty;
        AcquisitionOfBusiness = stmt.AcquisitionOfBusiness;
        OtherInvestingActivities = stmt.OtherInvestingActivities;
        CashFromInvestingActivities = stmt.CashFromInvestingActivities;
        TaxWithholdingForShareBasedCompensation = stmt.TaxWithholdingForShareBasedCompensation;
        PaymentsOfDividends = stmt.PaymentsOfDividends;
        IssuanceOfCommonStock = stmt.IssuanceOfCommonStock;
        RepurchaseOfCommonStock = stmt.RepurchaseOfCommonStock;
        IssuanceOfLongTermDebt = stmt.IssuanceOfLongTermDebt;
        RepaymentOfLongTermDebt = stmt.RepaymentOfLongTermDebt;
        OtherFinancingActivities = stmt.OtherFinancingActivities;
        CashFromFinancingActivities = stmt.CashFromFinancingActivities;
        ChangeInCash = stmt.ChangeInCash;
        CashAtEndOfPeriod = stmt.CashAtEndOfPeriod;
        IncomeTaxesPaid = stmt.IncomeTaxesPaid;
        InterestPaid = stmt.InterestPaid;
    }

    public string Symbol { get; init; }
    public string CentralIndexKey { get; init; }
    public string FiscalYear { get; init; }
    public string FiscalPeriod { get; init; }
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
    public override bool IsValid() => base.IsValid() &&
        !string.IsNullOrWhiteSpace(CentralIndexKey) &&
        !string.IsNullOrWhiteSpace(FiscalYear) &&
        !string.IsNullOrWhiteSpace(FiscalPeriod);

    public Analysis.Fundamental.CashFlowStatement ToDomain()
    {
        return new()
        {
            Symbol = Symbol,
            CentralIndexKey = CentralIndexKey,
            Registrant = Registrant,
            AcquisitionOfBusiness = AcquisitionOfBusiness,
            AcquisitionOfProperty = AcquisitionOfProperty,
            CashAtEndOfPeriod = CashAtEndOfPeriod,
            CashFromFinancingActivities = CashFromFinancingActivities,
            CashFromInvestingActivities = CashFromInvestingActivities,
            CashFromOperatingActivities = CashFromOperatingActivities,
            ChangeInAccountsPayable = ChangeInAccountsPayable,
            ChangeInCash = ChangeInCash,
            ChangeInAccountsReceivable = ChangeInCash,
            ChangeInDeferredRevenue = ChangeInDeferredRevenue,
            ChangeInInventories = ChangeInInventories,
            ChangeInNonTradeReceivables = ChangeInNonTradeReceivables,
            ChangeInOtherAssets = ChangeInOtherAssets,
            ChangeInOtherLiabilities = ChangeInOtherLiabilities,
            DeferredIncomeTaxExpense = DeferredIncomeTaxExpense,
            Depreciation = Depreciation,
            FiscalPeriod = FiscalPeriod,
            FiscalYear = FiscalYear,
            IncomeTaxesPaid = IncomeTaxesPaid,
            InterestPaid = InterestPaid,
            IssuanceOfCommonStock = IssuanceOfCommonStock,
            IssuanceOfLongTermDebt = IssuanceOfLongTermDebt,
            OtherFinancingActivities = OtherFinancingActivities,
            OtherInvestingActivities = OtherInvestingActivities,
            OtherNonCashIncomeExpense = OtherNonCashIncomeExpense,
            PaymentsOfDividends = PaymentsOfDividends,
            PeriodEndDate = PeriodEndDate,
            PurchasesOfMarketableSecurities = PurchasesOfMarketableSecurities,
            RepaymentOfLongTermDebt = RepaymentOfLongTermDebt,
            RepurchaseOfCommonStock = RepurchaseOfCommonStock,
            SalesOfMarketableSecurities = SalesOfMarketableSecurities,
            ShareBasedCompensationExpense = ShareBasedCompensationExpense,
            TaxWithholdingForShareBasedCompensation = TaxWithholdingForShareBasedCompensation
        };
    }
}