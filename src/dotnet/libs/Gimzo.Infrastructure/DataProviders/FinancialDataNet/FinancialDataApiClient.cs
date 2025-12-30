using Gimzo.Common;
using Microsoft.Extensions.Logging;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

[assembly: InternalsVisibleTo("Gimzo.Infrastructure.Tests"),
    InternalsVisibleTo("Gimzo.AppServices")]
namespace Gimzo.Infrastructure.DataProviders.FinancialDataNet;

public sealed class FinancialDataApiClient(string apiKey, ILogger<FinancialDataApiClient> logger) : IDisposable
{
    private readonly string _apiKey = apiKey ?? throw new ArgumentNullException(nameof(apiKey));
    private readonly ILogger<FinancialDataApiClient> _logger = logger;
    private readonly HttpClient _httpClient = new();
    private readonly string _baseUrl = "https://financialdata.net/api/v1/";

    static class EndPoints
    {
        public const string BalanceSheetStatements = "balance-sheet-statements";
        public const string CashFlowStatements = "cash-flow-statements";
        public const string CommodityPrices = "commodity-prices";
        public const string CommoditySymbols = "commodity-symbols";
        public const string CompanyInformation = "company-information";
        public const string CryptoInformation = "crypto-information";
        public const string CryptoMinutePrices = "crypto-minute-prices";
        public const string CryptoPrices = "crypto-prices";
        public const string CryptoSymbols = "crypto-symbols";
        public const string Dividends = "dividends";
        public const string EarningsReleases = "earnings-releases";
        public const string EfficiencyRatios = "efficiency-ratios";
        public const string EmployeeCount = "employee-count";
        public const string EtfSymbols = "etf-symbols";
        public const string ExecutiveCompensation = "executive-compensation";
        public const string FuturesPrices = "futures-prices";
        public const string FuturesSymbols = "futures-symbols";
        public const string IncomeStatements = "income-statements";
        public const string IndexConstituents = "index-constituents";
        public const string IndexPrices = "index-prices";
        public const string IndexSymbols = "index-symbols";
        public const string InitialPublicOfferings = "initial-public-offerings";
        public const string KeyMetrics = "key-metrics";
        public const string LiquidityRatios = "liquidity-ratios";
        public const string MarketCap = "market-cap";
        public const string MinutePrices = "minute-prices";
        public const string OptionChain = "option-chain";
        public const string OptionGreeks = "option-greeks";
        public const string OptionPrices = "option-prices";
        public const string OtcPrices = "otc-prices";
        public const string OtcSymbols = "otc-symbols";
        public const string ProfitabilityRatios = "profitability-ratios";
        public const string SecuritiesInformation = "securities-information";
        public const string ShortInterest = "short-interest";
        public const string SolvencyRatios = "solvency-ratios";
        public const string StockPrices = "stock-prices";
        public const string StockSplits = "stock-splits";
        public const string StockSymbols = "stock-symbols";
        public const string ValuationRatios = "valuation-ratios";
    }

    private async Task<JsonElement[]?> MakeRequestAsync(string endpoint, Dictionary<string, string>? parameters = null, CancellationToken ct = default)
    {
        parameters ??= [];
        parameters["key"] = _apiKey;

        var queryBuilder = new StringBuilder();
        bool first = true;
        foreach (var kvp in parameters)
        {
            if (!first)
                queryBuilder.Append('&');
            queryBuilder.Append(Uri.EscapeDataString(kvp.Key));
            queryBuilder.Append('=');
            queryBuilder.Append(Uri.EscapeDataString(kvp.Value));
            first = false;
        }

        string url = $"{_baseUrl}{endpoint}?{queryBuilder}";

        int backoff = 1;
        int retries = 0;
        const int MaxRetries = 5;
        var random = new Random();

        while (retries < MaxRetries)
        {
            try
            {
                var response = await _httpClient.GetAsync(url, ct);
                if (response.IsSuccessStatusCode)
                {
                    string content = await response.Content.ReadAsStringAsync(ct);
                    if (string.IsNullOrWhiteSpace(content))
                    {
                        LogHelper.LogWarning(_logger, "Empty response from {Url}. Returning empty array.", url);
                        return [];
                    }
                    // fixes a defect in the financialdata.net API.
                    // suppressing this warning because using it forces this
                    // class into a partial class. Just don't like it.
#pragma warning disable SYSLIB1045 // Convert to 'GeneratedRegexAttribute'.
                    var json = Regex.Replace(content, @"\:\s*NaN\b", ": null");
#pragma warning restore SYSLIB1045 // Convert to 'GeneratedRegexAttribute'.
                    return JsonSerializer.Deserialize<JsonElement[]>(json);
                }
                else
                {
                    int status = (int)response.StatusCode;
                    if (status is 429 or 500 or 502 or 503 or 504)
                    {
                        LogHelper.LogWarning(_logger, "Transient error {Status} at {Endpoint}. Headers: {Headers}",
                                status, endpoint, string.Join("; ", response.Headers.Select(h => $"{h.Key}: {string.Join(",", h.Value)}")));
                        int delay = backoff * 500 + random.Next(0, 500);
                        await Task.Delay(delay, ct);
                        backoff++;
                        retries++;
                    }
                    else
                    {
                        string errorContent = await response.Content.ReadAsStringAsync(ct);
                        LogHelper.LogError(_logger, "Non-transient error {Status} at {Endpoint}: {ErrorContent}", status, endpoint, errorContent);
                        throw new HttpRequestException($"Error {status} at {endpoint}: {errorContent}");
                    }
                }
            }
            catch (Exception ex) when (ex is not TaskCanceledException and not OperationCanceledException)
            {
                LogHelper.LogError(_logger, ex, "Request failed for {Endpoint}", endpoint);
                throw new Exception($"Request failed for {endpoint}: {ex.Message}", ex);
            }
        }
        throw new Exception($"Max retries ({MaxRetries}) exceeded for {endpoint}");
    }

    private async Task<JsonElement[]> GetDataAsync(string endpoint, Dictionary<string, string>? parameters = null, int limit = 500)
    {
        parameters ??= [];
        var allData = new List<JsonElement>();
        int offset = 0;
        bool done = false;

        while (!done)
        {
            parameters["offset"] = offset.ToString();
            var data = (await MakeRequestAsync(endpoint, parameters)) ?? [];

            allData.AddRange(data);
            if (data.Length < limit)
                done = true;
            else
                offset += limit;
        }

        parameters.Remove("offset");
        return [.. allData];
    }

    // Symbols Endpoints
    public async Task<Stock[]> GetStockSymbolsAsync()
    {
        var symbols = await GetDataAsync(EndPoints.StockSymbols, limit: 500);
        if (symbols.Length == 0)
            throw new Exception("Unable to retrieve stock symbols");
        return [.. symbols.Select(static k => new Stock(k.GetProperty("trading_symbol").GetString() ?? "",
            k.GetProperty("registrant_name").GetString() ?? ""))];
    }

    public async Task<ExchangeTradedFund[]> GetEtfSymbolsAsync()
    {
        var symbols = await GetDataAsync(EndPoints.EtfSymbols, limit: 500);
        if (symbols.Length == 0)
            throw new Exception("Unable to retrieve ETF symbols");
        return [.. symbols.Select(static k => new ExchangeTradedFund(k.GetProperty("trading_symbol").GetString() ?? "",
            k.GetProperty("description").GetString() ?? ""))];
    }

    public async Task<Commodity[]> GetCommoditySymbolsAsync()
    {
        var symbols = await GetDataAsync(EndPoints.CommoditySymbols);
        if (symbols.Length == 0)
            throw new Exception("Unable to retrieve commodity symbols");
        return [.. symbols.Select(static k => new Commodity(k.GetProperty("trading_symbol").GetString() ?? "",
            k.GetProperty("description").GetString() ?? ""))];
    }

    public async Task<OverTheCounter[]> GetOtcSymbolsAsync()
    {
        var symbols = await GetDataAsync(EndPoints.OtcSymbols, limit: 500);
        if (symbols.Length == 0)
            throw new Exception("Unable to retrieve OTC symbols");
        return [.. symbols.Select(static k => new OverTheCounter(k.GetProperty("trading_symbol").GetString() ?? "",
            k.GetProperty("title_of_security").GetString() ?? ""))];
    }

    public async Task<Index[]> GetIndexSymbolsAsync()
    {
        var symbols = await GetDataAsync(EndPoints.IndexSymbols);
        if (symbols.Length == 0)
            throw new Exception("Unable to retrieve index symbols");
        return [.. symbols.Select(static k => new Index(k.GetProperty("trading_symbol").GetString() ?? "",
            k.GetProperty("index_name").GetString() ?? ""))];
    }

    public async Task<Future[]> GetFuturesSymbolsAsync()
    {
        var symbols = await GetDataAsync(EndPoints.FuturesSymbols, limit: 500);
        if (symbols.Length == 0)
            throw new Exception("Unable to retrieve futures symbols");
        return [.. symbols.Select(static k => new Future(k.GetProperty("trading_symbol").GetString() ?? "",
            k.GetProperty("description").GetString() ?? "",
            k.GetProperty("type").GetString() ?? ""))];
    }

    public async Task<Crypto[]> GetCryptoSymbolsAsync()
    {
        var symbols = await GetDataAsync(EndPoints.CryptoSymbols, limit: 500);
        if (symbols.Length == 0)
            throw new Exception("Unable to retrieve crypto symbols");
        return [.. symbols.Select(static k => new Crypto(k.GetProperty("trading_symbol").GetString() ?? "",
            k.GetProperty("base_asset").GetString() ?? "",
            k.GetProperty("quote_asset").GetString() ?? ""))];
    }

    public async Task<EodPrice[]> GetStockPricesAsync(string symbol)
    {
        if (string.IsNullOrWhiteSpace(symbol))
            throw new ArgumentException("Symbol cannot be empty", nameof(symbol));
        var paramsDict = new Dictionary<string, string> { { "identifier", symbol } };
        return ConvertToEodPrices(await GetDataAsync(EndPoints.StockPrices, paramsDict, 300));
    }

    public async Task<MinutePrice[]> GetMinutePricesAsync(string identifier, string date)
    {
        if (string.IsNullOrWhiteSpace(identifier))
            throw new ArgumentException("Identifier cannot be empty", nameof(identifier));
        if (!DateOnly.TryParse(date, out _))
            throw new ArgumentException("Invalid date format", nameof(date));
        var paramsDict = new Dictionary<string, string> { { "identifier", identifier }, { "date", date } };
        return ConvertToMinutePrices(await GetDataAsync(EndPoints.MinutePrices, paramsDict, 300));
    }

    public async Task<EodPrice[]> GetCommodityPricesAsync(string identifier)
    {
        if (string.IsNullOrWhiteSpace(identifier))
            throw new ArgumentException("Identifier cannot be empty", nameof(identifier));
        var paramsDict = new Dictionary<string, string> { { "identifier", identifier } };
        return ConvertToEodPrices(await GetDataAsync(EndPoints.CommodityPrices, paramsDict, 300));
    }

    public async Task<EodPrice[]> GetOtcPricesAsync(string identifier)
    {
        if (string.IsNullOrWhiteSpace(identifier))
            throw new ArgumentException("Identifier cannot be empty", nameof(identifier));
        var paramsDict = new Dictionary<string, string> { { "identifier", identifier } };
        return ConvertToEodPrices(await GetDataAsync(EndPoints.OtcPrices, paramsDict, 300));
    }

