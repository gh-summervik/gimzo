# Backtesting Results

Each test has up to 10 years of data.
No chart has fewer than 200 OHLC records.

## Buying after bullish lows.

| Stop Adjustment Method          | Num Stocks | Trades in 2025 | Total Profit   | Weighted Win Rate | Avg Profit/Trade | Avg Win | Avg Loss | Profit Factor | Total Trades |
|---------------------------------|------------|----------------|----------------|-------------------|------------------|---------|----------|---------------|--------------|
| Current High                    | 1,314      | 4,753          | $73,219.14     | 61.34%            | $1.04            | $3.70   | $3.19    | 1.84          | 70,611       |
| Previous Day's High             | 1,314      | 4,751          | $62,108.06     | 61.03%            | $0.88            | $3.47   | $3.17    | 1.71          | 70,609       |
| Current Midpoint                | 1,314      | 4,751          | $70,170.19     | 61.32%            | $0.99            | $3.63   | $3.19    | 1.81          | 70,609       |
| Current Low                     | 1,314      | 4,747          | $62,169.55     | 60.25%            | $0.88            | $3.53   | $3.14    | 1.71          | 70,605       |
| Previous Midpoint               | 1,314      | 4,749          | $57,463.21     | 59.73%            | $0.81            | $3.44   | $3.08    | 1.66          | 70,607       |

---

The code for buying is:

```csharp
private async Task<Ledger?> ExecuteBullishPriceExtremeFollowAsync(string symbol)
{
    var chart = await GetChartAsync(symbol);
    if (chart == null)
        return null;

    var ledger = new Ledger();

    for (int i = 1; i < chart.Extremes.Length && i < chart.PriceActions.Length - 1; i++)
    {
        var current = chart.Extremes[i];

        var prevHigh = chart.FindPreviousHigh(i);
        if (prevHigh == null)
            continue;

        if (!(current.IsLow && current.IsBullish && chart.TrendValues[current.Index] > -0.2 &&
            chart.PriceActions[current.Index + 1].Open > chart.PriceActions[current.Index].Close &&
            chart.PriceActions[current.Index + 1].MidPoint < prevHigh.Value.Price))
            continue;

        var v = Math.Max(0, current.Index - 21);
        var avgVol = chart.PriceActions[v..current.Index].Select(k => k.Volume).Average();

        var entry = new CashLedgerEntry(chart.PriceActions[current.Index + 1].Date, symbol, DefaultQty,
            chart.PriceActions[current.Index + 1].MidPoint, "");
        var exitPoint = FindExitOnLong(entry, chart);
        if (exitPoint != null)
        {
            ledger.Add(entry);
            ledger.Add(exitPoint);
        }
    }
    return ledger;
}
```

The code for exiting a trade is:

