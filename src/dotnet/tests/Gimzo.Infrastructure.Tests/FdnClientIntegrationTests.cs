namespace Gimzo.Infrastructure.Tests;

public class FdnClientIntegrationTests(IntegrationTestsFixture fixture) : IClassFixture<IntegrationTestsFixture>
{
    private readonly IntegrationTestsFixture _fixture = fixture;



    [Fact]
    public async Task GetStockSymbolsAsync()
    {
        var client = _fixture.Client;

        var symbols = await client.GetStockSymbolsAsync();

        Assert.NotNull(symbols);
        Assert.NotEmpty(symbols);
        //var msft = symbols.FirstOrDefault(k => k.Symbol.Equals("A"));
        Assert.Contains("A", symbols.Select(k => k.Symbol));
    }

    [Fact]
    public async Task GetEtfSymbolsAsync()
    {
        var client = _fixture.Client;

        var symbols = await client.GetEtfSymbolsAsync();

        Assert.NotNull(symbols);
        Assert.NotEmpty(symbols);
        Assert.Contains("SPY", symbols.Select(k => k.Symbol));
    }

    [Fact]
    public async Task GetCommoditySymbolsAsync()
    {
        var client = _fixture.Client;

        var symbols = await client.GetCommoditySymbolsAsync();

        Assert.NotNull(symbols);
        Assert.NotEmpty(symbols);
        Assert.Contains("ZW", symbols.Select(k => k.Symbol));
    }

    [Fact]
    public async Task GetOtcSymbolsAsync()
    {
        var client = _fixture.Client;

        var symbols = await client.GetOtcSymbolsAsync();

        Assert.NotNull(symbols);
        Assert.NotEmpty(symbols);
        Assert.Contains("ZZZOF", symbols.Select(k => k.Symbol));
    }

    [Fact]
    public async Task GetIndexSymbolsAsync()
    {
        var client = _fixture.Client;

        var symbols = await client.GetIndexSymbolsAsync();

        Assert.NotNull(symbols);
        Assert.NotEmpty(symbols);
        Assert.Contains("^XSP", symbols.Select(k => k.Symbol));
    }

    [Fact]
    public async Task GetFuturesSymbolsAsync()
    {
        var client = _fixture.Client;

        var symbols = await client.GetFuturesSymbolsAsync();

        Assert.NotNull(symbols);
        Assert.NotEmpty(symbols);
        Assert.Contains("ZWT", symbols.Select(k => k.Symbol));
    }

    [Fact]
    public async Task GetCryptoSymbolsAsync()
    {
        var client = _fixture.Client;

        var symbols = (await client.GetCryptoSymbolsAsync()).ToArray();

        Assert.NotNull(symbols);
        Assert.NotEmpty(symbols);
        Assert.Contains("ZRXUSD", symbols.Select(k => k.Symbol));
    }

    [Fact]
    public async Task GetStockPricesAsync()
    {
        var client = _fixture.Client;

        var prices = await client.GetStockPricesAsync("S");

        Assert.NotNull(prices);
        Assert.NotEmpty(prices);
    }

    [Fact]
    public async Task GetMinuteStockPricesAsync()
    {
        var client = _fixture.Client;

        var prices = (await client.GetMinutePricesAsync("NFLX", "2020-01-15")).ToArray();

        Assert.NotNull(prices);
        Assert.NotEmpty(prices);
    }

    [Fact]
    public async Task GetCommodityPricesAsync()
    {
        var client = _fixture.Client;

        var prices = (await client.GetCommodityPricesAsync("ZW")).ToArray();

        Assert.NotNull(prices);
        Assert.NotEmpty(prices);
    }


    [Fact]
    public async Task GetOtcPricesAsync()
    {
        var client = _fixture.Client;

        var prices = (await client.GetOtcPricesAsync("ZZZOF")).ToArray();

        Assert.NotNull(prices);
        Assert.NotEmpty(prices);
    }

    [Fact]
    public async Task GetIndexPricesAsync()
    {
        var client = _fixture.Client;

        var prices = (await client.GetIndexPricesAsync("^XSP")).ToArray();

        Assert.NotNull(prices);
        Assert.NotEmpty(prices);
    }

    [Fact]
    public async Task GetFuturePricesAsync()
    {
        var client = _fixture.Client;

        var prices = (await client.GetFuturesPricesAsync("ZWT")).ToArray();

        Assert.NotNull(prices);
        Assert.NotEmpty(prices);
    }

    /// <summary>
    /// This test fails - I think it's a defect in the API, but not 100% sure.
    /// </summary>
    [Fact]
    public async Task GetCryptoInformationAsync()
    {
        var client = _fixture.Client;

        var info = await client.GetCryptoInformationAsync("BTC");

        Assert.NotNull(info);
    }

    [Fact]
    public async Task GetCryptoPricesAsync()
    {
        var client = _fixture.Client;

        var prices = (await client.GetCryptoPricesAsync("ZRXUSD")).ToArray();

        Assert.NotNull(prices);
        Assert.NotEmpty(prices);
    }

    [Fact]
    public async Task GetMinuteCryptoPricesAsync()
    {
        var client = _fixture.Client;

        var prices = (await client.GetCryptoMinutePricesAsync("ZRXUSD", "2025-12-18")).ToArray();

        Assert.NotNull(prices);
        Assert.NotEmpty(prices);
    }

    [Fact]
    public async Task GetOptionChainAsync()
    {
        var client = _fixture.Client;
        var chain = (await client.GetOptionChainAsync("A")).ToArray();
        Assert.NotNull(chain);
        Assert.NotNull(chain);
    }

