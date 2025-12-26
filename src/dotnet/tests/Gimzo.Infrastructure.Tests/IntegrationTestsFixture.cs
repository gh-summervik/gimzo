using Dapper;
using Gimzo.Infrastructure.Database;
using Gimzo.Infrastructure.DataProviders.FinancialDataNet;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using System.Data;

namespace Gimzo.Infrastructure.Tests;

public class IntegrationTestsFixture : IDisposable
{
    public string ApiKey { get; }
    private readonly FinancialDataApiClient _client;
    private readonly DbDefPair _dbDefPair;

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

        SqlMapper.AddTypeHandler(new DateOnlyTypeHandler());
        SqlMapper.AddTypeHandler(new NullableDateOnlyTypeHandler());

        var dbDefs = DbDefPair.GetPairsFromConfiguration(configuration).ToArray();

        if (dbDefs.Length == 0)
            throw new Exception($"Could not extract {nameof(DbDefPair)} from configuration.");

        _dbDefPair = dbDefs.FirstOrDefault()!;
    }

    public (IDbConnection? CommandConn, IDbConnection? QueryConn) GetConnectionPairForDb() =>
        (_dbDefPair.GetCommandConnection(), _dbDefPair.GetQueryConnection());

    public FinancialDataApiClient Client => _client;

    public void Dispose()
    {
        _client.Dispose();
        GC.SuppressFinalize(this);
    }
}