    public async Task<EodPrice[]> GetIndexPricesAsync(string identifier)
    {
        if (string.IsNullOrWhiteSpace(identifier))
            throw new ArgumentException("Identifier cannot be empty", nameof(identifier));
        var paramsDict = new Dictionary<string, string> { { "identifier", identifier } };
        return ConvertToEodPrices(await GetDataAsync(EndPoints.IndexPrices, paramsDict, 300));
    }

    public async Task<EodPrice[]> GetFuturesPricesAsync(string identifier)
    {
        if (string.IsNullOrWhiteSpace(identifier))
            throw new ArgumentException("Identifier cannot be empty", nameof(identifier));
        var paramsDict = new Dictionary<string, string> { { "identifier", identifier } };
        return ConvertToEodPrices(await GetDataAsync(EndPoints.FuturesPrices, paramsDict, 300));
    }

    public async Task<EodPrice[]> GetCryptoPricesAsync(string identifier)
    {
        if (string.IsNullOrWhiteSpace(identifier))
            throw new ArgumentException("Identifier cannot be empty", nameof(identifier));
        var paramsDict = new Dictionary<string, string> { { "identifier", identifier } };
        return ConvertToEodPrices(await GetDataAsync(EndPoints.CryptoPrices, paramsDict, 300));
    }

    public async Task<MinutePrice[]> GetCryptoMinutePricesAsync(string identifier, string date)
    {
        if (string.IsNullOrWhiteSpace(identifier))
            throw new ArgumentException("Identifier cannot be empty", nameof(identifier));
        if (!DateOnly.TryParse(date, out _))
            throw new ArgumentException("Invalid date format", nameof(date));
        var paramsDict = new Dictionary<string, string> { { "identifier", identifier }, { "date", date } };
        return ConvertToMinutePrices(await GetDataAsync(EndPoints.CryptoMinutePrices, paramsDict, 300));
    }

    // Options Endpoints
    public async Task<OptionChain[]> GetOptionChainAsync(string identifier)
    {
        if (string.IsNullOrWhiteSpace(identifier))
            throw new ArgumentException("Identifier cannot be empty", nameof(identifier));
        var paramsDict = new Dictionary<string, string> { { "identifier", identifier } };
        var results = await GetDataAsync(EndPoints.OptionChain, paramsDict, 300);
        return [.. results.Select(static k =>
        {
            var sym = k.GetProperty("trading_symbol");
            var cik = k.GetProperty("central_index_key");
            var reg = k.GetProperty("registrant_name");
            var con = k.GetProperty("contract_name");
            var exp = k.GetProperty("expiration_date");
            var typ = k.GetProperty("put_or_call");
            var prc = k.GetProperty("strike_price");
            return new OptionChain(sym.GetString() ?? "", cik.GetString() ?? "",
                reg.GetString() ?? "", con.GetString() ?? "",
                exp.ValueKind == JsonValueKind.Null ? DateOnly.MinValue : DateOnly.FromDateTime(exp.GetDateTime()),
                typ.GetString() ?? "",
                prc.ValueKind == JsonValueKind.Null ? null : prc.GetDecimal());
        }).OrderBy(static k => k.Expiration).ThenBy(static k => k.StrikePrice)];
    }

    public async Task<OptionPrice[]> GetOptionPricesAsync(string identifier)
    {
        if (string.IsNullOrWhiteSpace(identifier))
            throw new ArgumentException("Identifier cannot be empty", nameof(identifier));
        var paramsDict = new Dictionary<string, string> { { "identifier", identifier } };
        var results = await GetDataAsync(EndPoints.OptionPrices, paramsDict, 300);
        return [.. results.Select(static k =>
        {
            var name = k.GetProperty("contract_name");
            var date = k.GetProperty("date");
            var open = k.GetProperty("open");
            var high = k.GetProperty("high");
            var low = k.GetProperty("low");
            var close = k.GetProperty("close");
            var vol = k.GetProperty("volume");
            return new OptionPrice(name.GetString() ?? "",
                DateOnly.FromDateTime(date.GetDateTime()),
                open.ValueKind == JsonValueKind.Null ? 0M : open.GetDecimal(),
                high.ValueKind == JsonValueKind.Null ? 0M : high.GetDecimal(),
                low.ValueKind == JsonValueKind.Null ? 0M : low.GetDecimal(),
                close.ValueKind == JsonValueKind.Null ? 0M : close.GetDecimal(),
                vol.ValueKind == JsonValueKind.Null ? 0D : vol.GetDouble());
        }).OrderBy(static k => k.Date)];
    }

    public async Task<OptionGreeks[]> GetOptionGreeksAsync(string identifier)
    {
        if (string.IsNullOrWhiteSpace(identifier))
            throw new ArgumentException("Identifier cannot be empty", nameof(identifier));
        var paramsDict = new Dictionary<string, string> { { "identifier", identifier } };
        var results = await GetDataAsync(EndPoints.OptionGreeks, paramsDict, 300);
        return [.. results.Select(static k =>
        {
            var delta = k.GetProperty("delta");
            var gamma = k.GetProperty("gamma");
            var theta = k.GetProperty("theta");
            var vega = k.GetProperty("vega");
            var rho = k.GetProperty("rho");
            return new OptionGreeks(k.GetProperty("contract_name").GetString() ?? "",
                DateOnly.FromDateTime(k.GetProperty("date").GetDateTime()),
                delta.ValueKind == JsonValueKind.Null ? null : delta.GetDouble(),
                gamma.ValueKind == JsonValueKind.Null ? null : gamma.GetDouble(),
                theta.ValueKind == JsonValueKind.Null ? null : theta.GetDouble(),
                vega.ValueKind == JsonValueKind.Null ? null : vega.GetDouble(),
                rho.ValueKind == JsonValueKind.Null ? null : rho.GetDouble());
        }).OrderBy(static k => k.Date)];
    }

    // Company Info Endpoints
    public async Task<CompanyInformation?> GetCompanyInformationAsync(string identifier)
    {
        if (string.IsNullOrWhiteSpace(identifier))
            throw new ArgumentException("Identifier cannot be empty", nameof(identifier));
        var paramsDict = new Dictionary<string, string> { { "identifier", identifier } };
        var info = (await GetDataAsync(EndPoints.CompanyInformation, paramsDict)).FirstOrDefault();
        if (info.ValueKind == JsonValueKind.Undefined)
            return null;

        var sharesIssued = info.GetProperty("shares_issued");
        var sharesOutstanding = info.GetProperty("shares_outstanding");
        var marketCap = info.GetProperty("market_cap");
        var numemp = info.GetProperty("number_of_employees");
        var industry = info.GetProperty("industry");

        string? standardIndustry = industry.ValueKind == JsonValueKind.Null
            ? null
            : StandardizeIndustry(industry.GetString() ?? "");

        return new CompanyInformation(info.GetProperty("trading_symbol").GetString() ?? "",
            info.GetProperty("central_index_key").GetString() ?? "",
            info.GetProperty("registrant_name").GetString() ?? "",
            info.GetProperty("isin_number").GetString() ?? "",
            info.GetProperty("lei_number").GetString() ?? "",
            info.GetProperty("ein_number").GetString() ?? "",
            info.GetProperty("exchange").GetString() ?? "",
            info.GetProperty("sic_code").GetString() ?? "",
            info.GetProperty("sic_description").GetString() ?? "",
            info.GetProperty("fiscal_year_end").GetString() ?? "",
            info.GetProperty("state_of_incorporation").GetString() ?? "",
            info.GetProperty("phone_number").GetString() ?? "",
            info.GetProperty("mailing_address").GetString() ?? "",
            info.GetProperty("business_address").GetString() ?? "",
            info.GetProperty("former_name").GetString(),
            standardIndustry,
            //info.GetProperty("industry").GetString() ?? "",
            info.GetProperty("founding_date").GetString() ?? "",
            info.GetProperty("chief_executive_officer").GetString() ?? "",
            numemp.ValueKind == JsonValueKind.Null ? 0 : numemp.GetInt32(),
            info.GetProperty("website").GetString() ?? "",
            marketCap.ValueKind == JsonValueKind.Null ? null : marketCap.GetDecimal(),
            sharesIssued.ValueKind == JsonValueKind.Null ? null : sharesIssued.GetDouble(),
            sharesOutstanding.ValueKind == JsonValueKind.Null ? null : sharesOutstanding.GetDouble(),
            info.GetProperty("description").GetString() ?? "");
    }

    public async Task<CryptoInformation?> GetCryptoInformationAsync(string identifier)
    {
        if (string.IsNullOrWhiteSpace(identifier))
            throw new ArgumentException("Identifier cannot be empty", nameof(identifier));

        var paramsDict = new Dictionary<string, string> { { "identifier", identifier } };
        var info = (await GetDataAsync(EndPoints.CryptoInformation, paramsDict)).FirstOrDefault();

        if (info.ValueKind == JsonValueKind.Undefined)
            return null;

        var hp = info.GetProperty("highest_price");
        var lp = info.GetProperty("lowest_price");
        var mc = info.GetProperty("market_cap");
        var fdv = info.GetProperty("fully_diluted_valuation");
        var ts = info.GetProperty("total_supply");
        var ms = info.GetProperty("max_supply");
        var cs = info.GetProperty("circulating_supply");

        return new CryptoInformation(info.GetProperty("trading_symbol").GetString() ?? "",
            info.GetProperty("crypto_name").GetString() ?? "",
            mc.ValueKind == JsonValueKind.Null ? null : mc.GetDecimal(),
            fdv.ValueKind == JsonValueKind.Null ? null : fdv.GetDecimal(),
            ts.ValueKind == JsonValueKind.Null ? null : ts.GetDecimal(),
            ms.ValueKind == JsonValueKind.Null ? null : ms.GetDecimal(),
            cs.ValueKind == JsonValueKind.Null ? null : cs.GetDecimal(),
            hp.ValueKind == JsonValueKind.Null ? null : hp.GetDecimal(),
            DateOnly.FromDateTime(info.GetProperty("highest_price_date").GetDateTime()),
            lp.ValueKind == JsonValueKind.Null ? null : lp.GetDecimal(),
            DateOnly.FromDateTime(info.GetProperty("lowest_price_date").GetDateTime()),
            info.GetProperty("hash_function").GetString() ?? "",
            info.GetProperty("block_time").GetString() ?? "",
            DateOnly.FromDateTime(info.GetProperty("ledger_start_date").GetDateTime()),
            info.GetProperty("website").GetString() ?? "",
            info.GetProperty("description").GetString() ?? "");
    }

