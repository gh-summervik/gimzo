using Gimzo.AppServices.Backtests;
using Gimzo.Common;
using Gimzo.Infrastructure.Database;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System.Collections.Frozen;
using System.Collections.Immutable;
using System.Data;

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

        if (chart is not null && chart.PriceActions.Length > 0 && chart.Extremes.Length > 0)
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

        if (chart is not null && chart.PriceActions.Length > 0)
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
                    chart.RelativeStrengthIndex == null ? "" : chart.RelativeStrengthIndex!.Values[i].ToString("#0.00")
                ];
                lines.Add(string.Join(',', row));
            }
            await File.WriteAllLinesAsync(outputFile.FullName, lines);
        }
        else
            throw new Exception($"Unable to create chart for {symbol}");
    }

    public static async Task WriteLedgerAsync(Ledger ledger, FileInfo outputFile, bool overwrite = false)
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
            var closeAction = pair.Close is null ? "" : openAction == "BTO" ? "STC" : "BTC";
            string duration = pair.Close is null ? "" :
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

    public static async Task CreateTradeDetailsReportAsync(IEnumerable<TradeDetails> details, string outputFileName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(outputFileName);

        int idx = 0;

        var dir = Path.GetDirectoryName(outputFileName);
        if (string.IsNullOrWhiteSpace(dir))
            throw new ArgumentException("Invalid output file name.");
        var fn = Path.GetFileNameWithoutExtension(outputFileName);
        var ext = Path.GetExtension(outputFileName);

        const string Header = "Symbol,Entry Date,Entry Price,Exit Date,Exit Price,Profit,Duration,MFEP,MFE%,MAEP,MAE%,PH Date,PH Price,PL Date,PL Price,% from PH,% from PL,ATR,Avg Vol,# Up,# Down,# Green,# Red,Pr Slope,RSI,E Reason,X Reason,D2MA1,D2MA2,D2MA3";

        foreach (var chunk in details.OrderBy(k => k.Entry!.Ledger!.Date).Chunk(Constants.DefaultChunkSize))
        {
            var fi = new FileInfo(Path.Combine(dir, $"{fn}_{(++idx).ToString().PadLeft(4, '0')}{ext}"));
            if (fi.Exists)
                fi.Delete();

            var f = File.Create(fi.FullName);
            await f.WriteLineAsync(Header);

            List<string> row = new(100);
            foreach (var d in chunk)
            {
                row.Add(d.Entry?.Ledger?.Symbol ?? "");
                row.Add(d.Entry?.Ledger?.Date.ToString("yyyy-MM-dd") ?? "");
                row.Add(d.Entry?.Ledger?.TradePrice.ToString("##0.00") ?? "");
                row.Add(d.Exit?.Ledger?.Date.ToString("yyyy-MM-dd") ?? "");
                row.Add(d.Exit?.Ledger?.TradePrice.ToString("##0.00") ?? "");
                row.Add(d.Profit?.ToString("##0.00") ?? "");
                row.Add(d.DurationDays?.ToString() ?? "");
                row.Add(d.MaxFavorableExcursionPrice?.ToString("##0.00") ?? "");
                row.Add(d.MaxFavorableExcursionPercent?.ToString("##0.00") ?? "");
                row.Add(d.MaxAdverseExcursionPrice?.ToString("##0.00") ?? "");
                row.Add(d.MaxAdverseExcursionPercent?.ToString("##0.00") ?? "");
                row.Add(d.Entry?.PreviousHigh?.Date.ToString("yyyy-MM-dd") ?? "");
                row.Add(d.Entry?.PreviousHigh?.Price.ToString("##0.00") ?? "");
                row.Add(d.Entry?.PreviousLow?.Date.ToString("yyyy-MM-dd") ?? "");
                row.Add(d.Entry?.PreviousLow?.Price.ToString("##0.00") ?? "");
                row.Add(d.Entry?.PercentFromPreviousHigh.GetValueOrDefault().ToString("##0.00") ?? "");
                row.Add(d.Entry?.PercentFromPreviousLow.GetValueOrDefault().ToString("##0.00") ?? "");
                row.Add(d.Entry?.AverageTrueRange?.Value.ToString("##0.00") ?? "");
                row.Add(d.Entry?.AverageVolume?.Value.ToString("##0.0") ?? "");
                row.Add(d.Entry?.NumberUpDays?.Value.ToString() ?? "");
                row.Add(d.Entry?.NumberDownDays?.Value.ToString() ?? "");
                row.Add(d.Entry?.NumberGreenDays?.Value.ToString() ?? "");
                row.Add(d.Entry?.NumberRedDays?.Value.ToString() ?? "");
                row.Add(d.Entry?.PriorSlope?.Value.ToString("##0.00") ?? "");
                row.Add(d.Entry?.RelativeStrengthIndex?.Value.ToString("##0.00") ?? "");
                row.Add(d.Entry?.Reason ?? "");
                row.Add(d.Exit?.Reason ?? "");

                int[] maPer = [.. d.Entry?.DistanceToMovingAverage?.Keys ?? []];
                for (int mi = 0; mi < maPer.Length; mi++)
                    row.Add(d.Entry?.DistanceToMovingAverage[maPer[mi]].ToString("##0.00") ?? "");
                for (int mi = maPer.Length; mi < 3; mi++)
                    row.Add("");

                await f.WriteLineAsync(string.Join(',', row));
                row.Clear();
            }
            await f.FlushAsync();
            f.Close();
        }


        //foreach (var pair in pairs)
        //{
        //    var openAction = pair.Open.Quantity > 0 ? "BTO" : "STO";
        //    var closeAction = pair.Close is null ? "" : openAction == "BTO" ? "STC" : "BTC";
        //    string duration = pair.Close is null ? "" :
        //        (pair.Close.Date.DayNumber - pair.Open.Date.DayNumber).ToString();
        //    string profit = pair.Profit.ToString("#0.00");
        //    string[] row = [
        //        pair.Open.Symbol,
        //        pair.Open.Date.ToString("yyyy-MM-dd"),
        //        openAction,
        //        pair.Open.Quantity.ToString(),
        //        pair.Open.CashValue.ToString("#0.00"),
        //        pair.Close?.Date.ToString("yyyy-MM-dd") ?? "",
        //        closeAction,
        //        pair.Close?.Quantity.ToString() ?? "",
        //        pair.Close?.CashValue.ToString("#0.00") ?? "",
        //        duration,
        //        profit,
        //        pair.Close?.Description ?? ""
        //    ];
        //    lines.Add(string.Join(',', row));
        //}
        //await File.WriteAllLinesAsync(outputFile.FullName, lines);


    }
}
