using Dapper;
using Gimzo.Infrastructure.Database;
using Gimzo.Infrastructure.Database.DataAccessObjects;

namespace Gimzo.Infrastructure.Tests;

public class DatabaseIntegrationTests : IClassFixture<IntegrationTestsFixture>
{
    private readonly IntegrationTestsFixture _fixture;

    public DatabaseIntegrationTests(IntegrationTestsFixture fixture)
    {
        _fixture = fixture;
    }

    private async Task<T?> FetchFromDb<T>(string sql, object dao)
    {
        using var queryConn = _fixture.GetConnectionPairForDb().QueryConn;
        Assert.NotNull(queryConn);
        var fromDb = await queryConn.QueryFirstOrDefaultAsync<T>(
            sql, dao);
        return fromDb;
    }

    [Fact]
    public async Task StockSymbols_WriteMergeReadDelete_Async()
    {
        using var cmdConn = _fixture.GetConnectionPairForDb().CommandConn;
        Assert.NotNull(cmdConn);
        var dao = new StockSymbol
        {
            Symbol = "TEST",
            Registrant = "registrant"
        };
        const string WhereClause = "WHERE symbol = @Symbol";
        // INSERT
        await cmdConn.ExecuteAsync(SqlRepository.InsertStockSymbols, dao);
        var fromDb = await FetchFromDb<StockSymbol>(
            $"{SqlRepository.SelectStockSymbols} {WhereClause}",
            dao);
        Assert.NotNull(fromDb);
        Assert.Equal(dao.Symbol, fromDb.Symbol);
        Assert.Equal(dao.Registrant, fromDb.Registrant);
        var dao2 = dao with { Registrant = nameof(StockSymbols_WriteMergeReadDelete_Async) };
        // MERGE
        await cmdConn.ExecuteAsync(SqlRepository.MergeStockSymbols, dao2);
        fromDb = await FetchFromDb<StockSymbol>(
            $"{SqlRepository.SelectStockSymbols} {WhereClause}",
            dao);
        Assert.NotNull(fromDb);
        Assert.Equal(dao2.Registrant, fromDb.Registrant);
        // DELETE
        await cmdConn.ExecuteAsync($"DELETE FROM public.stock_symbols {WhereClause}", dao);
        fromDb = await FetchFromDb<StockSymbol>(
            $"{SqlRepository.SelectStockSymbols} {WhereClause}",
            dao);
        Assert.Null(fromDb);
    }

    [Fact]
    public async Task EtfSymbols_WriteMergeReadDelete_Async()
    {
        using var cmdConn = _fixture.GetConnectionPairForDb().CommandConn;
        Assert.NotNull(cmdConn);
        var dao = new EtfSymbol
        {
            Symbol = "TEST",
            Description = "description"
        };
        const string WhereClause = "WHERE symbol = @Symbol";
        // INSERT
        await cmdConn.ExecuteAsync(SqlRepository.InsertEtfSymbols, dao);
        var fromDb = await FetchFromDb<EtfSymbol>(
            $"{SqlRepository.SelectEtfSymbols} {WhereClause}",
            dao);
        Assert.NotNull(fromDb);
        Assert.Equal(dao.Symbol, fromDb.Symbol);
        Assert.Equal(dao.Description, fromDb.Description);
        var dao2 = dao with { Description = nameof(EtfSymbols_WriteMergeReadDelete_Async) };
        // MERGE
        await cmdConn.ExecuteAsync(SqlRepository.MergeEtfSymbols, dao2);
        fromDb = await FetchFromDb<EtfSymbol>(
            $"{SqlRepository.SelectEtfSymbols} {WhereClause}",
            dao);
        Assert.NotNull(fromDb);
        Assert.Equal(dao2.Description, fromDb.Description);
        // DELETE
        await cmdConn.ExecuteAsync($"DELETE FROM public.etf_symbols {WhereClause}", dao);
        fromDb = await FetchFromDb<EtfSymbol>(
            $"{SqlRepository.SelectEtfSymbols} {WhereClause}",
            dao);
        Assert.Null(fromDb);
    }

    [Fact]
    public async Task CommoditySymbols_WriteMergeReadDelete_Async()
    {
        using var cmdConn = _fixture.GetConnectionPairForDb().CommandConn;
        Assert.NotNull(cmdConn);
        var dao = new CommoditySymbol
        {
            Symbol = "TEST",
            Description = "description"
        };
        const string WhereClause = "WHERE symbol = @Symbol";
        // INSERT
        await cmdConn.ExecuteAsync(SqlRepository.InsertCommoditySymbols, dao);
        var fromDb = await FetchFromDb<CommoditySymbol>(
            $"{SqlRepository.SelectCommoditySymbols} {WhereClause}",
            dao);
        Assert.NotNull(fromDb);
        Assert.Equal(dao.Symbol, fromDb.Symbol);
        Assert.Equal(dao.Description, fromDb.Description);
        var dao2 = dao with { Description = nameof(CommoditySymbols_WriteMergeReadDelete_Async) };
        // MERGE
        await cmdConn.ExecuteAsync(SqlRepository.MergeCommoditySymbols, dao2);
        fromDb = await FetchFromDb<CommoditySymbol>(
            $"{SqlRepository.SelectCommoditySymbols} {WhereClause}",
            dao);
        Assert.NotNull(fromDb);
        Assert.Equal(dao2.Description, fromDb.Description);
        // DELETE
        await cmdConn.ExecuteAsync($"DELETE FROM public.commodity_symbols {WhereClause}", dao);
        fromDb = await FetchFromDb<CommoditySymbol>(
            $"{SqlRepository.SelectCommoditySymbols} {WhereClause}",
            dao);
        Assert.Null(fromDb);
    }

    [Fact]
    public async Task OtcSymbols_WriteMergeReadDelete_Async()
    {
        using var cmdConn = _fixture.GetConnectionPairForDb().CommandConn;
        Assert.NotNull(cmdConn);
        var dao = new OtcSymbol
        {
            Symbol = "TEST",
            Title = "title"
        };
        const string WhereClause = "WHERE symbol = @Symbol";
        // INSERT
        await cmdConn.ExecuteAsync(SqlRepository.InsertOtcSymbols, dao);
        var fromDb = await FetchFromDb<OtcSymbol>(
            $"{SqlRepository.SelectOtcSymbols} {WhereClause}",
            dao);
        Assert.NotNull(fromDb);
        Assert.Equal(dao.Symbol, fromDb.Symbol);
        Assert.Equal(dao.Title, fromDb.Title);
        var dao2 = dao with { Title = nameof(OtcSymbols_WriteMergeReadDelete_Async) };
        // MERGE
        await cmdConn.ExecuteAsync(SqlRepository.MergeOtcSymbols, dao2);
        fromDb = await FetchFromDb<OtcSymbol>(
            $"{SqlRepository.SelectOtcSymbols} {WhereClause}",
            dao);
        Assert.NotNull(fromDb);
        Assert.Equal(dao2.Title, fromDb.Title);
        // DELETE
        await cmdConn.ExecuteAsync($"DELETE FROM public.otc_symbols {WhereClause}", dao);
        fromDb = await FetchFromDb<OtcSymbol>(
            $"{SqlRepository.SelectOtcSymbols} {WhereClause}",
            dao);
        Assert.Null(fromDb);
    }

    [Fact]
    public async Task CryptoSymbols_WriteMergeReadDelete_Async()
    {
        using var cmdConn = _fixture.GetConnectionPairForDb().CommandConn;
        Assert.NotNull(cmdConn);
        var dao = new CryptoSymbol
        {
            Symbol = "TEST",
            BaseAsset = "base_asset",
            QuoteAsset = "quote_asset"
        };
        const string WhereClause = "WHERE symbol = @Symbol";
        // INSERT
        await cmdConn.ExecuteAsync(SqlRepository.InsertCryptoSymbols, dao);
        var fromDb = await FetchFromDb<CryptoSymbol>(
            $"{SqlRepository.SelectCryptoSymbols} {WhereClause}",
            dao);
        Assert.NotNull(fromDb);
        Assert.Equal(dao.Symbol, fromDb.Symbol);
        Assert.Equal(dao.BaseAsset, fromDb.BaseAsset);
        Assert.Equal(dao.QuoteAsset, fromDb.QuoteAsset);
        var dao2 = dao with { BaseAsset = nameof(CryptoSymbols_WriteMergeReadDelete_Async) };
        // MERGE
        await cmdConn.ExecuteAsync(SqlRepository.MergeCryptoSymbols, dao2);
        fromDb = await FetchFromDb<CryptoSymbol>(
            $"{SqlRepository.SelectCryptoSymbols} {WhereClause}",
            dao);
        Assert.NotNull(fromDb);
        Assert.Equal(dao2.BaseAsset, fromDb.BaseAsset);
        // DELETE
        await cmdConn.ExecuteAsync($"DELETE FROM public.crypto_symbols {WhereClause}", dao);
        fromDb = await FetchFromDb<CryptoSymbol>(
            $"{SqlRepository.SelectCryptoSymbols} {WhereClause}",
            dao);
        Assert.Null(fromDb);
    }

    [Fact]
    public async Task IndexSymbols_WriteMergeReadDelete_Async()
    {
        using var cmdConn = _fixture.GetConnectionPairForDb().CommandConn;
        Assert.NotNull(cmdConn);

        var dao = new IndexSymbol
        {
            Symbol = "TEST",
            IndexName = "NAME"
        };
        const string WhereClause = "WHERE symbol = @Symbol";
        // INSERT
        await cmdConn.ExecuteAsync(SqlRepository.InsertIndexSymbols, dao);
        var fromDb = await FetchFromDb<IndexSymbol>(
            $"{SqlRepository.SelectIndexSymbols} {WhereClause}",
            dao);
        Assert.NotNull(fromDb);
        Assert.Equal(dao.Symbol, fromDb.Symbol);
        Assert.Equal(dao.IndexName, fromDb.IndexName);
        var dao2 = dao with { IndexName = nameof(IndexSymbols_WriteMergeReadDelete_Async) };
        // MERGE
        await cmdConn.ExecuteAsync(SqlRepository.MergeIndexSymbols, dao2);
        fromDb = await FetchFromDb<IndexSymbol>(
            $"{SqlRepository.SelectIndexSymbols} {WhereClause}",
            dao);
        Assert.NotNull(fromDb);
        Assert.Equal(dao2.IndexName, fromDb.IndexName);
        // DELETE
        await cmdConn.ExecuteAsync($"DELETE FROM public.index_symbols {WhereClause}", dao);
        fromDb = await FetchFromDb<IndexSymbol>(
            $"{SqlRepository.SelectIndexSymbols} {WhereClause}",
            dao);
        Assert.Null(fromDb);
    }

