using Gimzo.Analysis.Fundamental;
using Microsoft.Extensions.Logging;

namespace Gimzo.Infrastructure.Database;

internal class DatabaseService
{
    private readonly DbDefPair _dbDefPair;
    private readonly ILogger<DatabaseService> _logger;

    public DatabaseService(DbDefPair dbDefPair, ILogger<DatabaseService> logger)
    {
        _dbDefPair = dbDefPair;
        _logger = logger;
    }

    public async Task SaveSecurity(Security security, Guid? userId = null)
    {
        ArgumentNullException.ThrowIfNull(security);
        userId ??= Common.Constants.SystemId;

        using var cmdCtx = _dbDefPair.GetCommandConnection();

        //var secDao = new DataAccessObjects.Security(security, userId.GetValueOrDefault());

    }

    
}


/*
public sealed class Security
{
    public required string Symbol { get; init; }
    public required string Type { get; init; }
    public string? Issuer { get; init; }
    public string? Cusip { get; init; }
    public string? Isin { get; init; }
    public string? Figi { get; init; }
    public CompanyInformation? Company { get; init; }
    public IncomeStatement[]? IncomeStatements { get; init; }
    public BalanceSheet[]? BalanceSheets { get; init; }
    public CashFlowStatement[]? CashFlowStatements { get; init; }
    public StockSplit[]? Splits { get; init; }
    public Dividend[]? Dividends { get; init; }
    public EfficiencyRatios[]? EfficiencyRatios { get; init; }
    public KeyMetrics[]? KeyMetrics { get; init; }
    public MarketCap[]? MarketCaps { get; init; }
    public LiquidityRatios[]? LiquidityRatios { get; init; }
    public ProfitabilityRatios[]? ProfitabilityRatios { get; init; }
    public ValuationRatios[]? ValuationRatios { get; init; }
    public EarningsRelease[]? EarningsReleases { get; init; }
    public ShortInterest[]? ShortInterests { get; init; }
    public ExecutiveCompensation[]? ExecutiveCompensations { get; init; }
}

 */