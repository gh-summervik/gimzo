/*
 * What do I want this to do?
 * 
 * First, it needs to anticipate possible bullish lows.
 *      It would need to look at the most recent full day and ask:
 *          If this is a low, would it be bullish?
 *      If the answer is YES, then it goes on the list for real-time monitoring.
 * 
 * Second, it needs to to look at the real-time monitoring list and
 * capture real-time data for the current day, and ask:
 *      Was yesterday a low?
 *          If YES, then where was the previous high?
 *              If the distance to the previous high is sufficient, then
 *              recommend going long.
 */
using Gimzo.AppServices;
using Gimzo.Common;
using Gimzo.Infrastructure.Database;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Reflection;

IConfiguration? configuration;

int exitCode = -1;

Stopwatch timer = Stopwatch.StartNew();

var appName = Assembly.GetExecutingAssembly().GetName().Name;
Debug.Assert(appName is not null);

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
services.AddMemoryCache();

IServiceProvider serviceProvider = services.BuildServiceProvider();

var logger = serviceProvider.GetRequiredService<ILogger<Program>>();

try
{
    ParseArguments(args, out string[] childArgs);

    Debug.Assert(config is not null);

    if (config.ShowHelp)
    {
        ShowHelp();
        Environment.Exit(0);
    }

    var dbDefPairs = DbDefPair.GetPairsFromConfiguration(configuration).ToArray();


    exitCode = 0;
}
catch (ArgumentException exc)
{
    exitCode = 4;
    LogHelper.LogError(logger, exc, exc.ToString());
}
catch (Exception exc)
{
    exitCode = 5;
    LogHelper.LogError(logger, exc, exc.ToString());
}
finally
{
    timer.Stop();
    LogHelper.LogInfo(logger, "Completed in {time}", timer.Elapsed.ToGeneralText());
    Thread.Sleep(500); // let the logger catch up.
    Environment.Exit(exitCode);
}

void ParseArguments(string[] args, out string[] childArgs)
{
    config = new Config(Assembly.GetExecutingAssembly().GetName().Name ?? nameof(Program), "v0.2", "US stock data collection and analysis.");

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
            default:
                throw new ArgumentException($"Unknown argument: {args[a]}");
        }
    }

    if (!config.IsValid(out string message))
    {
        Console.WriteLine(message);
        config.ShowHelp = true;
    }
}

void ShowHelp()
{
    CliArg[] args =
    [
        new CliArg(["-?", "?", "-h", "--help", "help"], [], false, "Show this help.")
    ];

    Console.WriteLine($"{config.AppName} {config.AppVersion}".Trim());
    Console.WriteLine();
    if (!string.IsNullOrWhiteSpace(config.Description))
    {
        Console.WriteLine(config.Description);
        Console.WriteLine();
    }
    Console.WriteLine(CliHelper.FormatArguments(args));
}

class Config(string appName, string appVersion, string? description)
{
    public string AppName { get; } = appName;
    public string AppVersion { get; } = appVersion;
    public string? Description { get; } = description;
    public bool Verbose { get; set; }
    public bool ShowHelp { get; set; }

    public bool IsValid(out string message)
    {
        message = "";
        if (!ShowHelp)
            message = "Temp message";

        return string.IsNullOrWhiteSpace(message);
    }
}