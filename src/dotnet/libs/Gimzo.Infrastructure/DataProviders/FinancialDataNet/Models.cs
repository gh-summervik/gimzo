namespace Gimzo.Infrastructure.DataProviders.FinancialDataNet;

public readonly record struct Stock(string Symbol, string? Registrant);

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
    double? SharesIssued, double? SharesOutstanding, string? Description)
{
    public Analysis.Fundamental.CompanyInformation ToDomain()
    {
        return new()
        {
            CentralIndexKey = CentralIndexKey,
            Exchange = Exchange,
            Symbol = Symbol,
            BusinessAddress = BusinessAddress,
            FormerName = FormerName,
            Industry = Industry,
            ChiefExecutiveOfficer = ChiefExecutiveOfficer,
            NumberEmployees = NumberEmployees,
            WebSite = WebSite,
            MarketCap = MarketCap,
            DateFounding = FoundingDate,
            Description = Description,
            Ein = Ein,
            FiscalYearEnd = FiscalYearEnd,
            Isin = Isin,
            Lei = Lei,
            MailingAddress = MailingAddress,
            PhoneNumber = PhoneNumber,
            Registrant = Registrant,
            SharesIssued = SharesIssued,
            SharesOutstanding = SharesOutstanding,
            SicCode = SicCode,
            SicDescription = SicDescription,
            StateOfIncorporation = StateOfIncorporation
        };
    }
}

public readonly record struct LiquidityRatios(string Symbol, string CentralIndexKey,
    string? Registrant, string FiscalYear, string FiscalPeriod, DateOnly? PeriodEndDate,
    decimal? WorkingCapital, double? CurrentRatio, double? CashRatio, double? QuickRatio,
    double? DaysOfInventoryOutstanding, double? DaysOfSalesOutstanding,
    double? DaysPayableOutstanding, double? CashConversionCycle,
    double? SalesToWorkingCapitalRatio, double? CashToCurrentLiabilitiesRatio,
    double? WorkingCapitalToDebtRatio, double? CashFlowAdequacyRatio,
    double? SalesToCurrentAssetsRatio, double? CashToCurrentAssetsRatio,
    double? CashToWorkingCapitalRatio, double? InventoryToWorkingCapitalRatio,
    decimal? NetDebt)
{
    public Analysis.Fundamental.LiquidityRatios ToDomain()
    {
        return new()
        {
            CashConversionCycle = CashConversionCycle,
            CashFlowAdequacyRatio = CashFlowAdequacyRatio,
            CashRatio = CashRatio,
            CashToCurrentAssetsRatio = CashToCurrentAssetsRatio,
            CashToCurrentLiabilitiesRatio = CashToCurrentLiabilitiesRatio,
            CashToWorkingCapitalRatio = CashToWorkingCapitalRatio,
            CentralIndexKey = CentralIndexKey,
            CurrentRatio = CurrentRatio,
            DaysOfInventoryOutstanding = DaysOfInventoryOutstanding,
            DaysOfSalesOutstanding = DaysOfSalesOutstanding,
            DaysPayableOutstanding = DaysPayableOutstanding,
            FiscalPeriod = FiscalPeriod,
            FiscalYear = FiscalYear,
            InventoryToWorkingCapitalRatio = InventoryToWorkingCapitalRatio,
            NetDebt = NetDebt,
            PeriodEndDate = PeriodEndDate,
            QuickRatio = QuickRatio,
            Registrant = Registrant,
            SalesToCurrentAssetsRatio = SalesToCurrentAssetsRatio,
            SalesToWorkingCapitalRatio = SalesToWorkingCapitalRatio,
            Symbol = Symbol,
            WorkingCapital = WorkingCapital,
            WorkingCapitalToDebtRatio = WorkingCapitalToDebtRatio
        };
    }
}

