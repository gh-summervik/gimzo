using Gimzo.Analysis.Technical.Charts;

namespace Gimzo.Analysis.Fundamental;

/// <summary>
/// Represents a security.
/// </summary>
public record struct Security(string symbol,
    string securityType,
    string? type = null,
    string? issuer = null,
    string? cusip = null,
    string? isin = null,
    string? figi = null,
    string? registrant = null,
    string? description = null,
    string? title = null,
    string? name = null,
    string? baseAsset = null,
    string? quoteAsset = null,
    CompanyInformation? company = null,
    IncomeStatement[]? incomeStatements = null,
    BalanceSheet[]? balanceSheets = null,
    CashFlowStatement[]? cashFlowStatements = null,
    StockSplit[]? splits = null,
    Dividend[]? dividends = null,
    EfficiencyRatios[]? efficiencyRatios = null,
    KeyMetrics[]? keyMetrics = null,
    MarketCap[]? marketCaps = null,
    LiquidityRatios[]? liquidityRatios = null,
    ProfitabilityRatios[]? profitabilityRatios = null,
    ValuationRatios[]? valuationRatios = null,
    EarningsRelease[]? earningsReleases = null,
    ShortInterest[]? shortInterests = null,
    ExecutiveCompensation[]? executiveCompensations = null,
    EmployeeCount[]? employeeCounts = null,
    SolvencyRatios[]? solvencyRatios = null,
    Ohlc[]? priceActions = null)
{
    public string Symbol { get; init; } = symbol;
    public string SecurityType { get; init; } = securityType;
    public string? Type { get; init; } = type;
    public string? Issuer { get; init; } = issuer;
    public string? Cusip { get; init; } = cusip;
    public string? Isin { get; init; } = isin;
    public string? Figi { get; init; } = figi;
    public string? Registrant { get; init; } = registrant;
    public string? Description { get; init; } = description;
    public string? Title { get; init; } = title;
    public string? Name { get; init; } = name;
    public string? BaseAsset { get; init; } = baseAsset;
    public string? QuoteAsset { get; init; } = quoteAsset;
    public CompanyInformation? Company { get; init; } = company;
    public IncomeStatement[]? IncomeStatements { get; init; } = incomeStatements;
    public BalanceSheet[]? BalanceSheets { get; init; } = balanceSheets;
    public CashFlowStatement[]? CashFlowStatements { get; init; } = cashFlowStatements;
    public StockSplit[]? Splits { get; init; } = splits;
    public Dividend[]? Dividends { get; init; } = dividends;
    public EfficiencyRatios[]? EfficiencyRatios { get; init; } = efficiencyRatios;
    public KeyMetrics[]? KeyMetrics { get; init; } = keyMetrics;
    public MarketCap[]? MarketCaps { get; init; } = marketCaps;
    public LiquidityRatios[]? LiquidityRatios { get; init; } = liquidityRatios;
    public ProfitabilityRatios[]? ProfitabilityRatios { get; init; } = profitabilityRatios;
    public ValuationRatios[]? ValuationRatios { get; init; } = valuationRatios;
    public EarningsRelease[]? EarningsReleases { get; init; } = earningsReleases;
    public ShortInterest[]? ShortInterests { get; init; } = shortInterests;
    public ExecutiveCompensation[]? ExecutiveCompensations { get; init; } = executiveCompensations;
    public EmployeeCount[]? EmployeeCounts { get; init; } = employeeCounts;
    public SolvencyRatios[]? SolvencyRatios { get; init; } = solvencyRatios;
    public Ohlc[]? PriceActions { get; init; } = priceActions;
}
