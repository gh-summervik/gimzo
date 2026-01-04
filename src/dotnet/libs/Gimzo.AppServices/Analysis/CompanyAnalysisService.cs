using Dapper;
using Gimzo.Analysis.Fundamental;
using Gimzo.Infrastructure.Database;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Npgsql;
using System.Collections.Frozen;
using System.Collections.Immutable;

namespace Gimzo.AppServices.Analysis;

public readonly record struct CompanyScore(string Symbol, int Score, int Percentile);

public sealed class CompanyAnalysisService(
    DbDefPair dbDefPair,
    IMemoryCache memoryCache,
    ILogger<CompanyAnalysisService> logger) : ServiceBase(dbDefPair, memoryCache)
{
    private readonly ILogger<CompanyAnalysisService> _logger = logger;

    private readonly FrozenDictionary<string, double> _siloWeights = new Dictionary<string, double>
    {
        ["BalanceSheet"] = 0.20,
        ["IncomeStatement"] = 0.15,
        ["CashFlow"] = 0.10,
        ["Earnings"] = 0.10,
        ["Valuation"] = 0.10,
        ["Efficiency"] = 0.10,
        ["Growth"] = 0.15,
        ["RiskSentiment"] = 0.10
    }.ToFrozenDictionary();

    public async Task<CompanyScore?> GetSiloScoreForSymbolAsync(string symbol)
    {
        LiquidityRatios? liquidity = await GetLatestLiquidityRatiosAsync(symbol);
        SolvencyRatios? solvency = await GetLatestSolvencyRatiosAsync(symbol);
        ProfitabilityRatios? profitability = await GetLatestProfitabilityRatiosAsync(symbol);
        KeyMetrics? keyMetrics = await GetLatestKeyMetricsAsync(symbol);
        ValuationRatios? valuation = await GetLatestValuationRatiosAsync(symbol);
        EfficiencyRatios? efficiency = await GetLatestEfficiencyRatiosAsync(symbol);
        ShortInterest? shortInterest = await GetLatestShortInterestAsync(symbol);
        IReadOnlyCollection<QuarterlyGrowthInput> growthInputs = await GetLatestFiveQuarterlyGrowthInputsAsync(symbol);

        if (liquidity is null || solvency is null || profitability is null || keyMetrics is null || efficiency is null)
            return null;

        BalanceSheetAssessment balance = BalanceSheetHealthAnalyzer.Assess(liquidity, solvency);
        IncomeStatementAssessment income = IncomeStatementHealthAnalyzer.Assess(profitability, efficiency);
        CashFlowAssessment cashFlow = CashFlowHealthAnalyzer.Assess(profitability);
        EarningsAssessment earnings = EarningsHealthAnalyzer.Assess(keyMetrics);
        ValuationAssessment val = ValuationHealthAnalyzer.Assess(keyMetrics, valuation);
        EfficiencyAssessment eff = EfficiencyHealthAnalyzer.Assess(efficiency);
        GrowthAssessment growth = GrowthHealthAnalyzer.Assess(growthInputs);

        double? latestDaysToCover = shortInterest?.DaysToCover;
        RiskSentimentAssessment risk = RiskSentimentHealthAnalyzer.Assess(keyMetrics, latestDaysToCover);

        return new CompanyScore(symbol,
            ComputeAbsoluteScore(balance, income, cashFlow, earnings, val, eff, risk, growth), 0);
    }

    public async Task<IReadOnlyCollection<CompanyScore>> GetAllSiloScoresAsync()
    {
        var liquidityTask = GetAllLatestLiquidityRatiosAsync();
        var solvencyTask = GetAllLatestSolvencyRatiosAsync();
        var profitabilityTask = GetAllLatestProfitabilityRatiosAsync();
        var keyMetricsTask = GetAllLatestKeyMetricsAsync();
        var valuationTask = GetAllLatestValuationRatiosAsync();
        var efficiencyTask = GetAllLatestEfficiencyRatiosAsync();
        var shortInterestTask = GetAllLatestShortInterestAsync();
        var growthTask = GetAllLatestFiveQuarterlyGrowthInputsAsync();

        await Task.WhenAll(
            liquidityTask,
            solvencyTask,
            profitabilityTask,
            keyMetricsTask,
            valuationTask,
            efficiencyTask,
            shortInterestTask,
            growthTask);

        var liquidity = liquidityTask.Result.ToFrozenDictionary(r => r.Symbol!);
        var solvency = solvencyTask.Result.ToFrozenDictionary(r => r.Symbol!);
        var profitability = profitabilityTask.Result.ToFrozenDictionary(r => r.Symbol!);
        var keyMetrics = keyMetricsTask.Result.ToFrozenDictionary(r => r.Symbol!);
        var valuation = valuationTask.Result.ToFrozenDictionary(r => r!.Symbol!, r => r);
        var efficiency = efficiencyTask.Result.ToFrozenDictionary(r => r.Symbol!);
        var shortInterest = shortInterestTask.Result.ToFrozenDictionary(r => r.Symbol!, r => r);
        var growthGroups = growthTask.Result.GroupBy(g => g.Symbol!).ToFrozenDictionary(g => g.Key, g => g.ToImmutableArray());

        var symbols = liquidity.Keys
            .Intersect(solvency.Keys)
            .Intersect(profitability.Keys)
            .Intersect(keyMetrics.Keys)
            .Intersect(efficiency.Keys).ToImmutableArray();

        var absoluteResults = new List<(string Symbol, int Absolute)>(symbols.Length);

        foreach (var symbol in symbols)
        {
            var l = liquidity[symbol];
            var s = solvency[symbol];
            var p = profitability[symbol];
            var k = keyMetrics[symbol];
            var v = valuation.GetValueOrDefault(symbol);
            var e = efficiency[symbol];
            var si = shortInterest.GetValueOrDefault(symbol);
            var gi = growthGroups.GetValueOrDefault(symbol, ImmutableArray<QuarterlyGrowthInput>.Empty);

            BalanceSheetAssessment balance = BalanceSheetHealthAnalyzer.Assess(l, s);
            IncomeStatementAssessment income = IncomeStatementHealthAnalyzer.Assess(p, e);
            CashFlowAssessment cashFlow = CashFlowHealthAnalyzer.Assess(p);
            EarningsAssessment earnings = EarningsHealthAnalyzer.Assess(k);
            ValuationAssessment val = ValuationHealthAnalyzer.Assess(k, v);
            EfficiencyAssessment eff = EfficiencyHealthAnalyzer.Assess(e);
            GrowthAssessment growth = GrowthHealthAnalyzer.Assess(gi);

            double? daysToCover = si?.DaysToCover;
            RiskSentimentAssessment risk = RiskSentimentHealthAnalyzer.Assess(k, daysToCover);

            int absolute = ComputeAbsoluteScore(balance, income, cashFlow, earnings, val, eff, risk, growth);

            absoluteResults.Add((symbol, absolute));
        }

        var sorted = absoluteResults.OrderByDescending(r => r.Absolute).ToArray();

        var results = new List<CompanyScore>(sorted.Length);

        for (int i = 0; i < sorted.Length; i++)
        {
            double percentile = 99 - (i * 98.0 / (sorted.Length - 1));
            int percentileScore = (int)Math.Round(percentile);

            results.Add(new(sorted[i].Symbol, sorted[i].Absolute, percentileScore));
        }

        return results.OrderByDescending(k => k.Percentile).ToImmutableArray();
    }

    internal async Task<LiquidityRatios?> GetLatestLiquidityRatiosAsync(string symbol)
    {
        string sql = $@"{SqlRepository.SelectLiquidityRatios}
WHERE symbol = @Symbol AND fiscal_period IN ('Q1','Q2','Q3','Q4')
ORDER BY period_end_date DESC LIMIT 1";

        using var queryCtx = _dbDefPair.GetQueryConnection();
        var result = await queryCtx.QueryFirstOrDefaultAsync<Infrastructure.Database.DataAccessObjects.LiquidityRatios>(sql,
            new { Symbol = symbol });

        return result?.ToDomain();
    }

    internal async Task<SolvencyRatios?> GetLatestSolvencyRatiosAsync(string symbol)
    {
        string sql = $@"{SqlRepository.SelectSolvencyRatios}
WHERE symbol = @Symbol AND fiscal_period IN ('Q1','Q2','Q3','Q4')
ORDER BY period_end_date DESC LIMIT 1";

        using var queryCtx = _dbDefPair.GetQueryConnection();
        var result = await queryCtx.QueryFirstOrDefaultAsync<Infrastructure.Database.DataAccessObjects.SolvencyRatios>(sql,
            new { Symbol = symbol });

        return result?.ToDomain();
    }

    internal async Task<ProfitabilityRatios?> GetLatestProfitabilityRatiosAsync(string symbol)
    {
        string sql = $@"{SqlRepository.SelectProfitabilityRatios}
WHERE symbol = @Symbol AND fiscal_period IN ('Q1','Q2','Q3','Q4')
ORDER BY period_end_date DESC LIMIT 1";

        using var queryCtx = _dbDefPair.GetQueryConnection();
        var result = await queryCtx.QueryFirstOrDefaultAsync<Infrastructure.Database.DataAccessObjects.ProfitabilityRatios>(sql,
            new { Symbol = symbol });

        return result?.ToDomain();
    }

    internal async Task<KeyMetrics?> GetLatestKeyMetricsAsync(string symbol)
    {
        string sql = $@"{SqlRepository.SelectKeyMetrics}
WHERE symbol = @Symbol ORDER BY period_end_date DESC LIMIT 1";

        using var queryCtx = _dbDefPair.GetQueryConnection();
        var result = await queryCtx.QueryFirstOrDefaultAsync<Infrastructure.Database.DataAccessObjects.KeyMetrics>(sql,
            new { Symbol = symbol });

        return result?.ToDomain();
    }

    internal async Task<ValuationRatios?> GetLatestValuationRatiosAsync(string symbol)
    {
        string sql = $@"{SqlRepository.SelectValuationRatios}
WHERE symbol = @Symbol AND fiscal_period IN ('Q1','Q2','Q3','Q4')
ORDER BY period_end_date DESC LIMIT 1";

        using var queryCtx = _dbDefPair.GetQueryConnection();
        var result = await queryCtx.QueryFirstOrDefaultAsync<Infrastructure.Database.DataAccessObjects.ValuationRatios>(sql,
            new { Symbol = symbol });

        return result?.ToDomain();
    }

    internal async Task<EfficiencyRatios?> GetLatestEfficiencyRatiosAsync(string symbol)
    {
        string sql = $@"{SqlRepository.SelectEfficiencyRatios}
WHERE symbol = @Symbol AND fiscal_period IN ('Q1','Q2','Q3','Q4')
ORDER BY period_end_date DESC LIMIT 1";

        using var queryCtx = _dbDefPair.GetQueryConnection();
        var result = await queryCtx.QueryFirstOrDefaultAsync<Infrastructure.Database.DataAccessObjects.EfficiencyRatios>(sql,
            new { Symbol = symbol });

        return result?.ToDomain();
    }

    internal async Task<ShortInterest?> GetLatestShortInterestAsync(string symbol)
    {
        string sql = $@"{SqlRepository.SelectShortInterests}
WHERE symbol = @Symbol
ORDER BY settlement_date DESC LIMIT 1";

        using var queryCtx = _dbDefPair.GetQueryConnection();
        var result = await queryCtx.QueryFirstOrDefaultAsync<Infrastructure.Database.DataAccessObjects.ShortInterest>(sql,
            new { Symbol = symbol });

        return result?.ToDomain();
    }

    internal async Task<IReadOnlyCollection<QuarterlyGrowthInput>> GetLatestFiveQuarterlyGrowthInputsAsync(string symbol)
    {
        const string Sql = @"
SELECT
    period_end_date AS PeriodEndDate,
    revenue AS Revenue,
    earnings_per_share_diluted AS EarningsPerShareDiluted
FROM public.income_statements
WHERE symbol = @Symbol
    AND fiscal_period IN ('Q1','Q2','Q3','Q4')
ORDER BY period_end_date DESC
LIMIT 5";

        using var queryCtx = _dbDefPair.GetQueryConnection();
        var daos = await queryCtx.QueryAsync<Infrastructure.Database.DataAccessObjects.QuarterlyGrowthInput>(Sql,
            new { Symbol = symbol.ToUpperInvariant() });

        return daos.Select(d => d.ToDomain()).ToImmutableArray();
    }

    private async Task<IReadOnlyCollection<LiquidityRatios>> GetAllLatestLiquidityRatiosAsync()
    {
        const string Sql = @"
    WITH ranked AS (
        SELECT
            symbol,
            central_index_key,
            registrant,
            fiscal_year,
            fiscal_period,
            period_end_date,
            working_capital,
            current_ratio,
            cash_ratio,
            quick_ratio,
            days_of_inventory_outstanding,
            days_of_sales_outstanding,
            days_payable_outstanding,
            cash_conversion_cycle,
            sales_to_working_capital_ratio,
            cash_to_current_liabilities_ratio,
            working_capital_to_debt_ratio,
            cash_flow_adequacy_ratio,
            sales_to_current_assets_ratio,
            cash_to_current_assets_ratio,
            cash_to_working_capital_ratio,
            inventory_to_working_capital_ratio,
            net_debt,
            ROW_NUMBER() OVER (PARTITION BY symbol ORDER BY period_end_date DESC) AS rn
        FROM public.liquidity_ratios
        WHERE fiscal_period IN ('Q1','Q2','Q3','Q4')
    )
    SELECT
        symbol,
        central_index_key,
        registrant,
        fiscal_year,
        fiscal_period,
        period_end_date,
        working_capital,
        current_ratio,
        cash_ratio,
        quick_ratio,
        days_of_inventory_outstanding,
        days_of_sales_outstanding,
        days_payable_outstanding,
        cash_conversion_cycle,
        sales_to_working_capital_ratio,
        cash_to_current_liabilities_ratio,
        working_capital_to_debt_ratio,
        cash_flow_adequacy_ratio,
        sales_to_current_assets_ratio,
        cash_to_current_assets_ratio,
        cash_to_working_capital_ratio,
        inventory_to_working_capital_ratio,
        net_debt
    FROM ranked
    WHERE rn = 1";

        await using var ds = NpgsqlDataSource.Create(_dbDefPair.Query.ConnectionString);
        await using var connection = await ds.OpenConnectionAsync();
        await using var command = new NpgsqlCommand(Sql, connection);
        await using var reader = await command.ExecuteReaderAsync();

        var daos = new List<Infrastructure.Database.DataAccessObjects.LiquidityRatios>(2000);

        while (await reader.ReadAsync())
        {
            daos.Add(new Infrastructure.Database.DataAccessObjects.LiquidityRatios
            {
                Symbol = reader.GetString(0),
                CentralIndexKey = reader.GetString(1),
                Registrant = reader.GetString(2),
                FiscalYear = reader.GetString(3),
                FiscalPeriod = reader.GetString(4),
                PeriodEndDate = reader.IsDBNull(5) ? null : DateOnly.FromDateTime(reader.GetDateTime(5)),
                WorkingCapital = reader.IsDBNull(6) ? null : reader.GetDecimal(6),
                CurrentRatio = reader.IsDBNull(7) ? null : reader.GetDouble(7),
                CashRatio = reader.IsDBNull(8) ? null : reader.GetDouble(8),
                QuickRatio = reader.IsDBNull(9) ? null : reader.GetDouble(9),
                DaysOfInventoryOutstanding = reader.IsDBNull(10) ? null : reader.GetDouble(10),
                DaysOfSalesOutstanding = reader.IsDBNull(11) ? null : reader.GetDouble(11),
                DaysPayableOutstanding = reader.IsDBNull(12) ? null : reader.GetDouble(12),
                CashConversionCycle = reader.IsDBNull(13) ? null : reader.GetDouble(13),
                SalesToWorkingCapitalRatio = reader.IsDBNull(14) ? null : reader.GetDouble(14),
                CashToCurrentLiabilitiesRatio = reader.IsDBNull(15) ? null : reader.GetDouble(15),
                WorkingCapitalToDebtRatio = reader.IsDBNull(16) ? null : reader.GetDouble(16),
                CashFlowAdequacyRatio = reader.IsDBNull(17) ? null : reader.GetDouble(17),
                SalesToCurrentAssetsRatio = reader.IsDBNull(18) ? null : reader.GetDouble(18),
                CashToCurrentAssetsRatio = reader.IsDBNull(19) ? null : reader.GetDouble(19),
                CashToWorkingCapitalRatio = reader.IsDBNull(20) ? null : reader.GetDouble(20),
                InventoryToWorkingCapitalRatio = reader.IsDBNull(21) ? null : reader.GetDouble(21),
                NetDebt = reader.IsDBNull(22) ? null : reader.GetDecimal(22)
            });
        }

        return daos.Select(d => d.ToDomain()).ToImmutableArray();
    }

    private async Task<IReadOnlyCollection<SolvencyRatios>> GetAllLatestSolvencyRatiosAsync()
    {
        const string Sql = @"
    WITH ranked AS (
        SELECT
            symbol,
            central_index_key,
            registrant,
            fiscal_year,
            fiscal_period,
            period_end_date,
            equity_ratio,
            debt_coverage_ratio,
            asset_coverage_ratio,
            interest_coverage_ratio,
            debt_to_equity_ratio,
            debt_to_assets_ratio,
            debt_to_capital_ratio,
            debt_to_income_ratio,
            cash_flow_to_debt_ratio,
            ROW_NUMBER() OVER (PARTITION BY symbol ORDER BY period_end_date DESC) AS rn
        FROM public.solvency_ratios
        WHERE fiscal_period IN ('Q1','Q2','Q3','Q4')
    )
    SELECT
        symbol,
        central_index_key,
        registrant,
        fiscal_year,
        fiscal_period,
        period_end_date,
        equity_ratio,
        debt_coverage_ratio,
        asset_coverage_ratio,
        interest_coverage_ratio,
        debt_to_equity_ratio,
        debt_to_assets_ratio,
        debt_to_capital_ratio,
        debt_to_income_ratio,
        cash_flow_to_debt_ratio
    FROM ranked
    WHERE rn = 1";

        await using var ds = NpgsqlDataSource.Create(_dbDefPair.Query.ConnectionString);
        await using var connection = await ds.OpenConnectionAsync();
        await using var command = new NpgsqlCommand(Sql, connection);
        await using var reader = await command.ExecuteReaderAsync();

        var daos = new List<Infrastructure.Database.DataAccessObjects.SolvencyRatios>(2000);

        while (await reader.ReadAsync())
        {
            daos.Add(new Infrastructure.Database.DataAccessObjects.SolvencyRatios
            {
                Symbol = reader.GetString(0),
                CentralIndexKey = reader.GetString(1),
                Registrant = reader.GetString(2),
                FiscalYear = reader.GetString(3),
                FiscalPeriod = reader.GetString(4),
                PeriodEndDate = reader.IsDBNull(5) ? null : DateOnly.FromDateTime(reader.GetDateTime(5)),
                EquityRatio = reader.IsDBNull(6) ? null : reader.GetDouble(6),
                DebtCoverageRatio = reader.IsDBNull(7) ? null : reader.GetDouble(7),
                AssetCoverageRatio = reader.IsDBNull(8) ? null : reader.GetDouble(8),
                InterestCoverageRatio = reader.IsDBNull(9) ? null : reader.GetDouble(9),
                DebtToEquityRatio = reader.IsDBNull(10) ? null : reader.GetDouble(10),
                DebtToAssetsRatio = reader.IsDBNull(11) ? null : reader.GetDouble(11),
                DebtToCapitalRatio = reader.IsDBNull(12) ? null : reader.GetDouble(12),
                DebtToIncomeRatio = reader.IsDBNull(13) ? null : reader.GetDouble(13),
                CashFlowToDebtRatio = reader.IsDBNull(14) ? null : reader.GetDouble(14)
            });
        }

        return daos.Select(d => d.ToDomain()).ToImmutableArray();
    }

    private async Task<IReadOnlyCollection<ProfitabilityRatios>> GetAllLatestProfitabilityRatiosAsync()
    {
        const string Sql = @"
    WITH ranked AS (
        SELECT
            symbol,
            central_index_key,
            registrant,
            fiscal_year,
            fiscal_period,
            period_end_date,
            ebit,
            ebitda,
            profit_margin,
            gross_margin,
            operating_margin,
            operating_cash_flow_margin,
            return_on_equity,
            return_on_assets,
            return_on_debt,
            cash_return_on_assets,
            cash_turnover_ratio,
            ROW_NUMBER() OVER (PARTITION BY symbol ORDER BY period_end_date DESC) AS rn
        FROM public.profitability_ratios
        WHERE fiscal_period IN ('Q1','Q2','Q3','Q4')
    )
    SELECT
        symbol,
        central_index_key,
        registrant,
        fiscal_year,
        fiscal_period,
        period_end_date,
        ebit,
        ebitda,
        profit_margin,
        gross_margin,
        operating_margin,
        operating_cash_flow_margin,
        return_on_equity,
        return_on_assets,
        return_on_debt,
        cash_return_on_assets,
        cash_turnover_ratio
    FROM ranked
    WHERE rn = 1";

        await using var ds = NpgsqlDataSource.Create(_dbDefPair.Query.ConnectionString);
        await using var connection = await ds.OpenConnectionAsync();
        await using var command = new NpgsqlCommand(Sql, connection);
        await using var reader = await command.ExecuteReaderAsync();

        var daos = new List<Infrastructure.Database.DataAccessObjects.ProfitabilityRatios>(2000);

        while (await reader.ReadAsync())
        {
            daos.Add(new Infrastructure.Database.DataAccessObjects.ProfitabilityRatios
            {
                Symbol = reader.GetString(0),
                CentralIndexKey = reader.GetString(1),
                Registrant = reader.GetString(2),
                FiscalYear = reader.GetString(3),
                FiscalPeriod = reader.GetString(4),
                PeriodEndDate = reader.IsDBNull(5) ? null : DateOnly.FromDateTime(reader.GetDateTime(5)),
                Ebit = reader.IsDBNull(6) ? null : reader.GetDecimal(6),
                Ebitda = reader.IsDBNull(7) ? null : reader.GetDecimal(7),
                ProfitMargin = reader.IsDBNull(8) ? null : reader.GetDouble(8),
                GrossMargin = reader.IsDBNull(9) ? null : reader.GetDouble(9),
                OperatingMargin = reader.IsDBNull(10) ? null : reader.GetDouble(10),
                OperatingCashFlowMargin = reader.IsDBNull(11) ? null : reader.GetDouble(11),
                ReturnOnEquity = reader.IsDBNull(12) ? null : reader.GetDouble(12),
                ReturnOnAssets = reader.IsDBNull(13) ? null : reader.GetDouble(13),
                ReturnOnDebt = reader.IsDBNull(14) ? null : reader.GetDouble(14),
                CashReturnOnAssets = reader.IsDBNull(15) ? null : reader.GetDouble(15),
                CashTurnoverRatio = reader.IsDBNull(16) ? null : reader.GetDouble(16)
            });
        }

        return daos.Select(d => d.ToDomain()).ToImmutableArray();
    }

    private async Task<IReadOnlyCollection<KeyMetrics>> GetAllLatestKeyMetricsAsync()
    {
        const string Sql = @"
    WITH ranked AS (
        SELECT
            symbol,
            central_index_key,
            registrant,
            fiscal_year,
            period_end_date,
            earnings_per_share,
            earnings_per_share_forecast,
            price_to_earnings_ratio,
            forward_price_to_earnings_ratio,
            earnings_growth_rate,
            price_earnings_to_growth_rate,
            book_value_per_share,
            price_to_book_ratio,
            ebitda,
            enterprise_value,
            dividend_yield,
            dividend_payout_ratio,
            debt_to_equity_ratio,
            capital_expenditures,
            free_cash_flow,
            return_on_equity,
            one_year_beta,
            three_year_beta,
            five_year_beta,
            ROW_NUMBER() OVER (PARTITION BY symbol ORDER BY period_end_date DESC) AS rn
        FROM public.key_metrics
    )
    SELECT
        symbol,
        central_index_key,
        registrant,
        fiscal_year,
        period_end_date,
        earnings_per_share,
        earnings_per_share_forecast,
        price_to_earnings_ratio,
        forward_price_to_earnings_ratio,
        earnings_growth_rate,
        price_earnings_to_growth_rate,
        book_value_per_share,
        price_to_book_ratio,
        ebitda,
        enterprise_value,
        dividend_yield,
        dividend_payout_ratio,
        debt_to_equity_ratio,
        capital_expenditures,
        free_cash_flow,
        return_on_equity,
        one_year_beta,
        three_year_beta,
        five_year_beta
    FROM ranked
    WHERE rn = 1";

        await using var ds = NpgsqlDataSource.Create(_dbDefPair.Query.ConnectionString);
        await using var connection = await ds.OpenConnectionAsync();
        await using var command = new NpgsqlCommand(Sql, connection);
        await using var reader = await command.ExecuteReaderAsync();

        var daos = new List<Infrastructure.Database.DataAccessObjects.KeyMetrics>(2000);

        while (await reader.ReadAsync())
        {
            daos.Add(new Infrastructure.Database.DataAccessObjects.KeyMetrics
            {
                Symbol = reader.GetString(0),
                CentralIndexKey = reader.GetString(1),
                Registrant = reader.GetString(2),
                FiscalYear = reader.GetString(3),
                PeriodEndDate = reader.IsDBNull(4) ? null : DateOnly.FromDateTime(reader.GetDateTime(4)),
                EarningsPerShare = reader.IsDBNull(5) ? null : reader.GetDecimal(5),
                EarningsPerShareForecast = reader.IsDBNull(6) ? null : reader.GetDecimal(6),
                PriceToEarningsRatio = reader.IsDBNull(7) ? null : reader.GetDouble(7),
                ForwardPriceToEarningsRatio = reader.IsDBNull(8) ? null : reader.GetDouble(8),
                EarningsGrowthRate = reader.IsDBNull(9) ? null : reader.GetDouble(9),
                PriceEarningsToGrowthRate = reader.IsDBNull(10) ? null : reader.GetDouble(10),
                BookValuePerShare = reader.IsDBNull(11) ? null : reader.GetDecimal(11),
                PriceToBookRatio = reader.IsDBNull(12) ? null : reader.GetDouble(12),
                Ebitda = reader.IsDBNull(13) ? null : reader.GetDecimal(13),
                EnterpriseValue = reader.IsDBNull(14) ? null : reader.GetDecimal(14),
                DividendYield = reader.IsDBNull(15) ? null : reader.GetDouble(15),
                DividendPayoutRatio = reader.IsDBNull(16) ? null : reader.GetDouble(16),
                DebtToEquityRatio = reader.IsDBNull(17) ? null : reader.GetDouble(17),
                CapitalExpenditures = reader.IsDBNull(18) ? null : reader.GetDecimal(18),
                FreeCashFlow = reader.IsDBNull(19) ? null : reader.GetDecimal(19),
                ReturnOnEquity = reader.IsDBNull(20) ? null : reader.GetDouble(20),
                OneYearBeta = reader.IsDBNull(21) ? null : reader.GetDouble(21),
                ThreeYearBeta = reader.IsDBNull(22) ? null : reader.GetDouble(22),
                FiveYearBeta = reader.IsDBNull(23) ? null : reader.GetDouble(23)
            });
        }

        return daos.Select(d => d.ToDomain()).ToImmutableArray();
    }

    private async Task<IReadOnlyCollection<ValuationRatios?>> GetAllLatestValuationRatiosAsync()
    {
        const string Sql = @"
    WITH ranked AS (
        SELECT
            symbol,
            central_index_key,
            registrant,
            fiscal_year,
            fiscal_period,
            period_end_date,
            dividends_per_share,
            dividend_payout_ratio,
            book_value_per_share,
            retention_ratio,
            net_fixed_assets,
            ROW_NUMBER() OVER (PARTITION BY symbol ORDER BY period_end_date DESC) AS rn
        FROM public.valuation_ratios
        WHERE fiscal_period IN ('Q1','Q2','Q3','Q4')
    )
    SELECT
        symbol,
        central_index_key,
        registrant,
        fiscal_year,
        fiscal_period,
        period_end_date,
        dividends_per_share,
        dividend_payout_ratio,
        book_value_per_share,
        retention_ratio,
        net_fixed_assets
    FROM ranked
    WHERE rn = 1";

        await using var ds = NpgsqlDataSource.Create(_dbDefPair.Query.ConnectionString);
        await using var connection = await ds.OpenConnectionAsync();
        await using var command = new NpgsqlCommand(Sql, connection);
        await using var reader = await command.ExecuteReaderAsync();

        var daos = new List<Infrastructure.Database.DataAccessObjects.ValuationRatios?>(2000);

        while (await reader.ReadAsync())
        {
            daos.Add(new Infrastructure.Database.DataAccessObjects.ValuationRatios
            {
                Symbol = reader.GetString(0),
                CentralIndexKey = reader.GetString(1),
                Registrant = reader.GetString(2),
                FiscalYear = reader.GetString(3),
                FiscalPeriod = reader.GetString(4),
                PeriodEndDate = reader.IsDBNull(5) ? null : DateOnly.FromDateTime(reader.GetDateTime(5)),
                DividendsPerShare = reader.IsDBNull(6) ? null : reader.GetDecimal(6),
                DividendPayoutRatio = reader.IsDBNull(7) ? null : reader.GetDouble(7),
                BookValuePerShare = reader.IsDBNull(8) ? null : reader.GetDecimal(8),
                RetentionRatio = reader.IsDBNull(9) ? null : reader.GetDouble(9),
                NetFixedAssets = reader.IsDBNull(10) ? null : reader.GetDecimal(10)
            });
        }

        return daos.Select(d => d?.ToDomain()).ToImmutableArray();
    }

    private async Task<IReadOnlyCollection<EfficiencyRatios>> GetAllLatestEfficiencyRatiosAsync()
    {
        const string Sql = @"
    WITH ranked AS (
        SELECT
            symbol,
            central_index_key,
            registrant,
            fiscal_year,
            fiscal_period,
            period_end_date,
            asset_turnover_ratio,
            inventory_turnover_ratio,
            accounts_receivable_turnover_ratio,
            accounts_payable_turnover_ratio,
            equity_multiplier,
            days_sales_in_inventory,
            fixed_asset_turnover_ratio,
            days_working_capital,
            working_capital_turnover_ratio,
            days_cash_on_hand,
            capital_intensity_ratio,
            sales_to_equity_ratio,
            inventory_to_sales_ratio,
            investment_turnover_ratio,
            sales_to_operating_income_ratio,
            ROW_NUMBER() OVER (PARTITION BY symbol ORDER BY period_end_date DESC) AS rn
        FROM public.efficiency_ratios
        WHERE fiscal_period IN ('Q1','Q2','Q3','Q4')
    )
    SELECT
        symbol,
        central_index_key,
        registrant,
        fiscal_year,
        fiscal_period,
        period_end_date,
        asset_turnover_ratio,
        inventory_turnover_ratio,
        accounts_receivable_turnover_ratio,
        accounts_payable_turnover_ratio,
        equity_multiplier,
        days_sales_in_inventory,
        fixed_asset_turnover_ratio,
        days_working_capital,
        working_capital_turnover_ratio,
        days_cash_on_hand,
        capital_intensity_ratio,
        sales_to_equity_ratio,
        inventory_to_sales_ratio,
        investment_turnover_ratio,
        sales_to_operating_income_ratio
    FROM ranked
    WHERE rn = 1";

        await using var ds = NpgsqlDataSource.Create(_dbDefPair.Query.ConnectionString);
        await using var connection = await ds.OpenConnectionAsync();
        await using var command = new NpgsqlCommand(Sql, connection);
        await using var reader = await command.ExecuteReaderAsync();

        var daos = new List<Infrastructure.Database.DataAccessObjects.EfficiencyRatios>(2000);

        while (await reader.ReadAsync())
        {
            daos.Add(new Infrastructure.Database.DataAccessObjects.EfficiencyRatios
            {
                Symbol = reader.GetString(0),
                CentralIndexKey = reader.GetString(1),
                Registrant = reader.GetString(2),
                FiscalYear = reader.GetString(3),
                FiscalPeriod = reader.GetString(4),
                PeriodEndDate = reader.IsDBNull(5) ? null : DateOnly.FromDateTime(reader.GetDateTime(5)),
                AssetTurnoverRatio = reader.IsDBNull(6) ? null : reader.GetDouble(6),
                InventoryTurnoverRatio = reader.IsDBNull(7) ? null : reader.GetDouble(7),
                AccountsReceivableTurnoverRatio = reader.IsDBNull(8) ? null : reader.GetDouble(8),
                AccountsPayableTurnoverRatio = reader.IsDBNull(9) ? null : reader.GetDouble(9),
                EquityMultiplier = reader.IsDBNull(10) ? null : reader.GetDouble(10),
                DaysSalesInInventory = reader.IsDBNull(11) ? null : reader.GetDouble(11),
                FixedAssetTurnoverRatio = reader.IsDBNull(12) ? null : reader.GetDouble(12),
                DaysWorkingCapital = reader.IsDBNull(13) ? null : reader.GetDouble(13),
                WorkingCapitalTurnoverRatio = reader.IsDBNull(14) ? null : reader.GetDouble(14),
                DaysCashOnHand = reader.IsDBNull(15) ? null : reader.GetDouble(15),
                CapitalIntensityRatio = reader.IsDBNull(16) ? null : reader.GetDouble(16),
                SalesToEquityRatio = reader.IsDBNull(17) ? null : reader.GetDouble(17),
                InventoryToSalesRatio = reader.IsDBNull(18) ? null : reader.GetDouble(18),
                InvestmentTurnoverRatio = reader.IsDBNull(19) ? null : reader.GetDouble(19),
                SalesToOperatingIncomeRatio = reader.IsDBNull(20) ? null : reader.GetDouble(20)
            });
        }

        return daos.Select(d => d.ToDomain()).ToImmutableArray();
    }

    private async Task<IReadOnlyCollection<ShortInterest>> GetAllLatestShortInterestAsync()
    {
        const string Sql = @"
    WITH ranked AS (
        SELECT
            symbol,
            settlement_date,
            days_to_cover,
            ROW_NUMBER() OVER (PARTITION BY symbol ORDER BY settlement_date DESC) AS rn
        FROM public.short_interests
    )
    SELECT
        symbol,
        settlement_date,
        days_to_cover
    FROM ranked
    WHERE rn = 1";

        await using var ds = NpgsqlDataSource.Create(_dbDefPair.Query.ConnectionString);
        await using var connection = await ds.OpenConnectionAsync();
        await using var command = new NpgsqlCommand(Sql, connection);
        await using var reader = await command.ExecuteReaderAsync();

        var daos = new List<Infrastructure.Database.DataAccessObjects.ShortInterest>(2000);

        while (await reader.ReadAsync())
        {
            daos.Add(new Infrastructure.Database.DataAccessObjects.ShortInterest
            {
                Symbol = reader.GetString(0),
                SettlementDate = DateOnly.FromDateTime(reader.GetDateTime(1)),
                DaysToCover = reader.IsDBNull(2) ? null : reader.GetDouble(2)
            });
        }

        return daos.Select(d => d.ToDomain()).ToImmutableArray();
    }

    private async Task<IReadOnlyCollection<QuarterlyGrowthInput>> GetAllLatestFiveQuarterlyGrowthInputsAsync()
    {
        const string Sql = @"
    WITH ranked AS (
        SELECT
            symbol,
            period_end_date,
            revenue,
            earnings_per_share_diluted,
            ROW_NUMBER() OVER (PARTITION BY symbol ORDER BY period_end_date DESC) AS rn
        FROM public.income_statements
        WHERE fiscal_period IN ('Q1','Q2','Q3','Q4')
    )
    SELECT
        symbol,
        period_end_date,
        revenue,
        earnings_per_share_diluted
    FROM ranked
    WHERE rn <= 5";

        await using var ds = NpgsqlDataSource.Create(_dbDefPair.Query.ConnectionString);
        await using var connection = await ds.OpenConnectionAsync();
        await using var command = new NpgsqlCommand(Sql, connection);
        await using var reader = await command.ExecuteReaderAsync();

        var daos = new List<Infrastructure.Database.DataAccessObjects.QuarterlyGrowthInput>(10000);

        while (await reader.ReadAsync())
        {
            daos.Add(new Infrastructure.Database.DataAccessObjects.QuarterlyGrowthInput
            {
                Symbol = reader.GetString(0),
                PeriodEndDate = reader.IsDBNull(1) ? null : DateOnly.FromDateTime(reader.GetDateTime(1)),
                Revenue = reader.IsDBNull(2) ? null : reader.GetDecimal(2),
                EarningsPerShareDiluted = reader.IsDBNull(3) ? null : reader.GetDecimal(3)
            });
        }

        return daos.Select(d => d.ToDomain()).ToImmutableArray();
    }

    private async Task<IReadOnlyDictionary<string, string?>> GetAllSicCodesAsync()
    {
        const string Sql = "SELECT symbol, sic_code FROM public.us_companies WHERE sic_code IS NOT NULL";

        using var queryCtx = _dbDefPair.GetQueryConnection();
        var rows = await queryCtx.QueryAsync<dynamic>(Sql);

        return rows.ToFrozenDictionary(r => (string)r.symbol, r => (string?)r.sic_code);
    }

    private int ComputeAbsoluteScore(
        BalanceSheetAssessment balanceSheet,
        IncomeStatementAssessment income,
        CashFlowAssessment cashFlow,
        EarningsAssessment earnings,
        ValuationAssessment valuation,
        EfficiencyAssessment efficiency,
        RiskSentimentAssessment riskSentiment,
        GrowthAssessment growth)
    {
        var subScores = new Dictionary<string, int>
        {
            ["BalanceSheet"] = balanceSheet.Score,
            ["IncomeStatement"] = income.Score,
            ["CashFlow"] = cashFlow.Score,
            ["Earnings"] = earnings.Score,
            ["Valuation"] = valuation.Score,
            ["Efficiency"] = efficiency.Score,
            ["RiskSentiment"] = riskSentiment.Score,
            ["Growth"] = growth.Score
        };

        double weightedSum = 0.0;
        double totalWeight = 0.0;

        foreach (var kvp in _siloWeights)
        {
            if (subScores.TryGetValue(kvp.Key, out int subScore))
            {
                weightedSum += subScore * kvp.Value;
                totalWeight += kvp.Value;
            }
        }

        double composite = totalWeight > 0 ? weightedSum / totalWeight : 0.0;
        return (int)Math.Round(composite);
    }
}