    public async Task<KeyMetrics[]> GetKeyMetricsAsync(string identifier)
    {
        if (string.IsNullOrWhiteSpace(identifier))
            throw new ArgumentException("Identifier cannot be empty", nameof(identifier));
        var paramsDict = new Dictionary<string, string> { { "identifier", identifier } };
        var metrics = (await GetDataAsync(EndPoints.KeyMetrics, paramsDict)).ToArray();

        var json = JsonSerializer.Serialize(metrics);

        return [.. metrics.Select(static k =>
        {
            var eps = k.GetProperty("earnings_per_share");
            var epsf = k.GetProperty("earnings_per_share_forecast");
            var pe = k.GetProperty("price_to_earnings_ratio");
            var fpe = k.GetProperty("forward_price_to_earnings_ratio");
            var egr = k.GetProperty("earnings_growth_rate");
            var peg = k.GetProperty("price_earnings_to_growth_ratio");
            var book = k.GetProperty("book_value_per_share");
            var ptbr = k.GetProperty("price_to_book_ratio");
            var ebitda = k.GetProperty("ebitda");
            var entval = k.GetProperty("enterprise_value");
            var yield = k.GetProperty("dividend_yield");
            var dpr = k.GetProperty("dividend_payout_ratio");
            var der = k.GetProperty("debt_to_equity_ratio");
            var capex = k.GetProperty("capital_expenditures");
            var fcf = k.GetProperty("free_cash_flow");
            var roe = k.GetProperty("return_on_equity");
            var beta1 = k.GetProperty("one_year_beta");
            var beta3 = k.GetProperty("three_year_beta");
            var beta5 = k.GetProperty("five_year_beta");
            var ped = k.GetProperty("period_end_date");
            return new KeyMetrics(k.GetProperty("trading_symbol").GetString() ?? "",
                k.GetProperty("central_index_key").GetString() ?? "",
                k.GetProperty("registrant_name").GetString() ?? "",
                k.GetProperty("fiscal_year").GetString() ?? "",
                ped.ValueKind == JsonValueKind.Null ? null : DateOnly.FromDateTime(ped.GetDateTime()),
                eps.ValueKind == JsonValueKind.Null ? null : eps.GetDecimal(),
                epsf.ValueKind == JsonValueKind.Null ? null : epsf.GetDecimal(),
                pe.ValueKind == JsonValueKind.Null ? null : pe.GetDouble(),
                fpe.ValueKind == JsonValueKind.Null ? null : fpe.GetDouble(),
                egr.ValueKind == JsonValueKind.Null ? null : egr.GetDouble(),
                peg.ValueKind == JsonValueKind.Null ? null : peg.GetDouble(),
                book.ValueKind == JsonValueKind.Null ? null : book.GetDecimal(),
                ptbr.ValueKind == JsonValueKind.Null ? null : ptbr.GetDouble(),
                ebitda.ValueKind == JsonValueKind.Null ? null : ebitda.GetDouble(),
                entval.ValueKind == JsonValueKind.Null ? null : entval.GetDecimal(),
                yield.ValueKind == JsonValueKind.Null ? null : yield.GetDouble(),
                dpr.ValueKind == JsonValueKind.Null ? null : dpr.GetDouble(),
                der.ValueKind == JsonValueKind.Null ? null : der.GetDouble(),
                capex.ValueKind == JsonValueKind.Null ? null : capex.GetDecimal(),
                fcf.ValueKind == JsonValueKind.Null ? null : fcf.GetDecimal(),
                roe.ValueKind == JsonValueKind.Null ? null : roe.GetDecimal(),
                beta1.ValueKind == JsonValueKind.Null ? null : beta1.GetDouble(),
                beta3.ValueKind == JsonValueKind.Null ? null : beta3.GetDouble(),
                beta5.ValueKind == JsonValueKind.Null ? null : beta5.GetDouble());
        }).OrderBy(static k => k.PeriodEndDate)];
    }

    public async Task<MarketCap[]> GetMarketCapAsync(string identifier)
    {
        if (string.IsNullOrWhiteSpace(identifier))
            throw new ArgumentException("Identifier cannot be empty", nameof(identifier));
        var paramsDict = new Dictionary<string, string> { { "identifier", identifier } };
        var results = await GetDataAsync(EndPoints.MarketCap, paramsDict);
        return [.. results.Select(static k => {
            var mc = k.GetProperty("market_cap");
            var cimc = k.GetProperty("change_in_market_cap");
            var pcimc = k.GetProperty("percentage_change_in_market_cap");
            var so = k.GetProperty("shares_outstanding");
            var cso = k.GetProperty("change_in_shares_outstanding");
            var pcso = k.GetProperty("percentage_change_in_shares_outstanding");

            return new MarketCap(k.GetProperty("trading_symbol").GetString() ?? "",
                k.GetProperty("central_index_key").GetString() ?? "",
                k.GetProperty("registrant_name").GetString() ?? "",
                k.GetProperty("fiscal_year").GetString() ?? "",
                mc.ValueKind == JsonValueKind.Null ? null : mc.GetDouble(),
                cimc.ValueKind == JsonValueKind.Null ? null : cimc.GetDouble(),
                pcimc.ValueKind == JsonValueKind.Null ? null : pcimc.GetDouble(),
                so.ValueKind == JsonValueKind.Null ? null : so.GetInt64(),
                cso.ValueKind == JsonValueKind.Null ? null : cso.GetInt64(),
                pcso.ValueKind == JsonValueKind.Null ? null : pcso.GetDouble());
            }
        ).OrderBy(static k => k.FiscalYear)];
    }

    public async Task<EmployeeCount[]> GetEmployeeCountAsync(string identifier)
    {
        if (string.IsNullOrWhiteSpace(identifier))
            throw new ArgumentException("Identifier cannot be empty", nameof(identifier));
        var paramsDict = new Dictionary<string, string> { { "identifier", identifier } };
        var counts = await GetDataAsync(EndPoints.EmployeeCount, paramsDict);
        return [.. counts.Select(static k => {
            var empcnt = k.GetProperty("employee_count");
            return new EmployeeCount(k.GetProperty("trading_symbol").GetString() ?? "",
                k.GetProperty("central_index_key").GetString() ?? "",
                k.GetProperty("registrant_name").GetString() ?? "",
                k.GetProperty("fiscal_year").GetString() ?? "",
                empcnt.ValueKind == JsonValueKind.Null ? 0 : empcnt.GetInt32());
            }
        ).OrderBy(static k => k.FiscalYear)];
    }

    public async Task<ExecutiveCompensation[]> GetExecutiveCompensationAsync(string identifier)
    {
        if (string.IsNullOrWhiteSpace(identifier))
            throw new ArgumentException("Identifier cannot be empty", nameof(identifier));
        var paramsDict = new Dictionary<string, string> { { "identifier", identifier } };
        var comps = await GetDataAsync(EndPoints.ExecutiveCompensation, paramsDict, 100);
        return [.. comps.Select(static k =>
        {
            var salary = k.GetProperty("salary");
            var bonus = k.GetProperty("bonus");
            var awards = k.GetProperty("stock_awards");
            var incentives = k.GetProperty("incentive_plan_compensation");
            var other = k.GetProperty("other_compensation");
            var total = k.GetProperty("total_compensation");
            return new ExecutiveCompensation(k.GetProperty("trading_symbol").GetString() ?? "",
                k.GetProperty("central_index_key").GetString() ?? "",
                k.GetProperty("registrant_name").GetString() ?? "",
                k.GetProperty("executive_name").GetString() ?? "",
                k.GetProperty("executive_position").GetString() ?? "",
                k.GetProperty("fiscal_year").GetString() ?? "",
                salary.ValueKind == JsonValueKind.Null ? null : salary.GetDecimal(),
                bonus.ValueKind == JsonValueKind.Null ? null : bonus.GetDecimal(),
                awards.ValueKind == JsonValueKind.Null ? null : awards.GetDecimal(),
                incentives.ValueKind == JsonValueKind.Null ? null : incentives.GetDecimal(),
                other.ValueKind == JsonValueKind.Null ? null : other.GetDecimal(),
                total.ValueKind == JsonValueKind.Null ? null : total.GetDecimal());
        }).OrderBy(static k => k.Name).ThenBy(static k => k.FiscalYear)];
    }

    public async Task<SecurityInformation?> GetSecurityInformationAsync(string identifier)
    {
        if (string.IsNullOrWhiteSpace(identifier))
            throw new ArgumentException("Identifier cannot be empty", nameof(identifier));
        var paramsDict = new Dictionary<string, string> { { "identifier", identifier } };
        var info = (await GetDataAsync(EndPoints.SecuritiesInformation, paramsDict)).FirstOrDefault();
        if (info.ValueKind == JsonValueKind.Undefined)
            return null;
        return new SecurityInformation(info.GetProperty("trading_symbol").GetString() ?? "",
            info.GetProperty("issuer_name").GetString() ?? "",
            info.GetProperty("cusip_number").GetString() ?? "",
            info.GetProperty("isin_number").GetString() ?? "",
            info.GetProperty("figi_identifier").GetString() ?? "",
            info.GetProperty("security_type").GetString() ?? "");
    }

    // Financial Statements
    public async Task<IncomeStatement[]> GetIncomeStatementsAsync(string identifier, string? period = null)
    {
        if (string.IsNullOrWhiteSpace(identifier))
            throw new ArgumentException("Identifier cannot be empty", nameof(identifier));
        var paramsDict = new Dictionary<string, string> { { "identifier", identifier } };
        if (!string.IsNullOrEmpty(period))
            paramsDict["period"] = period;
        var statements = await GetDataAsync(EndPoints.IncomeStatements, paramsDict, 50);
        return [.. statements.Select(static k =>
        {
            var r = k.GetProperty("revenue");
            var cr = k.GetProperty("cost_of_revenue");
            var gp = k.GetProperty("gross_profit");
            var rd = k.GetProperty("research_and_development_expenses");
            var gen = k.GetProperty("general_and_administrative_expenses");
            var opex = k.GetProperty("operating_expenses");
            var opinc = k.GetProperty("operating_income");
            var intex = k.GetProperty("interest_expense");
            var intinc = k.GetProperty("interest_income");
            var netinc = k.GetProperty("net_income");
            var epsBasic = k.GetProperty("earnings_per_share_basic");
            var epsDil = k.GetProperty("earnings_per_share_diluted");
            var soBasic = k.GetProperty("weighted_average_shares_outstanding_basic");
            var soDil = k.GetProperty("weighted_average_shares_outstanding_diluted");
            var ped = k.GetProperty("period_end_date");

            return new IncomeStatement(k.GetProperty("trading_symbol").GetString() ?? "",
                k.GetProperty("central_index_key").GetString() ?? "",
                k.GetProperty("registrant_name").GetString() ?? "",
                k.GetProperty("fiscal_year").GetString() ?? "",
                k.GetProperty("fiscal_period").GetString() ?? "",
                ped.ValueKind == JsonValueKind.Null ? null : DateOnly.FromDateTime(ped.GetDateTime()),
                r.ValueKind == JsonValueKind.Null ? null : r.GetDecimal(),
                cr.ValueKind == JsonValueKind.Null ? null : cr.GetDecimal(),
                gp.ValueKind == JsonValueKind.Null ? null : gp.GetDecimal(),
                rd.ValueKind == JsonValueKind.Null ? null : rd.GetDecimal(),
                gen.ValueKind == JsonValueKind.Null ? null : gen.GetDecimal(),
                opex.ValueKind == JsonValueKind.Null ? null : opex.GetDecimal(),
                opinc.ValueKind == JsonValueKind.Null ? null : opinc.GetDecimal(),
                intex.ValueKind == JsonValueKind.Null ? null : intex.GetDecimal(),
                intinc.ValueKind == JsonValueKind.Null ? null : intinc.GetDecimal(),
                netinc.ValueKind == JsonValueKind.Null ? null : netinc.GetDecimal(),
                epsBasic.ValueKind == JsonValueKind.Null ? null : epsBasic.GetDecimal(),
                epsDil.ValueKind == JsonValueKind.Null ? null : epsDil.GetDecimal(),
                soBasic.ValueKind == JsonValueKind.Null ? null : soBasic.GetInt64(),
                soDil.ValueKind == JsonValueKind.Null ? null : soDil.GetInt64());
        }).OrderBy(static k => k.FiscalYear).ThenBy(static k => k.FiscalPeriod)];
    }

