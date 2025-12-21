using Gimzo.Infrastructure.DataProviders;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gimzo.Infrastructure.Tests;

public class IntegrationTestsFixture : IDisposable
{
    public string ApiKey { get; }

    public IntegrationTestsFixture()
    {
        IConfigurationBuilder builder = new ConfigurationBuilder()
            .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .AddJsonFile("secrets.json", optional: true, reloadOnChange: true);

        var configuration = builder.Build();

        ApiKey = configuration.GetSection("ApiKeys:financialdata.net")?.Value ?? "";
        if (string.IsNullOrWhiteSpace(ApiKey))
            throw new Exception("Could not find api key.");
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}