```csharp
private static CashLedgerEntry? FindExitOnLong(CashLedgerEntry? entry, Chart chart)
{
    if (entry == null || entry.IsShort || chart == null || chart.PriceActions.Length == 0)
        return null;

    var idx = chart.GetIndexOfDate(entry.Date);

    List<decimal> targets = new(10);

    var prevHigh = chart.FindPreviousHigh(idx);

    if (prevHigh.HasValue)
    {
        var prevLow = chart.FindPreviousLow(prevHigh.Value.Index);
        if (prevLow.HasValue && prevLow.Value.Price > 0 && prevLow.Value.Price < prevHigh.Value.Price)
            targets.Add(prevHigh.Value.Price - prevLow.Value.Price + entry.TradePrice);
    }

    targets.Add(entry.TradePrice * 1.20M);
    targets.Add(entry.TradePrice * 1.25M);
    targets.Add(entry.TradePrice * 1.5M);
    targets.Add(entry.TradePrice * 2M);
    targets.Add(entry.TradePrice * 3M);
    targets.Add(entry.TradePrice * 4M);

    targets.Sort();

    int tgtIdx = 0;
    bool sellOnWeakness = false;
    decimal tgtPrice;
    decimal stopPrice = entry.TradePrice * 0.92M;

    for (int i = idx + 1; i < chart.PriceActions.Length - 1; i++)
    {
        if (chart.PriceActions[i].Date >= entry.Date.AddDays(40))
            return new CashLedgerEntry(chart.PriceActions[i].Date,
               entry.Symbol,
               entry.Quantity * -1,
               chart.PriceActions[i].Close,
               "Time ran out",
               relatesTo: entry.Id);

        if (chart.PriceActions[i].Close <= stopPrice)
            return new CashLedgerEntry(chart.PriceActions[i].Date,
                entry.Symbol,
                entry.Quantity * -1,
                chart.PriceActions[i].Contains(stopPrice) 
                    ? stopPrice
                    : chart.PriceActions[i].Open < stopPrice 
                        ? chart.PriceActions[i].Open 
                        : chart.PriceActions[i].MidPoint,
                "Stop Loss",
                relatesTo: entry.Id);

        if (sellOnWeakness && chart.PriceActions[i].Close < chart.PriceActions[i - 1].Low)
        {
            return new CashLedgerEntry(chart.PriceActions[i].Date,
               entry.Symbol,
               entry.Quantity * -1,
               chart.PriceActions[i].Close,
               $"Sold on weakness after target ({tgtIdx + 1}) reached",
               relatesTo: entry.Id);
        }
        
        if (targets.Count < 2)
        {
            tgtPrice = targets.First();
            if (chart.PriceActions[i].High >= tgtPrice)
                return new CashLedgerEntry(chart.PriceActions[i].Date,
                   entry.Symbol,
                   entry.Quantity * -1,
                   tgtPrice,
                   $"Target ({tgtIdx + 1}) reached",
                   relatesTo: entry.Id);
        }
        else
        {
            tgtPrice = targets.ElementAt(tgtIdx);

            if (tgtIdx == targets.Count - 1)
            {
                return new CashLedgerEntry(chart.PriceActions[i].Date,
                   entry.Symbol,
                   entry.Quantity * -1,
                   tgtPrice,
                   $"Target ({tgtIdx + 1}) reached",
                   relatesTo: entry.Id);
            }

            if (chart.PriceActions[i].High >= tgtPrice)
            {
                tgtIdx++;
                //stopPrice = Math.Max(stopPrice, chart.PriceActions[i].High);
                //stopPrice = Math.Max(stopPrice, chart.PriceActions[i - 1].High);
                //stopPrice = Math.Max(stopPrice, chart.PriceActions[i].MidPoint);
                //stopPrice = Math.Max(stopPrice, chart.PriceActions[i].Low);
                stopPrice = Math.Max(stopPrice, chart.PriceActions[i - 1].MidPoint);
                sellOnWeakness = true;
            }
        }
    }

    return null;
}
```

The code for selecting stocks is:

```csharp
    public async Task<IReadOnlyCollection<string>> GetSymbolsToTest(
        int minAbsoluteScore = 0,
        int minCompanyPercentileRank = 0,
        int minIndustryRank = 0)
    {
        const string Sql = @"
WITH latest_company AS (
     SELECT * FROM (
        SELECT *,
        ROW_NUMBER() OVER (PARTITION BY symbol ORDER BY date_eval DESC) AS rn
        FROM public.company_valuations
    ) sub
    WHERE rn = 1
),
latest_sic AS (
    SELECT *
    FROM (
        SELECT *,
        ROW_NUMBER() OVER (PARTITION BY sic_code ORDER BY date_eval DESC) AS rn
        FROM public.sic_money_flow
        ) sub
    WHERE rn = 1
)
SELECT lc.symbol FROM latest_company lc
    JOIN public.us_companies c ON lc.symbol = c.symbol
    JOIN latest_sic ls ON c.sic_code = ls.sic_code
WHERE lc.absolute_value >= @AV AND lc.percentile_rank >= @PR AND ls.rank >= @R;";

        await using var ds = NpgsqlDataSource.Create(_dbDefPair.Query.ConnectionString);
        await using var connection = await ds.OpenConnectionAsync();
        await using var command = new NpgsqlCommand(Sql, connection);

        command.Parameters.Add(new NpgsqlParameter("@AV", NpgsqlDbType.Integer) { Value = minAbsoluteScore });
        command.Parameters.Add(new NpgsqlParameter("@PR", NpgsqlDbType.Integer) { Value = minCompanyPercentileRank });
        command.Parameters.Add(new NpgsqlParameter("@R", NpgsqlDbType.Integer) { Value = minIndustryRank });

        await using var reader = await command.ExecuteReaderAsync();

        var symbols = new List<string>(5_000);

        while (await reader.ReadAsync())
            symbols.Add(reader.GetString(0));

        return [.. symbols];
    }
```
---