public readonly record struct ProfitabilityRatios(string Symbol, string CentralIndexKey,
    string? Registrant, string FiscalYear, string FiscalPeriod, DateOnly? PeriodEndDate,
    decimal? Ebit, decimal? Ebitda, double? ProfitMargin, double? GrossMargin,
    double? OperatingMargin, double? OperatingCashFlowMargin, double? ReturnOnEquity,
    double? ReturnOnAssets, double? ReturnOnDebt, double? CashReturnOnAssets,
    double? CashTurnoverRatio)
{
    public Analysis.Fundamental.ProfitabilityRatios ToDomain()
    {
        return new()
        {
            CashReturnOnAssets = CashReturnOnAssets,
            CashTurnoverRatio = CashTurnoverRatio,
            CentralIndexKey = CentralIndexKey,
            Ebit = Ebit,
            Ebitda = Ebitda,
            FiscalPeriod = FiscalPeriod,
            FiscalYear = FiscalYear,
            GrossMargin = GrossMargin,
            OperatingMargin = OperatingMargin,
            OperatingCashFlowMargin = OperatingCashFlowMargin,
            ReturnOnEquity = ReturnOnEquity,
            ReturnOnAssets = ReturnOnAssets,
            ReturnOnDebt = ReturnOnDebt,
            PeriodEndDate = PeriodEndDate,
            ProfitMargin = ProfitMargin,
            Registrant  = Registrant,
            Symbol = Symbol
        };
    }
}

public readonly record struct SolvencyRatios(string Symbol, string CentralIndexKey,
    string? Registrant, string FiscalYear, string FiscalPeriod, DateOnly? PeriodEndDate,
    double? EquityRatio, double? DebtCoverageRatio, double? AssetCoverageRatio,
    double? InterestCoverageRatio, double? DebtToEquityRatio, double? DebtToAssetsRatio,
    double? DebtToCapitalRatio, double? DebtToIncomeRatio, double? CashFlowToDebtRatio)
{
    public Analysis.Fundamental.SolvencyRatios ToDomain()
    {
        return new()
        {
            AssetCoverageRatio = AssetCoverageRatio,
            InterestCoverageRatio = InterestCoverageRatio,
            DebtCoverageRatio = DebtCoverageRatio,
            CashFlowToDebtRatio = CashFlowToDebtRatio,
            CentralIndexKey = CentralIndexKey,
            DebtToAssetsRatio = DebtToAssetsRatio,
            DebtToCapitalRatio = DebtToCapitalRatio,
            DebtToEquityRatio = DebtToEquityRatio,
            DebtToIncomeRatio = DebtToIncomeRatio,
            EquityRatio = EquityRatio,
            FiscalPeriod = FiscalPeriod,
            PeriodEndDate = PeriodEndDate,
            FiscalYear = FiscalYear,
            Registrant = Registrant,
            Symbol = Symbol
        };
    }
}

public readonly record struct ValuationRatios(string Symbol, string CentralIndexKey,
    string? Registrant, string FiscalYear, string FiscalPeriod, DateOnly? PeriodEndDate,
    decimal? DividendsPerShare, double? DividendPayoutRatio, decimal? BookValuePerShare,
    double? RetentionRatio, decimal? NetFixedAssets)
{
    public Analysis.Fundamental.ValuationRatios ToDomain()
    {
        return new() { 
            BookValuePerShare = BookValuePerShare,
            CentralIndexKey = CentralIndexKey,
            DividendPayoutRatio = DividendPayoutRatio,
            RetentionRatio = RetentionRatio,
            NetFixedAssets = NetFixedAssets,
            DividendsPerShare = DividendsPerShare,
            FiscalPeriod = FiscalPeriod,
            FiscalYear= FiscalYear,
            Registrant = Registrant,
            Symbol = Symbol,
            PeriodEndDate = PeriodEndDate
        };
    }
}

