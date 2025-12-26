namespace Gimzo.Infrastructure.DataProviders.FinancialDataNet;

public readonly record struct Stock(string Symbol, string? Registrant, bool IsInternational = false);

public readonly record struct ExchangeTradedFund(string Symbol, string? Description);

public readonly record struct Commodity(string Symbol, string? Description);

public readonly record struct OverTheCounter(string Symbol, string? Title);

public readonly record struct Index(string Symbol, string? Name);

public readonly record struct Future(string Symbol, string? Description, string? Type);

public readonly record struct Crypto(string Symbol, string? BaseAsset, string? QuoteAsset);

public readonly record struct SecurityInformation(
    string Symbol, string? Issuer, string? Cusip, string? Isin, string? Figi, string? Type);

public readonly record struct CompanyInformation(string Symbol,
    string CentralIndexKey, string? Registrant, string? Isin,
    string? Lei, string? Ein, string Exchange, string? SicCode,
    string? SicDescription, string? FiscalYearEnd, string? StateOfIncorporation, string? PhoneNumber,
    string? MailingAddress, string? BusinessAddress, string? FormerName,
    string? Industry, string? FoundingDate, string? ChiefExecutiveOfficer,
    int? NumberEmployees, string? WebSite, decimal? MarketCap,
    double? SharesIssued, double? SharesOutstanding, string? Description);

public readonly record struct InternationalCompanyInformation(string Symbol,
    string? Registrant, string Exchange, string? Isin, string? Industry,
    string? YearFounding, string? ChiefExecutiveOfficer,
    int? NumberEmployees, string? WebSite, string? Description);

public readonly record struct LiquidityRatios(string Symbol, string CentralIndexKey,
    string? Registrant, string FiscalYear, string FiscalPeriod, DateOnly? PeriodEndDate,
    decimal? WorkingCapital, double? CurrentRatio, double? CashRatio, double? QuickRatio,
    double? DaysOfInventoryOutstanding, double? DaysOfSalesOutstanding,
    double? DaysPayableOutstanding, double? CashConversionCycle,
    double? SalesToWorkingCapitalRatio, double? CashToCurrentLiabilitiesRatio,
    double? WorkingCapitalToDebtRatio, double? CashFlowAdequacyRatio,
    double? SalesToCurrentAssetsRatio, double? CashToCurrentAssetsRatio,
    double? CashToWorkingCapitalRatio, double? InventoryToWorkingCapitalRatio,
    decimal? NetDebt);

public readonly record struct ProfitabilityRatios(string Symbol, string CentralIndexKey,
    string? Registrant, string FiscalYear, string FiscalPeriod, DateOnly? PeriodEndDate,
    decimal? Ebit, decimal? Ebitda, double? ProfitMargin, double? GrossMargin,
    double? OperatingMargin, double? OperatingCashFlowMargin, double? ReturnOnEquity,
    double? ReturnOnAssets, double? ReturnOnDebt, double? CashReturnOnAssets,
    double? CashTurnoverRatio);

public readonly record struct SolvencyRatios(string Symbol, string CentralIndexKey,
    string? Registrant, string FiscalYear, string FiscalPeriod, DateOnly? PeriodEndDate,
    double? EquityRatio, double? DebtCoverageRatio, double? AssetCoverageRatio,
    double? InterestCoverageRatio, double? DebtToEquityRatio, double? DebtToAssetsRatio,
    double? DebtToCapitalRatio, double? DebtToIncomeRatio, double? CashFlowToDebtRatio);

public readonly record struct ValuationRatios(string Symbol, string CentralIndexKey,
    string? Registrant, string FiscalYear, string FiscalPeriod, DateOnly? PeriodEndDate,
    decimal? DividendsPerShare, double? DividendPayoutRatio, decimal? BookValuePerShare,
    double? RetentionRatio, decimal? NetFixedAssets);