    [Fact]
    public async Task SecurityInformation_WriteMergeReadDelete_Async()
    {
        using var cmdConn = _fixture.GetConnectionPairForDb().CommandConn;
        Assert.NotNull(cmdConn);
        var dao = new SecurityInformation
        {
            Symbol = "TEST",
            Issuer = "issuer",
            Cusip = "cusip",
            Isin = "isin",
            Figi = "figi",
            Type = "type"
        };
        const string WhereClause = "WHERE symbol = @Symbol";
        // INSERT
        await cmdConn.ExecuteAsync(SqlRepository.InsertSecurityInformation, dao);
        var fromDb = await FetchFromDb<SecurityInformation>(
            $"{SqlRepository.SelectSecurityInformation} {WhereClause}",
            dao);
        Assert.NotNull(fromDb);
        Assert.Equal(dao.Symbol, fromDb.Symbol);
        Assert.Equal(dao.Issuer, fromDb.Issuer);
        Assert.Equal(dao.Cusip, fromDb.Cusip);
        Assert.Equal(dao.Isin, fromDb.Isin);
        Assert.Equal(dao.Figi, fromDb.Figi);
        Assert.Equal(dao.Type, fromDb.Type);
        var dao2 = dao with { Issuer = nameof(SecurityInformation_WriteMergeReadDelete_Async) };
        // MERGE
        await cmdConn.ExecuteAsync(SqlRepository.MergeSecurityInformation, dao2);
        fromDb = await FetchFromDb<SecurityInformation>(
            $"{SqlRepository.SelectSecurityInformation} {WhereClause}",
            dao);
        Assert.NotNull(fromDb);
        Assert.Equal(dao2.Issuer, fromDb.Issuer);
        // DELETE
        await cmdConn.ExecuteAsync($"DELETE FROM public.security_information {WhereClause}", dao);
        fromDb = await FetchFromDb<SecurityInformation>(
            $"{SqlRepository.SelectSecurityInformation} {WhereClause}",
            dao);
        Assert.Null(fromDb);
    }

    [Fact]
    public async Task EodPrices_WriteMergeReadDelete_Async()
    {
        using var cmdConn = _fixture.GetConnectionPairForDb().CommandConn;
        Assert.NotNull(cmdConn);
        var dao = new EodPrice
        {
            Symbol = "TEST",
            Date = new DateOnly(2023, 1, 1),
            Open = 100m,
            High = 110m,
            Low = 90m,
            Close = 105m,
            Volume = 100000
        };
        const string WhereClause = "WHERE symbol = @Symbol AND date_eod = @Date";
        // INSERT
        await cmdConn.ExecuteAsync(SqlRepository.InsertEodPrices, dao);
        var fromDb = await FetchFromDb<EodPrice>(
            $"{SqlRepository.SelectEodPrices} {WhereClause}",
            dao);
        Assert.NotNull(fromDb);
        Assert.Equal(dao.Symbol, fromDb.Symbol);
        Assert.Equal(dao.Date, fromDb.Date);
        Assert.Equal(dao.Open, fromDb.Open);
        Assert.Equal(dao.High, fromDb.High);
        Assert.Equal(dao.Low, fromDb.Low);
        Assert.Equal(dao.Close, fromDb.Close);
        Assert.Equal(dao.Volume, fromDb.Volume);
        var dao2 = dao with { Close = 106m };
        // MERGE
        await cmdConn.ExecuteAsync(SqlRepository.MergeEodPrices, dao2);
        fromDb = await FetchFromDb<EodPrice>(
            $"{SqlRepository.SelectEodPrices} {WhereClause}",
            dao);
        Assert.NotNull(fromDb);
        Assert.Equal(dao2.Close, fromDb.Close);
        // DELETE
        await cmdConn.ExecuteAsync($"DELETE FROM public.eod_prices {WhereClause}", dao);
        fromDb = await FetchFromDb<EodPrice>(
            $"{SqlRepository.SelectEodPrices} {WhereClause}",
            dao);
        Assert.Null(fromDb);
    }

    [Fact]
    public async Task UsCompanies_WriteMergeReadDelete_Async()
    {
        using var cmdConn = _fixture.GetConnectionPairForDb().CommandConn;
        Assert.NotNull(cmdConn);
        var dao = new CompanyInformation
        {
            CentralIndexKey = "TEST",
            Exchange = "exchange",
            Symbol = "TEST",
            Registrant = "registrant",
            Isin = "isin",
            Lei = "lei",
            Ein = "ein",
            SicCode = "sic_code",
            SicDescription = "sic_description",
            SicTitle = "sic_title",
            FiscalYearEnd = "fiscal_year_end",
            StateOfIncorporation = "state_of_incorporation",
            PhoneNumber = "phone_number",
            MailingAddress = "mailing_address",
            BusinessAddress = "business_address",
            FormerName = "former_name",
            Industry = "industry",
            DateFounding = "1999",
            ChiefExecutiveOfficer = "chief_executive_officer",
            NumberEmployees = 100,
            WebSite = "web_site",
            MarketCap = 1000000,
            SharesIssued = 100000,
            SharesOutstanding = 90000,
            Description = "description"
        };
        const string WhereClause = "WHERE central_index_key = @CentralIndexKey AND exchange = @Exchange AND symbol = @Symbol";
        // INSERT
        await cmdConn.ExecuteAsync(SqlRepository.InsertCompanyInfo, dao);
        var fromDb = await FetchFromDb<CompanyInformation>(
            $"{SqlRepository.SelectCompanyInfo} {WhereClause}",
            dao);
        Assert.NotNull(fromDb);
        Assert.Equal(dao.CentralIndexKey, fromDb.CentralIndexKey);
        Assert.Equal(dao.Exchange, fromDb.Exchange);
        Assert.Equal(dao.Symbol, fromDb.Symbol);
        Assert.Equal(dao.Registrant, fromDb.Registrant);
        Assert.Equal(dao.Isin, fromDb.Isin);
        Assert.Equal(dao.Lei, fromDb.Lei);
        Assert.Equal(dao.Ein, fromDb.Ein);
        Assert.Equal(dao.SicCode, fromDb.SicCode);
        Assert.Equal(dao.SicDescription, fromDb.SicDescription);
        Assert.Equal(dao.SicTitle, fromDb.SicTitle);
        Assert.Equal(dao.FiscalYearEnd, fromDb.FiscalYearEnd);
        Assert.Equal(dao.StateOfIncorporation, fromDb.StateOfIncorporation);
        Assert.Equal(dao.PhoneNumber, fromDb.PhoneNumber);
        Assert.Equal(dao.MailingAddress, fromDb.MailingAddress);
        Assert.Equal(dao.BusinessAddress, fromDb.BusinessAddress);
        Assert.Equal(dao.FormerName, fromDb.FormerName);
        Assert.Equal(dao.Industry, fromDb.Industry);
        Assert.Equal(dao.DateFounding, fromDb.DateFounding);
        Assert.Equal(dao.ChiefExecutiveOfficer, fromDb.ChiefExecutiveOfficer);
        Assert.Equal(dao.NumberEmployees, fromDb.NumberEmployees);
        Assert.Equal(dao.WebSite, fromDb.WebSite);
        Assert.Equal(dao.MarketCap, fromDb.MarketCap);
        Assert.Equal(dao.SharesIssued, fromDb.SharesIssued);
        Assert.Equal(dao.SharesOutstanding, fromDb.SharesOutstanding);
        Assert.Equal(dao.Description, fromDb.Description);
        var dao2 = dao with { Registrant = nameof(UsCompanies_WriteMergeReadDelete_Async) };
        // MERGE
        await cmdConn.ExecuteAsync(SqlRepository.MergeCompanyInfo, dao2);
        fromDb = await FetchFromDb<CompanyInformation>(
            $"{SqlRepository.SelectCompanyInfo} {WhereClause}",
            dao);
        Assert.NotNull(fromDb);
        Assert.Equal(dao2.Registrant, fromDb.Registrant);
        // DELETE
        await cmdConn.ExecuteAsync($"DELETE FROM public.us_companies {WhereClause}", dao);
        fromDb = await FetchFromDb<CompanyInformation>(
            $"{SqlRepository.SelectCompanyInfo} {WhereClause}",
            dao);
        Assert.Null(fromDb);
    }