public readonly record struct KeyMetrics(string Symbol,
    string CentralIndexKey, string? Registrant, string FiscalYear,
    DateOnly? PeriodEndDate, decimal? EarningsPerShare, decimal? EarningsPerShareForecast,
    double? PriceToEarningsRatio, double? ForwardPriceToEarningsRatio,
    double? EarningsGrowthRate, double? PriceEarningsToGrowthRate,
    decimal? BookValuePerShare, double? PriceToBookRatio,
    double? Ebitda, decimal? EnterpriseValue, double? DividendYield,
    double? DividendPayoutRatio, double? DebtToEquityRatio,
    decimal? CapitalExpenditures, decimal? FreeCashFlow, decimal? ReturnOnEquity,
    double? OneYearBeta, double? ThreeYearBeta, double? FiveYearBeta)
{ 
    public Analysis.Fundamental.KeyMetrics ToDomain()
    {
        return new()
        {
            BookValuePerShare = BookValuePerShare,
            CapitalExpenditures = CapitalExpenditures,
            FreeCashFlow = FreeCashFlow,
            ReturnOnEquity = ReturnOnEquity,
            OneYearBeta = OneYearBeta,
            ThreeYearBeta = ThreeYearBeta,
            FiveYearBeta = FiveYearBeta,
            CentralIndexKey = CentralIndexKey,
            DebtToEquityRatio = DebtToEquityRatio,
            DividendPayoutRatio = DividendPayoutRatio,
            DividendYield = DividendYield,
            EarningsGrowthRate = EarningsGrowthRate,
            EarningsPerShare = EarningsPerShare,
            EarningsPerShareForecast = EarningsPerShareForecast,
            Ebitda = Ebitda,
            EnterpriseValue = EnterpriseValue,
            FiscalYear = FiscalYear,
            ForwardPriceToEarningsRatio = ForwardPriceToEarningsRatio,
            PeriodEndDate = PeriodEndDate,
            PriceEarningsToGrowthRate = PriceEarningsToGrowthRate,
            PriceToBookRatio = PriceToBookRatio,
            PriceToEarningsRatio = PriceToEarningsRatio,
            Registrant = Registrant,
            Symbol = Symbol
        };
    }
}

public readonly record struct MarketCap(string Symbol, string CentralIndexKey,
    string? Registrant, string FiscalYear, double? Value,
    double? ChangeInMarketCap, double? PercentageChangeInMarketCap,
    long? SharesOutstanding, long? ChangeInSharesOutstanding,
    double? PercentageChangeInSharesOutstanding)
{
    public Analysis.Fundamental.MarketCap ToDomain()
    {
        return new()
        {
            Symbol = Symbol,
            CentralIndexKey = CentralIndexKey,
            Registrant = Registrant,
            FiscalYear = FiscalYear,
            Value = Value,
            ChangeInMarketCap = ChangeInMarketCap,
            ChangeInSharesOutstanding = ChangeInSharesOutstanding,
            PercentageChangeInMarketCap = PercentageChangeInMarketCap,
            PercentageChangeInSharesOutstanding = PercentageChangeInSharesOutstanding,
            SharesOutstanding = SharesOutstanding
        };
    }
}

public readonly record struct EmployeeCount(string Symbol, string CentralIndexKey,
    string? Registrant, string FiscalYear, int? Count)
{
    public Analysis.Fundamental.EmployeeCount ToDomain()
    {
        return new()
        {
            Symbol = Symbol,
            CentralIndexKey = CentralIndexKey,
            Registrant = Registrant,
            FiscalYear = FiscalYear,
            Count = Count
        };
    }
}

public readonly record struct ExecutiveCompensation(string Symbol, string CentralIndexKey,
    string? Registrant, string Name, string Position, string FiscalYear,
    decimal? Salary, decimal? Bonus, decimal? StockAwards,
    decimal? IncentivePlanCompensation, decimal? OtherCompensation,
    decimal? TotalCompensation)
{
    public Analysis.Fundamental.ExecutiveCompensation ToDomain()
    {
        return new()
        {
            Symbol = Symbol,
            Bonus = Bonus,
            StockAwards = StockAwards,
            CentralIndexKey = CentralIndexKey,
            FiscalYear = FiscalYear,
            IncentivePlanCompensation = IncentivePlanCompensation,
            OtherCompensation = OtherCompensation,
            Name = Name,
            Position = Position,
            Registrant = Registrant,
            Salary = Salary,
            TotalCompensation = TotalCompensation
        };
    }
}