    public async Task<BalanceSheet[]> GetBalanceSheetStatementsAsync(string identifier, string? period = null)
    {
        if (string.IsNullOrWhiteSpace(identifier))
            throw new ArgumentException("Identifier cannot be empty", nameof(identifier));
        var paramsDict = new Dictionary<string, string> { { "identifier", identifier } };
        if (!string.IsNullOrEmpty(period))
            paramsDict["period"] = period;
        var statements = await GetDataAsync(EndPoints.BalanceSheetStatements, paramsDict, 50);

        var json = JsonSerializer.Serialize(statements);

        return [.. statements.Select(static k =>
        {
            var cash = k.GetProperty("cash_and_cash_equivalents");
            var msc = k.GetProperty("marketable_securities_current");
            var ar = k.GetProperty("accounts_receivable");
            var inv = k.GetProperty("inventories");
            var ntr = k.GetProperty("non_trade_receivables");
            var oac = k.GetProperty("other_assets_current");
            var tac = k.GetProperty("total_assets_current");
            var msnc = k.GetProperty("marketable_securities_non_current");
            var ppe = k.GetProperty("property_plant_and_equipment");
            var oanc = k.GetProperty("other_assets_non_current");
            var tanc = k.GetProperty("total_assets_non_current");
            var ta = k.GetProperty("total_assets");
            var ap = k.GetProperty("accounts_payable");
            var dr = k.GetProperty("deferred_revenue");
            var std = k.GetProperty("short_term_debt");
            var olc = k.GetProperty("other_liabilities_current");
            var tlc = k.GetProperty("total_liabilities_current");
            var ltd = k.GetProperty("long_term_debt");
            var olnc = k.GetProperty("other_liabilities_non_current");
            var tlnc = k.GetProperty("total_liabilities_non_current");
            var tl = k.GetProperty("total_liabilities");
            var cs = k.GetProperty("common_stock");
            var re = k.GetProperty("retained_earnings");
            var aoci = k.GetProperty("accumulated_other_comprehensive_income");
            var tse = k.GetProperty("total_shareholders_equity");
            var ped = k.GetProperty("period_end_date");
            return new BalanceSheet(k.GetProperty("trading_symbol").GetString() ?? "",
                k.GetProperty("central_index_key").GetString() ?? "",
                k.GetProperty("registrant_name").GetString() ?? "",
                k.GetProperty("fiscal_year").GetString() ?? "",
                k.GetProperty("fiscal_period").GetString() ?? "",
                ped.ValueKind == JsonValueKind.Null ? null : DateOnly.FromDateTime(ped.GetDateTime()),
                cash.ValueKind == JsonValueKind.Null ? null : cash.GetDecimal(),
                msc.ValueKind == JsonValueKind.Null ? null : msc.GetDecimal(),
                ar.ValueKind == JsonValueKind.Null ? null : ar.GetDecimal(),
                inv.ValueKind == JsonValueKind.Null ? null : inv.GetDecimal(),
                ntr.ValueKind == JsonValueKind.Null ? null : ntr.GetDecimal(),
                oac.ValueKind == JsonValueKind.Null ? null : oac.GetDecimal(),
                tac.ValueKind == JsonValueKind.Null ? null : tac.GetDecimal(),
                msnc.ValueKind == JsonValueKind.Null ? null : msnc.GetDecimal(),
                ppe.ValueKind == JsonValueKind.Null ? null : ppe.GetDecimal(),
                oanc.ValueKind == JsonValueKind.Null ? null : oanc.GetDecimal(),
                tanc.ValueKind == JsonValueKind.Null ? null : tanc.GetDecimal(),
                ta.ValueKind == JsonValueKind.Null ? null : ta.GetDecimal(),
                ap.ValueKind == JsonValueKind.Null ? null : ap.GetDecimal(),
                dr.ValueKind == JsonValueKind.Null ? null : dr.GetDecimal(),
                std.ValueKind == JsonValueKind.Null ? null : std.GetDecimal(),
                olc.ValueKind == JsonValueKind.Null ? null : olc.GetDecimal(),
                tlc.ValueKind == JsonValueKind.Null ? null : tlc.GetDecimal(),
                ltd.ValueKind == JsonValueKind.Null ? null : ltd.GetDecimal(),
                olnc.ValueKind == JsonValueKind.Null ? null : olnc.GetDecimal(),
                tlnc.ValueKind == JsonValueKind.Null ? null : tlnc.GetDecimal(),
                tl.ValueKind == JsonValueKind.Null ? null : tl.GetDecimal(),
                cs.ValueKind == JsonValueKind.Null ? null : cs.GetDecimal(),
                re.ValueKind == JsonValueKind.Null ? null : re.GetDecimal(),
                aoci.ValueKind == JsonValueKind.Null ? null : aoci.GetDecimal(),
                tse.ValueKind == JsonValueKind.Null ? null : tse.GetDecimal());
        }).OrderBy(static k => k.FiscalYear).ThenBy(static k => k.FiscalPeriod)];
    }

    public async Task<CashFlowStatement[]> GetCashFlowStatementsAsync(string identifier, string? period = null)
    {
        if (string.IsNullOrWhiteSpace(identifier))
            throw new ArgumentException("Identifier cannot be empty", nameof(identifier));
        var paramsDict = new Dictionary<string, string> { { "identifier", identifier } };
        if (!string.IsNullOrEmpty(period))
            paramsDict["period"] = period;
        var statements = await GetDataAsync(EndPoints.CashFlowStatements, paramsDict, 50);
        return [.. statements.Select(static k =>
        {
            var da = k.GetProperty("depreciation_and_amortization");
            var sbce = k.GetProperty("share_based_compensation_expense");
            var dite = k.GetProperty("deferred_income_tax_expense");
            var oncie = k.GetProperty("other_non_cash_income_expense");
            var ciar = k.GetProperty("change_in_accounts_receivable");
            var cii = k.GetProperty("change_in_inventories");
            var cintr = k.GetProperty("change_in_non_trade_receivables");
            var cioa = k.GetProperty("change_in_other_assets");
            var ciap = k.GetProperty("change_in_accounts_payable");
            var cidr = k.GetProperty("change_in_deferred_revenue");
            var ciol = k.GetProperty("change_in_other_liabilities");
            var cfoa = k.GetProperty("cash_from_operating_activities");
            var poms = k.GetProperty("purchases_of_marketable_securities");
            var soms = k.GetProperty("sales_of_marketable_securities");
            var aoppe = k.GetProperty("acquisition_of_property_plant_and_equipment");
            var aob = k.GetProperty("acquisition_of_business");
            var oia = k.GetProperty("other_investing_activities");
            var cfia = k.GetProperty("cash_from_investing_activities");
            var twfsbc = k.GetProperty("tax_withholding_for_share_based_compensation");
            var pod = k.GetProperty("payments_of_dividends");
            var iocs = k.GetProperty("issuance_of_common_stock");
            var rocs = k.GetProperty("repurchase_of_common_stock");
            var ioltd = k.GetProperty("issuance_of_long_term_debt");
            var roltd = k.GetProperty("repayment_of_long_term_debt");
            var ofa = k.GetProperty("other_financing_activities");
            var cffa = k.GetProperty("cash_from_financing_activities");
            var cic = k.GetProperty("change_in_cash");
            var caeop = k.GetProperty("cash_at_end_of_period");
            var itp = k.GetProperty("income_taxes_paid");
            var ip = k.GetProperty("interest_paid");
            var ped = k.GetProperty("period_end_date");
            return new CashFlowStatement(k.GetProperty("trading_symbol").GetString() ?? "",
                k.GetProperty("central_index_key").GetString() ?? "",
                k.GetProperty("registrant_name").GetString() ?? "",
                k.GetProperty("fiscal_year").GetString() ?? "",
                k.GetProperty("fiscal_period").GetString() ?? "",
                ped.ValueKind == JsonValueKind.Null ? null : DateOnly.FromDateTime(ped.GetDateTime()),
                da.ValueKind == JsonValueKind.Null ? null : da.GetDecimal(),
                sbce.ValueKind == JsonValueKind.Null ? null : sbce.GetDecimal(),
                dite.ValueKind == JsonValueKind.Null ? null : dite.GetDecimal(),
                oncie.ValueKind == JsonValueKind.Null ? null : oncie.GetDecimal(),
                ciar.ValueKind == JsonValueKind.Null ? null : ciar.GetDecimal(),
                cii.ValueKind == JsonValueKind.Null ? null : cii.GetDecimal(),
                cintr.ValueKind == JsonValueKind.Null ? null : cintr.GetDecimal(),
                cioa.ValueKind == JsonValueKind.Null ? null : cioa.GetDecimal(),
                ciap.ValueKind == JsonValueKind.Null ? null : ciap.GetDecimal(),
                cidr.ValueKind == JsonValueKind.Null ? null : cidr.GetDecimal(),
                ciol.ValueKind == JsonValueKind.Null ? null : ciol.GetDecimal(),
                cfoa.ValueKind == JsonValueKind.Null ? null : cfoa.GetDecimal(),
                poms.ValueKind == JsonValueKind.Null ? null : poms.GetDecimal(),
                soms.ValueKind == JsonValueKind.Null ? null : soms.GetDecimal(),
                aoppe.ValueKind == JsonValueKind.Null ? null : aoppe.GetDecimal(),
                aob.ValueKind == JsonValueKind.Null ? null : aob.GetDecimal(),
                oia.ValueKind == JsonValueKind.Null ? null : oia.GetDecimal(),
                cfia.ValueKind == JsonValueKind.Null ? null : cfia.GetDecimal(),
                twfsbc.ValueKind == JsonValueKind.Null ? null : twfsbc.GetDecimal(),
                pod.ValueKind == JsonValueKind.Null ? null : pod.GetDecimal(),
                iocs.ValueKind == JsonValueKind.Null ? null : iocs.GetDecimal(),
                rocs.ValueKind == JsonValueKind.Null ? null : rocs.GetDecimal(),
                ioltd.ValueKind == JsonValueKind.Null ? null : ioltd.GetDecimal(),
                roltd.ValueKind == JsonValueKind.Null ? null : roltd.GetDecimal(),
                ofa.ValueKind == JsonValueKind.Null ? null : ofa.GetDecimal(),
                cffa.ValueKind == JsonValueKind.Null ? null : cffa.GetDecimal(),
                cic.ValueKind == JsonValueKind.Null ? null : cic.GetDecimal(),
                caeop.ValueKind == JsonValueKind.Null ? null : caeop.GetDecimal(),
                itp.ValueKind == JsonValueKind.Null ? null : itp.GetDecimal(),
                ip.ValueKind == JsonValueKind.Null ? null : ip.GetDecimal());
        }).OrderBy(static k => k.FiscalYear).ThenBy(static k => k.FiscalPeriod)];
    }

    // Dividends Endpoints
    public async Task<Dividend[]> GetDividendsAsync(string identifier)
    {
        if (string.IsNullOrWhiteSpace(identifier))
            throw new ArgumentException("Identifier cannot be empty", nameof(identifier));
        var paramsDict = new Dictionary<string, string> { { "identifier", identifier } };
        var divs = (await GetDataAsync(EndPoints.Dividends, paramsDict, 300)).ToArray();

        if (divs.Length == 0)
            return [];

        return [.. divs.Select(k =>
        {
            var amt = k.GetProperty("amount");
            var dd = k.GetProperty("declaration_date");
            var exd = k.GetProperty("ex_date");
            var recd = k.GetProperty("record_date");
            var pd = k.GetProperty("payment_date");

            if (recd.ValueKind == JsonValueKind.Null)
                throw new Exception($"Record date on dividend cannot be null - symbol is {identifier}");

            return new Dividend(k.GetProperty("trading_symbol").GetString() ?? "",
                k.GetProperty("registrant_name").GetString() ?? "",
                k.GetProperty("type").GetString() ?? "",
                amt.ValueKind == JsonValueKind.Null ? null : amt.GetDecimal(),
                dd.ValueKind == JsonValueKind.Null ? null : DateOnly.FromDateTime(dd.GetDateTime()),
                exd.ValueKind == JsonValueKind.Null ? null : DateOnly.FromDateTime(exd.GetDateTime()),
                DateOnly.FromDateTime(recd.GetDateTime()),
                pd.ValueKind == JsonValueKind.Null ? null : DateOnly.FromDateTime(pd.GetDateTime()));
        }).OrderBy(static k => k.DeclarationDate)];
    }