//public async Task<IReadOnlyCollection<CompanyScore2>> GetAllSiloScores2Async()
//{
//    var liquidityTask = GetAllLatestLiquidityRatiosAsync();
//    var solvencyTask = GetAllLatestSolvencyRatiosAsync();
//    var profitabilityTask = GetAllLatestProfitabilityRatiosAsync();
//    var keyMetricsTask = GetAllLatestKeyMetricsAsync();
//    var valuationTask = GetAllLatestValuationRatiosAsync();
//    var efficiencyTask = GetAllLatestEfficiencyRatiosAsync();
//    var shortInterestTask = GetAllLatestShortInterestAsync();
//    var growthTask = GetAllLatestFiveQuarterlyGrowthInputsAsync();

//    await Task.WhenAll(
//        liquidityTask,
//        solvencyTask,
//        profitabilityTask,
//        keyMetricsTask,
//        valuationTask,
//        efficiencyTask,
//        shortInterestTask,
//        growthTask);

//    var liquidity = liquidityTask.Result.ToFrozenDictionary(r => r.Symbol!);
//    var solvency = solvencyTask.Result.ToFrozenDictionary(r => r.Symbol!);
//    var profitability = profitabilityTask.Result.ToFrozenDictionary(r => r.Symbol!);
//    var keyMetrics = keyMetricsTask.Result.ToFrozenDictionary(r => r.Symbol!);
//    var valuation = valuationTask.Result.ToFrozenDictionary(r => r!.Symbol!, r => r);
//    var efficiency = efficiencyTask.Result.ToFrozenDictionary(r => r.Symbol!);
//    var shortInterest = shortInterestTask.Result.ToFrozenDictionary(r => r.Symbol!, r => r);
//    var growthGroups = growthTask.Result.GroupBy(g => g.Symbol!).ToFrozenDictionary(g => g.Key, g => g.ToImmutableArray());