    [Fact]
    public async Task LiquidityRatios_WriteMergeReadDelete_Async()
    {
        using var cmdConn = _fixture.GetConnectionPairForDb().CommandConn;
        Assert.NotNull(cmdConn);
        var dao = new LiquidityRatios
        {
            Symbol = "TEST",
            CentralIndexKey = "TEST",
            Registrant = "registrant",
            FiscalYear = "2023",
            FiscalPeriod = "Q1",
            PeriodEndDate = new DateOnly(2023, 3, 31),
            WorkingCapital = 100000m,
            CurrentRatio = 2.5,
            CashRatio = 1.2,
            QuickRatio = 1.8,
            DaysOfInventoryOutstanding = 45,
            DaysOfSalesOutstanding = 30,
            DaysPayableOutstanding = 60,
            CashConversionCycle = 15,
            SalesToWorkingCapitalRatio = 5.0,
            CashToCurrentLiabilitiesRatio = 0.8,
            WorkingCapitalToDebtRatio = 1.5,
            CashFlowAdequacyRatio = 1.2,
            SalesToCurrentAssetsRatio = 3.0,
            CashToCurrentAssetsRatio = 0.4,
            CashToWorkingCapitalRatio = 0.6,
            InventoryToWorkingCapitalRatio = 0.3,
            NetDebt = 50000m
        };
        const string WhereClause = "WHERE central_index_key = @CentralIndexKey AND fiscal_year = @FiscalYear AND fiscal_period = @FiscalPeriod";
        // INSERT
        await cmdConn.ExecuteAsync(SqlRepository.InsertLiquidityRatios, dao);
        var fromDb = await FetchFromDb<LiquidityRatios>(
            $"{SqlRepository.SelectLiquidityRatios} {WhereClause}",
            dao);
        Assert.NotNull(fromDb);
        Assert.Equal(dao.Symbol, fromDb.Symbol);
        Assert.Equal(dao.CentralIndexKey, fromDb.CentralIndexKey);
        Assert.Equal(dao.Registrant, fromDb.Registrant);
        Assert.Equal(dao.FiscalYear, fromDb.FiscalYear);
        Assert.Equal(dao.FiscalPeriod, fromDb.FiscalPeriod);
        Assert.Equal(dao.PeriodEndDate, fromDb.PeriodEndDate);
        Assert.Equal(dao.WorkingCapital, fromDb.WorkingCapital);
        Assert.Equal(dao.CurrentRatio, fromDb.CurrentRatio);
        Assert.Equal(dao.CashRatio, fromDb.CashRatio);
        Assert.Equal(dao.QuickRatio, fromDb.QuickRatio);
        Assert.Equal(dao.DaysOfInventoryOutstanding, fromDb.DaysOfInventoryOutstanding);
        Assert.Equal(dao.DaysOfSalesOutstanding, fromDb.DaysOfSalesOutstanding);
        Assert.Equal(dao.DaysPayableOutstanding, fromDb.DaysPayableOutstanding);
        Assert.Equal(dao.CashConversionCycle, fromDb.CashConversionCycle);
        Assert.Equal(dao.SalesToWorkingCapitalRatio, fromDb.SalesToWorkingCapitalRatio);
        Assert.Equal(dao.CashToCurrentLiabilitiesRatio, fromDb.CashToCurrentLiabilitiesRatio);
        Assert.Equal(dao.WorkingCapitalToDebtRatio, fromDb.WorkingCapitalToDebtRatio);
        Assert.Equal(dao.CashFlowAdequacyRatio, fromDb.CashFlowAdequacyRatio);
        Assert.Equal(dao.SalesToCurrentAssetsRatio, fromDb.SalesToCurrentAssetsRatio);
        Assert.Equal(dao.CashToCurrentAssetsRatio, fromDb.CashToCurrentAssetsRatio);
        Assert.Equal(dao.CashToWorkingCapitalRatio, fromDb.CashToWorkingCapitalRatio);
        Assert.Equal(dao.InventoryToWorkingCapitalRatio, fromDb.InventoryToWorkingCapitalRatio);
        Assert.Equal(dao.NetDebt, fromDb.NetDebt);
        var dao2 = dao with { Registrant = nameof(LiquidityRatios_WriteMergeReadDelete_Async) };
        // MERGE
        await cmdConn.ExecuteAsync(SqlRepository.MergeLiquidityRatios, dao2);
        fromDb = await FetchFromDb<LiquidityRatios>(
            $"{SqlRepository.SelectLiquidityRatios} {WhereClause}",
            dao);
        Assert.NotNull(fromDb);
        Assert.Equal(dao2.Registrant, fromDb.Registrant);
        // DELETE
        await cmdConn.ExecuteAsync($"DELETE FROM public.liquidity_ratios {WhereClause}", dao);
        fromDb = await FetchFromDb<LiquidityRatios>(
            $"{SqlRepository.SelectLiquidityRatios} {WhereClause}",
            dao);
        Assert.Null(fromDb);
    }

    [Fact]
    public async Task ProfitabilityRatios_WriteMergeReadDelete_Async()
    {
        using var cmdConn = _fixture.GetConnectionPairForDb().CommandConn;
        Assert.NotNull(cmdConn);
        var dao = new ProfitabilityRatios
        {
            Symbol = "TEST",
            CentralIndexKey = "TEST",
            Registrant = "registrant",
            FiscalYear = "2023",
            FiscalPeriod = "Q1",
            PeriodEndDate = new DateOnly(2023, 3, 31),
            Ebit = 50000m,
            Ebitda = 60000m,
            ProfitMargin = 0.2,
            GrossMargin = 0.5,
            OperatingMargin = 0.3,
            OperatingCashFlowMargin = 0.25,
            ReturnOnEquity = 0.15,
            ReturnOnAssets = 0.1,
            ReturnOnDebt = 0.08,
            CashReturnOnAssets = 0.12,
            CashTurnoverRatio = 4.0
        };
        const string WhereClause = "WHERE central_index_key = @CentralIndexKey AND fiscal_year = @FiscalYear AND fiscal_period = @FiscalPeriod";
        // INSERT
        await cmdConn.ExecuteAsync(SqlRepository.InsertProfitabilityRatios, dao);
        var fromDb = await FetchFromDb<ProfitabilityRatios>(
            $"{SqlRepository.SelectProfitabilityRatios} {WhereClause}",
            dao);
        Assert.NotNull(fromDb);
        Assert.Equal(dao.Symbol, fromDb.Symbol);
        Assert.Equal(dao.CentralIndexKey, fromDb.CentralIndexKey);
        Assert.Equal(dao.Registrant, fromDb.Registrant);
        Assert.Equal(dao.FiscalYear, fromDb.FiscalYear);
        Assert.Equal(dao.FiscalPeriod, fromDb.FiscalPeriod);
        Assert.Equal(dao.PeriodEndDate, fromDb.PeriodEndDate);
        Assert.Equal(dao.Ebit, fromDb.Ebit);
        Assert.Equal(dao.Ebitda, fromDb.Ebitda);
        Assert.Equal(dao.ProfitMargin, fromDb.ProfitMargin);
        Assert.Equal(dao.GrossMargin, fromDb.GrossMargin);
        Assert.Equal(dao.OperatingMargin, fromDb.OperatingMargin);
        Assert.Equal(dao.OperatingCashFlowMargin, fromDb.OperatingCashFlowMargin);
        Assert.Equal(dao.ReturnOnEquity, fromDb.ReturnOnEquity);
        Assert.Equal(dao.ReturnOnAssets, fromDb.ReturnOnAssets);
        Assert.Equal(dao.ReturnOnDebt, fromDb.ReturnOnDebt);
        Assert.Equal(dao.CashReturnOnAssets, fromDb.CashReturnOnAssets);
        Assert.Equal(dao.CashTurnoverRatio, fromDb.CashTurnoverRatio);
        var dao2 = dao with { Registrant = nameof(ProfitabilityRatios_WriteMergeReadDelete_Async) };
        // MERGE
        await cmdConn.ExecuteAsync(SqlRepository.MergeProfitabilityRatios, dao2);
        fromDb = await FetchFromDb<ProfitabilityRatios>(
            $"{SqlRepository.SelectProfitabilityRatios} {WhereClause}",
            dao);
        Assert.NotNull(fromDb);
        Assert.Equal(dao2.Registrant, fromDb.Registrant);
        // DELETE
        await cmdConn.ExecuteAsync($"DELETE FROM public.profitability_ratios {WhereClause}", dao);
        fromDb = await FetchFromDb<ProfitabilityRatios>(
            $"{SqlRepository.SelectProfitabilityRatios} {WhereClause}",
            dao);
        Assert.Null(fromDb);
    }

    [Fact]
    public async Task SolvencyRatios_WriteMergeReadDelete_Async()
    {
        using var cmdConn = _fixture.GetConnectionPairForDb().CommandConn;
        Assert.NotNull(cmdConn);
        var dao = new SolvencyRatios
        {
            Symbol = "TEST",
            CentralIndexKey = "TEST",
            Registrant = "registrant",
            FiscalYear = "2023",
            FiscalPeriod = "Q1",
            PeriodEndDate = new DateOnly(2023, 3, 31),
            EquityRatio = 0.6,
            DebtCoverageRatio = 2.0,
            AssetCoverageRatio = 1.5,
            InterestCoverageRatio = 3.0,
            DebtToEquityRatio = 0.67,
            DebtToAssetsRatio = 0.4,
            DebtToCapitalRatio = 0.4,
            DebtToIncomeRatio = 2.5,
            CashFlowToDebtRatio = 0.3
        };
        const string WhereClause = "WHERE central_index_key = @CentralIndexKey AND fiscal_year = @FiscalYear AND fiscal_period = @FiscalPeriod";
        // INSERT
        await cmdConn.ExecuteAsync(SqlRepository.InsertSolvencyRatios, dao);
        var fromDb = await FetchFromDb<SolvencyRatios>(
            $"{SqlRepository.SelectSolvencyRatios} {WhereClause}",
            dao);
        Assert.NotNull(fromDb);
        Assert.Equal(dao.Symbol, fromDb.Symbol);
        Assert.Equal(dao.CentralIndexKey, fromDb.CentralIndexKey);
        Assert.Equal(dao.Registrant, fromDb.Registrant);
        Assert.Equal(dao.FiscalYear, fromDb.FiscalYear);
        Assert.Equal(dao.FiscalPeriod, fromDb.FiscalPeriod);
        Assert.Equal(dao.PeriodEndDate, fromDb.PeriodEndDate);
        Assert.Equal(dao.EquityRatio, fromDb.EquityRatio);
        Assert.Equal(dao.DebtCoverageRatio, fromDb.DebtCoverageRatio);
        Assert.Equal(dao.AssetCoverageRatio, fromDb.AssetCoverageRatio);
        Assert.Equal(dao.InterestCoverageRatio, fromDb.InterestCoverageRatio);
        Assert.Equal(dao.DebtToEquityRatio, fromDb.DebtToEquityRatio);
        Assert.Equal(dao.DebtToAssetsRatio, fromDb.DebtToAssetsRatio);
        Assert.Equal(dao.DebtToCapitalRatio, fromDb.DebtToCapitalRatio);
        Assert.Equal(dao.DebtToIncomeRatio, fromDb.DebtToIncomeRatio);
        Assert.Equal(dao.CashFlowToDebtRatio, fromDb.CashFlowToDebtRatio);
        var dao2 = dao with { Registrant = nameof(SolvencyRatios_WriteMergeReadDelete_Async) };
        // MERGE
        await cmdConn.ExecuteAsync(SqlRepository.MergeSolvencyRatios, dao2);
        fromDb = await FetchFromDb<SolvencyRatios>(
            $"{SqlRepository.SelectSolvencyRatios} {WhereClause}",
            dao);
        Assert.NotNull(fromDb);
        Assert.Equal(dao2.Registrant, fromDb.Registrant);
        // DELETE
        await cmdConn.ExecuteAsync($"DELETE FROM public.solvency_ratios {WhereClause}", dao);
        fromDb = await FetchFromDb<SolvencyRatios>(
            $"{SqlRepository.SelectSolvencyRatios} {WhereClause}",
            dao);
        Assert.Null(fromDb);
    }

