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
using System.Collections.Immutable;
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

        var importer = new FinancialDataImporter(
            apiClient,
            dbDefPairs![0],
            serviceProvider.GetRequiredService<IMemoryCache>(),
            serviceProvider.GetRequiredService<ILogger<FinancialDataImporter>>());

        LogHelper.LogInfo(logger, "New process started; id: {id}", process.ProcessId);

        await importer.InitializeImportAsync(process, config.Weekday, config.Saturday, config.Sunday, config.ImportCleanupOnly);
        await importer.ImportAsync();
        importer.Dispose();

        LogHelper.LogInfo(logger, "Capturing company valuations.");
        var coAnalysisService = new CompanyAnalysisService(dbDefPairs![0],
            serviceProvider.GetRequiredService<IMemoryCache>(),
            serviceProvider.GetRequiredService<ILogger<CompanyAnalysisService>>());

        LogHelper.LogInfo(logger, "Capturing industry valuations.");
        var indAnalysisService = new IndustryAnalysisService(dbDefPairs![0],
            serviceProvider.GetRequiredService<IMemoryCache>(),
            serviceProvider.GetRequiredService<ILogger<IndustryAnalysisService>>());

        var t1 = coAnalysisService.SaveAllSiloScoresAsync(process.ProcessId);
        var t2 = indAnalysisService.SaveAllIndustryScoresAsync(process.ProcessId);

        await Task.WhenAll(t1, t2);
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

        var symbols = (await backtestService.GetSymbolsToTest(minAbsoluteScore: 40,
            minCompanyPercentileRank: 50,
            minIndustryRank: 0)).ToImmutableArray();

        List<Ledger.PerformanceSummary> summaries = new(symbols.Length);

        DirectoryInfo targetDirInfo = new(RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            ? Path.Combine("C:", "temp", "gimzo")
            : Path.Combine("/", "c", "temp", "gimzo"));

        if (!targetDirInfo.Exists)
            targetDirInfo.Create();
        else
            foreach (var file in targetDirInfo.GetFiles("gimzo_ledger*.csv").Union(targetDirInfo.GetFiles("gimzo_ledger*.txt")))
                file.Delete();

        int i = 0;
        int count = symbols.Length;
        int tradesIn2025 = 0;

        foreach (var symbol in symbols)
        {
            i++;

            if (string.IsNullOrWhiteSpace(symbol))
            {
                LogHelper.LogWarning(logger, "Logic error - a blank symbol was found in the list of symbols. Do better.");
                continue;
            }

            Console.Write($"\rProcessing {symbol,-8}\t{i,4}/{count}");

            var ledger = await backtestService.ExecuteAsync(config.Scenario!, symbol);

            if (ledger != null)
            {
                var entries2025 = ledger.GetPairs().Where(k => k.Open.Date > new DateOnly(2024, 12, 31)).ToImmutableArray();
                tradesIn2025 += entries2025.Length;

                var ledgerFileName = $"gimzo_ledger_{symbol}_{DateTime.Now:yyyyMMddHHmm}.csv";

                string fullPath = Path.Combine(targetDirInfo.FullName, ledgerFileName);

                await reportService.WriteLedgerAsync(ledger, new FileInfo(fullPath), true);
                var perf = ledger.GetPerformance();

                if (perf != null)
                    summaries.Add(perf);
            }
        }

        Console.WriteLine($"\rNum Stocks Considered   : {symbols.Length:#,##0}              ");
        Console.WriteLine($"Trades in 2025          : {tradesIn2025:#,##0}");

        decimal totalProfit = summaries.Sum(s => s.TotalProfit);
        long totalTrades = summaries.Sum(s => s.TotalTrades);
        double totalWins = summaries.Sum(s => s.TotalTrades * s.WinRate);
        decimal totalGrossWin = summaries.Sum(s => s.AverageWin * (decimal)(s.TotalTrades * s.WinRate));
        decimal totalGrossLoss = summaries.Sum(s => s.AverageLoss * (decimal)(s.TotalTrades * (1 - s.WinRate)));

        Console.WriteLine();
        Console.WriteLine($"Total profit      : {totalProfit:C2}");
        Console.WriteLine($"Weighted win rate : {(totalTrades > 0 ? totalWins / totalTrades : 0):P2}");
        Console.WriteLine($"Avg profit/trade  : {(totalTrades > 0 ? totalProfit / totalTrades : 0):C2}");
        Console.WriteLine($"Overall avg win   : {(totalWins > 0 ? totalGrossWin / (decimal)totalWins : 0):C2}");
        Console.WriteLine($"Overall avg loss  : {(totalTrades - totalWins > 0 ? totalGrossLoss / (decimal)(totalTrades - totalWins) : 0):C2}");
        Console.WriteLine($"Overall PF        : {(totalGrossLoss > 0M ? totalGrossWin / totalGrossLoss : 999999M):F2}");
        Console.WriteLine($"Total trades      : {totalTrades:#,##0}");
    }

    if (config.Analyze)
    {
        var companyAnalysisService = new CompanyAnalysisService(dbDefPairs![0],
            serviceProvider.GetRequiredService<IMemoryCache>(),
            serviceProvider.GetRequiredService<ILogger<CompanyAnalysisService>>());

        if (string.IsNullOrWhiteSpace(config.Symbol))
        {
            string baseDir = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                ? Path.Combine("C:", "temp", "gimzo")
                : Path.Combine("/", "c", "temp", "gimzo");

            var allCompanyScores = (await companyAnalysisService.GetAllSiloScoresAsync()).ToImmutableArray();

            if (allCompanyScores.Length > 0)
            {
                await using var f = File.Create(Path.Combine(baseDir, "company_value_ranking.txt"));

                foreach (var r in allCompanyScores)
                {
                    var line = $"{r.Symbol,7}:\tb:{r.Percentile}\ta:{r.Score}";
                    Console.WriteLine(line);
                    await f.WriteLineAsync(line);
                }

                await f.FlushAsync();
                f.Close();
            }

            var industryAnalysisService = new IndustryAnalysisService(dbDefPairs![0],
                serviceProvider.GetRequiredService<IMemoryCache>(),
                serviceProvider.GetRequiredService<ILogger<IndustryAnalysisService>>());

            var allIndustryScores = (await industryAnalysisService.GetAllIndustryScoresAsync()).ToImmutableArray();

            if (allIndustryScores.Length > 0)
            {
                await using var f = File.Create(Path.Combine(baseDir, "industry_value_ranking.txt"));

                foreach (var r in allIndustryScores)
                {
                    int key = Convert.ToInt32(r.SicCode);
                    if (key == 0)
                        continue;

                    var title = $"{r.SicCode} ({Constants.SicTitles[key]})"; // maybe this dictionary should be keyed on a string.
                    var line = $"{r.Rank.ToString().PadLeft(3, '0')}  {r.ValueBillions,25:#,##0.00}B\t\t{title}";
                    Console.WriteLine(line);
                    await f.WriteLineAsync(line);
                }

                await f.FlushAsync();
                f.Close();
            }
        }
        else
        {
            var score = await companyAnalysisService.GetSiloScoreForSymbolAsync(config.Symbol);
            Console.WriteLine($"{config.Symbol}\t{score}");
        }
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
            case "--cleanup":
                config.ImportCleanupOnly = true;
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
    Console.WriteLine("Importing Notes");
    Console.WriteLine($"\tWhen choosing '-i', {config.AppName} will perform the workflow according to the current day of week,");
    Console.WriteLine("\tbut you can override this behavior with the --[weekday|sat|sun] options.");
    Console.WriteLine();
    Console.WriteLine("\tOn a first run, use the `--weekday` argument to ensure best behavior.");
    Console.WriteLine($"\tYou can run them all, as in `{config.AppName} --import --weekday --sat --sun`");
    Console.WriteLine("\tbut this is not recommended on a first run because you'll process more data than necessary.");
    Console.WriteLine();
    Console.WriteLine("Backtesting Notes");
    foreach (var kvp in BacktestingService.Scenarios)
        Console.WriteLine($"\t{kvp.Key,-16}\t{kvp.Value}");
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
    public bool ImportCleanupOnly { get; set; }
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
        if (!ShowHelp && !Csv && !Import && !Backtest && !Analyze && !ImportCleanupOnly)
            message = "Either csv, import, backtest, analyze, or cleanup must be specified.";
        else if ((Weekday || Saturday || Sunday) && !Import)
            message = "When specifying a day of the week, the import flag (--import) is required.";
        else if (Csv && (string.IsNullOrWhiteSpace(Symbol) || string.IsNullOrWhiteSpace(OutputFileName)))
            message = "When specifying CSV, both symbol and an output file name are required.";

        if (!string.IsNullOrWhiteSpace(OutputFileName))
        {
            var fInfo = new FileInfo(OutputFileName);
            if (!fInfo.Extension.ToLowerInvariant().Equals(".csv"))
                message = "The output file must be of type csv.";
        }

        if (Backtest && string.IsNullOrWhiteSpace(Scenario))
            message = "Scenario must be specified when backtest is chosen.";
        if (ImportCleanupOnly && !Import)
            message = "Cleanup is not valid without import.";

        return string.IsNullOrWhiteSpace(message);
    }
}