    public async Task<StockSplit[]> GetStockSplitsAsync(string identifier)
    {
        if (string.IsNullOrWhiteSpace(identifier))
            throw new ArgumentException("Identifier cannot be empty", nameof(identifier));
        var paramsDict = new Dictionary<string, string> { { "identifier", identifier } };
        var splits = await GetDataAsync(EndPoints.StockSplits, paramsDict);
        return [.. splits.Select(static k =>
            new StockSplit(k.GetProperty("trading_symbol").GetString() ?? "",
                k.GetProperty("central_index_key").GetString() ?? "",
                k.GetProperty("registrant_name").GetString() ?? "",
                DateOnly.FromDateTime(k.GetProperty("execution_date").GetDateTime()),
                k.GetProperty("multiplier").GetDouble())
        ).OrderBy(static k => k.ExecutionDate)];
    }

    // Short Interest Endpoints
    public async Task<ShortInterest[]> GetShortInterestAsync(string identifier)
    {
        if (string.IsNullOrWhiteSpace(identifier))
            throw new ArgumentException("Identifier cannot be empty", nameof(identifier));
        var paramsDict = new Dictionary<string, string> { { "identifier", identifier } };
        var shorts = await GetDataAsync(EndPoints.ShortInterest, paramsDict, 300);
        return [.. shorts.Select(static k =>
        {
            var ss = k.GetProperty("shorted_securities");
            var pss = k.GetProperty("previous_shorted_securities");
            var css = k.GetProperty("change_in_shorted_securities");
            var pcss = k.GetProperty("percentage_change_in_shorted_securities");
            var adv = k.GetProperty("average_daily_volume");
            var dtc = k.GetProperty("days_to_cover");
            return new ShortInterest(k.GetProperty("trading_symbol").GetString() ?? "",
                k.GetProperty("title_of_security").GetString() ?? "",
                k.GetProperty("market_code").GetString() ?? "",
                DateOnly.FromDateTime(k.GetProperty("settlement_date").GetDateTime()),
                ss.ValueKind == JsonValueKind.Null ? null : ss.GetInt64(),
                pss.ValueKind == JsonValueKind.Null ? null : pss.GetInt64(),
                css.ValueKind == JsonValueKind.Null ? null : css.GetInt64(),
                pcss.ValueKind == JsonValueKind.Null ? null : pcss.GetDouble(),
                adv.ValueKind == JsonValueKind.Null ? null : adv.GetInt64(),
                dtc.ValueKind == JsonValueKind.Null ? null : dtc.GetDouble(),
                k.GetProperty("is_stock_split").GetBoolean());
        }).OrderBy(static k => k.SettlementDate)];
    }

    public async Task<EarningsRelease[]> GetEarningsReleasesAsync(string identifier)
    {
        if (string.IsNullOrWhiteSpace(identifier))
            throw new ArgumentException("Identifier cannot be empty", nameof(identifier));
        var paramsDict = new Dictionary<string, string> { { "identifier", identifier } };
        var releases = await GetDataAsync(EndPoints.EarningsReleases, paramsDict);
        return [.. releases.Select(static k =>
        {
            var eps = k.GetProperty("earnings_per_share");
            var epsForecast = k.GetProperty("earnings_per_share_forecast");
            var percentageSurprise = k.GetProperty("percentage_surprise");
            var numberOfForecasts = k.GetProperty("number_of_forecasts");
            var conferenceCallTime = k.GetProperty("conference_call_time");
            var mc = k.GetProperty("market_cap");

            return new EarningsRelease(k.GetProperty("trading_symbol").GetString() ?? "",
                k.GetProperty("central_index_key").GetString() ?? "",
                k.GetProperty("registrant_name").GetString() ?? "",
                mc.ValueKind == JsonValueKind.Null ? null : mc.GetDecimal(),
                k.GetProperty("fiscal_quarter_end_date").GetString() ?? "",
                eps.ValueKind == JsonValueKind.Null ? null : eps.GetDecimal(),
                epsForecast.ValueKind == JsonValueKind.Null ? null : epsForecast.GetDecimal(),
                percentageSurprise.ValueKind == JsonValueKind.Null ? null : percentageSurprise.GetDouble(),
                numberOfForecasts.ValueKind == JsonValueKind.Null ? 0 : numberOfForecasts.GetInt32(),
                conferenceCallTime.ValueKind == JsonValueKind.Null ? null : DateTime.Parse(conferenceCallTime.GetString()!));
        }).OrderByDescending(static k => k.FiscalQuarterEndDate)];
    }

    public async Task<EfficiencyRatios[]> GetEfficiencyRatiosAsync(string identifier, string? period = null)
    {
        if (string.IsNullOrWhiteSpace(identifier))
            throw new ArgumentException("Identifier cannot be empty", nameof(identifier));
        var paramsDict = new Dictionary<string, string> { { "identifier", identifier } };
        if (!string.IsNullOrEmpty(period))
            paramsDict["period"] = period;
        var ratios = await GetDataAsync(EndPoints.EfficiencyRatios, paramsDict, 50);
        return [.. ratios.Select(static k =>
        {
            var assetTurnover = k.GetProperty("asset_turnover_ratio");
            var inventoryTurnover = k.GetProperty("inventory_turnover_ratio");
            var accountsReceivableTurnover = k.GetProperty("accounts_receivable_turnover_ratio");
            var accountsPayableTurnover = k.GetProperty("accounts_payable_turnover_ratio");
            var equityMultiplier = k.GetProperty("equity_multiplier");
            var daysSalesInInventory = k.GetProperty("days_sales_in_inventory");
            var fixedAssetTurnover = k.GetProperty("fixed_asset_turnover_ratio");
            var daysWorkingCapital = k.GetProperty("days_working_capital");
            var workingCapitalTurnover = k.GetProperty("working_capital_turnover_ratio");
            var daysCashOnHand = k.GetProperty("days_cash_on_hand");
            var capitalIntensity = k.GetProperty("capital_intensity_ratio");
            var salesToEquity = k.GetProperty("sales_to_equity_ratio");
            var inventoryToSales = k.GetProperty("inventory_to_sales_ratio");
            var investmentTurnover = k.GetProperty("investment_turnover_ratio");
            var salesToOperatingIncome = k.GetProperty("sales_to_operating_income_ratio");
            var ped = k.GetProperty("period_end_date");

            return new EfficiencyRatios(k.GetProperty("trading_symbol").GetString() ?? "",
                k.GetProperty("central_index_key").GetString() ?? "",
                k.GetProperty("registrant_name").GetString() ?? "",
                k.GetProperty("fiscal_year").GetString() ?? "",
                k.GetProperty("fiscal_period").GetString() ?? "",
                ped.ValueKind == JsonValueKind.Null ? null : DateOnly.FromDateTime(ped.GetDateTime()),
                assetTurnover.ValueKind == JsonValueKind.Null ? null : assetTurnover.GetDouble(),
                inventoryTurnover.ValueKind == JsonValueKind.Null ? null : inventoryTurnover.GetDouble(),
                accountsReceivableTurnover.ValueKind == JsonValueKind.Null ? null : accountsReceivableTurnover.GetDouble(),
                accountsPayableTurnover.ValueKind == JsonValueKind.Null ? null : accountsPayableTurnover.GetDouble(),
                equityMultiplier.ValueKind == JsonValueKind.Null ? null : equityMultiplier.GetDouble(),
                daysSalesInInventory.ValueKind == JsonValueKind.Null ? null : daysSalesInInventory.GetDouble(),
                fixedAssetTurnover.ValueKind == JsonValueKind.Null ? null : fixedAssetTurnover.GetDouble(),
                daysWorkingCapital.ValueKind == JsonValueKind.Null ? null : daysWorkingCapital.GetDouble(),
                workingCapitalTurnover.ValueKind == JsonValueKind.Null ? null : workingCapitalTurnover.GetDouble(),
                daysCashOnHand.ValueKind == JsonValueKind.Null ? null : daysCashOnHand.GetDouble(),
                capitalIntensity.ValueKind == JsonValueKind.Null ? null : capitalIntensity.GetDouble(),
                salesToEquity.ValueKind == JsonValueKind.Null ? null : salesToEquity.GetDouble(),
                inventoryToSales.ValueKind == JsonValueKind.Null ? null : inventoryToSales.GetDouble(),
                investmentTurnover.ValueKind == JsonValueKind.Null ? null : investmentTurnover.GetDouble(),
                salesToOperatingIncome.ValueKind == JsonValueKind.Null ? null : salesToOperatingIncome.GetDouble());
        }).OrderBy(static k => k.PeriodEndDate)];
    }

    public async Task<IndexConstituent[]> GetIndexConstituentsAsync(string identifier)
    {
        if (string.IsNullOrWhiteSpace(identifier))
            throw new ArgumentException("Identifier cannot be empty", nameof(identifier));
        var paramsDict = new Dictionary<string, string> { { "identifier", identifier } };
        var constituents = await GetDataAsync(EndPoints.IndexConstituents, paramsDict);
        return [.. constituents.Select(static k =>
            new IndexConstituent(k.GetProperty("trading_symbol").GetString() ?? "",
                k.GetProperty("index_name").GetString() ?? "",
                k.GetProperty("constituent_symbol").GetString() ?? "",
                k.GetProperty("constituent_name").GetString() ?? "",
                k.GetProperty("sector").GetString() ?? "",
                k.GetProperty("industry").GetString() ?? "",
                DateOnly.FromDateTime(k.GetProperty("date_added").GetDateTime()))
        ).OrderBy(static k => k.DateAdded)];
    }

    public async Task<InitialPublicOffering[]> GetInitialPublicOfferingsAsync(string identifier)
    {
        if (string.IsNullOrWhiteSpace(identifier))
            throw new ArgumentException("Identifier cannot be empty", nameof(identifier));
        var paramsDict = new Dictionary<string, string> { { "identifier", identifier } };
        var ipos = await GetDataAsync(EndPoints.InitialPublicOfferings, paramsDict);
        return [.. ipos.Select(static k =>
        {
            var sharePrice = k.GetProperty("share_price");
            var offeringValue = k.GetProperty("offering_value");
            var sharesOff = k.GetProperty("shares_offered");
            return new InitialPublicOffering(k.GetProperty("trading_symbol").GetString() ?? "",
                k.GetProperty("registrant_name").GetString() ?? "",
                k.GetProperty("exchange").GetString() ?? "",
                DateOnly.Parse(k.GetProperty("pricing_date").GetString() ?? DateOnly.MinValue.ToString()),
                sharePrice.ValueKind == JsonValueKind.Null ? null : sharePrice.GetDecimal(),
                sharesOff.ValueKind == JsonValueKind.Null ? 0 : sharesOff.GetInt64(),
                offeringValue.ValueKind == JsonValueKind.Null ? null : offeringValue.GetDecimal());
        }).OrderBy(static k => k.PricingDate)];
    }

