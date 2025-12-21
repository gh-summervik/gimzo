using System.Text.Json;

namespace Gimzo.Infrastructure.DataProviders;

public class FinancialDataClient
{
    private readonly string _apiKey;
    private readonly HttpClient _client;
    private readonly string _baseUrl = "https://financialdata.net/api/v1/";

    public FinancialDataClient(string apiKey)
    {
        _apiKey = apiKey ?? throw new ArgumentNullException(nameof(apiKey));
        _client = new HttpClient();
    }

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
        public const string InternationalCompanyInformation = "international-company-information";
        public const string InternationalStockPrices = "international-stock-prices";
        public const string InternationalStockSymbols = "international-stock-symbols";
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

    private async Task<JsonElement[]?> MakeRequestAsync(string endpoint, Dictionary<string, string>? parameters = null)
    {
        parameters ??= [];
        var queryParams = new List<string>();
        foreach (var kvp in parameters)
            queryParams.Add($"{Uri.EscapeDataString(kvp.Key)}={Uri.EscapeDataString(kvp.Value)}");

        queryParams.Add($"key={Uri.EscapeDataString(_apiKey)}");
        string query = string.Join("&", queryParams);

        string url = $"{_baseUrl}{endpoint}?{query}";

        int backoff = 1;
        while (true)
        {
            try
            {
                var response = await _client.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    string content = await response.Content.ReadAsStringAsync();
                    if (string.IsNullOrWhiteSpace(content))
                        throw new Exception($"Call to '{url}' returned empty response.");

                    return JsonSerializer.Deserialize<JsonElement[]>(content);
                }
                else
                {
                    int status = (int)response.StatusCode;
                    if (status == 429 || status == 500 || status == 503)
                    {
                        Console.WriteLine($"Error {status} at {endpoint} - Retrying after {backoff} seconds...");
                        await Task.Delay(backoff * 1000);
                        backoff *= 2;
                    }
                    else
                    {
                        string errorContent = await response.Content.ReadAsStringAsync();
                        throw new HttpRequestException($"Error {status} at {endpoint}: {errorContent}");
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Request failed for {endpoint}: {ex.Message}", ex);
            }
        }
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

        // Remove offset from params for next calls if reused
        parameters.Remove("offset");
        return [.. allData];
    }

    // Symbols Endpoints
    public async Task<Stock[]> GetStockSymbolsAsync()
    {
        var symbols = await GetDataAsync(EndPoints.StockSymbols, limit: 500);

        if ((symbols?.Length ?? 0) == 0)
            throw new Exception("Unable to retrieve stock symbols");

        return [.. symbols!.Select(k => new Stock(k.GetProperty("trading_symbol").GetString() ?? "",
            k.GetProperty("registrant_name").GetString() ?? "", IsInternational: false))];
    }

    public async Task<Stock[]> GetInternationalStockSymbolsAsync()
    {
        var symbols = await GetDataAsync(EndPoints.InternationalStockSymbols, limit: 500);

        if ((symbols?.Length ?? 0) == 0)
            throw new Exception("Unable to retrieve international stock symbols");

        return [.. symbols!.Select(k => new Stock(k.GetProperty("trading_symbol").GetString() ?? "",
            k.GetProperty("registrant_name").GetString() ?? "", IsInternational: true))];
    }

    public async Task<ExchangeTradedFund[]> GetEtfSymbolsAsync()
    {
        var symbols = await GetDataAsync(EndPoints.EtfSymbols, limit: 500);

        if ((symbols?.Length ?? 0) == 0)
            throw new Exception("Unable to retrieve ETF symbols");

        return [.. symbols!.Select(k => new ExchangeTradedFund(k.GetProperty("trading_symbol").GetString() ?? "",
            k.GetProperty("description").GetString() ?? ""))];
    }

    public async Task<Commodity[]> GetCommoditySymbolsAsync()
    {
        var symbols = await GetDataAsync(EndPoints.CommoditySymbols);
        if ((symbols?.Length ?? 0) == 0)
            throw new Exception("Unable to retrieve commodity symbols");
        return [.. symbols!.Select(k => new Commodity(k.GetProperty("trading_symbol").GetString() ?? "",
            k.GetProperty("description").GetString() ?? ""))];
    }

    public async Task<OverTheCounter[]> GetOtcSymbolsAsync()
    {
        var symbols = await GetDataAsync(EndPoints.OtcSymbols, limit: 500);

        if ((symbols?.Length ?? 0) == 0)
            throw new Exception("Unable to retrieve OTC symbols");

        return [.. symbols!.Select(k => new OverTheCounter(k.GetProperty("trading_symbol").GetString() ?? "",
            k.GetProperty("title_of_security").GetString() ?? ""))];
    }

    public async Task<Index[]> GetIndexSymbolsAsync()
    {
        var symbols = (await GetDataAsync(EndPoints.IndexSymbols)).ToArray();
        if ((symbols?.Length ?? 0) == 0)
            throw new Exception("Unable to retrieve index symbols");
        return [.. symbols!.Select(k => new Index(k.GetProperty("trading_symbol").GetString() ?? "",
            k.GetProperty("index_name").GetString() ?? ""))];
    }

    public async Task<Future[]> GetFuturesSymbolsAsync()
    {
        var symbols = (await GetDataAsync(EndPoints.FuturesSymbols, limit: 500)).ToArray();
        if ((symbols?.Length ?? 0) == 0)
            throw new Exception("Unable to retrieve futures symbols");
        return [.. symbols!.Select(k => new Future(k.GetProperty("trading_symbol").GetString() ?? "",
            k.GetProperty("description").GetString() ?? "",
            k.GetProperty("type").GetString() ?? ""))];
    }

    public async Task<Crypto[]> GetCryptoSymbolsAsync()
    {
        var symbols = (await GetDataAsync(EndPoints.CryptoSymbols, limit: 500)).ToArray();
        if ((symbols?.Length ?? 0) == 0)
            throw new Exception("Unable to retrieve crypto symbols");
        return [.. symbols!.Select(k => new Crypto(k.GetProperty("trading_symbol").GetString() ?? "",
            k.GetProperty("base_asset").GetString() ?? "",
            k.GetProperty("quote_asset").GetString() ?? ""))];
    }

    public async Task<EodPrice[]> GetStockPricesAsync(string symbol)
    {
        var paramsDict = new Dictionary<string, string> { { "identifier", symbol } };
        return ConvertToEodPrices(await GetDataAsync(EndPoints.StockPrices, paramsDict, 300));
    }

    public async Task<EodPrice[]> GetInternationalStockPricesAsync(string identifier)
    {
        var paramsDict = new Dictionary<string, string> { { "identifier", identifier } };
        return ConvertToEodPrices(await GetDataAsync(EndPoints.InternationalStockPrices, paramsDict, 300));
    }

    public async Task<MinutePrice[]> GetMinutePricesAsync(string identifier, string date)
    {
        var paramsDict = new Dictionary<string, string> { { "identifier", identifier }, { "date", date } };
        return ConvertToMinutePrices(await GetDataAsync(EndPoints.MinutePrices, paramsDict, 300));
    }

    public async Task<EodPrice[]> GetCommodityPricesAsync(string identifier)
    {
        var paramsDict = new Dictionary<string, string> { { "identifier", identifier } };
        return ConvertToEodPrices(await GetDataAsync(EndPoints.CommodityPrices, paramsDict, 300));
    }

    public async Task<EodPrice[]> GetOtcPricesAsync(string identifier)
    {
        var paramsDict = new Dictionary<string, string> { { "identifier", identifier } };
        return ConvertToEodPrices(await GetDataAsync(EndPoints.OtcPrices, paramsDict, 300));
    }

    public async Task<EodPrice[]> GetIndexPricesAsync(string identifier)
    {
        var paramsDict = new Dictionary<string, string> { { "identifier", identifier } };
        return ConvertToEodPrices(await GetDataAsync(EndPoints.IndexPrices, paramsDict, 300));
    }

    public async Task<EodPrice[]> GetFuturesPricesAsync(string identifier)
    {
        var paramsDict = new Dictionary<string, string> { { "identifier", identifier } };
        return ConvertToEodPrices(await GetDataAsync(EndPoints.FuturesPrices, paramsDict, 300));
    }

    public async Task<EodPrice[]> GetCryptoPricesAsync(string identifier)
    {
        var paramsDict = new Dictionary<string, string> { { "identifier", identifier } };
        return ConvertToEodPrices(await GetDataAsync(EndPoints.CryptoPrices, paramsDict, 300));
    }

    public async Task<MinutePrice[]> GetCryptoMinutePricesAsync(string identifier, string date)
    {
        var paramsDict = new Dictionary<string, string> { { "identifier", identifier }, { "date", date } };
        return ConvertToMinutePrices(await GetDataAsync(EndPoints.CryptoMinutePrices, paramsDict, 300));
    }

    // Options Endpoints
    public async Task<OptionChain[]> GetOptionChainAsync(string identifier)
    {
        var paramsDict = new Dictionary<string, string> { { "identifier", identifier } };
        var results = await GetDataAsync(EndPoints.OptionChain, paramsDict, 300);

        return [.. results.Select(k =>
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
                prc.ValueKind == JsonValueKind.Null ? 0M : prc.GetDecimal());

        }).OrderBy(k => k.Expiration).ThenBy(k => k.StrikePrice)];
    }

    public async Task<OptionPrice[]> GetOptionPricesAsync(string identifier)
    {
        var paramsDict = new Dictionary<string, string> { { "identifier", identifier } };
        var results = (await GetDataAsync(EndPoints.OptionPrices, paramsDict, 300)).ToArray();

        return [.. results.Select(k => {
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
        }).OrderBy(k => k.Date)];
    }

    public async Task<OptionGreeks[]> GetOptionGreeksAsync(string identifier)
    {
        var paramsDict = new Dictionary<string, string> { { "identifier", identifier } };
        var results = (await GetDataAsync(EndPoints.OptionGreeks, paramsDict, 300)).ToArray();

        return [.. results.Select(k => {
            var delta = k.GetProperty("delta");
            var gamma = k.GetProperty("gamma");
            var theta = k.GetProperty("theta");
            var vega = k.GetProperty("vega");
            var rho = k.GetProperty("rho");

            return new OptionGreeks(k.GetProperty("contract_name").GetString() ?? "",
                DateOnly.FromDateTime(k.GetProperty("date").GetDateTime()),
                delta.ValueKind == JsonValueKind.Null ? 0D : delta.GetDouble(),
                gamma.ValueKind == JsonValueKind.Null ? 0D : gamma.GetDouble(),
                theta.ValueKind == JsonValueKind.Null ? 0D : theta.GetDouble(),
                vega.ValueKind == JsonValueKind.Null ? 0D : vega.GetDouble(),
                rho.ValueKind == JsonValueKind.Null ? 0D : rho.GetDouble());
        }).OrderBy(k => k.Date)];
    }

    // Company Info Endpoints
    public async Task<CompanyInformation?> GetCompanyInformationAsync(string identifier)
    {
        var paramsDict = new Dictionary<string, string> { { "identifier", identifier } };
        var info = (await GetDataAsync(EndPoints.CompanyInformation, paramsDict)).FirstOrDefault();

        if (info.ValueKind == JsonValueKind.Undefined)
            return null;

        var sharesIssued = info.GetProperty("shares_issued");

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
            info.GetProperty("industry").GetString() ?? "",
            DateOnly.FromDateTime(info.GetProperty("founding_date").GetDateTime()),
            info.GetProperty("chief_executive_officer").GetString() ?? "",
            info.GetProperty("number_of_employees").GetInt32(),
            info.GetProperty("website").GetString() ?? "",
            info.GetProperty("market_cap").GetDouble(),
            sharesIssued.ValueKind == JsonValueKind.Null ? null : sharesIssued.GetInt32(),
            info.GetProperty("shares_outstanding").GetInt64(),
            info.GetProperty("description").GetString() ?? "");
    }

    public async Task<InternationalCompanyInformation?> GetInternationalCompanyInformationAsync(string identifier)
    {
        var paramsDict = new Dictionary<string, string> { { "identifier", identifier } };
        var info = (await GetDataAsync(EndPoints.InternationalCompanyInformation, paramsDict)).FirstOrDefault();

        if (info.ValueKind == JsonValueKind.Undefined)
            return null;

        return new InternationalCompanyInformation(
            info.GetProperty("trading_symbol").GetString() ?? "",
            info.GetProperty("registrant_name").GetString() ?? "",
            info.GetProperty("exchange").GetString() ?? "",
            info.GetProperty("isin_number").GetString() ?? "",
            info.GetProperty("industry").GetString() ?? "",
            info.GetProperty("founding_date").GetString() ?? "",
            info.GetProperty("chief_executive_officer").GetString() ?? "",
            info.GetProperty("number_of_employees").GetInt32(),
            info.GetProperty("website").GetString() ?? "",
            info.GetProperty("description").GetString() ?? "");
    }

    public async Task<KeyMetrics[]> GetKeyMetricsAsync(string identifier)
    {
        var paramsDict = new Dictionary<string, string> { { "identifier", identifier } };
        var metrics = (await GetDataAsync(EndPoints.KeyMetrics, paramsDict)).ToArray();

        return [.. metrics.Select(k =>
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

            return new KeyMetrics(k.GetProperty("trading_symbol").GetString() ?? "",
                    k.GetProperty("central_index_key").GetString() ?? "",
                    k.GetProperty("registrant_name").GetString() ?? "",
                    k.GetProperty("fiscal_year").GetString() ?? "",
                    DateOnly.FromDateTime(k.GetProperty("period_end_date").GetDateTime()),
                    eps.ValueKind == JsonValueKind.Null ? 0M : eps.GetDecimal(),
                    epsf.ValueKind == JsonValueKind.Null ? 0M : epsf.GetDecimal(),
                    pe.ValueKind == JsonValueKind.Null ? 0D : pe.GetDouble(),
                    fpe.ValueKind == JsonValueKind.Null ? 0D : fpe.GetDouble(),
                    egr.ValueKind == JsonValueKind.Null ? 0D : egr.GetDouble(),
                    peg.ValueKind == JsonValueKind.Null ? 0D : peg.GetDouble(),
                    book.ValueKind == JsonValueKind.Null ? 0M : book.GetDecimal(),
                    ptbr.ValueKind == JsonValueKind.Null ? 0D : ptbr.GetDouble(),
                    ebitda.ValueKind == JsonValueKind.Null ? 0D : ebitda.GetDouble(),
                    entval.ValueKind == JsonValueKind.Null ? 0M : entval.GetDecimal(),
                    yield.ValueKind == JsonValueKind.Null ? 0D : yield.GetDouble(),
                    dpr.ValueKind == JsonValueKind.Null ? 0D : dpr.GetDouble(),
                    der.ValueKind == JsonValueKind.Null ? 0D : der.GetDouble(),
                    capex.ValueKind == JsonValueKind.Null ? 0M : capex.GetDecimal(),
                    fcf.ValueKind == JsonValueKind.Null ? 0M : fcf.GetDecimal(),
                    roe.ValueKind == JsonValueKind.Null ? 0M : roe.GetDecimal(),
                    beta1.ValueKind == JsonValueKind.Null ? 0D : beta1.GetDouble(),
                    beta3.ValueKind == JsonValueKind.Null ? 0D : beta3.GetDouble(),
                    beta5.ValueKind == JsonValueKind.Null ? 0D : beta5.GetDouble());
            }).OrderBy(k => k.PeriodEndDate)];
    }

    public async Task<MarketCap[]> GetMarketCapAsync(string identifier)
    {
        var paramsDict = new Dictionary<string, string> { { "identifier", identifier } };
        var results = (await GetDataAsync(EndPoints.MarketCap, paramsDict)).ToArray();

        return [.. results.Select(k =>
        {
            return new MarketCap(k.GetProperty("trading_symbol").GetString() ?? "",
                k.GetProperty("central_index_key").GetString() ?? "",
                k.GetProperty("registrant_name").GetString() ?? "",
                k.GetProperty("fiscal_year").GetString() ?? "",
                k.GetProperty("market_cap").GetDecimal(),
                k.GetProperty("change_in_market_cap").GetDecimal(),
                k.GetProperty("percentage_change_in_market_cap").GetDouble(),
                k.GetProperty("shares_outstanding").GetInt64(),
                k.GetProperty("change_in_shares_outstanding").GetInt64(),
                k.GetProperty("percentage_change_in_shares_outstanding").GetDouble());
        }).OrderBy(k => k.FiscalYear)];
    }

    public async Task<EmployeeCount[]> GetEmployeeCountAsync(string identifier)
    {
        var paramsDict = new Dictionary<string, string> { { "identifier", identifier } };
        var counts = (await GetDataAsync(EndPoints.EmployeeCount, paramsDict)).ToArray();

        return [.. counts.Select(k => {
            return new EmployeeCount(k.GetProperty("trading_symbol").GetString() ?? "",
                k.GetProperty("central_index_key").GetString() ?? "",
                k.GetProperty("registrant_name").GetString() ?? "",
                k.GetProperty("fiscal_year").GetString() ?? "",
                k.GetProperty("employee_count").GetInt32());
        }).OrderBy(k => k.FiscalYear)];
    }

    public async Task<ExecutiveCompensation[]> GetExecutiveCompensationAsync(string identifier)
    {
        var paramsDict = new Dictionary<string, string> { { "identifier", identifier } };
        var comps = (await GetDataAsync(EndPoints.ExecutiveCompensation, paramsDict, 100)).ToArray();

        return [.. comps.Select(k => {
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
                salary.ValueKind == JsonValueKind.Null ? 0M : salary.GetDecimal(),
                bonus.ValueKind == JsonValueKind.Null ? 0M : bonus.GetDecimal(),
                awards.ValueKind == JsonValueKind.Null ? 0M : awards.GetDecimal(),
                incentives.ValueKind == JsonValueKind.Null ? 0M : incentives.GetDecimal(),
                other.ValueKind == JsonValueKind.Null ? 0M : other.GetDecimal(),
                total.ValueKind == JsonValueKind.Null ? 0M : total.GetDecimal());
        }).OrderBy(k => k.Name).ThenBy(k => k.FiscalYear)];
    }

    public async Task<SecurityInformation?> GetSecurityInformationAsync(string identifier)
    {
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
        var paramsDict = new Dictionary<string, string> { { "identifier", identifier } };
        if (!string.IsNullOrEmpty(period))
            paramsDict["period"] = period;

        var statements = (await GetDataAsync(EndPoints.IncomeStatements, paramsDict, 50)).ToArray();

        var json = statements.First().GetRawText();

        return [.. statements.Select(k => {
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

               return new IncomeStatement(k.GetProperty("trading_symbol").GetString() ?? "",
                   k.GetProperty("central_index_key").GetString() ?? "",
                   k.GetProperty("registrant_name").GetString() ?? "",
                   k.GetProperty("fiscal_year").GetString() ?? "",
                   k.GetProperty("fiscal_period").GetString() ?? "",
                   DateOnly.FromDateTime(k.GetProperty("period_end_date").GetDateTime()),
                   r.ValueKind == JsonValueKind.Null ? 0M : r.GetDecimal(),
                   cr.ValueKind == JsonValueKind.Null ? 0M : cr.GetDecimal(),
                   gp.ValueKind == JsonValueKind.Null ? 0M : gp.GetDecimal(),
                   rd.ValueKind == JsonValueKind.Null ? 0M : rd.GetDecimal(),
                   gen.ValueKind == JsonValueKind.Null ? 0M : gen.GetDecimal(),
                   opex.ValueKind == JsonValueKind.Null ? 0M : opex.GetDecimal(),
                   opinc.ValueKind == JsonValueKind.Null ? 0M : opinc.GetDecimal(),
                   intex.ValueKind == JsonValueKind.Null ? 0M : intex.GetDecimal(),
                   intinc.ValueKind == JsonValueKind.Null ? 0M : intinc.GetDecimal(),
                   netinc.ValueKind == JsonValueKind.Null ? 0M : netinc.GetDecimal(),
                   epsBasic.ValueKind == JsonValueKind.Null ? 0M : epsBasic.GetDecimal(),
                   epsDil.ValueKind == JsonValueKind.Null ? 0M : epsDil.GetDecimal(),
                   soBasic.ValueKind == JsonValueKind.Null ? 0 : soBasic.GetInt64(),
                   soDil.ValueKind == JsonValueKind.Null ? 0 : soDil.GetInt64());
            }).OrderBy(k => k.FiscalYear).ThenBy(k => k.FiscalPeriod)];

        //return [];
    }

    public async Task<BalanceSheet[]> GetBalanceSheetStatementsAsync(string identifier, string? period = null)
    {
        var paramsDict = new Dictionary<string, string> { { "identifier", identifier } };
        if (!string.IsNullOrEmpty(period))
            paramsDict["period"] = period;

        var statements = (await GetDataAsync(EndPoints.BalanceSheetStatements, paramsDict, 50)).ToArray();

        return [.. statements.Select(k => {
            var cash = k.GetProperty("cash_and_cash_equivalents");
            var msc = k.GetProperty("marketable_securities_current");
            var ar = k.GetProperty("accounts_receivable");
            var inv = k.GetProperty("inventories");
            var ntr = k.GetProperty("accounts_receivable");
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

            return new BalanceSheet(k.GetProperty("trading_symbol").GetString() ?? "",
                k.GetProperty("central_index_key").GetString() ?? "",
                k.GetProperty("registrant_name").GetString() ?? "",
                k.GetProperty("fiscal_year").GetString() ?? "",
                k.GetProperty("fiscal_period").GetString() ?? "",
                DateOnly.FromDateTime(k.GetProperty("period_end_date").GetDateTime()),
                cash.ValueKind == JsonValueKind.Null ? 0M : cash.GetDecimal(),
                msc.ValueKind == JsonValueKind.Null ? 0M : msc.GetDecimal(),
                ar.ValueKind == JsonValueKind.Null ? 0M : ar.GetDecimal(),
                inv.ValueKind == JsonValueKind.Null ? 0M : inv.GetDecimal(),
                ntr.ValueKind == JsonValueKind.Null ? 0M : ntr.GetDecimal(),
                oac.ValueKind == JsonValueKind.Null ? 0M : oac.GetDecimal(),
                tac.ValueKind == JsonValueKind.Null ? 0M : tac.GetDecimal(),
                msnc.ValueKind == JsonValueKind.Null ? 0M : msnc.GetDecimal(),
                ppe.ValueKind == JsonValueKind.Null ? 0M : ppe.GetDecimal(),
                oanc.ValueKind == JsonValueKind.Null ? 0M : oanc.GetDecimal(),
                tanc.ValueKind == JsonValueKind.Null ? 0M : tanc.GetDecimal(),
                ta.ValueKind == JsonValueKind.Null ? 0M : ta.GetDecimal(),
                ap.ValueKind == JsonValueKind.Null ? 0M : ap.GetDecimal(),
                dr.ValueKind == JsonValueKind.Null ? 0M : dr.GetDecimal(),
                std.ValueKind == JsonValueKind.Null ? 0M : std.GetDecimal(),
                olc.ValueKind == JsonValueKind.Null ? 0M : olc.GetDecimal(),
                tlc.ValueKind == JsonValueKind.Null ? 0M : tlc.GetDecimal(),
                ltd.ValueKind == JsonValueKind.Null ? 0M : ltd.GetDecimal(),
                olnc.ValueKind == JsonValueKind.Null ? 0M : olnc.GetDecimal(),
                tlnc.ValueKind == JsonValueKind.Null ? 0M : tlnc.GetDecimal(),
                tl.ValueKind == JsonValueKind.Null ? 0M : tl.GetDecimal(),
                cs.ValueKind == JsonValueKind.Null ? 0M : cs.GetDecimal(),
                re.ValueKind == JsonValueKind.Null ? 0M : re.GetDecimal(),
                aoci.ValueKind == JsonValueKind.Null ? 0M : aoci.GetDecimal(),
                tse.ValueKind == JsonValueKind.Null ? 0M : tse.GetDecimal());
        }).OrderBy(k => k.FiscalYear).ThenBy(k => k.FiscalPeriod)];
    }

    public async Task<CashFlowStatement[]> GetCashFlowStatementsAsync(string identifier, string? period = null)
    {
        var paramsDict = new Dictionary<string, string> { { "identifier", identifier } };
        if (!string.IsNullOrEmpty(period))
            paramsDict["period"] = period;
        var statements = (await GetDataAsync(EndPoints.CashFlowStatements, paramsDict, 50)).ToArray();

        return [.. statements.Select(k => {
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

            return new CashFlowStatement(k.GetProperty("trading_symbol").GetString() ?? "",
                k.GetProperty("central_index_key").GetString() ?? "",
                k.GetProperty("registrant_name").GetString() ?? "",
                k.GetProperty("fiscal_year").GetString() ?? "",
                k.GetProperty("fiscal_period").GetString() ?? "",
                DateOnly.FromDateTime(k.GetProperty("period_end_date").GetDateTime()),
                da.ValueKind == JsonValueKind.Null ? 0M : da.GetDecimal(),
                sbce.ValueKind == JsonValueKind.Null ? 0M : sbce.GetDecimal(),
                dite.ValueKind == JsonValueKind.Null ? 0M : dite.GetDecimal(),
                oncie.ValueKind == JsonValueKind.Null ? 0M : oncie.GetDecimal(),
                ciar.ValueKind == JsonValueKind.Null ? 0M : ciar.GetDecimal(),
                cii.ValueKind == JsonValueKind.Null ? 0M : cii.GetDecimal(),
                cintr.ValueKind == JsonValueKind.Null ? 0M : cintr.GetDecimal(),
                cioa.ValueKind == JsonValueKind.Null ? 0M : cioa.GetDecimal(),
                ciap.ValueKind == JsonValueKind.Null ? 0M : ciap.GetDecimal(),
                cidr.ValueKind == JsonValueKind.Null ? 0M : cidr.GetDecimal(),
                ciol.ValueKind == JsonValueKind.Null ? 0M : ciol.GetDecimal(),
                cfoa.ValueKind == JsonValueKind.Null ? 0M : cfoa.GetDecimal(),
                poms.ValueKind == JsonValueKind.Null ? 0M : poms.GetDecimal(),
                soms.ValueKind == JsonValueKind.Null ? 0M : soms.GetDecimal(),
                aoppe.ValueKind == JsonValueKind.Null ? 0M : aoppe.GetDecimal(),
                aob.ValueKind == JsonValueKind.Null ? 0M : aob.GetDecimal(),
                oia.ValueKind == JsonValueKind.Null ? 0M : oia.GetDecimal(),
                cfia.ValueKind == JsonValueKind.Null ? 0M : cfia.GetDecimal(),
                twfsbc.ValueKind == JsonValueKind.Null ? 0M : twfsbc.GetDecimal(),
                pod.ValueKind == JsonValueKind.Null ? 0M : pod.GetDecimal(),
                iocs.ValueKind == JsonValueKind.Null ? 0M : iocs.GetDecimal(),
                rocs.ValueKind == JsonValueKind.Null ? 0M : rocs.GetDecimal(),
                ioltd.ValueKind == JsonValueKind.Null ? 0M : ioltd.GetDecimal(),
                roltd.ValueKind == JsonValueKind.Null ? 0M : roltd.GetDecimal(),
                ofa.ValueKind == JsonValueKind.Null ? 0M : ofa.GetDecimal(),
                cffa.ValueKind == JsonValueKind.Null ? 0M : cffa.GetDecimal(),
                cic.ValueKind == JsonValueKind.Null ? 0M : cic.GetDecimal(),
                caeop.ValueKind == JsonValueKind.Null ? 0M : caeop.GetDecimal(),
                itp.ValueKind == JsonValueKind.Null ? 0M : itp.GetDecimal(),
                ip.ValueKind == JsonValueKind.Null ? 0M : ip.GetDecimal());
        }).OrderBy(k => k.FiscalYear).ThenBy(k => k.FiscalPeriod)];
    }

    // Dividends Endpoints
    public async Task<Dividend[]> GetDividendsAsync(string identifier)
    {
        var paramsDict = new Dictionary<string, string> { { "identifier", identifier } };
        var divs = (await GetDataAsync(EndPoints.Dividends, paramsDict, 300)).ToArray();

        return [.. divs.Select(k => {
            var amt = k.GetProperty("amount");

            return new Dividend(k.GetProperty("trading_symbol").GetString() ?? "",
                k.GetProperty("registrant_name").GetString() ?? "",
                k.GetProperty("type").GetString() ?? "",
                amt.ValueKind == JsonValueKind.Null ? 0M : amt.GetDecimal(),
                DateOnly.FromDateTime(k.GetProperty("declaration_date").GetDateTime()),
                DateOnly.FromDateTime(k.GetProperty("ex_date").GetDateTime()),
                DateOnly.FromDateTime(k.GetProperty("record_date").GetDateTime()),
                DateOnly.FromDateTime(k.GetProperty("payment_date").GetDateTime()));
        }).OrderBy(k => k.DeclarationDate)];
    }

    // Short Interest Endpoints
    public async Task<ShortInterest[]> GetShortInterestAsync(string identifier)
    {
        var paramsDict = new Dictionary<string, string> { { "identifier", identifier } };
        var shorts = (await GetDataAsync(EndPoints.ShortInterest, paramsDict, 300)).ToArray();

        return [.. shorts.Select(k => {

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
                ss.ValueKind == JsonValueKind.Null ? 0L : ss.GetInt64(),
                pss.ValueKind == JsonValueKind.Null ? 0L : pss.GetInt64(),
                css.ValueKind == JsonValueKind.Null ? 0L : css.GetInt64(),
                pcss.ValueKind == JsonValueKind.Null ? 0D : pcss.GetDouble(),
                adv.ValueKind == JsonValueKind.Null ? 0L : adv.GetInt64(),
                dtc.ValueKind == JsonValueKind.Null ? 0D : dtc.GetDouble(),
                k.GetProperty("is_stock_split").GetBoolean());
        }).OrderBy(k => k.SettlementDate)];
    }

    private static EodPrice[] ConvertToEodPrices(JsonElement[] elements)
    {
        if ((elements?.Length ?? 0) == 0)
            return [];

        return [.. elements!.Select(k =>
        {
            var open = k.GetProperty("open");
            var high = k.GetProperty("high");
            var low = k.GetProperty("low");
            var close = k.GetProperty("close");
            var volume = k.GetProperty("volume");

            return new EodPrice(k.GetProperty("trading_symbol").GetString() ?? throw new Exception("Trading symbol is null"),
                DateOnly.FromDateTime(k.GetProperty("date").GetDateTime()),
                open.ValueKind == JsonValueKind.Null ? 0M : open.GetDecimal(),
                high.ValueKind == JsonValueKind.Null ? 0M : high.GetDecimal(),
                low.ValueKind == JsonValueKind.Null ? 0M : low.GetDecimal(),
                close.ValueKind == JsonValueKind.Null ? 0M : close.GetDecimal(),
                volume.ValueKind == JsonValueKind.Null ? 0D : volume.GetDouble());
        }).OrderBy(k => k.Date)];
    }

    private static MinutePrice[] ConvertToMinutePrices(JsonElement[] elements)
    {
        if ((elements?.Length ?? 0) == 0)
            return [];

        return [.. elements!.Select(k =>
        {
            var open = k.GetProperty("open");
            var high = k.GetProperty("high");
            var low = k.GetProperty("low");
            var close = k.GetProperty("close");
            var volume = k.GetProperty("volume");

            return new MinutePrice(k.GetProperty("trading_symbol").GetString() ?? throw new Exception("Trading symbol is null"),
                DateTime.Parse(k.GetProperty("time").GetString() ?? ""),
                open.ValueKind == JsonValueKind.Null ? 0M : open.GetDecimal(),
                high.ValueKind == JsonValueKind.Null ? 0M : high.GetDecimal(),
                low.ValueKind == JsonValueKind.Null ? 0M : low.GetDecimal(),
                close.ValueKind == JsonValueKind.Null ? 0M : close.GetDecimal(),
                volume.ValueKind == JsonValueKind.Null ? 0D : volume.GetDouble());
        }).OrderBy(k => k.DateTime)];
    }
}

