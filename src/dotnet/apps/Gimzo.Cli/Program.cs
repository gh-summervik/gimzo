using Gimzo.AppServices.Cli;
using Gimzo.AppServices.DataImports;
using Gimzo.Infrastructure.DataProviders.FinancialDataNet;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Reflection;

IConfiguration? configuration;

int exitCode = -1;

var appName = Assembly.GetExecutingAssembly().GetName().Name;
Debug.Assert(appName != null);

Config? config = null;

IConfigurationBuilder builder = new ConfigurationBuilder()
    .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile("secrets.json", optional: false, reloadOnChange: true);

configuration = builder.Build();

IServiceCollection services = new ServiceCollection();
services.AddLogging(builder =>
{
    builder.AddConsole();
});

IServiceProvider serviceProvider = services.BuildServiceProvider();

var logger = serviceProvider.GetRequiredService<ILogger<Program>>();


try
{
    ParseArguments(args, out string[] childArgs);

    Debug.Assert(config != null);

    if (config.ShowHelp)
    {
        ShowHelp();
        Environment.Exit(0);
    }

    if (config.Import)
    {
        var fdnApiKey = configuration.GetSection("ApiKeys:financialdata.net").Value;

        Debug.Assert(fdnApiKey != null);
        var apiClient = new FinancialDataApiClient(fdnApiKey,
            serviceProvider.GetRequiredService<ILogger<FinancialDataApiClient>>());
        var importer = new FinancialDataImporter(apiClient,
            serviceProvider.GetRequiredService<ILogger<FinancialDataImporter>>());
        Guid processId = Guid.NewGuid();
        await importer.Import(processId);
    }

    exitCode = 0;
}
catch (ArgumentException exc)
{
    exitCode = 4; 
    logger?.LogError(exc, exc.ToString());
}
catch (Exception exc)
{
    exitCode = 5;
    logger?.LogError(exc, exc.ToString());
}
finally
{
    Environment.Exit(exitCode);
}

void ShowHelp()
{
    CliArg[] args =
    [
        new CliArg(["--import","-i"], [], false, "Import from financialdata.net."),
        new CliArg(["-?", "?", "-h", "--help", "help"], ["command name"], false, "Show this help.")
    ];

    Console.WriteLine($"{config.AppName} {config.AppVersion}".Trim());
    Console.WriteLine();
    if (!string.IsNullOrWhiteSpace(config.Description))
   {
        Console.WriteLine(config.Description);
        Console.WriteLine();
    }
    Console.WriteLine(CliHelper.FormatArguments(args));
    Console.WriteLine();
}

void ParseArguments(string[] args, out string[] childArgs)
{
    config = new Config(Assembly.GetExecutingAssembly().GetName().Name ?? nameof(Program), "v1", "Gimzo");

    childArgs = [];

    for (int a = 0; a < args.Length; a++)
    {
        string argument = args[a].ToLower();

        switch (argument)
        {
            case "help":
            case "--help":
            case "-h":
            case "-?":
            case "?":
                config.ShowHelp = true;
                break;
            case "--import":
            case "-i":
                config.Import = true;
                break;
            default:
                throw new ArgumentException($"Unknown argument: {args[a]}");
        }
    }
}

class Config(string appName, string appVersion, string? description)
    : CliConfigBase(appName, appVersion, description)
{
    public bool Import { get; set; }
}