    public async Task<LiquidityRatios[]> GetLiquidityRatiosAsync(string identifier, string? period = null)
    {
        if (string.IsNullOrWhiteSpace(identifier))
            throw new ArgumentException("Identifier cannot be empty", nameof(identifier));
        var paramsDict = new Dictionary<string, string> { { "identifier", identifier } };
        if (!string.IsNullOrEmpty(period))
            paramsDict["period"] = period;
        var ratios = await GetDataAsync(EndPoints.LiquidityRatios, paramsDict, 50);
        return [.. ratios.Select(static k =>
        {
            var wc = k.GetProperty("working_capital");
            var cr = k.GetProperty("current_ratio");
            var cashr = k.GetProperty("cash_ratio");
            var qr = k.GetProperty("quick_ratio");
            var dio = k.GetProperty("days_of_inventory_outstanding");
            var dso = k.GetProperty("days_sales_outstanding");
            var dpo = k.GetProperty("days_payables_outstanding");
            var ccc = k.GetProperty("cash_conversion_cycle");
            var swcr = k.GetProperty("sales_to_working_capital_ratio");
            var cclr = k.GetProperty("cash_to_current_liabilities_ratio");
            var wcdr = k.GetProperty( "working_capital_to_debt_ratio");
            var cfar = k.GetProperty("cash_flow_adequacy_ratio");
            var scar = k.GetProperty("sales_to_current_assets_ratio");
            var ccar = k.GetProperty("cash_to_current_assets_ratio");
            var cwcr = k.GetProperty("cash_to_working_capital_ratio");
            var iwcr = k.GetProperty("inventory_to_working_capital_ratio");
            var nd = k.GetProperty("net_debt");
            var ped = k.GetProperty("period_end_date");

            return new LiquidityRatios(k.GetProperty("trading_symbol").GetString() ?? "",
                k.GetProperty("central_index_key").GetString() ?? "",
                k.GetProperty("registrant_name").GetString() ?? "",
                k.GetProperty("fiscal_year").GetString() ?? "",
                k.GetProperty("fiscal_period").GetString() ?? "",
                ped.ValueKind == JsonValueKind.Null ? null : DateOnly.FromDateTime(ped.GetDateTime()),
                wc.ValueKind == JsonValueKind.Null ? null : wc.GetDecimal(),
                cr.ValueKind == JsonValueKind.Null ? null : cr.GetDouble(),
                cashr.ValueKind == JsonValueKind.Null ? null : cashr.GetDouble(),
                qr.ValueKind == JsonValueKind.Null ? null : qr.GetDouble(),
                dio.ValueKind == JsonValueKind.Null ? null : dio.GetDouble(),
                dso.ValueKind == JsonValueKind.Null ? null : dso.GetDouble(),
                dpo.ValueKind == JsonValueKind.Null ? null : dpo.GetDouble(),
                ccc.ValueKind == JsonValueKind.Null ? null : ccc.GetDouble(),
                swcr.ValueKind == JsonValueKind.Null ? null : swcr.GetDouble(),
                cclr.ValueKind == JsonValueKind.Null ? null : cclr.GetDouble(),
                wcdr.ValueKind == JsonValueKind.Null ? null : wcdr.GetDouble(),
                cfar.ValueKind == JsonValueKind.Null ? null : cfar.GetDouble(),
                scar.ValueKind == JsonValueKind.Null ? null : scar.GetDouble(),
                ccar.ValueKind == JsonValueKind.Null ? null : ccar.GetDouble(),
                cwcr.ValueKind == JsonValueKind.Null ? null : cwcr.GetDouble(),
                iwcr.ValueKind == JsonValueKind.Null ? null : iwcr.GetDouble(),
                nd.ValueKind == JsonValueKind.Null ? null : nd.GetDecimal());
        }).OrderBy(static k => k.PeriodEndDate)];
    }

    public async Task<ProfitabilityRatios[]> GetProfitabilityRatiosAsync(string identifier, string? period = null)
    {
        if (string.IsNullOrWhiteSpace(identifier))
            throw new ArgumentException("Identifier cannot be empty", nameof(identifier));
        var paramsDict = new Dictionary<string, string> { { "identifier", identifier } };
        if (!string.IsNullOrEmpty(period))
            paramsDict["period"] = period;
        var ratios = await GetDataAsync(EndPoints.ProfitabilityRatios, paramsDict, 50);
        return [.. ratios.Select(static k =>
        {
            var ebit = k.GetProperty("ebit");
            var ebitda = k.GetProperty("ebitda");
            var profitMargin = k.GetProperty("profit_margin");
            var grossMargin = k.GetProperty("gross_margin");
            var operatingMargin = k.GetProperty("operating_margin");
            var operatingCashFlowMargin = k.GetProperty("operating_cash_flow_margin");
            var returnOnEquity = k.GetProperty("return_on_equity");
            var returnOnAssets = k.GetProperty("return_on_assets");
            var returnOnDebt = k.GetProperty("return_on_debt");
            var cashReturnOnAssets = k.GetProperty("cash_return_on_assets");
            var cashTurnoverRatio = k.GetProperty("cash_turnover_ratio");
            var ped = k.GetProperty("period_end_date");
            return new ProfitabilityRatios(k.GetProperty("trading_symbol").GetString() ?? "",
                k.GetProperty("central_index_key").GetString() ?? "",
                k.GetProperty("registrant_name").GetString() ?? "",
                k.GetProperty("fiscal_year").GetString() ?? "",
                k.GetProperty("fiscal_period").GetString() ?? "",
                ped.ValueKind == JsonValueKind.Null ? null : DateOnly.FromDateTime(ped.GetDateTime()),
                ebit.ValueKind == JsonValueKind.Null ? null : ebit.GetDecimal(),
                ebitda.ValueKind == JsonValueKind.Null ? null : ebitda.GetDecimal(),
                profitMargin.ValueKind == JsonValueKind.Null ? null : profitMargin.GetDouble(),
                grossMargin.ValueKind == JsonValueKind.Null ? null : grossMargin.GetDouble(),
                operatingMargin.ValueKind == JsonValueKind.Null ? null : operatingMargin.GetDouble(),
                operatingCashFlowMargin.ValueKind == JsonValueKind.Null ? null : operatingCashFlowMargin.GetDouble(),
                returnOnEquity.ValueKind == JsonValueKind.Null ? null : returnOnEquity.GetDouble(),
                returnOnAssets.ValueKind == JsonValueKind.Null ? null : returnOnAssets.GetDouble(),
                returnOnDebt.ValueKind == JsonValueKind.Null ? null : returnOnDebt.GetDouble(),
                cashReturnOnAssets.ValueKind == JsonValueKind.Null ? null : cashReturnOnAssets.GetDouble(),
                cashTurnoverRatio.ValueKind == JsonValueKind.Null ? null : cashTurnoverRatio.GetDouble());
        }).OrderBy(static k => k.PeriodEndDate)];
    }

    public async Task<SolvencyRatios[]> GetSolvencyRatiosAsync(string identifier, string? period = null)
    {
        if (string.IsNullOrWhiteSpace(identifier))
            throw new ArgumentException("Identifier cannot be empty", nameof(identifier));
        var paramsDict = new Dictionary<string, string> { { "identifier", identifier } };
        if (!string.IsNullOrEmpty(period))
            paramsDict["period"] = period;
        var ratios = await GetDataAsync(EndPoints.SolvencyRatios, paramsDict, 50);
        return [.. ratios.Select(static k =>
        {
            var equityRatio = k.GetProperty("equity_ratio");
            var debtCoverageRatio = k.GetProperty("debt_coverage_ratio");
            var assetCoverageRatio = k.GetProperty("asset_coverage_ratio");
            var interestCoverageRatio = k.GetProperty("interest_coverage_ratio");
            var debtToEquityRatio = k.GetProperty("debt_to_equity_ratio");
            var debtToAssetsRatio = k.GetProperty("debt_to_assets_ratio");
            var debtToCapitalRatio = k.GetProperty("debt_to_capital_ratio");
            var debtToIncomeRatio = k.GetProperty("debt_to_income_ratio");
            var cashFlowToDebtRatio = k.GetProperty("cash_flow_to_debt_ratio");
            var ped = k.GetProperty("period_end_date");
            return new SolvencyRatios(k.GetProperty("trading_symbol").GetString() ?? "",
                k.GetProperty("central_index_key").GetString() ?? "",
                k.GetProperty("registrant_name").GetString() ?? "",
                k.GetProperty("fiscal_year").GetString() ?? "",
                k.GetProperty("fiscal_period").GetString() ?? "",
                ped.ValueKind == JsonValueKind.Null ? null : DateOnly.FromDateTime(ped.GetDateTime()),
                equityRatio.ValueKind == JsonValueKind.Null ? null : equityRatio.GetDouble(),
                debtCoverageRatio.ValueKind == JsonValueKind.Null ? null : debtCoverageRatio.GetDouble(),
                assetCoverageRatio.ValueKind == JsonValueKind.Null ? null : assetCoverageRatio.GetDouble(),
                interestCoverageRatio.ValueKind == JsonValueKind.Null ? null : interestCoverageRatio.GetDouble(),
                debtToEquityRatio.ValueKind == JsonValueKind.Null ? null : debtToEquityRatio.GetDouble(),
                debtToAssetsRatio.ValueKind == JsonValueKind.Null ? null : debtToAssetsRatio.GetDouble(),
                debtToCapitalRatio.ValueKind == JsonValueKind.Null ? null : debtToCapitalRatio.GetDouble(),
                debtToIncomeRatio.ValueKind == JsonValueKind.Null ? null : debtToIncomeRatio.GetDouble(),
                cashFlowToDebtRatio.ValueKind == JsonValueKind.Null ? null : cashFlowToDebtRatio.GetDouble());
        }).OrderBy(static k => k.PeriodEndDate)];
    }

    public async Task<ValuationRatios[]> GetValuationRatiosAsync(string identifier, string? period = null)
    {
        if (string.IsNullOrWhiteSpace(identifier))
            throw new ArgumentException("Identifier cannot be empty", nameof(identifier));
        var paramsDict = new Dictionary<string, string> { { "identifier", identifier } };
        if (!string.IsNullOrEmpty(period))
            paramsDict["period"] = period;
        var ratios = await GetDataAsync(EndPoints.ValuationRatios, paramsDict, 50);
        return [.. ratios.Select(static k =>
        {
            var dividendsPerShare = k.GetProperty("dividends_per_share");
            var dividendPayoutRatio = k.GetProperty("dividend_payout_ratio");
            var bookValuePerShare = k.GetProperty("book_value_per_share");
            var retentionRatio = k.GetProperty("retention_ratio");
            var netFixedAssets = k.GetProperty("net_fixed_assets");
            var ped = k.GetProperty("period_end_date");
            return new ValuationRatios(k.GetProperty("trading_symbol").GetString() ?? "",
                k.GetProperty("central_index_key").GetString() ?? "",
                k.GetProperty("registrant_name").GetString() ?? "",
                k.GetProperty("fiscal_year").GetString() ?? "",
                k.GetProperty("fiscal_period").GetString() ?? "",
                ped.ValueKind == JsonValueKind.Null ? null : DateOnly.FromDateTime(ped.GetDateTime()),
                dividendsPerShare.ValueKind == JsonValueKind.Null ? null : dividendsPerShare.GetDecimal(),
                dividendPayoutRatio.ValueKind == JsonValueKind.Null ? null : dividendPayoutRatio.GetDouble(),
                bookValuePerShare.ValueKind == JsonValueKind.Null ? null : bookValuePerShare.GetDecimal(),
                retentionRatio.ValueKind == JsonValueKind.Null ? null : retentionRatio.GetDouble(),
                netFixedAssets.ValueKind == JsonValueKind.Null ? null : netFixedAssets.GetDecimal());
        }).OrderBy(static k => k.PeriodEndDate)];
    }