public readonly record struct Stock(string Symbol, string Registrant, bool IsInternational = false);

public readonly record struct ExchangeTradedFund(string Symbol, string Description);

public readonly record struct Commodity(string Symbol, string Description);

public readonly record struct OverTheCounter(string Symbol, string Title);

public readonly record struct Index(string Symbol, string Name);

public readonly record struct Future(string Symbol, string Description, string Type);

public readonly record struct Crypto(string Symbol, string BaseAsset, string QuoteAsset);

public readonly record struct EodPrice(string Symbol, DateOnly Date, decimal Open,
    decimal High, decimal Low, decimal Close, double Volume);

public readonly record struct MinutePrice(string Symbol, DateTime DateTime, decimal Open,
    decimal High, decimal Low, decimal Close, double Volume);

public readonly record struct OptionChain(string Symbol,
    string CentralIndexKey, string Registrant, string ContractName,
    DateOnly Expiration, string Type, decimal StrikePrice);

public readonly record struct OptionPrice(string ContractName,
    DateOnly Date, decimal Open, decimal High, decimal Low, decimal Close, double Volume);

public readonly record struct OptionGreeks(string ContractName,
    DateOnly Date, double Delta, double Gamma, double Theta, double Vega, double Rho);

public readonly record struct CryptoInformation(string Symbol, string Name,
    double MarketCap, double FullyDilutedValuation, double TotalSupply,
    double MaxSupply, double CirculationSupply, decimal HighestPrice,
    DateOnly DateHighestPrice, decimal LowestPrice, DateOnly DateLowestPrice,
    string HashFunction, string BlockTime, DateOnly LedgerStartDate,
    string WebSite, string Description);


