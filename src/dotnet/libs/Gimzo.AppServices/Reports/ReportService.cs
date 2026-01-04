using Gimzo.AppServices.Backtesting;
using Gimzo.Common;
using Gimzo.Infrastructure.Database;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace Gimzo.AppServices.Reports;

public sealed class ReportService(DbDefPair dbDefPair, IMemoryCache memoryCache, 
    ILogger<ReportService> logger) : ServiceBase(dbDefPair, memoryCache)
{
    private readonly ILogger<ReportService> _logger = logger;

    public async Task CreateExtremeReportAsync(string symbol, FileInfo outputFile, bool overwrite = false)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(symbol, nameof(symbol));
        ArgumentNullException.ThrowIfNull(outputFile, nameof(outputFile));

        if (outputFile.Exists && !overwrite)
            throw new Exception($"{outputFile.Name} already exists.");

        const string Header = "Date,High,Low,High Sentiment,Low Sentiment";

        var chart = await GetChartAsync(symbol);

        if (chart != null && chart.PriceActions.Length > 0 && chart.Extremes.Length > 0)
        {
            List<string> lines = new(chart.PriceActions.Length + 1)
            {
                Header
            };
            foreach (var pe in chart.Extremes)
            {
                string[] row = [
                    chart.PriceActions[pe.Index].Date.ToString("yyyy-MM-dd"),
                    pe.IsHigh ? pe.Price.ToString("#0.00") : "",
                    pe.IsLow ? pe.Price.ToString("#0.00") : "",
                    pe.IsHigh ? pe.Sentiment.GetEnumDescription() : "",
                    pe.IsLow ? pe.Sentiment.GetEnumDescription() : ""
                ];
                lines.Add(string.Join(',', row));
            }
            await File.WriteAllLinesAsync(outputFile.FullName, lines);
        }
        else
            throw new Exception($"Unable to create price extreme report for {symbol}");
    }

    public async Task CreatePriceAndTrendReportAsync(string symbol, FileInfo outputFile, bool overwrite = false)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(symbol, nameof(symbol));
        ArgumentNullException.ThrowIfNull(outputFile, nameof(outputFile));

        if (outputFile.Exists && !overwrite)
            throw new Exception($"{outputFile.Name} already exists.");

        const string Header = "Date,Open,High,Low,Close,Volume,Trend";

        var chart = await GetChartAsync(symbol);

        if (chart != null && chart.PriceActions.Length > 0)
        {
            List<string> lines = new(chart.PriceActions.Length + 1)
            {
                Header
            };
            for (int i = 0; i < chart.PriceActions.Length; i++)
            {
                var pa = chart.PriceActions[i];
                string[] row = [
                    pa.Date.ToString("yyyy-MM-dd"),
                    pa.Open.ToString("#0.00"),
                    pa.High.ToString("#0.00"),
                    pa.Low.ToString("#0.00"),
                    pa.Close.ToString("#0.00"),
                    pa.Volume.ToString("#0.00"),
                    chart.TrendValues[i].ToString("#0.00")
                ];
                lines.Add(string.Join(',', row));
            }
            await File.WriteAllLinesAsync(outputFile.FullName, lines);
        }
        else
            throw new Exception($"Unable to create chart for {symbol}");
    }

    public async Task WriteLedgerAsync(Ledger ledger, FileInfo outputFile, bool overwrite = false)
    {
        ArgumentNullException.ThrowIfNull(ledger, nameof(ledger));
        ArgumentNullException.ThrowIfNull(outputFile, nameof(outputFile));

        if (outputFile.Exists && !overwrite)
            throw new ArgumentException($"File '{outputFile.Name}' already exists.");

        var notesFileName = Path.Combine(outputFile.DirectoryName!, Path.GetFileNameWithoutExtension(outputFile.FullName) + ".txt");

        const string Header = "Symbol,Date,Action,Qty,Cash Value,Date,Action,Qty,Cash Value,Duration,Profit/Loss,Description";
        var pairs = ledger.GetPairs().ToArray();
        List<string> lines = new(pairs.Length + 1)
            {
                Header
            };
        foreach (var pair in pairs)
        {
            var openAction = pair.Open.Quantity > 0 ? "BTO" : "STO";
            var closeAction = pair.Close == null ? "" : openAction == "BTO" ? "STC" : "BTC";
            string duration = pair.Close == null ? "" :
                (pair.Close.Date.DayNumber - pair.Open.Date.DayNumber).ToString();
            string profit = pair.Profit.ToString("#0.00");
            string[] row = [
                pair.Open.Symbol,
                pair.Open.Date.ToString("yyyy-MM-dd"),
                openAction,
                pair.Open.Quantity.ToString(),
                pair.Open.CashValue.ToString("#0.00"),
                pair.Close?.Date.ToString("yyyy-MM-dd") ?? "",
                closeAction,
                pair.Close?.Quantity.ToString() ?? "",
                pair.Close?.CashValue.ToString("#0.00") ?? "",
                duration,
                profit,
                pair.Close?.Description ?? ""
            ];
            lines.Add(string.Join(',', row));
        }
        await File.WriteAllLinesAsync(outputFile.FullName, lines);

        await File.WriteAllTextAsync(notesFileName, ledger.GetPerformance().ToCliOutput());
    }
}