    private static EodPrice[] ConvertToEodPrices(JsonElement[] elements)
    {
        if (elements.Length == 0)
            return [];
        return [.. elements.Select(static k =>
        {
            var open = k.GetProperty("open");
            var high = k.GetProperty("high");
            var low = k.GetProperty("low");
            var close = k.GetProperty("close");
            var volume = k.GetProperty("volume");
            return new EodPrice(k.GetProperty("trading_symbol").GetString() ?? throw new Exception("Trading symbol is null"),
                DateOnly.FromDateTime(k.GetProperty("date").GetDateTime()),
                open.ValueKind == JsonValueKind.Null ? null : open.GetDecimal(),
                high.ValueKind == JsonValueKind.Null ? null : high.GetDecimal(),
                low.ValueKind == JsonValueKind.Null ? null : low.GetDecimal(),
                close.ValueKind == JsonValueKind.Null ? null : close.GetDecimal(),
                volume.ValueKind == JsonValueKind.Null ? null : volume.GetDouble());
        }).OrderBy(static k => k.Date)];
    }

    private static MinutePrice[] ConvertToMinutePrices(JsonElement[] elements)
    {
        if (elements.Length == 0)
            return [];
        return [.. elements.Select(static k =>
        {
            var open = k.GetProperty("open");
            var high = k.GetProperty("high");
            var low = k.GetProperty("low");
            var close = k.GetProperty("close");
            var volume = k.GetProperty("volume");
            return new MinutePrice(k.GetProperty("trading_symbol").GetString() ?? throw new Exception("Trading symbol is null"),
                DateTime.Parse(k.GetProperty("time").GetString() ?? throw new Exception("Time is null")),
                open.ValueKind == JsonValueKind.Null ? null : open.GetDecimal(),
                high.ValueKind == JsonValueKind.Null ? null : high.GetDecimal(),
                low.ValueKind == JsonValueKind.Null ? null : low.GetDecimal(),
                close.ValueKind == JsonValueKind.Null ? null : close.GetDecimal(),
                volume.ValueKind == JsonValueKind.Null ? null : volume.GetDouble());
        }).OrderBy(static k => k.DateTime)];
    }
    private static string StandardizeIndustry(string input) =>
        _industryMapping.TryGetValue(input, out string? value) ? value : input;

