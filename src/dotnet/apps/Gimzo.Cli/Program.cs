using Gimzo.AppServices;
using Gimzo.AppServices.Analysis;
using Gimzo.AppServices.Backtesting;
using Gimzo.AppServices.Data;
using Gimzo.AppServices.Reports;
using Gimzo.Common;
using Gimzo.Infrastructure.Database;
using Gimzo.Infrastructure.DataProviders.FinancialDataNet;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;

IConfiguration? configuration;

int exitCode = -1;

Stopwatch timer = Stopwatch.StartNew();

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
services.AddMemoryCache();

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

    var dbDefPairs = DbDefPair.GetPairsFromConfiguration(configuration).ToArray();

    if (config.Import)
    {
        var process = Gimzo.AppServices.Process.Create("CLI", null, null, args);

        var fdnApiKey = configuration.GetSection("ApiKeys:financialdata.net").Value;
        Debug.Assert(fdnApiKey != null);

        Debug.Assert((dbDefPairs?.Length ?? 0) > 0);

        var apiClient = new FinancialDataApiClient(fdnApiKey,
            serviceProvider.GetRequiredService<ILogger<FinancialDataApiClient>>());

        var importer = new FinancialDataImporter(apiClient,
            dbDefPairs![0],
            serviceProvider.GetRequiredService<IMemoryCache>(),
            serviceProvider.GetRequiredService<ILogger<FinancialDataImporter>>());

        LogHelper.LogInfo(logger, "New process started; id: {id}", process.ProcessId);

        await importer.InitializeImportAsync(process, config.Weekday, config.Saturday, config.Sunday);
        await importer.ImportAsync();
        apiClient.Dispose();

        // do fundamental analysis here.
        //var analysisService = new CompanyAnalysisService(dbDefPairs![0],
        //    serviceProvider.GetRequiredService<IMemoryCache>(),
        //    serviceProvider.GetRequiredService<ILogger<CompanyAnalysisService>>());
    }

    if (config.Csv)
    {
        Debug.Assert((dbDefPairs?.Length ?? 0) > 0);

        var reportService = new ReportService(dbDefPairs![0],
            serviceProvider.GetRequiredService<IMemoryCache>(),
            serviceProvider.GetRequiredService<ILogger<ReportService>>());

        var fInfo = new FileInfo(config!.OutputFileName!);

        if (config.ReportName == "trend")
            await reportService.CreatePriceAndTrendReportAsync(config!.Symbol!, fInfo, config!.OverwriteOutputFile);

        if (config.ReportName == "extremes")
            await reportService.CreateExtremeReportAsync(config!.Symbol!, fInfo, config!.OverwriteOutputFile);

        Console.WriteLine($"{fInfo.FullName} created.");
    }

    if (config.Backtest)
    {
        var reportService = new ReportService(dbDefPairs![0],
            serviceProvider.GetRequiredService<IMemoryCache>(),
            serviceProvider.GetRequiredService<ILogger<ReportService>>());
        var backtestService = new BacktestingService(dbDefPairs![0],
            serviceProvider.GetRequiredService<IMemoryCache>(),
            serviceProvider.GetRequiredService<ILogger<BacktestingService>>());

        var pathToData = Path.Combine("data", "winners.txt");
        var symbols = await File.ReadAllLinesAsync(pathToData);
        //var symbols = (await reportService.GetAllSymbolsAsync()).ToArray();
        List<Ledger.PerformanceSummary> summaries = new(symbols.Length);
        List<string> winners = new(1500);

        int i = 0;
        int count = symbols.Length;
        foreach (var symbol in symbols)
        {
            if (string.IsNullOrWhiteSpace(symbol))
                continue;

            i++;
            //if (i > 5)
            //    break;

            Console.WriteLine($"Processing {symbol} - {i}/{count}");

            var ledger = await backtestService.ExecuteAsync(config.Scenario!, symbol);

            if (ledger != null)
            {
                var ledgerFileName = $"gimzo_ledger_{symbol}_{DateTime.Now:yyyyMMddHHmm}.csv";

                string fullPath = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                    ? Path.Combine("C:", "temp", "gimzo", ledgerFileName)
                    : Path.Combine("/c", "temp", "gimzo", ledgerFileName);

                await reportService.WriteLedgerAsync(ledger, new FileInfo(fullPath), true);
                var perf = ledger.GetPerformance();

                if (perf != null)
                {
                    summaries.Add(perf);
                    if (perf.TotalProfit > 0)
                        winners.Add(symbol);
                    //Console.WriteLine(perf.ToCliOutput());
                    continue;
                }
            }
        }

        string winnersPath = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            ? Path.Combine("C:", "temp", "gimzo", "winners.txt")
            : Path.Combine("/c", "temp", "gimzo", "winners.txt");

        await File.WriteAllLinesAsync(winnersPath, winners);

        decimal totalProfit = summaries.Sum(s => s.TotalProfit);
        long totalTrades = summaries.Sum(s => s.TotalTrades);
        double totalWins = summaries.Sum(s => s.TotalTrades * s.WinRate);
        decimal totalGrossWin = summaries.Sum(s => s.AverageWin * (decimal)(s.TotalTrades * s.WinRate));
        decimal totalGrossLoss = summaries.Sum(s => s.AverageLoss * (decimal)(s.TotalTrades * (1 - s.WinRate)));

        Console.WriteLine($"Total profit      : {totalProfit}");
        Console.WriteLine($"Weighted win rate : {(totalTrades > 0 ? totalWins / totalTrades : 0):P2}");
        Console.WriteLine($"Avg profit/trade  : {(totalTrades > 0 ? totalProfit / totalTrades : 0):C2}");
        Console.WriteLine($"Overall avg win   : {(totalWins > 0 ? totalGrossWin / (decimal)totalWins : 0):C2}");
        Console.WriteLine($"Overall avg loss  : {(totalTrades - totalWins > 0 ? totalGrossLoss / (decimal)(totalTrades - totalWins) : 0):C2}");
        Console.WriteLine($"Overall PF        : {(totalGrossLoss > 0M ? totalGrossWin / totalGrossLoss : 999999M):F2}");
        Console.WriteLine($"Total trades      : {totalTrades}");
    }

    if (config.Analyze)
    {
        var reportService = new ReportService(dbDefPairs![0],
            serviceProvider.GetRequiredService<IMemoryCache>(),
            serviceProvider.GetRequiredService<ILogger<ReportService>>());

        var analysisService = new CompanyAnalysisService(dbDefPairs![0],
            serviceProvider.GetRequiredService<IMemoryCache>(),
            serviceProvider.GetRequiredService<ILogger<CompanyAnalysisService>>());

        var symbols = string.IsNullOrWhiteSpace(config.Symbol)
            ? (await reportService.GetAllSymbolsAsync()).ToArray()
            : [config.Symbol];

        Dictionary<string, int> scores = new();

        foreach (var symbol in symbols)
        {
            var score = await analysisService.GetSiloScoreForSymbolAsync(symbol);
            if (score == null)
                continue;
            scores.Add(symbol, score.Value.Score1To99);
            if (symbols.Length > 1)
                Console.WriteLine($"{symbol}\t\t{score.Value.Score1To99}");
        }

        var sorted = scores.OrderByDescending(k => k.Value);

        foreach (var kvp in sorted)
            Console.WriteLine($"{kvp.Key}: {kvp.Value}");
    }

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
            case "--import":
            case "-i":
                config.Import = true;
                break;
            case "--saturday":
            case "--sat":
                config.Saturday = true;
                break;
            case "--sunday":
            case "--sun":
                config.Sunday = true;
                break;
            case "--weekday":
                config.Weekday = true;
                break;
            case "--csv":
            case "--report":
            case "-r":
                if (a < args.Length - 3)
                {
                    config.Csv = true;
                    config.ReportName = args[++a].ToLowerInvariant();
                    config.Symbol = args[++a].ToUpperInvariant();
                    config.OutputFileName = args[++a];
                }
                else
                    throw new ArgumentException($"{args[a]} must be followed by a report name, a ticker symbol, and an output file name.");
                break;
            case "-o":
            case "--overwrite":
                config.OverwriteOutputFile = true;
                break;
            case "-b":
            case "--backtest":
            case "--backtesting":
                if (a < args.Length - 1)
                {
                    config.Backtest = true;
                    config.Scenario = args[++a].ToLowerInvariant();
                }
                else
                    throw new ArgumentException($"{args[a]} must be followed by a scenario name.");
                break;
            case "-a":
            case "--analyze":
                config.Analyze = true;
                if (a < args.Length - 1)
                    config.Symbol = args[++a].ToUpperInvariant();
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
        new CliArg(["-i","--import"], [], false, "Import from financialdata.net."),
        new CliArg(["-sat","--saturday"], [], false, "Force the Saturday import workflow."),
        new CliArg(["-sun", "--sunday"], [], false, "Force the Sunday import workflow."),
        new CliArg(["--weekday"], [], false, "Force the Weekday import workflow."),
        new CliArg(["-r", "--csv", "--report"], ["symbol","report name", "output file name"], false, "Produce CSV file with specified report."),
        new CliArg(["-b", "--backtest"],["scenario name"], false, "Runs a backtesting scenario."),
        new CliArg(["-r", "--report"],["symbol","name of report","output file name"], false, "Produce a given report."),
        new CliArg(["-a", "--analyze"],["symbol (optional)"], false, "Get fundamental scores."),
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
    Console.WriteLine($"\t{config.AppName} will perform the workflow according to the current day of week,");
    Console.WriteLine("\tbut you can override this behavior with the --[weekday|sat|sun] options.");
    Console.WriteLine();
    Console.WriteLine("\tOn a first run, use the `--weekday` argument to ensure best behavior.");
    Console.WriteLine($"\tYou can run them all, as in `{config.AppName} --import --weekday --sat --sun`");
}
class Config(string appName, string appVersion, string? description)
{
    public string AppName { get; } = appName;
    public string AppVersion { get; } = appVersion;
    public string? Description { get; } = description;
    public bool Verbose { get; set; }
    public bool ShowHelp { get; set; }
    public bool Import { get; set; }
    public bool Csv { get; set; }
    public bool Weekday { get; set; }
    public bool Saturday { get; set; }
    public bool Sunday { get; set; }
    public string? OutputFileName { get; set; }
    public string? Symbol { get; set; }
    public bool Backtest { get; set; }
    public bool Analyze { get; set; }
    public string? Scenario { get; set; }
    public string? ReportName { get; set; }
    public bool OverwriteOutputFile { get; set; }

    public bool IsValid(out string message)
    {
        message = "";
        if (!Csv && !Import && !Backtest && !Analyze)
            message = "Either csv, import, backtest, or analyze must be specified.";
        else if ((Weekday || Saturday || Sunday) && !Import)
            message = "When specifying a day of the week, the import flag (--import) is required.";
        else if (Csv && (string.IsNullOrWhiteSpace(Symbol) || string.IsNullOrWhiteSpace(OutputFileName)))
            message = "When specifying CSV, both symbol and an output file name are required.";
        else if (!string.IsNullOrWhiteSpace(OutputFileName))
        {
            var fInfo = new FileInfo(OutputFileName);
            if (!fInfo.Extension.ToLowerInvariant().Equals(".csv"))
                message = "The output file must be of type csv.";
        }
        else if (Backtest && string.IsNullOrWhiteSpace(Scenario))
            message = "Scenario must be specified when backtest is chosen.";

        return string.IsNullOrWhiteSpace(message);
    }
}