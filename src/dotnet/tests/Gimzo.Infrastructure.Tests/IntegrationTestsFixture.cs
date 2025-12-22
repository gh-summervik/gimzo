using Gimzo.Infrastructure.DataProviders;
using Gimzo.Infrastructure.DataProviders.FinancialDataNet;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;

namespace Gimzo.Infrastructure.Tests;

public class IntegrationTestsFixture : IDisposable
{
    public string ApiKey { get; }
    private readonly FinancialDataApiClient _client;

    public IntegrationTestsFixture()
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .AddJsonFile("secrets.json", optional: true, reloadOnChange: true);

        var configuration = builder.Build();
        ApiKey = configuration.GetSection("ApiKeys:financialdata.net")?.Value ?? "";
        if (string.IsNullOrWhiteSpace(ApiKey))
            throw new Exception("Could not find api key.");

        var logger = NullLogger<FinancialDataApiClient>.Instance;
        _client = new FinancialDataApiClient(ApiKey, logger);
    }

    public FinancialDataApiClient Client => _client;

    public void Dispose()
    {
        _client.Dispose();
        GC.SuppressFinalize(this);
    }
}