public readonly record struct CompanyInformation(string Symbol,
    string CentralIndexKey, string Registrant, string Isin,
    string Lei, string Ein, string Exchange, string SicCode,
    string SicDescription, string FiscalYearEnd, string StateOfIncorporation, string PhoneNumber,
    string MailingAddress, string BusinessAddress, string? FormerName,
    string Industry, DateOnly DateFounding, string ChiefExecutiveOfficer,
    int NumberEmployees, string WebSite, double MarketCap,
    int? SharesIssued, long SharesOutstanding, string Description);

public readonly record struct InternationalCompanyInformation(string Symbol,
    string Registrant, string Exchange, string Isin, string Inudstry,
    string YearFounding, string ChiefExecutiveOfficer,
    int NumberEmployees, string WebSite, string Description);

public readonly record struct KeyMetrics(string Symbol,
    string CentralIndexKey, string Registrant, string FiscalYear,
    DateOnly PeriodEndDate, decimal EarningsPerShare, decimal EarningsPerShareForecast,
    double PriceToEarningsRatio, double FowardPriceToEarningsRatio,
    double EarningsGrowthRate, double PriceEarningsToGrowthRate,
    decimal BookValuePerShare, double PriceToBookRatio,
    double Ebitda, decimal EnterpriseValue, double DividendYield,
    double DividendPayoutRatio, double DebtToEquityRatio,
    decimal CapitalExpenditures, decimal FreeCashFlow, decimal ReturnOnEquity,
    double OneYearBeta, double ThreeYearBeta, double FiveYearBeta);