    [Fact]
    public async Task ValuationRatios_WriteMergeReadDelete_Async()
    {
        using var cmdConn = _fixture.GetConnectionPairForDb().CommandConn;
        Assert.NotNull(cmdConn);
        var dao = new ValuationRatios
        {
            Symbol = "TEST",
            CentralIndexKey = "TEST",
            Registrant = "registrant",
            FiscalYear = "2023",
            FiscalPeriod = "Q1",
            PeriodEndDate = new DateOnly(2023, 3, 31),
            DividendsPerShare = 1.5m,
            DividendPayoutRatio = 0.4,
            BookValuePerShare = 20m,
            RetentionRatio = 0.6,
            NetFixedAssets = 100000m
        };
        const string WhereClause = "WHERE central_index_key = @CentralIndexKey AND fiscal_year = @FiscalYear AND fiscal_period = @FiscalPeriod";
        // INSERT
        await cmdConn.ExecuteAsync(SqlRepository.InsertValuationRatios, dao);
        var fromDb = await FetchFromDb<ValuationRatios>(
            $"{SqlRepository.SelectValuationRatios} {WhereClause}",
            dao);
        Assert.NotNull(fromDb);
        Assert.Equal(dao.Symbol, fromDb.Symbol);
        Assert.Equal(dao.CentralIndexKey, fromDb.CentralIndexKey);
        Assert.Equal(dao.Registrant, fromDb.Registrant);
        Assert.Equal(dao.FiscalYear, fromDb.FiscalYear);
        Assert.Equal(dao.FiscalPeriod, fromDb.FiscalPeriod);
        Assert.Equal(dao.PeriodEndDate, fromDb.PeriodEndDate);
        Assert.Equal(dao.DividendsPerShare, fromDb.DividendsPerShare);
        Assert.Equal(dao.DividendPayoutRatio, fromDb.DividendPayoutRatio);
        Assert.Equal(dao.BookValuePerShare, fromDb.BookValuePerShare);
        Assert.Equal(dao.RetentionRatio, fromDb.RetentionRatio);
        Assert.Equal(dao.NetFixedAssets, fromDb.NetFixedAssets);
        var dao2 = dao with { Registrant = nameof(ValuationRatios_WriteMergeReadDelete_Async) };
        // MERGE
        await cmdConn.ExecuteAsync(SqlRepository.MergeValuationRatios, dao2);
        fromDb = await FetchFromDb<ValuationRatios>(
            $"{SqlRepository.SelectValuationRatios} {WhereClause}",
            dao);
        Assert.NotNull(fromDb);
        Assert.Equal(dao2.Registrant, fromDb.Registrant);
        // DELETE
        await cmdConn.ExecuteAsync($"DELETE FROM public.valuation_ratios {WhereClause}", dao);
        fromDb = await FetchFromDb<ValuationRatios>(
            $"{SqlRepository.SelectValuationRatios} {WhereClause}",
            dao);
        Assert.Null(fromDb);
    }

    [Fact]
    public async Task KeyMetrics_WriteMergeReadDelete_Async()
    {
        using var cmdConn = _fixture.GetConnectionPairForDb().CommandConn;
        Assert.NotNull(cmdConn);
        var dao = new KeyMetrics
        {
            Symbol = "TEST",
            CentralIndexKey = "TEST",
            Registrant = "registrant",
            FiscalYear = "2023",
            PeriodEndDate = new DateOnly(2023, 12, 31),
            EarningsPerShare = 5m,
            EarningsPerShareForecast = 5.5m,
            PriceToEarningsRatio = 20,
            ForwardPriceToEarningsRatio = 18,
            EarningsGrowthRate = 0.1,
            PriceEarningsToGrowthRate = 2.0,
            BookValuePerShare = 25m,
            PriceToBookRatio = 4.0,
            Ebitda = 100000,
            EnterpriseValue = 500000m,
            DividendYield = 0.03,
            DividendPayoutRatio = 0.4,
            DebtToEquityRatio = 0.5,
            CapitalExpenditures = 20000m,
            FreeCashFlow = 30000m,
            ReturnOnEquity = 0.2d,
            OneYearBeta = 1.1,
            ThreeYearBeta = 1.2,
            FiveYearBeta = 1.0
        };
        const string WhereClause = "WHERE central_index_key = @CentralIndexKey AND fiscal_year = @FiscalYear";
        // INSERT
        await cmdConn.ExecuteAsync(SqlRepository.InsertKeyMetrics, dao);
        var fromDb = await FetchFromDb<KeyMetrics>(
            $"{SqlRepository.SelectKeyMetrics} {WhereClause}",
            dao);
        Assert.NotNull(fromDb);
        Assert.Equal(dao.Symbol, fromDb.Symbol);
        Assert.Equal(dao.CentralIndexKey, fromDb.CentralIndexKey);
        Assert.Equal(dao.Registrant, fromDb.Registrant);
        Assert.Equal(dao.FiscalYear, fromDb.FiscalYear);
        Assert.Equal(dao.PeriodEndDate, fromDb.PeriodEndDate);
        Assert.Equal(dao.EarningsPerShare, fromDb.EarningsPerShare);
        Assert.Equal(dao.EarningsPerShareForecast, fromDb.EarningsPerShareForecast);
        Assert.Equal(dao.PriceToEarningsRatio, fromDb.PriceToEarningsRatio);
        Assert.Equal(dao.ForwardPriceToEarningsRatio, fromDb.ForwardPriceToEarningsRatio);
        Assert.Equal(dao.EarningsGrowthRate, fromDb.EarningsGrowthRate);
        Assert.Equal(dao.PriceEarningsToGrowthRate, fromDb.PriceEarningsToGrowthRate);
        Assert.Equal(dao.BookValuePerShare, fromDb.BookValuePerShare);
        Assert.Equal(dao.PriceToBookRatio, fromDb.PriceToBookRatio);
        Assert.Equal(dao.Ebitda, fromDb.Ebitda);
        Assert.Equal(dao.EnterpriseValue, fromDb.EnterpriseValue);
        Assert.Equal(dao.DividendYield, fromDb.DividendYield);
        Assert.Equal(dao.DividendPayoutRatio, fromDb.DividendPayoutRatio);
        Assert.Equal(dao.DebtToEquityRatio, fromDb.DebtToEquityRatio);
        Assert.Equal(dao.CapitalExpenditures, fromDb.CapitalExpenditures);
        Assert.Equal(dao.FreeCashFlow, fromDb.FreeCashFlow);
        Assert.Equal(dao.ReturnOnEquity, fromDb.ReturnOnEquity);
        Assert.Equal(dao.OneYearBeta, fromDb.OneYearBeta);
        Assert.Equal(dao.ThreeYearBeta, fromDb.ThreeYearBeta);
        Assert.Equal(dao.FiveYearBeta, fromDb.FiveYearBeta);
        var dao2 = dao with { Registrant = nameof(KeyMetrics_WriteMergeReadDelete_Async) };
        // MERGE
        await cmdConn.ExecuteAsync(SqlRepository.MergeKeyMetrics, dao2);
        fromDb = await FetchFromDb<KeyMetrics>(
            $"{SqlRepository.SelectKeyMetrics} {WhereClause}",
            dao);
        Assert.NotNull(fromDb);
        Assert.Equal(dao2.Registrant, fromDb.Registrant);
        // DELETE
        await cmdConn.ExecuteAsync($"DELETE FROM public.key_metrics {WhereClause}", dao);
        fromDb = await FetchFromDb<KeyMetrics>(
            $"{SqlRepository.SelectKeyMetrics} {WhereClause}",
            dao);
        Assert.Null(fromDb);
    }

    [Fact]
    public async Task MarketCaps_WriteMergeReadDelete_Async()
    {
        using var cmdConn = _fixture.GetConnectionPairForDb().CommandConn;
        Assert.NotNull(cmdConn);
        var dao = new MarketCap
        {
            Symbol = "TEST",
            CentralIndexKey = "TEST",
            Registrant = "registrant",
            FiscalYear = "2023",
            Value = 1000000D,
            ChangeInMarketCap = 100000D,
            PercentageChangeInMarketCap = 0.11,
            SharesOutstanding = 1000000,
            ChangeInSharesOutstanding = 100000,
            PercentageChangeInSharesOutstanding = 0.11
        };
        const string WhereClause = "WHERE central_index_key = @CentralIndexKey AND fiscal_year = @FiscalYear";
        // INSERT
        await cmdConn.ExecuteAsync(SqlRepository.InsertMarketCaps, dao);
        var fromDb = await FetchFromDb<MarketCap>(
            $"{SqlRepository.SelectMarketCaps} {WhereClause}",
            dao);
        Assert.NotNull(fromDb);
        Assert.Equal(dao.Symbol, fromDb.Symbol);
        Assert.Equal(dao.CentralIndexKey, fromDb.CentralIndexKey);
        Assert.Equal(dao.Registrant, fromDb.Registrant);
        Assert.Equal(dao.FiscalYear, fromDb.FiscalYear);
        Assert.Equal(dao.Value, fromDb.Value);
        Assert.Equal(dao.ChangeInMarketCap, fromDb.ChangeInMarketCap);
        Assert.Equal(dao.PercentageChangeInMarketCap, fromDb.PercentageChangeInMarketCap);
        Assert.Equal(dao.SharesOutstanding, fromDb.SharesOutstanding);
        Assert.Equal(dao.ChangeInSharesOutstanding, fromDb.ChangeInSharesOutstanding);
        Assert.Equal(dao.PercentageChangeInSharesOutstanding, fromDb.PercentageChangeInSharesOutstanding);
        var dao2 = dao with { Registrant = nameof(MarketCaps_WriteMergeReadDelete_Async) };
        // MERGE
        await cmdConn.ExecuteAsync(SqlRepository.MergeMarketCaps, dao2);
        fromDb = await FetchFromDb<MarketCap>(
            $"{SqlRepository.SelectMarketCaps} {WhereClause}",
            dao);
        Assert.NotNull(fromDb);
        Assert.Equal(dao2.Registrant, fromDb.Registrant);
        // DELETE
        await cmdConn.ExecuteAsync($"DELETE FROM public.market_caps {WhereClause}", dao);
        fromDb = await FetchFromDb<MarketCap>(
            $"{SqlRepository.SelectMarketCaps} {WhereClause}",
            dao);
        Assert.Null(fromDb);
    }