    private static readonly Dictionary<string, string> _industryMapping = new()
    {
        { "3D printing", "3D Printing" },
        { "Adhesives", "Adhesives" },
        { "Advertising", "Advertising" },
        { "Aerospace", "Aerospace" },
        { "Aerospace industry", "Aerospace" },
        { "Agribusiness", "Agribusiness" },
        { "Agricultural chemicals", "Agricultural Chemicals" },
        { "Agricultural equipment", "Agricultural Equipment" },
        { "Agricultural machinery", "Agricultural Machinery" },
        { "Agriculture", "Agriculture" },
        { "Aircraft leasing", "Aircraft Leasing" },
        { "Alcoholic beverage", "Alcoholic Beverage" },
        { "Alternative energy", "Alternative Energy" },
        { "Aluminum", "Aluminum" },
        { "Apartments", "Apartments" },
        { "Apparel", "Apparel" },
        { "Apparel industry", "Apparel" },
        { "Application performance monitoring", "Application Performance Monitoring" },
        { "Architecture", "Architecture" },
        { "Arms industry", "Arms" },
        { "Asset Management", "Asset Management" },
        { "Asset management", "Asset Management" },
        { "Auctions", "Auctions" },
        { "Audio encoding/compression", "Audio Encoding/Compression" },
        { "Auto dealership", "Auto Dealership" },
        { "Automobile", "Automobile" },
        { "Automotive", "Automotive" },
        { "Automotive dealerships", "Automotive Dealerships" },
        { "Automotive industry", "Automotive" },
        { "Autonomous vehicles", "Autonomous Vehicles" },
        { "Aviation", "Aviation" },
        { "B2B", "B2B" },
        { "Banking", "Banking" },
        { "Banking software", "Banking Software" },
        { "Beauty", "Beauty" },
        { "Beverage", "Beverage" },
        { "Beverages", "Beverages" },
        { "Biopharmaceutical", "Biopharmaceutical" },
        { "Biopharmaceuticals", "Biopharmaceuticals" },
        { "Biotechnology", "Biotechnology" },
        { "Biotherapeutics", "Biotherapeutics" },
        { "Bitcoin", "Bitcoin" },
        { "Boat building", "Boat Building" },
        { "Building materials", "Building Materials" },
        { "Bus manufacturing", "Bus Manufacturing" },
        { "Business Services", "Business Services" },
        { "Business process outsourcing", "Business Process Outsourcing" },
        { "Business services", "Business Services" },
        { "CAD", "CAD" },
        { "Cable TV", "Cable TV" },
        { "Car dealership", "Car Dealership" },
        { "Casinos", "Casinos" },
        { "Chemical", "Chemical" },
        { "Chemical industry", "Chemical" },
        { "Chemical manufacturing", "Chemical Manufacturing" },
        { "Chemicals", "Chemicals" },
        { "Clothing", "Clothing" },
        { "Cloud Security", "Cloud Security" },
        { "Cloud computing", "Cloud Computing" },
        { "Cloud storage", "Cloud Storage" },
        { "Coal mining", "Coal Mining" },
        { "Coatings", "Coatings" },
        { "Coffee shop", "Coffee Shop" },
        { "Commercial Banks", "Commercial Banks" },
        { "Commercial Cooking Equipment", "Commercial Cooking Equipment" },
        { "Commercial Real Estate", "Commercial Real Estate" },
        { "Commercial property", "Commercial Property" },
        { "Commercial real estate", "Commercial Real Estate" },
        { "Communications", "Communications" },
        { "Computer Software", "Computer Software" },
        { "Computer Systems", "Computer Systems" },
        { "Computer data storage", "Computer Data Storage" },
        { "Computer hardware", "Computer Hardware" },
        { "Computer networking", "Computer Networking" },
        { "Computer software", "Computer Software" },
        { "Computer storage", "Computer Storage" },
        { "Computers", "Computers" },
        { "Confectionery", "Confectionery" },
        { "Conglomerate", "Conglomerate" },
        { "Construction", "Construction" },
        { "Construction Materials", "Construction Materials" },
        { "Construction Software", "Construction Software" },
        { "Construction materials", "Construction Materials" },
        { "Consumer electronics", "Consumer Electronics" },
        { "Consumer goods", "Consumer Goods" },
        { "Consumer products", "Consumer Products" },
        { "Contract Research Organization", "Contract Research Organization" },
        { "Contract research organization", "Contract Research Organization" },
        { "Cosmetics", "Cosmetics" },
        { "Courier", "Courier" },
        { "Credit risk", "Credit Risk" },
        { "Cryptocurrency", "Cryptocurrency" },
        { "Cyber Security and Data Security", "Cyber Security And Data Security" },
        { "Cybersecurity", "Cybersecurity" },
        { "Daily fantasy sports", "Daily Fantasy Sports" },
        { "Data analytics", "Data Analytics" },
        { "Data broker", "Data Broker" },
        { "Data onboarding", "Data Onboarding" },
        { "Data storage", "Data Storage" },
        { "Death care", "Death Care" },
        { "Defense", "Defense" },
        { "Dental equipment", "Dental Equipment" },
        { "Digital marketing", "Digital Marketing" },
        { "Digital media", "Digital Media" },
        { "Discount retail", "Discount Retail" },
        { "Discount retailer", "Discount Retailer" },
        { "Discount store", "Discount Store" },
        { "Diversified investments", "Diversified Investments" },
        { "Domain Name Registration", "Domain Name Registration" },
        { "Downstream (petroleum industry)", "Downstream" },
        { "Drink", "Drink" },
        { "Drink industry", "Drink" },
        { "Drive-thru", "Drive-thru" },
        { "E-commerce", "E-commerce" },
        { "Education", "Education" },
        { "Electric & Gas Utilities", "Electric & Gas Utilities" },
        { "Electric Power", "Electric Power" },
        { "Electric Utility", "Electric Utility" },
        { "Electric power industry", "Electric Power" },
        { "Electric utilities", "Electric Utilities" },
        { "Electric utility", "Electric Utility" },
        { "Electric vehicle infrastructure", "Electric Vehicle Infrastructure" },
        { "Electrical equipment", "Electrical Equipment" },
        { "Electricity", "Electricity" },
        { "Electricity generation", "Electricity Generation" },
        { "Electronic commerce", "Electronic Commerce" },
        { "Electronics", "Electronics" },
        { "Electronics Manufacturing Services", "Electronics Manufacturing Services" },
        { "Electronics industry", "Electronics" },
        { "Electronics manufacturing services", "Electronics Manufacturing Services" },
        { "Employment agency", "Employment Agency" },
        { "Energy", "Energy" },
        { "Energy industry", "Energy" },
        { "Energy storage", "Energy Storage" },
        { "Engineering", "Engineering" },
        { "Enterprise software", "Enterprise Software" },
        { "Entertainment", "Entertainment" },
        { "Equipment rental", "Equipment Rental" },
        { "Experiential hospitality", "Experiential Hospitality" },
        { "Fashion", "Fashion" },
        { "Fiber Optics", "Fiber Optics" },
        { "Filtration", "Filtration" },
        { "Finance", "Finance" },
        { "Finance and Insurance", "Finance And Insurance" },
        { "Financial Technology", "Financial Technology" },
        { "Financial services", "Financial Services" },
        { "Financial technology", "Financial Technology" },
        { "Fintech", "Fintech" },
        { "Firearms", "Firearms" },
        { "Fitness", "Fitness" },
        { "Flooring Systems", "Flooring Systems" },
        { "Flooring manufacturing", "Flooring Manufacturing" },
        { "Food", "Food" },
        { "Food industry", "Food" },
        { "Food processing", "Food Processing" },
        { "Food technology", "Food Technology" },
        { "Foodservice", "Foodservice" },
        { "Footwear", "Footwear" },
        { "For-profit education", "For-profit Education" },
        { "Forestry", "Forestry" },
        { "Freelance marketplace", "Freelance Marketplace" },
        { "Freight transport", "Freight Transport" },
        { "Furniture", "Furniture" },
        { "Gambling", "Gambling" },
        { "Gaming", "Gaming" },
        { "Genetic testing", "Genetic Testing" },
        { "Gold mining", "Gold Mining" },
        { "Grocery store", "Grocery Store" },
        { "HVAC", "HVAC" },
        { "Health care", "Health Care" },
        { "Health food", "Health Food" },
        { "Health insurance", "Health Insurance" },
        { "Health technology", "Health Technology" },
        { "Healthcare", "Healthcare" },
        { "Heating", "Heating" },
        { "Heavy construction", "Heavy Construction" },
        { "Heavy equipment", "Heavy Equipment" },
        { "Higher education", "Higher Education" },
        { "Holding company", "Holding Company" },
        { "Home appliances", "Home Appliances" },
        { "Home construction", "Home Construction" },
        { "Home furnishings", "Home Furnishings" },
        { "Home improvement", "Home Improvement" },
        { "Homebuilding", "Homebuilding" },
        { "Hospitality", "Hospitality" },
        { "Hospitality Management", "Hospitality Management" },
        { "Human resource consulting", "Human Resource Consulting" },
        { "IT services", "IT Services" },
        { "Identity verification", "Identity Verification" },
        { "Industrial Technology", "Industrial Technology" },
        { "Industrial gas", "Industrial Gas" },
        { "Industrial manufacturer", "Industrial Manufacturer" },
        { "Industrials", "Industrials" },
        { "Information Technology", "Information Technology" },
        { "Information and communications technology", "Information And Communications Technology" },
        { "Information security", "Information Security" },
        { "Information storage", "Information Storage" },
        { "Information technology", "Information Technology" },
        { "Information technology consulting", "Information Technology Consulting" },
        { "Infrastructure asset management", "Infrastructure Asset Management" },
        { "Insurance", "Insurance" },
        { "Insurance broker", "Insurance Broker" },
        { "Insurance brokers", "Insurance Brokers" },
        { "Integrated circuit", "Integrated Circuit" },
        { "Internet", "Internet" },
        { "Internet media", "Internet Media" },
        { "Internet of things", "Internet Of Things" },
        { "Investment", "Investment" },
        { "Investment Management", "Investment Management" },
        { "Investment banking", "Investment Banking" },
        { "Investment management", "Investment Management" },
        { "Investment research", "Investment Research" },
        { "Investment services", "Investment Services" },
        { "Irrigation", "Irrigation" },
        { "Laboratory equipment", "Laboratory Equipment" },
        { "Land development", "Land Development" },
        { "Launch service provider", "Launch Service Provider" },
        { "Lidar", "Lidar" },
        { "Life insurance", "Life Insurance" },
        { "Litigation", "Litigation" },
        { "Local search", "Local Search" },
        { "Lodging", "Lodging" },
        { "Logistics", "Logistics" },
        { "Managed healthcare", "Managed Healthcare" },
        { "Management consulting", "Management Consulting" },
        { "Manufacturing", "Manufacturing" },
        { "Maritime", "Maritime" },
        { "Maritime transport", "Maritime Transport" },
        { "Marketing Automation", "Marketing Automation" },
        { "Marketplace", "Marketplace" },
        { "Mass customization", "Mass Customization" },
        { "Mass media", "Mass Media" },
        { "Meat processing", "Meat Processing" },
        { "Media", "Media" },
        { "Medical", "Medical" },
        { "Medical Devices", "Medical Devices" },
        { "Medical Technology", "Medical Technology" },
        { "Medical device", "Medical Device" },
        { "Medical devices", "Medical Devices" },
        { "Medical equipment", "Medical Equipment" },
        { "Medical instruments", "Medical Instruments" },
        { "Medical supplies", "Medical Supplies" },
        { "Medical technology", "Medical Technology" },
        { "Metal", "Metal" },
        { "Metals", "Metals" },
        { "Military technology", "Military Technology" },
        { "Mining", "Mining" },
        { "Mobile technology", "Mobile Technology" },
        { "Mortgage lending and servicing", "Mortgage Lending And Servicing" },
        { "Motorsports", "Motorsports" },
        { "Multi-industry", "Multi" },
        { "Music", "Music" },
        { "National security", "National Security" },
        { "Natural gas", "Natural Gas" },
        { "Natural gas extraction", "Natural Gas Extraction" },
        { "Natural gas utility", "Natural Gas Utility" },
        { "Network Security", "Network Security" },
        { "Network security", "Network Security" },
        { "Networking equipment", "Networking Equipment" },
        { "Networking hardware", "Networking Hardware" },
        { "Newspapers", "Newspapers" },
        { "Nuclear power", "Nuclear Power" },
        { "Oil", "Oil" },
        { "Oil & Gas Refining & Marketing", "Oil & Gas Refining & Marketing" },
        { "Oil and Gas Equipment, Services", "Oil And Gas Equipment, Services" },
        { "Oil and gas", "Oil And Gas" },
        { "Oil and gas equipment, services", "Oil And Gas Equipment, Services" },
        { "Oil and gas industry", "Oil And Gas" },
        { "Oil tanker", "Oil Tanker" },
        { "Oilfield", "Oilfield" },
        { "Oilfield services", "Oilfield Services" },
        { "Online dating", "Online Dating" },
        { "Online education", "Online Education" },
        { "Online food ordering", "Online Food Ordering" },
        { "Online gaming", "Online Gaming" },
        { "Online marketplace", "Online Marketplace" },
        { "Online payments", "Online Payments" },
        { "Online pharmacy", "Online Pharmacy" },
        { "Ophthalmology", "Ophthalmology" },
        { "Orthodontics", "Orthodontics" },
        { "Outdoor advertising", "Outdoor Advertising" },
        { "Outsourcing", "Outsourcing" },
        { "Packaging", "Packaging" },
        { "Payment cards services", "Payment Cards Services" },
        { "Payment processing", "Payment Processing" },
        { "Payroll service bureau", "Payroll Service Bureau" },
        { "Personal finance", "Personal Finance" },
        { "Pest control", "Pest Control" },
        { "Pet food", "Pet Food" },
        { "Pet insurance", "Pet Insurance" },
        { "Petroleum", "Petroleum" },
        { "Petroleum and natural gas", "Petroleum And Natural Gas" },
        { "Petroleum industry", "Petroleum" },
        { "Pharmaceutical", "Pharmaceutical" },
        { "Pharmaceutical (animal)", "Pharmaceutical" },
        { "Pharmaceutical Industry", "Pharmaceutical" },
        { "Pharmaceutical industry", "Pharmaceutical" },
        { "Pharmaceuticals", "Pharmaceuticals" },
        { "Pharmaceutics", "Pharmaceutics" },
        { "Photonics", "Photonics" },
        { "Photovoltaics", "Photovoltaics" },
        { "Piping", "Piping" },
        { "Point of sale", "Point Of Sale" },
        { "Pollution", "Pollution" },
        { "Power generation", "Power Generation" },
        { "Printing", "Printing" },
        { "Private equity", "Private Equity" },
        { "Private prisons", "Private Prisons" },
        { "Process management", "Process Management" },
        { "Professional employer organization", "Professional Employer Organization" },
        { "Professional services", "Professional Services" },
        { "Propane", "Propane" },
        { "Public Utility", "Public Utility" },
        { "Public utility", "Public Utility" },
        { "Publishing", "Publishing" },
        { "Pulp and paper", "Pulp And Paper" },
        { "Quantum computing", "Quantum Computing" },
        { "REIT", "REIT" },
        { "RFID", "RFID" },
        { "Radio broadcasting", "Radio Broadcasting" },
        { "Railways", "Railways" },
        { "Real Estate", "Real Estate" },
        { "Real Estate Investment Trust", "Real Estate Investment Trust" },
        { "Real estate", "Real Estate" },
        { "Real estate development", "Real Estate Development" },
        { "Real estate investment trust", "Real Estate Investment Trust" },
        { "Recreational Vehicle", "Recreational Vehicle" },
        { "Recreational vehicle (RV)", "Recreational Vehicle" },
        { "Reinsurance", "Reinsurance" },
        { "Renewable energy", "Renewable Energy" },
        { "Renewable power", "Renewable Power" },
        { "Rental", "Rental" },
        { "Restaurant", "Restaurant" },
        { "Restaurants", "Restaurants" },
        { "Retail", "Retail" },
        { "Retailing", "Retailing" },
        { "Robotics", "Robotics" },
        { "Rubber", "Rubber" },
        { "SaaS", "SaaS" },
        { "Safety equipment", "Safety Equipment" },
        { "Satellite telecommunications", "Satellite Telecommunications" },
        { "Scientific instruments", "Scientific Instruments" },
        { "Security", "Security" },
        { "Security software", "Security Software" },
        { "Security systems", "Security Systems" },
        { "Semiconductor", "Semiconductor" },
        { "Semiconductor Equipment & Materials", "Semiconductor Equipment & Materials" },
        { "Semiconductor industry", "Semiconductor" },
        { "Semiconductors", "Semiconductors" },
        { "Service", "Service" },
        { "Shipping", "Shipping" },
        { "Shoes", "Shoes" },
        { "Small Modular Reactor", "Small Modular Reactor" },
        { "Social media", "Social Media" },
        { "Software", "Software" },
        { "Software Product Development", "Software Product Development" },
        { "Software as a service", "Software As A Service" },
        { "Software development", "Software Development" },
        { "Software engineering", "Software Engineering" },
        { "Software publishing", "Software Publishing" },
        { "Space", "Space" },
        { "Specialty chemicals", "Specialty Chemicals" },
        { "Speech Recognition", "Speech Recognition" },
        { "Sports", "Sports" },
        { "Sports betting", "Sports Betting" },
        { "Sports equipment", "Sports Equipment" },
        { "Sportswear", "Sportswear" },
        { "Steel", "Steel" },
        { "Steel industry", "Steel" },
        { "Stock photography", "Stock Photography" },
        { "Strip clubs", "Strip Clubs" },
        { "Supply chain management", "Supply Chain Management" },
        { "Surplus", "Surplus" },
        { "Surveillance cameras", "Surveillance Cameras" },
        { "Swimming pools", "Swimming Pools" },
        { "Tax compliance software", "Tax Compliance Software" },
        { "Technology", "Technology" },
        { "Technology company", "Technology Company" },
        { "Telecommunication", "Telecommunication" },
        { "Telecommunications", "Telecommunications" },
        { "Telecommunications equipment", "Telecommunications Equipment" },
        { "Textile", "Textile" },
        { "Textile industry", "Textile" },
        { "Theme parks", "Theme Parks" },
        { "Timber", "Timber" },
        { "Timeshares", "Timeshares" },
        { "Tobacco", "Tobacco" },
        { "Tourism", "Tourism" },
        { "Toys", "Toys" },
        { "Transport", "Transport" },
        { "Transport systems", "Transport Systems" },
        { "Transportation", "Transportation" },
        { "Transportation and Logistics", "Transportation And Logistics" },
        { "Travel", "Travel" },
        { "Travel agency", "Travel Agency" },
        { "Travel industry", "Travel" },
        { "Trucking", "Trucking" },
        { "Utilities", "Utilities" },
        { "Utility", "Utility" },
        { "Variety store", "Variety Store" },
        { "Vehicle Tracking", "Vehicle Tracking" },
        { "Vehicle for hire", "Vehicle For Hire" },
        { "Video game industry", "Video Game" },
        { "Video games", "Video Games" },
        { "Waste management", "Waste Management" },
        { "Water industry", "Water" },
        { "Water technology", "Water Technology" },
        { "Water treatment", "Water Treatment" },
        { "Weight loss", "Weight Loss" },
        { "Wholesale", "Wholesale" },
        { "Wholesale trade", "Wholesale Trade" },
        { "Wireless", "Wireless" },
        { "buzzword", "Buzzword" },
        { "clothing", "Clothing" },
        { "construction", "Construction" },
        { "drug delivery", "Drug Delivery" },
        { "electronics", "Electronics" },
        { "electronics industry", "Electronics" },
        { "household goods", "Household Goods" },
        { "infrastructure", "Infrastructure" },
        { "machinery", "Machinery" },
        { "mobile applications", "Mobile Applications" },
        { "nutritional supplements", "Nutritional Supplements" },
        { "pharmaceuticals", "Pharmaceuticals" },
        { "photovoltaics", "Photovoltaics" },
        { "semiconductor industry", "Semiconductor" },
        { "thin-film renewables (solar and glass)", "Thin-film Renewables" },
    };
    public void Dispose()
    {
        _httpClient.Dispose();
        GC.SuppressFinalize(this);
    }
}