//    var symbols = liquidity.Keys
//        .Intersect(solvency.Keys)
//        .Intersect(profitability.Keys)
//        .Intersect(keyMetrics.Keys)
//        .Intersect(efficiency.Keys).ToImmutableArray();

//    var absoluteResults = new List<(string Symbol, int Absolute)>(symbols.Length);

//    foreach (var symbol in symbols)
//    {
//        var l = liquidity[symbol];
//        var s = solvency[symbol];
//        var p = profitability[symbol];
//        var k = keyMetrics[symbol];
//        var v = valuation.GetValueOrDefault(symbol);
//        var e = efficiency[symbol];
//        var si = shortInterest.GetValueOrDefault(symbol);
//        var gi = growthGroups.GetValueOrDefault(symbol, ImmutableArray<QuarterlyGrowthInput>.Empty);

//        BalanceSheetAssessment balance = BalanceSheetHealthAnalyzer.Assess(l, s);
//        IncomeStatementAssessment income = IncomeStatementHealthAnalyzer.Assess(p, e);
//        CashFlowAssessment cashFlow = CashFlowHealthAnalyzer.Assess(p);
//        EarningsAssessment earnings = EarningsHealthAnalyzer.Assess(k);
//        ValuationAssessment val = ValuationHealthAnalyzer.Assess(k, v);
//        EfficiencyAssessment eff = EfficiencyHealthAnalyzer.Assess(e);
//        GrowthAssessment growth = GrowthHealthAnalyzer.Assess(gi);

