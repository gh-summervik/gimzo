using Gimzo.Analysis.Fundamental;
using Gimzo.Common;
using Gimzo.Infrastructure.DataProviders.FinancialDataNet;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Gimzo.AppServices.DataImports;

public sealed class FinancialDataImporter
{
    private readonly FinancialDataApiClient _apiClient;
    private readonly ILogger<FinancialDataImporter> _logger;

    public FinancialDataImporter(FinancialDataApiClient apiClient,
        ILogger<FinancialDataImporter> logger)
    {
        _apiClient = apiClient;
        _logger = logger;
    }

    public async Task Import(Guid processId)
    {
        if (_logger.IsEnabled(LogLevel.Information))
            _logger.LogInformation("{Name} started import at {DateTime} with process id {ProcessId}", nameof(FinancialDataImporter),
                DateTimeOffset.Now, processId);

        var stockSymbols = await _apiClient.GetStockSymbolsAsync();

        int i = 0;
        var count = stockSymbols.Length;
        foreach (var symbol in stockSymbols)
        {
            i++;
            if (i < 1375)
                continue;

            Stopwatch timer = Stopwatch.StartNew();
            _logger.LogInformation("Fetching {SYMBOL}, i = {i}/{c}", symbol.Symbol, i, count);
            var ticker = symbol.Symbol;
            string securityType = symbol.GetType().Name;
            var secInfoModel = await _apiClient.GetSecurityInformationAsync(ticker);
            var coInfoModel = await _apiClient.GetCompanyInformationAsync(ticker);
            var priceActionsModel = await _apiClient.GetStockPricesAsync(ticker);
            var metricsModel = await _apiClient.GetKeyMetricsAsync(ticker);
            var marketCapsModel = await _apiClient.GetMarketCapAsync(ticker);
            var empCountsModel = await _apiClient.GetEmployeeCountAsync(ticker);
            var compsModel = await _apiClient.GetExecutiveCompensationAsync(ticker);
            var incStmtsModel = await _apiClient.GetIncomeStatementsAsync(ticker);
            var bsStmtsModel = await _apiClient.GetBalanceSheetStatementsAsync(ticker);
            var cfStmtsModel = await _apiClient.GetCashFlowStatementsAsync(ticker);
            var divsModel = await _apiClient.GetDividendsAsync(ticker);
            var splitsModel = await _apiClient.GetStockSplitsAsync(ticker);
            var shortsModel = await _apiClient.GetShortInterestAsync(ticker);
            var earningsReleasesModel = await _apiClient.GetEarningsReleasesAsync(ticker);
            var efficiencyRatiosModel = await _apiClient.GetEfficiencyRatiosAsync(ticker);
            var liquidityModel = await _apiClient.GetLiquidityRatiosAsync(ticker);
            var profitabilityModel = await _apiClient.GetProfitabilityRatiosAsync(ticker);
            var solvencyModel = await _apiClient.GetSolvencyRatiosAsync(ticker);
            var valuationsModel = await _apiClient.GetValuationRatiosAsync(ticker);

            var security = new Security(
                symbol: ticker,
                securityType: securityType,
                type: secInfoModel.HasValue ? secInfoModel.Value.Type : null,
                issuer: secInfoModel.HasValue ? secInfoModel.Value.Issuer : null,
                cusip: secInfoModel.HasValue ? secInfoModel.Value.Cusip : null,
                isin: secInfoModel.HasValue ? secInfoModel.Value.Isin : null,
                figi: secInfoModel.HasValue ? secInfoModel.Value.Figi : null,
                registrant: symbol.Registrant,
                description: null,
                title: null,
                name: secInfoModel.HasValue ? secInfoModel.Value.Issuer : null,
                baseAsset: null,
                quoteAsset: null,
                company: coInfoModel.HasValue ? new Analysis.Fundamental.CompanyInformation()
                {
                    BusinessAddress = coInfoModel.Value.BusinessAddress,
                    CentralIndexKey = coInfoModel.Value.CentralIndexKey,
                    ChiefExecutiveOfficer = coInfoModel.Value.ChiefExecutiveOfficer,
                    DateFounding = coInfoModel.Value.FoundingDate,
                    Description = coInfoModel.Value.Description,
                    Ein = coInfoModel.Value.Ein,
                    Exchange = coInfoModel.Value.Exchange,
                    FiscalYearEnd = coInfoModel.Value.FiscalYearEnd,
                    FormerName = coInfoModel.Value.FormerName,
                    Industry = coInfoModel.Value.Industry,
                    Isin = coInfoModel.Value.Isin,
                    Lei = coInfoModel.Value.Lei,
                    MailingAddress = coInfoModel.Value.MailingAddress,
                    MarketCap = coInfoModel.Value.MarketCap,
                    NumberEmployees = coInfoModel.Value.NumberEmployees,
                    PhoneNumber = coInfoModel.Value.PhoneNumber,
                    Registrant = coInfoModel.Value.Registrant,
                    SharesIssued = coInfoModel.Value.SharesIssued,
                    SharesOutstanding = coInfoModel.Value.SharesOutstanding,
                    SicCode = coInfoModel.Value.SicCode,
                    SicDescription = coInfoModel.Value.SicDescription,
                    StateOfIncorporation = coInfoModel.Value.StateOfIncorporation,
                    Symbol = ticker,
                    WebSite = coInfoModel.Value.WebSite
                } : null,
                incomeStatements: (incStmtsModel?.Length ?? 0) == 0 ? []
                    : [.. incStmtsModel!.Select(k => new Analysis.Fundamental.IncomeStatement() {
                        CentralIndexKey = k.CentralIndexKey,
                        CostOfRevenue = k.CostOfRevenue,
                        EarningsPerShareBasic = k.EarningsPerShareBasic,
                        EarningsPerShareDiluted = k.EarningsPerShareDiluted,
                        FiscalPeriod = k.FiscalPeriod,
                        FiscalYear = k.FiscalYear,
                        GeneralAdminExpenses = k.GeneralAdminExpenses,
                        GrossProfit = k.GrossProfit,
                        InterestExpense = k.InterestExpense,
                        InterestIncome = k.InterestIncome,
                        NetIncome = k.NetIncome,
                        OperatingExpenses = k.OperatingExpenses,
                        OperatingIncome = k.OperatingIncome,
                        PeriodEndDate = k.PeriodEndDate,
                        Registrant = k.Registrant,
                        ResearchDevelopmentExpenses = k.ResearchDevelopmentExpenses,
                        Revenue = k.Revenue,
                        Symbol = k.Symbol,
                        WeightedAverageSharesOutstandingBasic = k.WeightedAverageSharesOutstandingBasic,
                        WeightedAverageSharesOutstandingDiluted = k.WeightedAverageSharesOutstandingDiluted
                    })],
                balanceSheets: (bsStmtsModel?.Length ?? 0) == 0 ? []
                    : [.. bsStmtsModel!.Select(k => new Analysis.Fundamental.BalanceSheet() {
                        AccountsPayable = k.AccountsPayable,
                        AccountsReceivable = k.AccountsReceivable,
                        AccumulatedOtherComprehensiveIncome = k.AccumulatedOtherComprehensiveIncome,
                        Cash = k.Cash,
                        CentralIndexKey = k.CentralIndexKey,
                        CommonStock = k.CommonStock,
                        DeferredRevenue = k.DeferredRevenue,
                        FiscalPeriod = k.FiscalPeriod,
                        FiscalYear = k.FiscalYear,
                        LongTermDebt = k.LongTermDebt,
                        Inventories = k.Inventories,
                        MarketableSecuritiesCurrent = k.MarketableSecuritiesCurrent,
                        MarketableSecuritiesNonCurrent = k.MarketableSecuritiesNonCurrent,
                        PeriodEndDate = k.PeriodEndDate,
                        NonTradeReceivables = k.NonTradeReceivables,
                        OtherAssetsCurrent = k.OtherAssetsCurrent,
                        OtherAssetsNonCurrent = k.OtherAssetsNonCurrent,
                        OtherLiabilitiesCurrent = k.OtherLiabilitiesCurrent,
                        OtherLiabilitiesNonCurrent = k.OtherLiabilitiesNonCurrent,
                        PropertyPlantEquipment = k.PropertyPlantEquipment,
                        Registrant = k.Registrant,
                        RetainedEarnings = k.RetainedEarnings,
                        ShortTermDebt = k.ShortTermDebt,
                        Symbol = k.Symbol,
                        TotalAssets = k.TotalAssets,
                        TotalAssetsCurrent = k.TotalAssetsCurrent,
                        TotalAssetsNonCurrent = k.TotalAssetsNonCurrent,
                        TotalLiabilities = k.TotalLiabilities,
                        TotalLiabilitiesCurrent = k.TotalLiabilitiesCurrent,
                        TotalLiabilitiesNonCurrent = k.TotalLiabilitiesNonCurrent,
                        TotalShareholdersEquity = k.TotalShareholdersEquity
                    })],
                cashFlowStatements: (cfStmtsModel?.Length ?? 0) == 0 ? []
                    : [.. cfStmtsModel!.Select(k => new Analysis.Fundamental.CashFlowStatement() {
                        AcquisitionOfBusiness = k.AcquisitionOfBusiness,
                        AcquisitionOfProperty = k.AcquisitionOfProperty,
                        CashAtEndOfPeriod = k.CashAtEndOfPeriod,
                        CashFromFinancingActivities = k.CashFromFinancingActivities,
                        CashFromInvestingActivities = k.CashFromInvestingActivities,
                        CashFromOperatingActivities = k.CashFromOperatingActivities,
                        CentralIndexKey = k.CentralIndexKey,
                        ChangeInAccountsPayable = k.ChangeInAccountsPayable,
                        ChangeInAccountsReceivable = k.ChangeInAccountsReceivable,
                        ChangeInCash = k.ChangeInCash,
                        ChangeInDeferredRevenue = k.ChangeInDeferredRevenue,
                        ChangeInInventories = k.ChangeInInventories,
                        ChangeInNonTradeReceivables = k.ChangeInNonTradeReceivables,
                        ChangeInOtherAssets = k.ChangeInOtherAssets,
                        ChangeInOtherLiabilities = k.ChangeInOtherLiabilities,
                        DeferredIncomeTaxExpense = k.DeferredIncomeTaxExpense,
                        Depreciation = k.Depreciation,
                        FiscalPeriod = k.FiscalPeriod,
                        FiscalYear = k.FiscalYear,
                        IncomeTaxesPaid = k.IncomeTaxesPaid,
                        InterestPaid = k.InterestPaid,
                        IssuanceOfCommonStock = k.IssuanceOfCommonStock,
                        IssuanceOfLongTermDebt = k.IssuanceOfLongTermDebt,
                        OtherFinancingActivities = k.OtherFinancingActivities,
                        OtherInvestingActivities = k.OtherInvestingActivities,
                        OtherNonCashIncomeExpense = k.OtherNonCashIncomeExpense,
                        PaymentsOfDividends = k.PaymentsOfDividends,
                        PeriodEndDate = k.PeriodEndDate,
                        PurchasesOfMarketableSecurities = k.PurchasesOfMarketableSecurities,
                        Registrant = k.Registrant,
                        RepaymentOfLongTermDebt = k.RepaymentOfLongTermDebt,
                        RepurchaseOfCommonStock = k.RepurchaseOfCommonStock,
                        SalesOfMarketableSecurities = k.SalesOfMarketableSecurities,
                        ShareBasedCompensationExpense = k.ShareBasedCompensationExpense,
                        Symbol = k.Symbol,
                        TaxWithholdingForShareBasedCompensation = k.TaxWithholdingForShareBasedCompensation
                    })],
                splits: (splitsModel?.Length ?? 0) == 0 ? []
                    : [.. splitsModel!.Select(k => new Analysis.Fundamental.StockSplit() {
                        CentralIndexKey = k.CentralIndexKey,
                        ExecutionDate = k.ExecutionDate,
                        Multiplier = k.Multiplier,
                        Registrant = k.Registrant,
                        Symbol = k.Symbol
                    })],
                dividends: (divsModel?.Length ?? 0) == 0 ? []
                    : [.. divsModel!.Select(k => new Analysis.Fundamental.Dividend() {
                        Amount = k.Amount,
                        DeclarationDate = k.DeclarationDate,
                        ExDate = k.ExDate,
                        PaymentDate = k.PaymentDate,
                        RecordDate = k.RecordDate,
                        Registrant = k.Registrant,
                        Symbol = k.Symbol,
                        Type = k.Type
                    })],
                efficiencyRatios: (efficiencyRatiosModel?.Length ?? 0) == 0 ? []
                : [.. efficiencyRatiosModel!.Select(k => new Analysis.Fundamental.EfficiencyRatios() {
                        AccountsPayableTurnoverRatio = k.AccountsPayableTurnoverRatio,
                        AccountsReceivableTurnoverRatio = k.AccountsReceivableTurnoverRatio,
                        AssetTurnoverRatio = k.AssetTurnoverRatio,
                        CapitalIntensityRatio = k.CapitalIntensityRatio,
                        CentralIndexKey = k.CentralIndexKey,
                        DaysCashOnHand = k.DaysCashOnHand,
                        DaysSalesInInventory = k.DaysSalesInInventory,
                        DaysWorkingCapital = k.DaysWorkingCapital,
                        EquityMultiplier = k.EquityMultiplier,
                        FiscalPeriod = k.FiscalPeriod,
                        FiscalYear = k.FiscalYear,
                        FixedAssetTurnoverRatio = k.FixedAssetTurnoverRatio,
                        InventoryToSalesRatio = k.InventoryToSalesRatio,
                        InventoryTurnoverRatio = k.InventoryTurnoverRatio,
                        InvestmentTurnoverRatio = k.InvestmentTurnoverRatio,
                        PeriodEndDate = k.PeriodEndDate,
                        Registrant = k.Registrant,
                        SalesToEquityRatio = k.SalesToEquityRatio,
                        SalesToOperatingIncomeRatio = k.SalesToOperatingIncomeRatio,
                        Symbol = k.Symbol,
                        WorkingCapitalTurnoverRatio = k.WorkingCapitalTurnoverRatio
                     })],
                keyMetrics: (metricsModel?.Length ?? 0) == 0 ? []
                    : [.. metricsModel!.Select(k => new Analysis.Fundamental.KeyMetrics() {
                        BookValuePerShare = k.BookValuePerShare,
                        CapitalExpenditures = k.CapitalExpenditures,
                        CentralIndexKey = k.CentralIndexKey,
                        DebtToEquityRatio = k.DebtToEquityRatio,
                        DividendPayoutRatio = k.DividendPayoutRatio,
                        DividendYield = k.DividendYield,
                        EarningsGrowthRate = k.EarningsGrowthRate,
                        EarningsPerShare = k.EarningsPerShare,
                        EarningsPerShareForecast = k.EarningsPerShareForecast,
                        Ebitda = k.Ebitda,
                        EnterpriseValue = k.EnterpriseValue,
                        FiscalYear = k.FiscalYear,
                        FiveYearBeta = k.FiveYearBeta,
                        ForwardPriceToEarningsRatio = k.ForwardPriceToEarningsRatio,
                        FreeCashFlow = k.FreeCashFlow,
                        OneYearBeta = k.OneYearBeta,
                        PeriodEndDate = k.PeriodEndDate,
                        PriceEarningsToGrowthRate = k.PriceEarningsToGrowthRate,
                        PriceToBookRatio = k.PriceToBookRatio,
                        PriceToEarningsRatio = k.PriceToEarningsRatio,
                        Registrant = k.Registrant,
                        ReturnOnEquity = k.ReturnOnEquity,
                        Symbol = k.Symbol,
                        ThreeYearBeta = k.ThreeYearBeta
                    })],
                    marketCaps: (marketCapsModel?.Length ?? 0) == 0 ? []
                        : [.. marketCapsModel!.Select(k => new Analysis.Fundamental.MarketCap() {
                            CentralIndexKey = k.CentralIndexKey,
                            ChangeInMarketCap = k.ChangeInMarketCap,
                            ChangeInSharesOutstanding = k.ChangeInSharesOutstanding,
                            FiscalYear = k.FiscalYear,
                            PercentageChangeInMarketCap = k.PercentageChangeInMarketCap,
                            PercentageChangeInSharesOutstanding = k.PercentageChangeInSharesOutstanding,
                            Registrant = k.Registrant,
                            SharesOutstanding = k.SharesOutstanding,
                            Symbol = k.Symbol,
                            Value = k.Value
                        })],
                    liquidityRatios: (liquidityModel?.Length ?? 0) == 0 ? []
                        : [.. liquidityModel!.Select(k => new Analysis.Fundamental.LiquidityRatios() {
                            CashConversionCycle = k.CashConversionCycle,
                            CashFlowAdequacyRatio = k.CashFlowAdequacyRatio,
                            CashRatio = k.CashRatio,
                            CashToCurrentAssetsRatio = k.CashToCurrentAssetsRatio,
                            CashToCurrentLiabilitiesRatio = k.CashToCurrentLiabilitiesRatio,
                            CashToWorkingCapitalRatio = k.CashToWorkingCapitalRatio,
                            CentralIndexKey = k.CentralIndexKey,
                            CurrentRatio = k.CurrentRatio,
                            DaysOfInventoryOutstanding = k.DaysOfInventoryOutstanding,
                            DaysOfSalesOutstanding = k.DaysOfSalesOutstanding,
                            DaysPayableOutstanding = k.DaysPayableOutstanding,
                            FiscalPeriod = k.FiscalPeriod,
                            FiscalYear = k.FiscalYear,
                            InventoryToWorkingCapitalRatio = k.InventoryToWorkingCapitalRatio,
                            NetDebt = k.NetDebt,
                            PeriodEndDate = k.PeriodEndDate,
                            QuickRatio = k.QuickRatio,
                            Registrant = k.Registrant,
                            SalesToCurrentAssetsRatio = k.SalesToCurrentAssetsRatio,
                            SalesToWorkingCapitalRatio = k.SalesToWorkingCapitalRatio,
                            Symbol = k.Symbol,
                            WorkingCapital = k.WorkingCapital,
                            WorkingCapitalToDebtRatio = k.WorkingCapitalToDebtRatio
                        })],
                    profitabilityRatios: (profitabilityModel?.Length ?? 0) == 0 ? []
                        : [.. profitabilityModel!.Select(k => new Analysis.Fundamental.ProfitabilityRatios()
                        {
                            CashReturnOnAssets = k.CashReturnOnAssets,
                            CashTurnoverRatio = k.CashTurnoverRatio,
                            CentralIndexKey = k.CentralIndexKey,
                            Ebit = k.Ebit,
                            Ebitda = k.Ebitda,
                            FiscalPeriod = k.FiscalPeriod,
                            FiscalYear = k.FiscalYear,
                            GrossMargin = k.GrossMargin,
                            OperatingCashFlowMargin = k.OperatingCashFlowMargin,
                            OperatingMargin = k.OperatingMargin,
                            PeriodEndDate = k.PeriodEndDate,
                            ProfitMargin = k.ProfitMargin,
                            Registrant = k.Registrant,
                            ReturnOnAssets = k.ReturnOnAssets,
                            ReturnOnDebt = k.ReturnOnDebt,
                            ReturnOnEquity = k.ReturnOnEquity,
                            Symbol = k.Symbol
                        })],
                    valuationRatios: (valuationsModel?.Length ?? 0) == 0 ? []
                        : [.. valuationsModel!.Select(k => new Analysis.Fundamental.ValuationRatios()
                        {
                            BookValuePerShare = k.BookValuePerShare,
                            CentralIndexKey = k.CentralIndexKey,
                            DividendPayoutRatio = k.DividendPayoutRatio,
                            DividendsPerShare = k.DividendsPerShare,
                            FiscalPeriod = k.FiscalPeriod,
                            FiscalYear = k.FiscalYear,
                            NetFixedAssets = k.NetFixedAssets,
                            PeriodEndDate = k.PeriodEndDate,
                            Registrant = k.Registrant,
                            RetentionRatio = k.RetentionRatio,
                            Symbol = k.Symbol
                        })],
                    earningsReleases: (earningsReleasesModel?.Length ?? 0) == 0 ? []
                        : [.. earningsReleasesModel!.Select(k => new Analysis.Fundamental.EarningsRelease()
                        {
                            CentralIndexKey = k.CentralIndexKey,
                            ConferenceCallTime = k.ConferenceCallTime,
                            EarningsPerShare = k.EarningsPerShare,
                            EarningsPerShareForecast = k.EarningsPerShareForecast,
                            FiscalQuarterEndDate = k.FiscalQuarterEndDate,
                            MarketCap = k.MarketCap,
                            NumberOfForecasts = k.NumberOfForecasts,
                            PercentageSurprise = k.PercentageSurprise,
                            RegistrantName = k.RegistrantName,
                            Symbol = k.Symbol
                        })],
                    shortInterests: (shortsModel?.Length ?? 0) == 0 ? []
                        : [.. shortsModel!.Select(k => new Analysis.Fundamental.ShortInterest()
                        {
                            AverageDailyVolume = k.AverageDailyVolume,
                            ChangeInShortedSecurities = k.ChangeInShortedSecurities,
                            DaysToConvert = k.DaysToConvert,
                            IsStockSplit = k.IsStockSplit,
                            MarketCode = k.MarketCode,
                            PercentageChangeInShortedSecurities = k.PercentageChangeInShortedSecurities,
                            PreviousShortedSecurities = k.PreviousShortedSecurities,
                            SettlementDate = k.SettlementDate,
                            ShortedSecurities = k.ShortedSecurities,
                            Symbol = k.Symbol,
                            Title = k.Title
                        })],
                    executiveCompensations: (compsModel?.Length ?? 0) == 0 ? []
                        : [.. compsModel!.Select(k => new Analysis.Fundamental.ExecutiveCompensation()
                        {
                            Bonus = k.Bonus,
                            CentralIndexKey = k.CentralIndexKey,
                            FiscalYear = k.FiscalYear,
                            IncentivePlanCompensation = k.IncentivePlanCompensation,
                            Name = k.Name,
                            OtherCompensation = k.OtherCompensation,
                            Position = k.Position,
                            Registrant = k.Registrant,
                            Salary = k.Salary,
                            StockAwards = k.StockAwards,
                            Symbol = k.Symbol,
                            TotalCompensation = k.TotalCompensation
                        })],
                    employeeCounts: (empCountsModel?.Length ?? 0) == 0 ? []
                        : [.. empCountsModel!.Select(k => new Analysis.Fundamental.EmployeeCount()
                        {
                            CentralIndexKey = k.CentralIndexKey,
                            Count = k.Count,
                            FiscalYear = k.FiscalYear,
                            Registrant = k.Registrant,
                            Symbol = k.Symbol
                        })],
                    solvencyRatios: (solvencyModel?.Length ?? 0) == 0 ? []
                        : [.. solvencyModel!.Select(k => new Analysis.Fundamental.SolvencyRatios()
                        {
                            AssetCoverageRatio = k.AssetCoverageRatio,
                            CashFlowToDebtRatio = k.CashFlowToDebtRatio,
                            CentralIndexKey = k.CentralIndexKey,
                            DebtCoverageRatio = k.DebtCoverageRatio,
                            DebtToAssetsRatio = k.DebtToAssetsRatio,
                            DebtToCapitalRatio = k.DebtToCapitalRatio,
                            DebtToEquityRatio = k.DebtToEquityRatio,
                            DebtToIncomeRatio = k.DebtToIncomeRatio,
                            EquityRatio = k.EquityRatio,
                            FiscalPeriod = k.FiscalPeriod,
                            FiscalYear = k.FiscalYear,
                            InterestCoverageRatio = k.InterestCoverageRatio,
                            PeriodEndDate = k.PeriodEndDate,
                            Registrant = k.Registrant,
                            Symbol = k.Symbol
                        })],
                    priceActions: (priceActionsModel?.Length ?? 0) == 0 ? []
                        : [.. priceActionsModel!.Select(k =>
                            new Analysis.Technical.Charts.Ohlc(k.Symbol,
                               k.Date.GetValueOrDefault(),
                               k.Open.GetValueOrDefault(),
                               k.High.GetValueOrDefault(),
                               k.Low.GetValueOrDefault(),
                               k.Close.GetValueOrDefault(),
                               k.Volume.GetValueOrDefault())
                        )]
                );
            timer.Stop();
            _logger.LogInformation("{SYMBOL} processed in {TIME}",
                symbol.Symbol, timer.Elapsed.ToGeneralText());
        }
    }
}