    [Fact]
    public async Task EmployeeCounts_WriteMergeReadDelete_Async()
    {
        using var cmdConn = _fixture.GetConnectionPairForDb().CommandConn;
        Assert.NotNull(cmdConn);
        var dao = new EmployeeCount
        {
            Symbol = "TEST",
            CentralIndexKey = "TEST",
            Registrant = "registrant",
            FiscalYear = "2023",
            Count = 1000
        };
        const string WhereClause = "WHERE central_index_key = @CentralIndexKey AND fiscal_year = @FiscalYear";
        // INSERT
        await cmdConn.ExecuteAsync(SqlRepository.InsertEmployeeCounts, dao);
        var fromDb = await FetchFromDb<EmployeeCount>(
            $"{SqlRepository.SelectEmployeeCounts} {WhereClause}",
            dao);
        Assert.NotNull(fromDb);
        Assert.Equal(dao.Symbol, fromDb.Symbol);
        Assert.Equal(dao.CentralIndexKey, fromDb.CentralIndexKey);
        Assert.Equal(dao.Registrant, fromDb.Registrant);
        Assert.Equal(dao.FiscalYear, fromDb.FiscalYear);
        Assert.Equal(dao.Count, fromDb.Count);
        var dao2 = dao with { Registrant = nameof(EmployeeCounts_WriteMergeReadDelete_Async) };
        // MERGE
        await cmdConn.ExecuteAsync(SqlRepository.MergeEmployeeCounts, dao2);
        fromDb = await FetchFromDb<EmployeeCount>(
            $"{SqlRepository.SelectEmployeeCounts} {WhereClause}",
            dao);
        Assert.NotNull(fromDb);
        Assert.Equal(dao2.Registrant, fromDb.Registrant);
        // DELETE
        await cmdConn.ExecuteAsync($"DELETE FROM public.employee_counts {WhereClause}", dao);
        fromDb = await FetchFromDb<EmployeeCount>(
            $"{SqlRepository.SelectEmployeeCounts} {WhereClause}",
            dao);
        Assert.Null(fromDb);
    }

    [Fact]
    public async Task ExecutiveCompensations_WriteMergeReadDelete_Async()
    {
        using var cmdConn = _fixture.GetConnectionPairForDb().CommandConn;
        Assert.NotNull(cmdConn);
        var dao = new ExecutiveCompensation
        {
            Symbol = "TEST",
            CentralIndexKey = "TEST",
            Registrant = "registrant",
            Name = "name",
            Position = "position",
            FiscalYear = "2023",
            Salary = 100000m,
            Bonus = 20000m,
            StockAwards = 30000m,
            IncentivePlanCompensation = 40000m,
            OtherCompensation = 5000m,
            TotalCompensation = 159000m
        };
        const string WhereClause = "WHERE central_index_key = @CentralIndexKey AND name = @Name AND position = @Position AND fiscal_year = @FiscalYear";
        // INSERT
        await cmdConn.ExecuteAsync(SqlRepository.InsertExecutiveCompensations, dao);
        var fromDb = await FetchFromDb<ExecutiveCompensation>(
            $"{SqlRepository.SelectExecutiveCompensations} {WhereClause}",
            dao);
        Assert.NotNull(fromDb);
        Assert.Equal(dao.Symbol, fromDb.Symbol);
        Assert.Equal(dao.CentralIndexKey, fromDb.CentralIndexKey);
        Assert.Equal(dao.Registrant, fromDb.Registrant);
        Assert.Equal(dao.Name, fromDb.Name);
        Assert.Equal(dao.Position, fromDb.Position);
        Assert.Equal(dao.FiscalYear, fromDb.FiscalYear);
        Assert.Equal(dao.Salary, fromDb.Salary);
        Assert.Equal(dao.Bonus, fromDb.Bonus);
        Assert.Equal(dao.StockAwards, fromDb.StockAwards);
        Assert.Equal(dao.IncentivePlanCompensation, fromDb.IncentivePlanCompensation);
        Assert.Equal(dao.OtherCompensation, fromDb.OtherCompensation);
        Assert.Equal(dao.TotalCompensation, fromDb.TotalCompensation);
        var dao2 = dao with { Registrant = nameof(ExecutiveCompensations_WriteMergeReadDelete_Async) };
        // MERGE
        await cmdConn.ExecuteAsync(SqlRepository.MergeExecutiveCompensations, dao2);
        fromDb = await FetchFromDb<ExecutiveCompensation>(
            $"{SqlRepository.SelectExecutiveCompensations} {WhereClause}",
            dao);
        Assert.NotNull(fromDb);
        Assert.Equal(dao2.Registrant, fromDb.Registrant);
        // DELETE
        await cmdConn.ExecuteAsync($"DELETE FROM public.executive_compensations {WhereClause}", dao);
        fromDb = await FetchFromDb<ExecutiveCompensation>(
            $"{SqlRepository.SelectExecutiveCompensations} {WhereClause}",
            dao);
        Assert.Null(fromDb);
    }

    [Fact]
    public async Task IncomeStatements_WriteMergeReadDelete_Async()
    {
        using var cmdConn = _fixture.GetConnectionPairForDb().CommandConn;
        Assert.NotNull(cmdConn);
        var dao = new IncomeStatement
        {
            Symbol = "TEST",
            CentralIndexKey = "TEST",
            Registrant = "registrant",
            FiscalYear = "2023",
            FiscalPeriod = "Q1",
            PeriodEndDate = new DateOnly(2023, 3, 31),
            Revenue = 100000m,
            CostOfRevenue = 60000m,
            GrossProfit = 40000m,
            ResearchDevelopmentExpenses = 5000m,
            GeneralAdminExpenses = 10000m,
            OperatingExpenses = 15000m,
            OperatingIncome = 25000m,
            InterestExpense = 2000m,
            InterestIncome = 500m,
            NetIncome = 23500m,
            EarningsPerShareBasic = 2.35m,
            EarningsPerShareDiluted = 2.3m,
            WeightedAverageSharesOutstandingBasic = 10000,
            WeightedAverageSharesOutstandingDiluted = 10200
        };
        const string WhereClause = "WHERE central_index_key = @CentralIndexKey AND fiscal_year = @FiscalYear AND fiscal_period = @FiscalPeriod";
        // INSERT
        await cmdConn.ExecuteAsync(SqlRepository.InsertIncomeStatements, dao);
        var fromDb = await FetchFromDb<IncomeStatement>(
            $"{SqlRepository.SelectIncomeStatements} {WhereClause}",
            dao);
        Assert.NotNull(fromDb);
        Assert.Equal(dao.Symbol, fromDb.Symbol);
        Assert.Equal(dao.CentralIndexKey, fromDb.CentralIndexKey);
        Assert.Equal(dao.Registrant, fromDb.Registrant);
        Assert.Equal(dao.FiscalYear, fromDb.FiscalYear);
        Assert.Equal(dao.FiscalPeriod, fromDb.FiscalPeriod);
        Assert.Equal(dao.PeriodEndDate, fromDb.PeriodEndDate);
        Assert.Equal(dao.Revenue, fromDb.Revenue);
        Assert.Equal(dao.CostOfRevenue, fromDb.CostOfRevenue);
        Assert.Equal(dao.GrossProfit, fromDb.GrossProfit);
        Assert.Equal(dao.ResearchDevelopmentExpenses, fromDb.ResearchDevelopmentExpenses);
        Assert.Equal(dao.GeneralAdminExpenses, fromDb.GeneralAdminExpenses);
        Assert.Equal(dao.OperatingExpenses, fromDb.OperatingExpenses);
        Assert.Equal(dao.OperatingIncome, fromDb.OperatingIncome);
        Assert.Equal(dao.InterestExpense, fromDb.InterestExpense);
        Assert.Equal(dao.InterestIncome, fromDb.InterestIncome);
        Assert.Equal(dao.NetIncome, fromDb.NetIncome);
        Assert.Equal(dao.EarningsPerShareBasic, fromDb.EarningsPerShareBasic);
        Assert.Equal(dao.EarningsPerShareDiluted, fromDb.EarningsPerShareDiluted);
        Assert.Equal(dao.WeightedAverageSharesOutstandingBasic, fromDb.WeightedAverageSharesOutstandingBasic);
        Assert.Equal(dao.WeightedAverageSharesOutstandingDiluted, fromDb.WeightedAverageSharesOutstandingDiluted);
        var dao2 = dao with { Registrant = nameof(IncomeStatements_WriteMergeReadDelete_Async) };
        // MERGE
        await cmdConn.ExecuteAsync(SqlRepository.MergeIncomeStatements, dao2);
        fromDb = await FetchFromDb<IncomeStatement>(
            $"{SqlRepository.SelectIncomeStatements} {WhereClause}",
            dao);
        Assert.NotNull(fromDb);
        Assert.Equal(dao2.Registrant, fromDb.Registrant);
        // DELETE
        await cmdConn.ExecuteAsync($"DELETE FROM public.income_statements {WhereClause}", dao);
        fromDb = await FetchFromDb<IncomeStatement>(
            $"{SqlRepository.SelectIncomeStatements} {WhereClause}",
            dao);
        Assert.Null(fromDb);
    }

