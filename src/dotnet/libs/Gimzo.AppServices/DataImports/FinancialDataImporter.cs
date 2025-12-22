using Gimzo.Infrastructure.DataProviders.FinancialDataNet;
using Microsoft.Extensions.Logging;

namespace Gimzo.AppServices.DataImports;

public sealed class FinancialDataImporter
{
    private readonly FinancialDataApiClient _apiClient;
    private readonly ILogger<FinancialDataImporter> _logger;

    public FinancialDataImporter(FinancialDataApiClient apiClient,
        ILogger<FinancialDataImporter> logger)
    {
        _apiClient = apiClient;
        _logger = logger;
    }

    public async Task Import(Guid processId)
    {
        if (_logger.IsEnabled(LogLevel.Information))
            _logger.LogInformation("{Name} started import at {DateTime} with process id {ProcessId}", nameof(FinancialDataImporter),
                DateTimeOffset.Now, processId);

        var stockSymbols = await _apiClient.GetStockSymbolsAsync();

        foreach (var symbol in stockSymbols)
        {
            var ticker = symbol.Symbol;
            var secInfo = await _apiClient.GetSecurityInformationAsync(ticker);
            var coInfo = await _apiClient.GetCompanyInformationAsync(ticker);
            var priceActions = await _apiClient.GetStockPricesAsync(ticker);
            var metrics = await _apiClient.GetKeyMetricsAsync(ticker);
            var marketCaps = await _apiClient.GetMarketCapAsync(ticker);
            var empCounts = await _apiClient.GetEmployeeCountAsync(ticker);
            var comps = await _apiClient.GetExecutiveCompensationAsync(ticker);
            var incStmts = await _apiClient.GetIncomeStatementsAsync(ticker);
            var bsStmts = await _apiClient.GetBalanceSheetStatementsAsync(ticker);
            var cfStmts = await _apiClient.GetCashFlowStatementsAsync(ticker);
            var divs = await _apiClient.GetDividendsAsync(ticker);
            var splits = await _apiClient.GetStockSplitsAsync(ticker);
            var shorts = await _apiClient.GetShortInterestAsync(ticker);
            var earningsReleases = await _apiClient.GetEarningsReleasesAsync(ticker);
            var efficiencyRatios = await _apiClient.GetEfficiencyRatiosAsync(ticker);
            var liquidity = await _apiClient.GetLiquidityRatiosAsync(ticker);
            var profitability = await _apiClient.GetProfitabilityRatiosAsync(ticker);
            var solvency = await _apiClient.GetSolvencyRatiosAsync(ticker);
            var valuations = await _apiClient.GetValuationRatiosAsync(ticker);

        }
    }
}