//        double? daysToCover = si?.DaysToCover;
//        RiskSentimentAssessment risk = RiskSentimentHealthAnalyzer.Assess(k, daysToCover);

//        int absolute = ComputeAbsoluteScore(balance, income, cashFlow, earnings, val, eff, risk, growth);

//        absoluteResults.Add((symbol, absolute));
//    }

//    var sorted = absoluteResults.OrderByDescending(r => r.Absolute).ToArray();

//    var results = new List<CompanyScore2>(sorted.Length);

//    for (int i = 0; i < sorted.Length; i++)
//    {
//        double percentile = 99 - (i * 98.0 / (sorted.Length - 1));
//        int percentileScore = (int)Math.Round(percentile);

//        results.Add(new(sorted[i].Symbol, sorted[i].Absolute, 0, percentileScore));
//    }

//    return results.OrderByDescending(k => k.BlendedScore).ToImmutableArray();
//}

//public async Task<IReadOnlyCollection<CompanyScore2>> GetAllSiloScores2Async()
//{
//    var liquidityTask = GetAllLatestLiquidityRatiosAsync();
//    var solvencyTask = GetAllLatestSolvencyRatiosAsync();
//    var profitabilityTask = GetAllLatestProfitabilityRatiosAsync();
//    var keyMetricsTask = GetAllLatestKeyMetricsAsync();
//    var valuationTask = GetAllLatestValuationRatiosAsync();
//    var efficiencyTask = GetAllLatestEfficiencyRatiosAsync();
//    var shortInterestTask = GetAllLatestShortInterestAsync();
//    var growthTask = GetAllLatestFiveQuarterlyGrowthInputsAsync();
//    var sicTask = GetAllSicCodesAsync();