    [Fact]
    public async Task BalanceSheets_WriteMergeReadDelete_Async()
    {
        using var cmdConn = _fixture.GetConnectionPairForDb().CommandConn;
        Assert.NotNull(cmdConn);
        var dao = new BalanceSheet
        {
            Symbol = "TEST",
            CentralIndexKey = "TEST",
            Registrant = "registrant",
            FiscalYear = "2023",
            FiscalPeriod = "Q1",
            PeriodEndDate = new DateOnly(2023, 3, 31),
            Cash = 50000m,
            MarketableSecuritiesCurrent = 20000m,
            AccountsReceivable = 30000m,
            Inventories = 40000m,
            NonTradeReceivables = 5000m,
            OtherAssetsCurrent = 10000m,
            TotalAssetsCurrent = 155000m,
            MarketableSecuritiesNonCurrent = 15000m,
            PropertyPlantEquipment = 100000m,
            OtherAssetsNonCurrent = 20000m,
            TotalAssetsNonCurrent = 135000m,
            TotalAssets = 290000m,
            AccountsPayable = 20000m,
            DeferredRevenue = 10000m,
            ShortTermDebt = 15000m,
            OtherLiabilitiesCurrent = 5000m,
            TotalLiabilitiesCurrent = 50000m,
            LongTermDebt = 80000m,
            OtherLiabilitiesNonCurrent = 10000m,
            TotalLiabilitiesNonCurrent = 90000m,
            TotalLiabilities = 140000m,
            CommonStock = 1000m,
            RetainedEarnings = 100000m,
            AccumulatedOtherComprehensiveIncome = 5000m,
            TotalShareholdersEquity = 150000m
        };
        const string WhereClause = "WHERE central_index_key = @CentralIndexKey AND fiscal_year = @FiscalYear AND fiscal_period = @FiscalPeriod";
        // INSERT
        await cmdConn.ExecuteAsync(SqlRepository.InsertBalanceSheets, dao);
        var fromDb = await FetchFromDb<BalanceSheet>(
            $"{SqlRepository.SelectBalanceSheets} {WhereClause}",
            dao);
        Assert.NotNull(fromDb);
        Assert.Equal(dao.Symbol, fromDb.Symbol);
        Assert.Equal(dao.CentralIndexKey, fromDb.CentralIndexKey);
        Assert.Equal(dao.Registrant, fromDb.Registrant);
        Assert.Equal(dao.FiscalYear, fromDb.FiscalYear);
        Assert.Equal(dao.FiscalPeriod, fromDb.FiscalPeriod);
        Assert.Equal(dao.PeriodEndDate, fromDb.PeriodEndDate);
        Assert.Equal(dao.Cash, fromDb.Cash);
        Assert.Equal(dao.MarketableSecuritiesCurrent, fromDb.MarketableSecuritiesCurrent);
        Assert.Equal(dao.AccountsReceivable, fromDb.AccountsReceivable);
        Assert.Equal(dao.Inventories, fromDb.Inventories);
        Assert.Equal(dao.NonTradeReceivables, fromDb.NonTradeReceivables);
        Assert.Equal(dao.OtherAssetsCurrent, fromDb.OtherAssetsCurrent);
        Assert.Equal(dao.TotalAssetsCurrent, fromDb.TotalAssetsCurrent);
        Assert.Equal(dao.MarketableSecuritiesNonCurrent, fromDb.MarketableSecuritiesNonCurrent);
        Assert.Equal(dao.PropertyPlantEquipment, fromDb.PropertyPlantEquipment);
        Assert.Equal(dao.OtherAssetsNonCurrent, fromDb.OtherAssetsNonCurrent);
        Assert.Equal(dao.TotalAssetsNonCurrent, fromDb.TotalAssetsNonCurrent);
        Assert.Equal(dao.TotalAssets, fromDb.TotalAssets);
        Assert.Equal(dao.AccountsPayable, fromDb.AccountsPayable);
        Assert.Equal(dao.DeferredRevenue, fromDb.DeferredRevenue);
        Assert.Equal(dao.ShortTermDebt, fromDb.ShortTermDebt);
        Assert.Equal(dao.OtherLiabilitiesCurrent, fromDb.OtherLiabilitiesCurrent);
        Assert.Equal(dao.TotalLiabilitiesCurrent, fromDb.TotalLiabilitiesCurrent);
        Assert.Equal(dao.LongTermDebt, fromDb.LongTermDebt);
        Assert.Equal(dao.OtherLiabilitiesNonCurrent, fromDb.OtherLiabilitiesNonCurrent);
        Assert.Equal(dao.TotalLiabilitiesNonCurrent, fromDb.TotalLiabilitiesNonCurrent);
        Assert.Equal(dao.TotalLiabilities, fromDb.TotalLiabilities);
        Assert.Equal(dao.CommonStock, fromDb.CommonStock);
        Assert.Equal(dao.RetainedEarnings, fromDb.RetainedEarnings);
        Assert.Equal(dao.AccumulatedOtherComprehensiveIncome, fromDb.AccumulatedOtherComprehensiveIncome);
        Assert.Equal(dao.TotalShareholdersEquity, fromDb.TotalShareholdersEquity);
        var dao2 = dao with { Registrant = nameof(BalanceSheets_WriteMergeReadDelete_Async) };
        // MERGE
        await cmdConn.ExecuteAsync(SqlRepository.MergeBalanceSheets, dao2);
        fromDb = await FetchFromDb<BalanceSheet>(
            $"{SqlRepository.SelectBalanceSheets} {WhereClause}",
            dao);
        Assert.NotNull(fromDb);
        Assert.Equal(dao2.Registrant, fromDb.Registrant);
        // DELETE
        await cmdConn.ExecuteAsync($"DELETE FROM public.balance_sheets {WhereClause}", dao);
        fromDb = await FetchFromDb<BalanceSheet>(
            $"{SqlRepository.SelectBalanceSheets} {WhereClause}",
            dao);
        Assert.Null(fromDb);
    }

    [Fact]
    public async Task CashFlowStatements_WriteMergeReadDelete_Async()
    {
        using var cmdConn = _fixture.GetConnectionPairForDb().CommandConn;
        Assert.NotNull(cmdConn);
        var dao = new CashFlowStatement
        {
            Symbol = "TEST",
            CentralIndexKey = "TEST",
            Registrant = "registrant",
            FiscalYear = "2023",
            FiscalPeriod = "Q1",
            PeriodEndDate = new DateOnly(2023, 3, 31),
            Depreciation = 5000m,
            ShareBasedCompensationExpense = 2000m,
            DeferredIncomeTaxExpense = 1000m,
            OtherNonCashIncomeExpense = 500m,
            ChangeInAccountsReceivable = -3000m,
            ChangeInInventories = -2000m,
            ChangeInNonTradeReceivables = -500m,
            ChangeInOtherAssets = -1000m,
            ChangeInAccountsPayable = 4000m,
            ChangeInDeferredRevenue = 2000m,
            ChangeInOtherLiabilities = 1000m,
            CashFromOperatingActivities = 25000m,
            PurchasesOfMarketableSecurities = -10000m,
            SalesOfMarketableSecurities = 5000m,
            AcquisitionOfProperty = -8000m,
            AcquisitionOfBusiness = -5000m,
            OtherInvestingActivities = -1000m,
            CashFromInvestingActivities = -19000m,
            TaxWithholdingForShareBasedCompensation = -500m,
            PaymentsOfDividends = -2000m,
            IssuanceOfCommonStock = 3000m,
            RepurchaseOfCommonStock = -4000m,
            IssuanceOfLongTermDebt = 10000m,
            RepaymentOfLongTermDebt = -5000m,
            OtherFinancingActivities = -1000m,
            CashFromFinancingActivities = -1500m,
            ChangeInCash = 4500m,
            CashAtEndOfPeriod = 55000m,
            IncomeTaxesPaid = 3000m,
            InterestPaid = 1500m
        };
        const string WhereClause = "WHERE central_index_key = @CentralIndexKey AND fiscal_year = @FiscalYear AND fiscal_period = @FiscalPeriod";
        // INSERT
        await cmdConn.ExecuteAsync(SqlRepository.InsertCashFlowStatements, dao);
        var fromDb = await FetchFromDb<CashFlowStatement>(
            $"{SqlRepository.SelectCashFlowStatements} {WhereClause}",
            dao);
        Assert.NotNull(fromDb);
        Assert.Equal(dao.Symbol, fromDb.Symbol);
        Assert.Equal(dao.CentralIndexKey, fromDb.CentralIndexKey);
        Assert.Equal(dao.Registrant, fromDb.Registrant);
        Assert.Equal(dao.FiscalYear, fromDb.FiscalYear);
        Assert.Equal(dao.FiscalPeriod, fromDb.FiscalPeriod);
        Assert.Equal(dao.PeriodEndDate, fromDb.PeriodEndDate);
        Assert.Equal(dao.Depreciation, fromDb.Depreciation);
        Assert.Equal(dao.ShareBasedCompensationExpense, fromDb.ShareBasedCompensationExpense);
        Assert.Equal(dao.DeferredIncomeTaxExpense, fromDb.DeferredIncomeTaxExpense);
        Assert.Equal(dao.OtherNonCashIncomeExpense, fromDb.OtherNonCashIncomeExpense);
        Assert.Equal(dao.ChangeInAccountsReceivable, fromDb.ChangeInAccountsReceivable);
        Assert.Equal(dao.ChangeInInventories, fromDb.ChangeInInventories);
        Assert.Equal(dao.ChangeInNonTradeReceivables, fromDb.ChangeInNonTradeReceivables);
        Assert.Equal(dao.ChangeInOtherAssets, fromDb.ChangeInOtherAssets);
        Assert.Equal(dao.ChangeInAccountsPayable, fromDb.ChangeInAccountsPayable);
        Assert.Equal(dao.ChangeInDeferredRevenue, fromDb.ChangeInDeferredRevenue);
        Assert.Equal(dao.ChangeInOtherLiabilities, fromDb.ChangeInOtherLiabilities);
        Assert.Equal(dao.CashFromOperatingActivities, fromDb.CashFromOperatingActivities);
        Assert.Equal(dao.PurchasesOfMarketableSecurities, fromDb.PurchasesOfMarketableSecurities);
        Assert.Equal(dao.SalesOfMarketableSecurities, fromDb.SalesOfMarketableSecurities);
        Assert.Equal(dao.AcquisitionOfProperty, fromDb.AcquisitionOfProperty);
        Assert.Equal(dao.AcquisitionOfBusiness, fromDb.AcquisitionOfBusiness);
        Assert.Equal(dao.OtherInvestingActivities, fromDb.OtherInvestingActivities);
        Assert.Equal(dao.CashFromInvestingActivities, fromDb.CashFromInvestingActivities);
        Assert.Equal(dao.TaxWithholdingForShareBasedCompensation, fromDb.TaxWithholdingForShareBasedCompensation);
        Assert.Equal(dao.PaymentsOfDividends, fromDb.PaymentsOfDividends);
        Assert.Equal(dao.IssuanceOfCommonStock, fromDb.IssuanceOfCommonStock);
        Assert.Equal(dao.RepurchaseOfCommonStock, fromDb.RepurchaseOfCommonStock);
        Assert.Equal(dao.IssuanceOfLongTermDebt, fromDb.IssuanceOfLongTermDebt);
        Assert.Equal(dao.RepaymentOfLongTermDebt, fromDb.RepaymentOfLongTermDebt);
        Assert.Equal(dao.OtherFinancingActivities, fromDb.OtherFinancingActivities);
        Assert.Equal(dao.CashFromFinancingActivities, fromDb.CashFromFinancingActivities);
        Assert.Equal(dao.ChangeInCash, fromDb.ChangeInCash);
        Assert.Equal(dao.CashAtEndOfPeriod, fromDb.CashAtEndOfPeriod);
        Assert.Equal(dao.IncomeTaxesPaid, fromDb.IncomeTaxesPaid);
        Assert.Equal(dao.InterestPaid, fromDb.InterestPaid);
        var dao2 = dao with { Registrant = nameof(CashFlowStatements_WriteMergeReadDelete_Async) };
        // MERGE
        await cmdConn.ExecuteAsync(SqlRepository.MergeCashFlowStatements, dao2);
        fromDb = await FetchFromDb<CashFlowStatement>(
            $"{SqlRepository.SelectCashFlowStatements} {WhereClause}",
            dao);
        Assert.NotNull(fromDb);
        Assert.Equal(dao2.Registrant, fromDb.Registrant);
        // DELETE
        await cmdConn.ExecuteAsync($"DELETE FROM public.cash_flow_statements {WhereClause}", dao);
        fromDb = await FetchFromDb<CashFlowStatement>(
            $"{SqlRepository.SelectCashFlowStatements} {WhereClause}",
            dao);
        Assert.Null(fromDb);
    }

