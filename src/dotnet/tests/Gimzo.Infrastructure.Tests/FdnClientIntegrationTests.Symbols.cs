using Gimzo.Infrastructure.DataProviders;

namespace Gimzo.Infrastructure.Tests;

public partial class FdnClientIntegrationTests : IClassFixture<IntegrationTestsFixture>
{
    private readonly IntegrationTestsFixture _fixture;

    public FdnClientIntegrationTests(IntegrationTestsFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task GetStockSymbolsAsync()
    {
        var client = new FinancialDataClient(_fixture.ApiKey);

        var symbols = await client.GetStockSymbolsAsync();

        Assert.NotNull(symbols);
        Assert.NotEmpty(symbols);
        Assert.Contains("MSFT", symbols.Select(k => k.Symbol));
    }

    [Fact]
    public async Task GetInternationalStockSymbolsAsync()
    {
        var client = new FinancialDataClient(_fixture.ApiKey);

        var symbols = await client.GetInternationalStockSymbolsAsync();

        Assert.NotNull(symbols);
        Assert.NotEmpty(symbols);
        Assert.Contains("key.TO", symbols.Select(k => k.Symbol));
    }


    [Fact]
    public async Task GetEtfSymbolsAsync()
    {
        var client = new FinancialDataClient(_fixture.ApiKey);

        var symbols = await client.GetEtfSymbolsAsync();

        Assert.NotNull(symbols);
        Assert.NotEmpty(symbols);
        Assert.Contains("SPY", symbols.Select(k => k.Symbol));
    }

    [Fact]
    public async Task GetCommoditySymbolsAsync()
    {
        var client = new FinancialDataClient(_fixture.ApiKey);

        var symbols = await client.GetCommoditySymbolsAsync();

        Assert.NotNull(symbols);
        Assert.NotEmpty(symbols);
        Assert.Contains("ZW", symbols.Select(k => k.Symbol));
    }

    [Fact]
    public async Task GetOtcSymbolsAsync()
    {
        var client = new FinancialDataClient(_fixture.ApiKey);

        var symbols = await client.GetOtcSymbolsAsync();

        Assert.NotNull(symbols);
        Assert.NotEmpty(symbols);
        Assert.Contains("ZZZOF", symbols.Select(k => k.Symbol));
    }

    [Fact]
    public async Task GetIndexSymbolsAsync()
    {
        var client = new FinancialDataClient(_fixture.ApiKey);

        var symbols = await client.GetIndexSymbolsAsync();

        Assert.NotNull(symbols);
        Assert.NotEmpty(symbols);
        Assert.Contains("^XSP", symbols.Select(k => k.Symbol));
    }

    [Fact]
    public async Task GetFuturesSymbolsAsync()
    {
        var client = new FinancialDataClient(_fixture.ApiKey);

        var symbols = await client.GetFuturesSymbolsAsync();

        Assert.NotNull(symbols);
        Assert.NotEmpty(symbols);
        Assert.Contains("ZWT", symbols.Select(k => k.Symbol));
    }

    [Fact]
    public async Task GetCryptoSymbolsAsync()
    {
        var client = new FinancialDataClient(_fixture.ApiKey);

        var symbols = await client.GetCryptoSymbolsAsync();

        Assert.NotNull(symbols);
        Assert.NotEmpty(symbols);
        Assert.Contains("ZRXUSD", symbols.Select(k => k.Symbol));
    }
}