//    await Task.WhenAll(
//        liquidityTask,
//        solvencyTask,
//        profitabilityTask,
//        keyMetricsTask,
//        valuationTask,
//        efficiencyTask,
//        shortInterestTask,
//        growthTask,
//        sicTask);

//    var liquidity = liquidityTask.Result.ToFrozenDictionary(r => r.Symbol!);
//    var solvency = solvencyTask.Result.ToFrozenDictionary(r => r.Symbol!);
//    var profitability = profitabilityTask.Result.ToFrozenDictionary(r => r.Symbol!);
//    var keyMetrics = keyMetricsTask.Result.ToFrozenDictionary(r => r.Symbol!);
//    var valuation = valuationTask.Result.ToFrozenDictionary(r => r!.Symbol!, r => r);
//    var efficiency = efficiencyTask.Result.ToFrozenDictionary(r => r.Symbol!);
//    var shortInterest = shortInterestTask.Result.ToFrozenDictionary(r => r.Symbol!, r => r);
//    var growthGroups = growthTask.Result.GroupBy(g => g.Symbol!).ToFrozenDictionary(g => g.Key, g => g.ToImmutableArray());
//    var sicCodes = sicTask.Result;

//    var symbols = liquidity.Keys
//        .Intersect(solvency.Keys)
//        .Intersect(profitability.Keys)
//        .Intersect(keyMetrics.Keys)
//        .Intersect(efficiency.Keys).ToImmutableArray();