    [Fact]
    public async Task StockSplits_WriteMergeReadDelete_Async()
    {
        using var cmdConn = _fixture.GetConnectionPairForDb().CommandConn;
        Assert.NotNull(cmdConn);
        var dao = new StockSplit
        {
            Symbol = "TEST",
            CentralIndexKey = "TEST",
            Registrant = "registrant",
            ExecutionDate = new DateOnly(2023, 6, 1),
            Multiplier = 2.0
        };
        const string WhereClause = "WHERE central_index_key = @CentralIndexKey AND execution_date = @ExecutionDate";
        // INSERT
        await cmdConn.ExecuteAsync(SqlRepository.InsertStockSplits, dao);
        var fromDb = await FetchFromDb<StockSplit>(
            $"{SqlRepository.SelectStockSplits} {WhereClause}",
            dao);
        Assert.NotNull(fromDb);
        Assert.Equal(dao.Symbol, fromDb.Symbol);
        Assert.Equal(dao.CentralIndexKey, fromDb.CentralIndexKey);
        Assert.Equal(dao.Registrant, fromDb.Registrant);
        Assert.Equal(dao.ExecutionDate, fromDb.ExecutionDate);
        Assert.Equal(dao.Multiplier, fromDb.Multiplier);
        var dao2 = dao with { Registrant = nameof(StockSplits_WriteMergeReadDelete_Async) };
        // MERGE
        await cmdConn.ExecuteAsync(SqlRepository.MergeStockSplits, dao2);
        fromDb = await FetchFromDb<StockSplit>(
            $"{SqlRepository.SelectStockSplits} {WhereClause}",
            dao);
        Assert.NotNull(fromDb);
        Assert.Equal(dao2.Registrant, fromDb.Registrant);
        // DELETE
        await cmdConn.ExecuteAsync($"DELETE FROM public.stock_splits {WhereClause}", dao);
        fromDb = await FetchFromDb<StockSplit>(
            $"{SqlRepository.SelectStockSplits} {WhereClause}",
            dao);
        Assert.Null(fromDb);
    }

    [Fact]
    public async Task Dividends_WriteMergeReadDelete_Async()
    {
        using var cmdConn = _fixture.GetConnectionPairForDb().CommandConn;
        Assert.NotNull(cmdConn);
        var dao = new Dividend
        {
            Symbol = "TEST",
            Registrant = "registrant",
            Type = "type",
            Amount = 1.0m,
            DeclarationDate = new DateOnly(2023, 2, 1),
            ExDate = new DateOnly(2023, 3, 1),
            RecordDate = new DateOnly(2023, 3, 2),
            PaymentDate = new DateOnly(2023, 3, 15)
        };
        const string WhereClause = "WHERE symbol = @Symbol AND ex_date = @ExDate";
        // INSERT
        await cmdConn.ExecuteAsync(SqlRepository.InsertDividends, dao);
        var fromDb = await FetchFromDb<Dividend>(
            $"{SqlRepository.SelectDividends} {WhereClause}",
            dao);
        Assert.NotNull(fromDb);
        Assert.Equal(dao.Symbol, fromDb.Symbol);
        Assert.Equal(dao.Registrant, fromDb.Registrant);
        Assert.Equal(dao.Type, fromDb.Type);
        Assert.Equal(dao.Amount, fromDb.Amount);
        Assert.Equal(dao.DeclarationDate, fromDb.DeclarationDate);
        Assert.Equal(dao.ExDate, fromDb.ExDate);
        Assert.Equal(dao.RecordDate, fromDb.RecordDate);
        Assert.Equal(dao.PaymentDate, fromDb.PaymentDate);
        var dao2 = dao with { Registrant = nameof(Dividends_WriteMergeReadDelete_Async) };
        // MERGE
        await cmdConn.ExecuteAsync(SqlRepository.MergeDividends, dao2);
        fromDb = await FetchFromDb<Dividend>(
            $"{SqlRepository.SelectDividends} {WhereClause}",
            dao);
        Assert.NotNull(fromDb);
        Assert.Equal(dao2.Registrant, fromDb.Registrant);
        // DELETE
        await cmdConn.ExecuteAsync($"DELETE FROM public.dividends {WhereClause}", dao);
        fromDb = await FetchFromDb<Dividend>(
            $"{SqlRepository.SelectDividends} {WhereClause}",
            dao);
        Assert.Null(fromDb);
    }

    [Fact]
    public async Task EarningsReleases_WriteMergeReadDelete_Async()
    {
        using var cmdConn = _fixture.GetConnectionPairForDb().CommandConn;
        Assert.NotNull(cmdConn);
        var dao = new EarningsRelease
        {
            Symbol = "TEST",
            CentralIndexKey = "TEST",
            RegistrantName = "registrant_name",
            MarketCap = 1000000m,
            FiscalQuarterEndDate = "2023-Q1",
            EarningsPerShare = 2.5m,
            EarningsPerShareForecast = 2.4m,
            PercentageSurprise = 0.04,
            NumberOfForecasts = 5,
            ConferenceCallTime = DateTime.UtcNow
        };
        const string WhereClause = "WHERE central_index_key = @CentralIndexKey AND fiscal_quarter_end_date = @FiscalQuarterEndDate";
        // INSERT
        await cmdConn.ExecuteAsync(SqlRepository.InsertEarningsReleases, dao);
        var fromDb = await FetchFromDb<EarningsRelease>(
            $"{SqlRepository.SelectEarningsReleases} {WhereClause}",
            dao);
        Assert.NotNull(fromDb);
        Assert.Equal(dao.Symbol, fromDb.Symbol);
        Assert.Equal(dao.CentralIndexKey, fromDb.CentralIndexKey);
        Assert.Equal(dao.RegistrantName, fromDb.RegistrantName);
        Assert.Equal(dao.MarketCap, fromDb.MarketCap);
        Assert.Equal(dao.FiscalQuarterEndDate, fromDb.FiscalQuarterEndDate);
        Assert.Equal(dao.EarningsPerShare, fromDb.EarningsPerShare);
        Assert.Equal(dao.EarningsPerShareForecast, fromDb.EarningsPerShareForecast);
        Assert.Equal(dao.PercentageSurprise, fromDb.PercentageSurprise);
        Assert.Equal(dao.NumberOfForecasts, fromDb.NumberOfForecasts);
        Assert.Equal(dao.ConferenceCallTime, fromDb.ConferenceCallTime);
        var dao2 = dao with { RegistrantName = nameof(EarningsReleases_WriteMergeReadDelete_Async) };
        // MERGE
        await cmdConn.ExecuteAsync(SqlRepository.MergeEarningsReleases, dao2);
        fromDb = await FetchFromDb<EarningsRelease>(
            $"{SqlRepository.SelectEarningsReleases} {WhereClause}",
            dao);
        Assert.NotNull(fromDb);
        Assert.Equal(dao2.RegistrantName, fromDb.RegistrantName);
        // DELETE
        await cmdConn.ExecuteAsync($"DELETE FROM public.earnings_releases {WhereClause}", dao);
        fromDb = await FetchFromDb<EarningsRelease>(
            $"{SqlRepository.SelectEarningsReleases} {WhereClause}",
            dao);
        Assert.Null(fromDb);
    }