public readonly record struct IncomeStatement(string Symbol, string CentralIndexKey,
    string? Registrant, string FiscalYear, string FiscalPeriod, DateOnly? PeriodEndDate,
    decimal? Revenue, decimal? CostOfRevenue, decimal? GrossProfit,
    decimal? ResearchDevelopmentExpenses, decimal? GeneralAdminExpenses,
    decimal? OperatingExpenses, decimal? OperatingIncome, decimal? InterestExpense,
    decimal? InterestIncome, decimal? NetIncome, decimal? EarningsPerShareBasic,
    decimal? EarningsPerShareDiluted, long? WeightedAverageSharesOutstandingBasic,
    long? WeightedAverageSharesOutstandingDiluted)
{
    public Analysis.Fundamental.IncomeStatement ToDomain()
    {
        return new()
        {
            Symbol = Symbol,
            CentralIndexKey = CentralIndexKey,
            Registrant = Registrant,
            CostOfRevenue = CostOfRevenue,
            EarningsPerShareBasic = EarningsPerShareBasic,
            EarningsPerShareDiluted = EarningsPerShareDiluted,
            PeriodEndDate = PeriodEndDate,
            Revenue = Revenue,
            FiscalPeriod = FiscalPeriod,
            FiscalYear = FiscalYear,
            GeneralAdminExpenses = GeneralAdminExpenses,
            GrossProfit = GrossProfit,
            InterestExpense = InterestExpense,
            InterestIncome = InterestIncome,
            NetIncome = NetIncome,
            OperatingExpenses = OperatingExpenses,
            OperatingIncome = OperatingIncome,
            ResearchDevelopmentExpenses = ResearchDevelopmentExpenses,
            WeightedAverageSharesOutstandingBasic = WeightedAverageSharesOutstandingBasic,
            WeightedAverageSharesOutstandingDiluted = WeightedAverageSharesOutstandingDiluted
        };
    }
}

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
    decimal? AccumulatedOtherComprehensiveIncome, decimal? TotalShareholdersEquity)
{
    public Analysis.Fundamental.BalanceSheet ToDomain()
    {
        return new()
        {
            Symbol = Symbol,
            CentralIndexKey = CentralIndexKey,
            Registrant = Registrant,
            AccountsPayable = AccountsPayable,
            AccountsReceivable = AccountsReceivable,
            AccumulatedOtherComprehensiveIncome = AccountsReceivable,
            Cash = Cash,
            CommonStock = CommonStock,
            DeferredRevenue = DeferredRevenue,
            ShortTermDebt = ShortTermDebt,
            OtherLiabilitiesCurrent = OtherLiabilitiesCurrent,
            TotalAssetsNonCurrent = TotalAssetsNonCurrent,
            FiscalPeriod = FiscalPeriod,
            FiscalYear = FiscalYear,
            Inventories = Inventories,
            LongTermDebt = LongTermDebt,
            MarketableSecuritiesCurrent = MarketableSecuritiesCurrent,
            MarketableSecuritiesNonCurrent = MarketableSecuritiesNonCurrent,
            NonTradeReceivables = NonTradeReceivables,
            OtherAssetsCurrent = OtherAssetsCurrent,
            OtherAssetsNonCurrent = OtherAssetsNonCurrent,
            OtherLiabilitiesNonCurrent = OtherLiabilitiesNonCurrent,
            PeriodEndDate = PeriodEndDate,
            PropertyPlantEquipment = PropertyPlantEquipment,
            RetainedEarnings = RetainedEarnings,
            TotalAssets = TotalAssets,
            TotalAssetsCurrent = TotalAssetsCurrent,
            TotalLiabilities = TotalLiabilities,
            TotalLiabilitiesCurrent = TotalLiabilitiesCurrent,
            TotalLiabilitiesNonCurrent = TotalLiabilitiesNonCurrent,
            TotalShareholdersEquity = TotalShareholdersEquity
        };
    }
}

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
    decimal? IncomeTaxesPaid, decimal? InterestPaid)
{
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

public readonly record struct StockSplit(string Symbol, string CentralIndexKey,
    string? Registrant, DateOnly ExecutionDate, double? Multiplier)
{
    public Analysis.Fundamental.StockSplit ToDomain()
    {
        return new()
        {
            CentralIndexKey = CentralIndexKey,
            ExecutionDate = ExecutionDate,
            Multiplier = Multiplier,
            Registrant = Registrant,
            Symbol = Symbol
        };
    }
}

public readonly record struct Dividend(string Symbol, string? Registrant,
    string? Type, decimal? Amount, DateOnly? DeclarationDate, DateOnly? ExDate,
    DateOnly RecordDate, DateOnly? PaymentDate)
{
    public Analysis.Fundamental.Dividend ToDomain()
    {
        return new()
        {
            Amount = Amount,
            DeclarationDate = DeclarationDate,
            ExDate = ExDate,
            RecordDate = RecordDate,
            PaymentDate = PaymentDate,
            Symbol = Symbol,
            Registrant = Registrant,
            Type = Type
        };
    }
}

public readonly record struct EarningsRelease(string Symbol, string CentralIndexKey,
    string? RegistrantName, decimal? MarketCap, string FiscalQuarterEndDate,
    decimal? EarningsPerShare, decimal? EarningsPerShareForecast, double? PercentageSurprise,
    int? NumberOfForecasts, DateTime? ConferenceCallTime)
{
    public Analysis.Fundamental.EarningsRelease ToDomain()
    {
        return new()
        {
            CentralIndexKey = CentralIndexKey,
            ConferenceCallTime = ConferenceCallTime,
            EarningsPerShare = EarningsPerShare,
            EarningsPerShareForecast = EarningsPerShareForecast,
            FiscalQuarterEndDate = FiscalQuarterEndDate,
            MarketCap = MarketCap,
            NumberOfForecasts = NumberOfForecasts,
            PercentageSurprise = PercentageSurprise,
            RegistrantName = RegistrantName,
            Symbol = Symbol
        };
    }
}

public readonly record struct EfficiencyRatios(string Symbol, string CentralIndexKey,
    string? Registrant, string FiscalYear, string FiscalPeriod, DateOnly? PeriodEndDate,
    double? AssetTurnoverRatio, double? InventoryTurnoverRatio,
    double? AccountsReceivableTurnoverRatio, double? AccountsPayableTurnoverRatio,
    double? EquityMultiplier, double? DaysSalesInInventory, double? FixedAssetTurnoverRatio,
    double? DaysWorkingCapital, double? WorkingCapitalTurnoverRatio, double? DaysCashOnHand,
    double? CapitalIntensityRatio, double? SalesToEquityRatio, double? InventoryToSalesRatio,
    double? InvestmentTurnoverRatio, double? SalesToOperatingIncomeRatio)
{
    public Analysis.Fundamental.EfficiencyRatios ToDomain()
    {
        return new() { 
            AccountsPayableTurnoverRatio = AccountsPayableTurnoverRatio,
            AccountsReceivableTurnoverRatio = AccountsReceivableTurnoverRatio,
            AssetTurnoverRatio = AssetTurnoverRatio,
            CapitalIntensityRatio = CapitalIntensityRatio,
            CentralIndexKey = CentralIndexKey,
            DaysCashOnHand = DaysCashOnHand,
            DaysSalesInInventory = DaysSalesInInventory,
            DaysWorkingCapital = DaysWorkingCapital,
            EquityMultiplier = EquityMultiplier,
            FiscalPeriod = FiscalPeriod,
            FiscalYear = FiscalYear,
            FixedAssetTurnoverRatio = FixedAssetTurnoverRatio,
            InventoryToSalesRatio = InventoryToSalesRatio,
            InventoryTurnoverRatio = InventoryTurnoverRatio,
            InvestmentTurnoverRatio = InvestmentTurnoverRatio,
            PeriodEndDate = PeriodEndDate,
            Registrant = Registrant,
            SalesToEquityRatio = SalesToEquityRatio,
            SalesToOperatingIncomeRatio = SalesToOperatingIncomeRatio,
            Symbol = Symbol,
            WorkingCapitalTurnoverRatio = WorkingCapitalTurnoverRatio
        };
    }
}

public readonly record struct ShortInterest(string Symbol, string? Title, string? MarketCode,
    DateOnly SettlementDate, long? ShortedSecurities, long? PreviousShortedSecurities,
    long? ChangeInShortedSecurities, double? PercentageChangeInShortedSecurities,
    long? AverageDailyVolume, double? DaysToCover, bool? IsStockSplit)
{
    public Analysis.Fundamental.ShortInterest ToDomain()
    {
        return new()
        {
            Symbol = Symbol,
            AverageDailyVolume = AverageDailyVolume,
            ChangeInShortedSecurities = ChangeInShortedSecurities,
            DaysToCover = DaysToCover,
            IsStockSplit = IsStockSplit,
            MarketCode = MarketCode,
            PercentageChangeInShortedSecurities = PercentageChangeInShortedSecurities,
            PreviousShortedSecurities = PreviousShortedSecurities,
            SettlementDate = SettlementDate,
            ShortedSecurities = ShortedSecurities,
            Title = Title
        };
    }
}

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