public readonly record struct KeyMetrics(string Symbol,
    string CentralIndexKey, string? Registrant, string FiscalYear,
    DateOnly? PeriodEndDate, decimal? EarningsPerShare, decimal? EarningsPerShareForecast,
    double? PriceToEarningsRatio, double? ForwardPriceToEarningsRatio,
    double? EarningsGrowthRate, double? PriceEarningsToGrowthRate,
    decimal? BookValuePerShare, double? PriceToBookRatio,
    double? Ebitda, decimal? EnterpriseValue, double? DividendYield,
    double? DividendPayoutRatio, double? DebtToEquityRatio,
    decimal? CapitalExpenditures, decimal? FreeCashFlow, decimal? ReturnOnEquity,
    double? OneYearBeta, double? ThreeYearBeta, double? FiveYearBeta);

public readonly record struct MarketCap(string Symbol, string CentralIndexKey,
    string? Registrant, string FiscalYear, decimal? Value,
    decimal? ChangeInMarketCap, double? PercentageChangeInMarketCap,
    long? SharesOutstanding, long? ChangeInSharesOutstanding,
    double? PercentageChangeInSharesOutstanding);

public readonly record struct EmployeeCount(string Symbol, string CentralIndexKey,
    string? Registrant, string FiscalYear, int? Count);

public readonly record struct ExecutiveCompensation(string Symbol, string CentralIndexKey,
    string? Registrant, string Name, string Position, string FiscalYear,
    decimal? Salary, decimal? Bonus, decimal? StockAwards,
    decimal? IncentivePlanCompensation, decimal? OtherCompensation,
    decimal? TotalCompensation);

public readonly record struct IncomeStatement(string Symbol, string CentralIndexKey,
    string? Registrant, string FiscalYear, string FiscalPeriod, DateOnly? PeriodEndDate,
    decimal? Revenue, decimal? CostOfRevenue, decimal? GrossProfit,
    decimal? ResearchDevelopmentExpenses, decimal? GeneralAdminExpenses,
    decimal? OperatingExpenses, decimal? OperatingIncome, decimal? InterestExpense,
    decimal? InterestIncome, decimal? NetIncome, decimal? EarningsPerShareBasic,
    decimal? EarningsPerShareDiluted, long? WeightedAverageSharesOutstandingBasic,
    long? WeightedAverageSharesOutstandingDiluted);

public readonly record struct BalanceSheet(string Symbol, string CentralIndexKey,
    string? Registrant, string FiscalYear, string FiscalPeriod,
    DateOnly? PeriodEndDate, decimal? Cash, decimal? MarketableSecuritiesCurrent,
    decimal? AccountsReceivable, decimal? Inventories,
    decimal? NonTradeReceivables, decimal? OtherAssetsCurrent, decimal? TotalAssetsCurrent,
    decimal? MarketableSecuritiesNonCurrent, decimal? PropertyPlantEquipment,
    decimal? OtherAssetsNonCurrent, decimal? TotalAssetsNonCurrent, decimal? TotalAssets,
    decimal? AccountsPayable, decimal? DeferredRevenue,
    decimal? ShortTermDebt, decimal? OtherLiabilitiesCurrent,
    decimal? TotalLiabilitiesCurrent, decimal? LongTermDebt,
    decimal? OtherLiabilitiesNonCurrent, decimal? TotalLiabilitiesNonCurrent,
    decimal? TotalLiabilities, decimal? CommonStock, decimal? RetainedEarnings,
    decimal? AccumulatedOtherComprehensiveIncome, decimal? TotalShareholdersEquity);

public readonly record struct CashFlowStatement(string Symbol, string CentralIndexKey,
    string? Registrant, string FiscalYear, string FiscalPeriod, DateOnly? PeriodEndDate,
    decimal? Depreciation, decimal? ShareBasedCompensationExpense,
    decimal? DeferredIncomeTaxExpense, decimal? OtherNonCashIncomeExpense,
    decimal? ChangeInAccountsReceivable, decimal? ChangeInInventories,
    decimal? ChangeInNonTradeReceivables, decimal? ChangeInOtherAssets,
    decimal? ChangeInAccountsPayable, decimal? ChangeInDeferredRevenue,
    decimal? ChangeInOtherLiabilities, decimal? CashFromOperatingActivities,
    decimal? PurchasesOfMarketableSecurities, decimal? SalesOfMarketableSecurities,
    decimal? AcquisitionOfProperty, decimal? AcquisitionOfBusiness,
    decimal? OtherInvestingActivities, decimal? CashFromInvestingActivities,
    decimal? TaxWithholdingForShareBasedCompensation, decimal? PaymentsOfDividends,
    decimal? IssuanceOfCommonStock, decimal? RepurchaseOfCommonStock,
    decimal? IssuanceOfLongTermDebt, decimal? RepaymentOfLongTermDebt,
    decimal? OtherFinancingActivities, decimal? CashFromFinancingActivities,
    decimal? ChangeInCash, decimal? CashAtEndOfPeriod,
    decimal? IncomeTaxesPaid, decimal? InterestPaid);