//    var absoluteResults = new List<(string Symbol, int Absolute)>(symbols.Length);

//    foreach (var symbol in symbols)
//    {
//        var l = liquidity[symbol];
//        var s = solvency[symbol];
//        var p = profitability[symbol];
//        var k = keyMetrics[symbol];
//        var v = valuation.GetValueOrDefault(symbol);
//        var e = efficiency[symbol];
//        var si = shortInterest.GetValueOrDefault(symbol);
//        var gi = growthGroups.GetValueOrDefault(symbol, ImmutableArray<QuarterlyGrowthInput>.Empty);

//        BalanceSheetAssessment balance = BalanceSheetHealthAnalyzer.Assess(l, s);
//        IncomeStatementAssessment income = IncomeStatementHealthAnalyzer.Assess(p, e);
//        CashFlowAssessment cashFlow = CashFlowHealthAnalyzer.Assess(p);
//        EarningsAssessment earnings = EarningsHealthAnalyzer.Assess(k);
//        ValuationAssessment val = ValuationHealthAnalyzer.Assess(k, v);
//        EfficiencyAssessment eff = EfficiencyHealthAnalyzer.Assess(e);
//        GrowthAssessment growth = GrowthHealthAnalyzer.Assess(gi);

//        double? daysToCover = si?.DaysToCover;
//        RiskSentimentAssessment risk = RiskSentimentHealthAnalyzer.Assess(k, daysToCover);

//        int absolute = ComputeAbsoluteScore(balance, income, cashFlow, earnings, val, eff, risk, growth);

//        absoluteResults.Add((symbol, absolute));
//    }

//    var groups = absoluteResults
//        .GroupBy(r => sicCodes.GetValueOrDefault(r.Symbol) is string sic && sic.Length == 4 ? sic[..2] : "Unknown")
//        .ToFrozenDictionary(g => g.Key, g => g.ToImmutableArray());

//    var results = new List<CompanyScore2>(absoluteResults.Count);

//    const int minGroupSize = 20;

//    foreach (var group in groups.Values)
//    {
//        var sorted = group.OrderByDescending(r => r.Absolute).ToArray();

//        bool largeGroup = sorted.Length >= minGroupSize;

//        for (int i = 0; i < sorted.Length; i++)
//        {
//            int relative = largeGroup
//                ? (int)Math.Round((i + 1.0) / sorted.Length * 98 + 1)
//                : 50;

//            int blended = (int)Math.Round(sorted[i].Absolute * 0.7 + relative * 0.3);

//            results.Add(new(sorted[i].Symbol, sorted[i].Absolute, relative, blended));
//        }
//    }

//    return [..results.OrderByDescending(r => r.BlendedScore)];
//}