public readonly record struct MarketCap(string Symbol, string CentralIndexKey,
    string Registrant, string FiscalYear, decimal Value,
    decimal ChangeInMarketCap, double PercentageChangeInMarketCap,
    long SharesOutstanding, long ChangeInSharesOutstanding,
    double PercentageChangeInSharesOutstanding);

public readonly record struct EmployeeCount(string Symbol, string CentralIndexKey,
    string Registrant, string FiscalYear, int Count);

public readonly record struct ExecutiveCompensation(string Symbol, string CentralIndexKey,
    string Registrant, string Name, string Position, string FiscalYear,
    decimal Salary, decimal Bonus, decimal StockAwards,
    decimal IncentivePlanCompensation, decimal OtherCompensation,
    decimal TotalCompensation);

public readonly record struct SecurityInformation(string Symbol,
    string Issuer, string Cusip, string Isin, string Figi, string Type);

public readonly record struct IncomeStatement(string Symbol, string CentralIndexKey,
    string Registrant, string FiscalYear, string FiscalPeriod, DateOnly PeriodEndDate,
    decimal Revenue, decimal CostOfRevenue, decimal GrossProfit,
    decimal ResearchDevelopmentExpenses, decimal GeneralAdminExpenses,
    decimal OperatingExpenses, decimal OperatingIncome, decimal InterestExpense,
    decimal InterestIncome, decimal NetIncome, decimal EarningsPerShareBasic,
    decimal EarningsPerShareDiluted, long WeightedAverageSharesOutstandingBasic,
    long WeightedAverageSharesOutstandingDiluted);