public readonly record struct StockSplit(string Symbol, string CentralIndexKey,
    string? Registrant, DateOnly ExecutionDate, double? Multiplier);

public readonly record struct Dividend(string Symbol, string? Registrant,
    string? Type, decimal? Amount, DateOnly? DeclarationDate, DateOnly? ExDate,
    DateOnly? RecordDate, DateOnly? PaymentDate);

public readonly record struct EarningsRelease(string Symbol, string CentralIndexKey,
    string? RegistrantName, decimal? MarketCap, string FiscalQuarterEndDate,
    decimal? EarningsPerShare, decimal? EarningsPerShareForecast, double? PercentageSurprise,
    int? NumberOfForecasts, DateTime? ConferenceCallTime);

public readonly record struct EfficiencyRatios(string Symbol, string CentralIndexKey,
    string? Registrant, string FiscalYear, string FiscalPeriod, DateOnly? PeriodEndDate,
    double? AssetTurnoverRatio, double? InventoryTurnoverRatio,
    double? AccountsReceivableTurnoverRatio, double? AccountsPayableTurnoverRatio,
    double? EquityMultiplier, double? DaysSalesInInventory, double? FixedAssetTurnoverRatio,
    double? DaysWorkingCapital, double? WorkingCapitalTurnoverRatio, double? DaysCashOnHand,
    double? CapitalIntensityRatio, double? SalesToEquityRatio, double? InventoryToSalesRatio,
    double? InvestmentTurnoverRatio, double? SalesToOperatingIncomeRatio);

public readonly record struct ShortInterest(string Symbol, string? Title, string? MarketCode,
    DateOnly SettlementDate, long? ShortedSecurities, long? PreviousShortedSecurities,
    long? ChangeInShortedSecurities, double? PercentageChangeInShortedSecurities,
    long? AverageDailyVolume, double? DaysToConvert, bool? IsStockSplit);

public readonly record struct EodPrice(string Symbol, DateOnly? Date, decimal? Open,
    decimal? High, decimal? Low, decimal? Close, double? Volume);

public readonly record struct MinutePrice(string Symbol, DateTime? DateTime, decimal? Open,
    decimal? High, decimal? Low, decimal? Close, double? Volume);

public readonly record struct IndexConstituent(string Symbol, string? IndexName,
    string? ConstituentSymbol, string? ConstituentName, string? Sector, string? Industry,
    DateOnly? DateAdded);

public readonly record struct InitialPublicOffering(string Symbol, string? Registrant,
    string? Exchange, DateOnly? PricingDate, decimal? SharePrice, long? SharesOffered,
    decimal? OfferingValue);

public readonly record struct OptionChain(string Symbol,
    string? CentralIndexKey, string? Registrant, string? ContractName,
    DateOnly? Expiration, string? Type, decimal? StrikePrice);

public readonly record struct OptionPrice(string ContractName,
    DateOnly Date, decimal? Open, decimal? High, decimal? Low, decimal? Close, double? Volume);

public readonly record struct OptionGreeks(string ContractName,
    DateOnly Date, double? Delta, double? Gamma, double? Theta, double? Vega, double? Rho);

public readonly record struct CryptoInformation(string Symbol, string? Name,
    decimal? MarketCap, decimal? FullyDilutedValuation, decimal? TotalSupply,
    decimal? MaxSupply, decimal? CirculationSupply, decimal? HighestPrice,
    DateOnly? DateHighestPrice, decimal? LowestPrice, DateOnly? DateLowestPrice,
    string? HashFunction, string? BlockTime, DateOnly? LedgerStartDate,
    string? WebSite, string? Description);