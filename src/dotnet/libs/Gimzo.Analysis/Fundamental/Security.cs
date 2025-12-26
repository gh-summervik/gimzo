using Gimzo.Analysis.Technical.Charts;

namespace Gimzo.Analysis.Fundamental;

/// <summary>
/// Represents a security.
/// </summary>
public sealed class Security
{
    public Security(string symbol, 
        string securityType, 
        string? type = null,
        string? issuer = null,
        string? cusip = null,
        string? isin = null,
        string? figi = null,
        string? registrant = null,
        bool isInternational = false,
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
        Symbol = symbol;
        SecurityType = securityType;
        Type = type;
        Issuer = issuer;
        Cusip = cusip;
        Isin = isin;
        Figi = figi;
        Registrant = registrant;
        IsInternational = isInternational;
        Description = description;
        Title = title;
        Name = name;
        BaseAsset = baseAsset;
        QuoteAsset = quoteAsset;
        Company = company;
        IncomeStatements = incomeStatements;
        BalanceSheets = balanceSheets;
        CashFlowStatements = cashFlowStatements;
        Splits = splits;
        Dividends = dividends;
        EfficiencyRatios = efficiencyRatios;
        KeyMetrics = keyMetrics;
        MarketCaps = marketCaps;
        LiquidityRatios = liquidityRatios;
        ProfitabilityRatios = profitabilityRatios;
        ValuationRatios = valuationRatios;
        EarningsReleases = earningsReleases;
        ShortInterests = shortInterests;
        ExecutiveCompensations = executiveCompensations;
        EmployeeCounts = employeeCounts;
        SolvencyRatios = solvencyRatios;
        PriceActions = priceActions;
    }

    public string Symbol { get; init; }
    public string SecurityType { get; init; }
    public string? Type { get; init; }
    public string? Issuer { get; init; }
    public string? Cusip { get; init; }
    public string? Isin { get; init; }
    public string? Figi { get; init; }
    public string? Registrant { get; init; }
    public bool IsInternational { get; init; }
    public string? Description { get; init; }
    public string? Title { get; init; }
    public string? Name { get; init; }
    public string? BaseAsset { get; init; }
    public string? QuoteAsset { get; init; }
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
    public EmployeeCount[]? EmployeeCounts { get; init; }
    public SolvencyRatios[]? SolvencyRatios { get; init; }
    public Ohlc[]? PriceActions { get; init; }
    
}
