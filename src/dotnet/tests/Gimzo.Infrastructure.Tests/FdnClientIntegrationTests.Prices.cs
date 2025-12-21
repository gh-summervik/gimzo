using Gimzo.Infrastructure.DataProviders;

namespace Gimzo.Infrastructure.Tests;

public partial class FdnClientIntegrationTests : IClassFixture<IntegrationTestsFixture>
{
    [Fact]
    public async Task GetStockPricesAsync()
    {
        var client = new FinancialDataClient(_fixture.ApiKey);

        var prices = await client.GetStockPricesAsync("NFLX");

        Assert.NotNull(prices);
        Assert.NotEmpty(prices);
    }

    [Fact]
    public async Task GetInternationalStockPricesAsync()
    {
        var client = new FinancialDataClient(_fixture.ApiKey);

        var prices = await client.GetInternationalStockPricesAsync("key.TO");

        Assert.NotNull(prices);
        Assert.NotEmpty(prices);
    }

    [Fact]
    public async Task GetMinuteStockPricesAsync()
    {
        var client = new FinancialDataClient(_fixture.ApiKey);

        var prices = (await client.GetMinutePricesAsync("NFLX", "2020-01-15")).ToArray();

        Assert.NotNull(prices);
        Assert.NotEmpty(prices);
    }

    [Fact]
    public async Task GetCommodityPricesAsync()
    {
        var client = new FinancialDataClient(_fixture.ApiKey);

        var prices = (await client.GetCommodityPricesAsync("ZW")).ToArray();

        Assert.NotNull(prices);
        Assert.NotEmpty(prices);
    }


    [Fact]
    public async Task GetOtcPricesAsync()
    {
        var client = new FinancialDataClient(_fixture.ApiKey);

        var prices = (await client.GetOtcPricesAsync("ZZZOF")).ToArray();

        Assert.NotNull(prices);
        Assert.NotEmpty(prices);
    }

    [Fact]
    public async Task GetIndexPricesAsync()
    {
        var client = new FinancialDataClient(_fixture.ApiKey);

        var prices = (await client.GetIndexPricesAsync("^XSP")).ToArray();

        Assert.NotNull(prices);
        Assert.NotEmpty(prices);
    }

    [Fact]
    public async Task GetFuturePricesAsync()
    {
        var client = new FinancialDataClient(_fixture.ApiKey);

        var prices = (await client.GetFuturesPricesAsync("ZWT")).ToArray();

        Assert.NotNull(prices);
        Assert.NotEmpty(prices);
    }

    [Fact]
    public async Task GetCryptoPricesAsync()
    {
        var client = new FinancialDataClient(_fixture.ApiKey);

        var prices = (await client.GetCryptoPricesAsync("ZRXUSD")).ToArray();

        Assert.NotNull(prices);
        Assert.NotEmpty(prices);
    }

    [Fact]
    public async Task GetMinuteCryptoPricesAsync()
    {
        var client = new FinancialDataClient(_fixture.ApiKey);

        var prices = (await client.GetCryptoMinutePricesAsync("ZRXUSD", "2025-12-18")).ToArray();

        Assert.NotNull(prices);
        Assert.NotEmpty(prices);
    }

    [Fact]
    public async Task GetOptionChainAsync()
    {
        var client = new FinancialDataClient(_fixture.ApiKey);
        var chain = (await client.GetOptionChainAsync("MSFT")).ToArray();
        Assert.NotNull(chain);
        Assert.NotNull(chain);
    }

    [Fact]
    public async Task GetOptionPricesAsync()
    {
        var client = new FinancialDataClient(_fixture.ApiKey);
        var prices = (await client.GetOptionPricesAsync("MSFT")).ToArray();
        Assert.NotNull(prices);
        Assert.NotNull(prices);
    }

    [Fact]
    public async Task GetOptionGreeksAsync()
    {
        var client = new FinancialDataClient(_fixture.ApiKey);
        var greeks = (await client.GetOptionGreeksAsync("MSFT")).ToArray();
        Assert.NotNull(greeks);
        Assert.NotNull(greeks);
    }

    [Fact]
    public async Task GetCompanyInformationAsync()
    {
        var client = new FinancialDataClient(_fixture.ApiKey);
        var info = await client.GetCompanyInformationAsync("MSFT");
        Assert.NotNull(info);
    }

    [Fact]
    public async Task GetInternationalCompanyInformationAsync()
    {
        var client = new FinancialDataClient(_fixture.ApiKey);
        var info = await client.GetInternationalCompanyInformationAsync("key.TO");
        Assert.NotNull(info);
    }

    [Fact]
    public async Task GetKeyMetricsAsync()
    {
        var client = new FinancialDataClient(_fixture.ApiKey);
        var metrics = (await client.GetKeyMetricsAsync("MSFT")).ToArray();
        Assert.NotNull(metrics);
        Assert.NotEmpty(metrics);
    }

    [Fact]
    public async Task GetMarketCapAsync()
    {
        var client = new FinancialDataClient(_fixture.ApiKey);
        var cap = (await client.GetMarketCapAsync("MSFT")).ToArray();
        Assert.NotNull(cap);
        Assert.NotEmpty(cap);
    }

    [Fact]
    public async Task GetEmployeeCountAsync()
    {
        var client = new FinancialDataClient(_fixture.ApiKey);
        var counts = (await client.GetEmployeeCountAsync("MSFT")).ToArray();
        Assert.NotNull(counts);
        Assert.NotEmpty(counts);
    }

    [Fact]
    public async Task GetExecutiveCompensationAsync()
    {
        var client = new FinancialDataClient(_fixture.ApiKey);
        var comps = (await client.GetExecutiveCompensationAsync("MSFT")).ToArray();
        Assert.NotNull(comps);
        Assert.NotEmpty(comps);
    }

    [Fact]
    public async Task GetSecurityInformationAsync()
    {
        var client = new FinancialDataClient(_fixture.ApiKey);
        var info = await client.GetSecurityInformationAsync("MSFT");
        Assert.NotNull(info);
    }

    [Fact]
    public async Task GetIncomeStatementsAsync()
    {
        var client = new FinancialDataClient(_fixture.ApiKey);
        var statements = (await client.GetIncomeStatementsAsync("MSFT")).ToArray();
        Assert.NotNull(statements);
        Assert.NotEmpty(statements);
    }

    [Fact]
    public async Task GetBalanceSheetStatementsAsync()
    {
        var client = new FinancialDataClient(_fixture.ApiKey);
        var statements = (await client.GetBalanceSheetStatementsAsync("MSFT")).ToArray();
        Assert.NotNull(statements);
        Assert.NotEmpty(statements);
    }

    [Fact]
    public async Task GetCashFlowStatementsAsync()
    {
        var client = new FinancialDataClient(_fixture.ApiKey);
        var statements = (await client.GetCashFlowStatementsAsync("MSFT")).ToArray();
        Assert.NotNull(statements);
        Assert.NotEmpty(statements);
    }

    [Fact]
    public async Task GetDividendsAsync()
    {
        var client = new FinancialDataClient(_fixture.ApiKey);
        var divs = (await client.GetDividendsAsync("MSFT")).ToArray();
        Assert.NotNull(divs);
        Assert.NotEmpty(divs);
    }

    [Fact]
    public async Task GetShortInterestAsync()
    {
        var client = new FinancialDataClient(_fixture.ApiKey);
        var shorts = (await client.GetShortInterestAsync("MSFT")).ToArray();
        Assert.NotNull(shorts);
        Assert.NotEmpty(shorts);
    }
}


