namespace Gimzo.Infrastructure;

public sealed class DbMetaInfo
{
    private const string StockSymbolsTableName = "public.stock_symbols";

    public IDictionary<string, int> TableCounts { get; init; } = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
    public string[] IgnoredSymbols { get; init; } = [];
    public bool HasStockSymbols => TableCounts.ContainsKey(StockSymbolsTableName) &&
        TableCounts[StockSymbolsTableName] > 0;
}