public readonly record struct BalanceSheet(string Symbol, string CentralIndexKey,
    string Registrant, string FiscalYear, string FiscalPeriod,
    DateOnly PeriodEndDate, decimal Cash, decimal MarketableSecuritiesCurrent,
    decimal AccountsReceivable, decimal Inventories,
    decimal NonTradeReceivables, decimal OtherAssetsCurrent, decimal TotalAssetsCurrent,
    decimal MarketableSecuritiesNonCurrent, decimal PropertyPlants,
    decimal OtherAssetsNonCurrent, decimal TotalAsetsNonCurrent, decimal TotalAssets,
    decimal AccountsPayable, decimal DeferredRevenue,
    decimal ShortTermDebt, decimal OtherLiabilitiesCurrent,
    decimal TotalLiabilitiesCurrent, decimal LongTermDebt,
    decimal OtherLiabilitiesNonCurrent, decimal TotalLiabilitiesNonCurrent,
    decimal TotalLiabilities, decimal CommonStock, decimal RetainedEarnings,
    decimal AccumulatedOtherComprehensiveIncome, decimal TotalShareholdrsEquity);

public readonly record struct CashFlowStatement(string Symbol, string CentralIndexKey,
    string Registrant, string FiscalYear, string FiscalPeriod, DateOnly PeriodEndDate,
    decimal Depreciation, decimal ShareBasedCompensationExpense,
    decimal DeferredIncomeTaxExpense, decimal OtherNonCashIncomeExpense,
    decimal ChangeInAccountsReceivable, decimal ChangeInInventories,
    decimal ChangeInNonTradeReceivables, decimal ChangeInOtherAssets,
    decimal ChangeInAccountsPayable, decimal ChangeInDeferredRevenue,
    decimal ChangeInOtherLiabilities, decimal CashFromOperatingActivities,
    decimal PurchasesOfMarketableSecurities, decimal SalesOfMarketableSecurities,
    decimal AcquisitionOfProperty, decimal AcquisitionOfBusiness,
    decimal OtherInvestingActivities, decimal CashFromInvestingActivities,
    decimal TaxWithholdingforShareBasedCompensation, decimal PaymentsOfDividends,
    decimal IssuanceOfCommonStock, decimal RepurchaseOfCommonStock,
    decimal IssuanceOfLongTermDebt, decimal RepaymentOfLongTermDebt,
    decimal OtherFinancingActivities, decimal CashFromFinancingActivities,
    decimal ChangeInCash, decimal CashAtEndOfPeriod,
    decimal IncomeTaxesPaid, decimal InterestPaid);

public readonly record struct Dividend(string Symbol, string Registrant,
    string Type, decimal Amount, DateOnly DeclarationDate, DateOnly ExDate,
    DateOnly RecordDate, DateOnly PaymentDate);

public readonly record struct ShortInterest(string Symbol, string Title, string MarketCode,
    DateOnly SettlementDate, long ShortedSecurities, long PreviousShortedSecurities,
    long ChangeInShortedSecurities, double PercentageChangeInShortedSecurities,
    long AverageDailyVolume, double DaysToConvert, bool IsStockSplit);
