using Gimzo.Analysis.Technical;
using Gimzo.Analysis.Technical.Charts;
using Gimzo.AppServices.Backtests;
using Gimzo.Common;
using Gimzo.Infrastructure.Database;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Text;
using System.Text.Json;

namespace Gimzo.Admin.Cli;

internal partial class CliInteractive(Config config, BacktestingService backtestService, ILogger<CliInteractive> logger)
{
    private readonly Config _config = config;
    private readonly BacktestingService _backtestService = backtestService;
    private readonly ILogger<CliInteractive> _logger = logger;

    private delegate void Callback();

    public void Start()
    {
        WriteInitialMessage();
        WriteMainMenu();
    }

    private void WriteInitialMessage()
    {
        Console.WriteLine($"{_config.AppName} {_config.AppVersion}".Trim());
        Console.WriteLine("Interactive Mode");
        Console.WriteLine();
        Console.WriteLine("Use `Q` to quit.");
        WriteDashes(50, true, true);
    }

    private void WriteMainMenu()
    {
        Console.WriteLine();
        Console.WriteLine("What would you like to do? (Q to quit)");
        Console.WriteLine();
        Console.WriteLine("\t1: Show all backtesting data.");
        Console.WriteLine("\t2: Delete all backtesting data.");
        Console.WriteLine("\t3: Delete a specific backtest.");
        Console.WriteLine("\t4: Show backtesting config files.");
        Console.WriteLine("\t5: Execute backtest.");
        Console.WriteLine();
        Console.Write("> ");

        var choice = Console.ReadLine()?.Trim();

        if (CheckForQuit(choice))
            Quit();
        else
            switch (choice)
            {
                case "1":
                    ShowAllBacktests(WriteMainMenu);
                    break;
                case "2":
                    DeleteAllBacktests(WriteMainMenu);
                    break;
                case "3":
                    DeleteBacktest(WriteMainMenu);
                    break;
                case "4":
                    ShowAllBacktestConfigFiles(WriteMainMenu);
                    break;
                case "5":
                    ExecuteBacktest(WriteMainMenu);
                    break;
                default:
                    Console.WriteLine($"{choice} is not a valid selection.");
                    WriteMainMenu();
                    break;
            }
    }

    private static void ShowAllBacktestConfigFiles(Callback callback)
    {
        var dirInfo = new DirectoryInfo("configs");
        var files = dirInfo.GetFiles("*.json");
        foreach (var file in files)
            Console.WriteLine(file.Name);
        callback();
    }

    private static IEnumerable<string> GetBacktestFiles()
    {
        var dirInfo = new DirectoryInfo("configs");
        var files = dirInfo.GetFiles("*.json");
        foreach (var file in files)
            yield return file.Name;
    }

    private void ExecuteBacktest(Callback callback)
    {
        Dictionary<string, string> files = new();
        int i = 0;
        foreach (var file in GetBacktestFiles())
            files.Add((++i).ToString(), file);

        if (files.Count == 0)
            Console.WriteLine("No configs found");
        else
        {
            foreach (var key in files.Keys)
                Console.WriteLine($"\t{key,-3}  {files[key]}");

            Console.WriteLine();
            Console.Write("> ");

            string? choice = null;
            while (string.IsNullOrWhiteSpace(choice))
            {
                choice = Console.ReadLine()?.Trim();
                if (!string.IsNullOrWhiteSpace(choice))
                {
                    if (!files.TryGetValue(choice, out var filename))
                    {
                        Console.WriteLine("File not found.");
                        choice = null;
                        continue;
                    }

                    var path = Path.Combine(Path.GetFullPath(Path.Combine("configs", filename)));

                    var process = AppServices.Process.Create("CLI", null, null, path);

                    var json = File.ReadAllText(path);
                    var options = JsonOptionsRepository.Gimzo;
                    options.Converters.Add(new EnumDescriptionConverter<PricePoint>());
                    options.Converters.Add(new EnumDescriptionConverter<MovingAverageType>());
                    options.Converters.Add(new EnumDescriptionConverter<ChartInterval>());
                    var backtestConfig = JsonSerializer.Deserialize<BacktestConfiguration>(json, options);

                    LogHelper.LogInfo(_logger, "Fetching symbols to test.");

                    var symbols = (_backtestService.GetSymbolsToTest(
                        minAbsoluteScore: backtestConfig.SymbolCriteria.MinAbsoluteScore,
                        maxAbsoluteScore: backtestConfig.SymbolCriteria.MaxAbsoluteScore,
                        minCompanyPercentileRank: backtestConfig.SymbolCriteria.MinCompanyPercentileRank,
                        maxCompanyPercentileRank: backtestConfig.SymbolCriteria.MaxCompanyPercentileRank,
                        minIndustryRank: backtestConfig.SymbolCriteria.MinIndustryRank,
                        maxIndustryRank: backtestConfig.SymbolCriteria.MaxIndustryRank)).GetAwaiter().GetResult().ToImmutableArray();
                    LogHelper.LogInfo(_logger, "{count} symbols found", symbols.Length.ToString("#,##0"));

                    LogHelper.LogInfo(_logger, "Running backtest.");
                    CancellationTokenSource cts = new();
                    backtestService.ExecuteAsync(backtestConfig, symbols, process.ProcessId, cts.Token).GetAwaiter().GetResult();
                    callback();
                }

            }

        }
    }

    private void ShowAllBacktests(Callback callback)
    {
        var results = _backtestService.GetBacktestResultsAsync().GetAwaiter().GetResult().ToImmutableArray();
        if (results.Length == 0)
            Console.WriteLine("No backtests found.");
        else
        {
            Console.Write("Process Id".PadRight(33, ' '));
            Console.Write("| Trades  ");
            Console.Write("| P/L %   ");
            Console.Write("| Rate ");
            Console.Write("| Win   ");
            Console.WriteLine("| Loss");
            WriteDashes(100);

            foreach (var result in results)
                Console.WriteLine(result.ToString());
        }
        callback();
    }

    private void DeleteAllBacktests(Callback callback)
    {
        if (ConfirmActionWithUser("This will delete all the backtesting data. Are you sure?"))
        {
            Console.WriteLine();
            Console.WriteLine("Deleting all backtesting data.");

            _backtestService.DeleteAllBacktestsAsync().GetAwaiter().GetResult();
        }
        else
            CancellationMessage();

        callback();
    }

    private void DeleteBacktest(Callback callback)
    {
        Console.WriteLine();
        Console.Write("Provide process id: ");
        var guidString = Console.ReadLine()?.Trim();
        if (Guid.TryParse(guidString, out var guid))
        {
            if (ConfirmActionWithUser($"This will delete the backtesting data for {guid}. Are you sure?"))
                _backtestService.DeleteBacktestAsync(guid).GetAwaiter().GetResult();
            else
                CancellationMessage();
        }
        else
            Console.WriteLine($"'{guidString}' is not a valid GUID.");

        callback();
    }


}