    [Fact]
    public async Task EfficiencyRatios_WriteMergeReadDelete_Async()
    {
        using var cmdConn = _fixture.GetConnectionPairForDb().CommandConn;
        Assert.NotNull(cmdConn);
        var dao = new EfficiencyRatios
        {
            Symbol = "TEST",
            CentralIndexKey = "TEST",
            Registrant = "registrant",
            FiscalYear = "2023",
            FiscalPeriod = "Q1",
            PeriodEndDate = new DateOnly(2023, 3, 31),
            AssetTurnoverRatio = 1.5,
            InventoryTurnoverRatio = 6.0,
            AccountsReceivableTurnoverRatio = 8.0,
            AccountsPayableTurnoverRatio = 5.0,
            EquityMultiplier = 2.0,
            DaysSalesInInventory = 60,
            FixedAssetTurnoverRatio = 4.0,
            DaysWorkingCapital = 45,
            WorkingCapitalTurnoverRatio = 8.0,
            DaysCashOnHand = 30,
            CapitalIntensityRatio = 0.5,
            SalesToEquityRatio = 3.0,
            InventoryToSalesRatio = 0.15,
            InvestmentTurnoverRatio = 2.5,
            SalesToOperatingIncomeRatio = 5.0
        };
        const string WhereClause = "WHERE central_index_key = @CentralIndexKey AND fiscal_year = @FiscalYear AND fiscal_period = @FiscalPeriod";
        // INSERT
        await cmdConn.ExecuteAsync(SqlRepository.InsertEfficiencyRatios, dao);
        var fromDb = await FetchFromDb<EfficiencyRatios>(
            $"{SqlRepository.SelectEfficiencyRatios} {WhereClause}",
            dao);
        Assert.NotNull(fromDb);
        Assert.Equal(dao.Symbol, fromDb.Symbol);
        Assert.Equal(dao.CentralIndexKey, fromDb.CentralIndexKey);
        Assert.Equal(dao.Registrant, fromDb.Registrant);
        Assert.Equal(dao.FiscalYear, fromDb.FiscalYear);
        Assert.Equal(dao.FiscalPeriod, fromDb.FiscalPeriod);
        Assert.Equal(dao.PeriodEndDate, fromDb.PeriodEndDate);
        Assert.Equal(dao.AssetTurnoverRatio, fromDb.AssetTurnoverRatio);
        Assert.Equal(dao.InventoryTurnoverRatio, fromDb.InventoryTurnoverRatio);
        Assert.Equal(dao.AccountsReceivableTurnoverRatio, fromDb.AccountsReceivableTurnoverRatio);
        Assert.Equal(dao.AccountsPayableTurnoverRatio, fromDb.AccountsPayableTurnoverRatio);
        Assert.Equal(dao.EquityMultiplier, fromDb.EquityMultiplier);
        Assert.Equal(dao.DaysSalesInInventory, fromDb.DaysSalesInInventory);
        Assert.Equal(dao.FixedAssetTurnoverRatio, fromDb.FixedAssetTurnoverRatio);
        Assert.Equal(dao.DaysWorkingCapital, fromDb.DaysWorkingCapital);
        Assert.Equal(dao.WorkingCapitalTurnoverRatio, fromDb.WorkingCapitalTurnoverRatio);
        Assert.Equal(dao.DaysCashOnHand, fromDb.DaysCashOnHand);
        Assert.Equal(dao.CapitalIntensityRatio, fromDb.CapitalIntensityRatio);
        Assert.Equal(dao.SalesToEquityRatio, fromDb.SalesToEquityRatio);
        Assert.Equal(dao.InventoryToSalesRatio, fromDb.InventoryToSalesRatio);
        Assert.Equal(dao.InvestmentTurnoverRatio, fromDb.InvestmentTurnoverRatio);
        Assert.Equal(dao.SalesToOperatingIncomeRatio, fromDb.SalesToOperatingIncomeRatio);
        var dao2 = dao with { Registrant = nameof(EfficiencyRatios_WriteMergeReadDelete_Async) };
        // MERGE
        await cmdConn.ExecuteAsync(SqlRepository.MergeEfficiencyRatios, dao2);
        fromDb = await FetchFromDb<EfficiencyRatios>(
            $"{SqlRepository.SelectEfficiencyRatios} {WhereClause}",
            dao);
        Assert.NotNull(fromDb);
        Assert.Equal(dao2.Registrant, fromDb.Registrant);
        // DELETE
        await cmdConn.ExecuteAsync($"DELETE FROM public.efficiency_ratios {WhereClause}", dao);
        fromDb = await FetchFromDb<EfficiencyRatios>(
            $"{SqlRepository.SelectEfficiencyRatios} {WhereClause}",
            dao);
        Assert.Null(fromDb);
    }

    [Fact]
    public async Task ShortInterests_WriteMergeReadDelete_Async()
    {
        using var cmdConn = _fixture.GetConnectionPairForDb().CommandConn;
        Assert.NotNull(cmdConn);
        var dao = new ShortInterest
        {
            Symbol = "TEST",
            Title = "title",
            MarketCode = "market_code",
            SettlementDate = new DateOnly(2023, 3, 31),
            ShortedSecurities = 100000,
            PreviousShortedSecurities = 90000,
            ChangeInShortedSecurities = 10000,
            PercentageChangeInShortedSecurities = 0.111,
            AverageDailyVolume = 500000,
            DaysToCover = 0.2,
            IsStockSplit = false
        };
        const string WhereClause = "WHERE symbol = @Symbol AND settlement_date = @SettlementDate";
        // INSERT
        await cmdConn.ExecuteAsync(SqlRepository.InsertShortInterests, dao);
        var fromDb = await FetchFromDb<ShortInterest>(
            $"{SqlRepository.SelectShortInterests} {WhereClause}",
            dao);
        Assert.NotNull(fromDb);
        Assert.Equal(dao.Symbol, fromDb.Symbol);
        Assert.Equal(dao.Title, fromDb.Title);
        Assert.Equal(dao.MarketCode, fromDb.MarketCode);
        Assert.Equal(dao.SettlementDate, fromDb.SettlementDate);
        Assert.Equal(dao.ShortedSecurities, fromDb.ShortedSecurities);
        Assert.Equal(dao.PreviousShortedSecurities, fromDb.PreviousShortedSecurities);
        Assert.Equal(dao.ChangeInShortedSecurities, fromDb.ChangeInShortedSecurities);
        Assert.Equal(dao.PercentageChangeInShortedSecurities, fromDb.PercentageChangeInShortedSecurities);
        Assert.Equal(dao.AverageDailyVolume, fromDb.AverageDailyVolume);
        Assert.Equal(dao.DaysToCover, fromDb.DaysToCover);
        Assert.Equal(dao.IsStockSplit, fromDb.IsStockSplit);
        var dao2 = dao with { Title = nameof(ShortInterests_WriteMergeReadDelete_Async) };
        // MERGE
        await cmdConn.ExecuteAsync(SqlRepository.MergeShortInterests, dao2);
        fromDb = await FetchFromDb<ShortInterest>(
            $"{SqlRepository.SelectShortInterests} {WhereClause}",
            dao);
        Assert.NotNull(fromDb);
        Assert.Equal(dao2.Title, fromDb.Title);
        // DELETE
        await cmdConn.ExecuteAsync($"DELETE FROM public.short_interests {WhereClause}", dao);
        fromDb = await FetchFromDb<ShortInterest>(
            $"{SqlRepository.SelectShortInterests} {WhereClause}",
            dao);
        Assert.Null(fromDb);
    }

    [Fact]
    public async Task Processes_WriteMergeReadDelete_Async()
    {
        using var cmdConn = _fixture.GetConnectionPairForDb().CommandConn;
        Assert.NotNull(cmdConn);
        var dao = new Process
        {
            ProcessId = Guid.NewGuid(),
            ProcessType = "TEST",
            StartTime = DateTimeOffset.Now.AddMinutes(-5),
            FinishTime = DateTimeOffset.Now,
            InputPath = "Input",
            OutputPath = "Output",
            ParentProcessId = Guid.NewGuid()
        };

        const string WhereClause = "WHERE process_id = @ProcessId";
        // INSERT
        await cmdConn.ExecuteAsync(SqlRepository.InsertProcess, dao);
        var fromDb = await FetchFromDb<Process>(
            $"{SqlRepository.SelectProcess} {WhereClause}",
            dao);
        Assert.NotNull(fromDb);
        Assert.Equal(dao.ProcessId, fromDb.ProcessId);
        Assert.Equal(dao.ProcessType, fromDb.ProcessType);
        Assert.Equal(dao.StartTime, fromDb.StartTime);
        Assert.Equal(dao.FinishTime, fromDb.FinishTime);
        Assert.Equal(dao.InputPath, fromDb.InputPath);
        Assert.Equal(dao.OutputPath, fromDb.OutputPath);

        var dao2 = dao with { FinishTime = DateTimeOffset.Now.AddMinutes(1) };
        // MERGE
        await cmdConn.ExecuteAsync(SqlRepository.MergeProcess, dao2);
        fromDb = await FetchFromDb<Process>(
            $"{SqlRepository.SelectProcess} {WhereClause}", dao);
        Assert.NotNull(fromDb);
        Assert.Equal(dao2.FinishTime, fromDb.FinishTime);
        // DELETE
        await cmdConn.ExecuteAsync($"DELETE FROM public.processes {WhereClause}", dao);
        fromDb = await FetchFromDb<Process>($"{SqlRepository.SelectProcess} {WhereClause}", dao);
        Assert.Null(fromDb);
    }

    [Fact]
    public async Task IgnoredSymbol_WriteMergeReadDelete_Async()
    {
        using var cmdConn = _fixture.GetConnectionPairForDb().CommandConn;
        Assert.NotNull(cmdConn);
        var dao = new IgnoredSymbol
        {
            Symbol = "SYMBOL",
            Reason = "Reason",
            Expiration = DateOnly.FromDateTime(DateTime.Now)
        };

        const string WhereClause = "WHERE symbol = @Symbol";
        // INSERT
        await cmdConn.ExecuteAsync(SqlRepository.InsertIgnoredSymbol, dao);
        var fromDb = await FetchFromDb<IgnoredSymbol>(
            $"{SqlRepository.SelectIgnoredSymbol} {WhereClause}", dao);
        Assert.NotNull(fromDb);

        Assert.Equal(dao.Symbol, fromDb.Symbol);
        Assert.Equal(dao.Reason, fromDb.Reason);
        Assert.Equal(dao.Expiration, fromDb.Expiration);

        var dao2 = dao with { Reason = nameof(IgnoredSymbol_WriteMergeReadDelete_Async) };
        // MERGE
        await cmdConn.ExecuteAsync(SqlRepository.MergeIgnoredSymbol, dao2);
        fromDb = await FetchFromDb<IgnoredSymbol>(
            $"{SqlRepository.SelectIgnoredSymbol} {WhereClause}", dao);
        Assert.NotNull(fromDb);
        Assert.Equal(dao2.Reason, fromDb.Reason);
        // DELETE
        await cmdConn.ExecuteAsync($"DELETE FROM public.ignored_symbols {WhereClause}", dao);
        fromDb = await FetchFromDb<IgnoredSymbol>($"{SqlRepository.SelectIgnoredSymbol} {WhereClause}", dao);
        Assert.Null(fromDb);
    }
}