    [Fact]
    public async Task GetOptionPricesAsync()
    {
        var client = _fixture.Client;
        var prices = (await client.GetOptionPricesAsync("A")).ToArray();
        Assert.NotNull(prices);
        Assert.NotNull(prices);
    }

    [Fact]
    public async Task GetOptionGreeksAsync()
    {
        var client = _fixture.Client;
        var greeks = (await client.GetOptionGreeksAsync("A")).ToArray();
        Assert.NotNull(greeks);
        Assert.NotNull(greeks);
    }

    [Fact]
    public async Task GetCompanyInformationAsync()
    {
        var client = _fixture.Client;
        var info = await client.GetCompanyInformationAsync("A");
        Assert.NotNull(info);
    }

    [Fact]
    public async Task GetKeyMetricsAsync()
    {
        var client = _fixture.Client;
        var metrics = (await client.GetKeyMetricsAsync("A")).ToArray();
        Assert.NotNull(metrics);
        Assert.NotEmpty(metrics);
    }

    [Fact]
    public async Task GetMarketCapAsync()
    {
        var client = _fixture.Client;
        var cap = (await client.GetMarketCapAsync("A")).ToArray();
        Assert.NotNull(cap);
        Assert.NotEmpty(cap);
    }

    [Fact]
    public async Task GetEmployeeCountAsync()
    {
        var client = _fixture.Client;
        var counts = (await client.GetEmployeeCountAsync("A")).ToArray();
        Assert.NotNull(counts);
        Assert.NotEmpty(counts);
    }

    [Fact]
    public async Task GetExecutiveCompensationAsync()
    {
        var client = _fixture.Client;
        var comps = (await client.GetExecutiveCompensationAsync("A")).ToArray();
        Assert.NotNull(comps);
        Assert.NotEmpty(comps);
    }

    [Fact]
    public async Task GetSecurityInformationAsync()
    {
        var client = _fixture.Client;
        var info = await client.GetSecurityInformationAsync("A");
        Assert.NotNull(info);
    }

    [Fact]
    public async Task GetIncomeStatementsAsync()
    {
        var client = _fixture.Client;
        var statements = (await client.GetIncomeStatementsAsync("A")).ToArray();
        Assert.NotNull(statements);
        Assert.NotEmpty(statements);
    }

    [Fact]
    public async Task GetBalanceSheetStatementsAsync()
    {
        var client = _fixture.Client;
        var statements = (await client.GetBalanceSheetStatementsAsync("A")).ToArray();
        Assert.NotNull(statements);
        Assert.NotEmpty(statements);
    }

    [Fact]
    public async Task GetCashFlowStatementsAsync()
    {
        var client = _fixture.Client;
        var statements = (await client.GetCashFlowStatementsAsync("A")).ToArray();
        Assert.NotNull(statements);
        Assert.NotEmpty(statements);
    }

    [Fact]
    public async Task GetDividendsAsync()
    {
        var client = _fixture.Client;
        var divs = (await client.GetDividendsAsync("MSFT")).ToArray();
        Assert.NotNull(divs);
        Assert.NotEmpty(divs);
    }

    [Fact]
    public async Task GetShortInterestAsync()
    {
        var client = _fixture.Client;
        var shorts = (await client.GetShortInterestAsync("A")).ToArray();
        Assert.NotNull(shorts);
        Assert.NotEmpty(shorts);
    }

    [Fact]
    public async Task GetEarningsReleasesAsync()
    {
        var client = _fixture.Client;

        var releases = await client.GetEarningsReleasesAsync("A");

        Assert.NotNull(releases);
        Assert.NotEmpty(releases);
    }

    [Fact]
    public async Task GetEfficiencyRatiosAsync()
    {
        var client = _fixture.Client;

        var ratios = await client.GetEfficiencyRatiosAsync("A");

        Assert.NotNull(ratios);
        Assert.NotEmpty(ratios);
    }

    [Fact]
    public async Task GetIndexConstituentsAsync()
    {
        var client = _fixture.Client;

        var constituents = await client.GetIndexConstituentsAsync("^GSPC");

        Assert.NotNull(constituents);
        Assert.NotEmpty(constituents);
    }

    [Fact]
    public async Task GetInitialPublicOfferingsAsync()
    {
        var client = _fixture.Client;

        var ipos = (await client.GetInitialPublicOfferingsAsync("ABNB")).ToArray();

        Assert.NotNull(ipos);
        Assert.NotEmpty(ipos);
    }

    [Fact]
    public async Task GetLiquidityRatiosAsync()
    {
        var client = _fixture.Client;

        var ratios = await client.GetLiquidityRatiosAsync("A");

        Assert.NotNull(ratios);
        Assert.NotEmpty(ratios);
    }

    [Fact]
    public async Task GetProfitabilityRatiosAsync()
    {
        var client = _fixture.Client;

        var ratios = await client.GetProfitabilityRatiosAsync("A");

        Assert.NotNull(ratios);
        Assert.NotEmpty(ratios);
    }

    [Fact]
    public async Task GetSolvencyRatiosAsync()
    {
        var client = _fixture.Client;

        var ratios = await client.GetSolvencyRatiosAsync("A");

        Assert.NotNull(ratios);
        Assert.NotEmpty(ratios);
    }

    [Fact]
    public async Task GetStockSplitsAsync()
    {
        var client = _fixture.Client;

        var splits = await client.GetStockSplitsAsync("A");

        Assert.NotNull(splits);
        Assert.NotEmpty(splits);
    }

    [Fact]
    public async Task GetValuationRatiosAsync()
    {
        var client = _fixture.Client;

        var ratios = await client.GetValuationRatiosAsync("A");

        Assert.NotNull(ratios);
        Assert.NotEmpty(ratios);
    }
}