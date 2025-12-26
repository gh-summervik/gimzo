CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

DROP TABLE IF EXISTS public.stock_symbols CASCADE;
DROP TABLE IF EXISTS public.etf_symbols CASCADE;
DROP TABLE IF EXISTS public.commodity_symbols CASCADE;
DROP TABLE IF EXISTS public.otc_symbols CASCADE;
DROP TABLE IF EXISTS public.crypto_symbols CASCADE;
DROP TABLE IF EXISTS public.index_symbols CASCADE;
DROP TABLE IF EXISTS public.eod_prices CASCADE;
DROP TABLE IF EXISTS public.us_companies CASCADE;
DROP TABLE IF EXISTS public.international_companies CASCADE;
DROP TABLE IF EXISTS public.security_information CASCADE;
DROP TABLE IF EXISTS public.liquidity_ratios CASCADE;
DROP TABLE IF EXISTS public.profitability_ratios CASCADE;
DROP TABLE IF EXISTS public.solvency_ratios CASCADE;
DROP TABLE IF EXISTS public.valuation_ratios CASCADE;
DROP TABLE IF EXISTS public.key_metrics CASCADE;
DROP TABLE IF EXISTS public.market_caps CASCADE;
DROP TABLE IF EXISTS public.employee_counts CASCADE;
DROP TABLE IF EXISTS public.executive_compensations CASCADE;
DROP TABLE IF EXISTS public.income_statements CASCADE;
DROP TABLE IF EXISTS public.balance_sheets CASCADE;
DROP TABLE IF EXISTS public.cash_flow_statements CASCADE;
DROP TABLE IF EXISTS public.stock_splits CASCADE;
DROP TABLE IF EXISTS public.dividends CASCADE;
DROP TABLE IF EXISTS public.earnings_releases CASCADE;
DROP TABLE IF EXISTS public.efficiency_ratios CASCADE;
DROP TABLE IF EXISTS public.short_interests CASCADE;

CREATE TABLE IF NOT EXISTS public.stock_symbols(
    symbol TEXT NOT NULL,
    registrant TEXT,
    created_by UUID NOT NULL,
    updated_by UUID NOT NULL,
    created_at TIMESTAMPTZ NOT NULL,
    updated_at TIMESTAMPTZ NOT NULL,
    created_at_unix_ms BIGINT NOT NULL,
    updated_at_unix_ms BIGINT NOT NULL,
    PRIMARY KEY (symbol)
);

CREATE TABLE IF NOT EXISTS public.etf_symbols(
    symbol TEXT NOT NULL,
    description TEXT,
    created_by UUID NOT NULL,
    updated_by UUID NOT NULL,
    created_at TIMESTAMPTZ NOT NULL,
    updated_at TIMESTAMPTZ NOT NULL,
    created_at_unix_ms BIGINT NOT NULL,
    updated_at_unix_ms BIGINT NOT NULL,
    PRIMARY KEY (symbol)
);

CREATE TABLE IF NOT EXISTS public.commodity_symbols(
    symbol TEXT NOT NULL,
    description TEXT,
    created_by UUID NOT NULL,
    updated_by UUID NOT NULL,
    created_at TIMESTAMPTZ NOT NULL,
    updated_at TIMESTAMPTZ NOT NULL,
    created_at_unix_ms BIGINT NOT NULL,
    updated_at_unix_ms BIGINT NOT NULL,
    PRIMARY KEY (symbol)
);

CREATE TABLE IF NOT EXISTS public.otc_symbols(
    symbol TEXT NOT NULL,
    title TEXT,
    created_by UUID NOT NULL,
    updated_by UUID NOT NULL,
    created_at TIMESTAMPTZ NOT NULL,
    updated_at TIMESTAMPTZ NOT NULL,
    created_at_unix_ms BIGINT NOT NULL,
    updated_at_unix_ms BIGINT NOT NULL,
    PRIMARY KEY (symbol)
);

CREATE TABLE IF NOT EXISTS public.crypto_symbols(
    symbol TEXT NOT NULL,
    base_asset TEXT,
    quote_asset TEXT,
    created_by UUID NOT NULL,
    updated_by UUID NOT NULL,
    created_at TIMESTAMPTZ NOT NULL,
    updated_at TIMESTAMPTZ NOT NULL,
    created_at_unix_ms BIGINT NOT NULL,
    updated_at_unix_ms BIGINT NOT NULL,
    PRIMARY KEY (symbol)
);

CREATE TABLE IF NOT EXISTS public.index_symbols(
    symbol TEXT NOT NULL,
    index_name TEXT,
    created_by UUID NOT NULL,
    updated_by UUID NOT NULL,
    created_at TIMESTAMPTZ NOT NULL,
    updated_at TIMESTAMPTZ NOT NULL,
    created_at_unix_ms BIGINT NOT NULL,
    updated_at_unix_ms BIGINT NOT NULL,
    PRIMARY KEY (symbol)
);

CREATE TABLE IF NOT EXISTS public.security_information (
    symbol TEXT NOT NULL,
    issuer TEXT,
    cusip TEXT,
    isin TEXT,
    figi TEXT,
    type TEXT,
    created_by UUID NOT NULL,
    updated_by UUID NOT NULL,
    created_at TIMESTAMPTZ NOT NULL,
    updated_at TIMESTAMPTZ NOT NULL,
    created_at_unix_ms BIGINT NOT NULL,
    updated_at_unix_ms BIGINT NOT NULL,
    PRIMARY KEY (symbol)
);

CREATE TABLE IF NOT EXISTS public.eod_prices (
    symbol TEXT NOT NULL,
    security_type TEXT NOT NULL,
    date_eod DATE NOT NULL,
    open NUMERIC(22,4) NOT NULL,
    high NUMERIC(22,4) NOT NULL,
    low NUMERIC(22,4) NOT NULL,
    close NUMERIC(22,4) NOT NULL,
    volume DOUBLE PRECISION NOT NULL,
    created_by UUID NOT NULL,
    updated_by UUID NOT NULL,
    created_at TIMESTAMPTZ NOT NULL,
    updated_at TIMESTAMPTZ NOT NULL,
    created_at_unix_ms BIGINT NOT NULL,
    updated_at_unix_ms BIGINT NOT NULL,
    PRIMARY KEY (symbol, security_type, date_eod)
);

CREATE TABLE IF NOT EXISTS public.us_companies (
    central_index_key TEXT NOT NULL,
    exchange TEXT NOT NULL,
    symbol TEXT NOT NULL,
    registrant TEXT,
    isin TEXT,
    lei TEXT,
    ein TEXT,
    sic_code TEXT,
    sic_description TEXT,
    fiscal_year_end TEXT,
    state_of_incorporation TEXT,
    phone_number TEXT,
    mailing_address TEXT,
    business_address TEXT,
    former_name TEXT,
    industry TEXT,
    date_founding TEXT,
    chief_executive_officer TEXT,
    number_employees INTEGER,
    web_site TEXT,
    market_cap DOUBLE PRECISION,
    shares_issued DOUBLE PRECISION,
    shares_outstanding DOUBLE PRECISION,
    description TEXT,
    created_by UUID NOT NULL,
    updated_by UUID NOT NULL,
    created_at TIMESTAMPTZ NOT NULL,
    updated_at TIMESTAMPTZ NOT NULL,
    created_at_unix_ms BIGINT NOT NULL,
    updated_at_unix_ms BIGINT NOT NULL,
    PRIMARY KEY (central_index_key, exchange, symbol)
);

CREATE TABLE IF NOT EXISTS public.international_companies (
    symbol TEXT NOT NULL,
    registrant TEXT NOT NULL,
    exchange TEXT NOT NULL,
    isin TEXT,
    industry TEXT,
    year_founding TEXT,
    chief_executive_officer TEXT,
    number_employees INTEGER,
    web_site TEXT,
    description TEXT,
    created_by UUID NOT NULL,
    updated_by UUID NOT NULL,
    created_at TIMESTAMPTZ NOT NULL,
    updated_at TIMESTAMPTZ NOT NULL,
    created_at_unix_ms BIGINT NOT NULL,
    updated_at_unix_ms BIGINT NOT NULL,
    PRIMARY KEY (symbol, exchange)
);

CREATE TABLE IF NOT EXISTS public.liquidity_ratios (
    symbol TEXT NOT NULL,
    central_index_key TEXT NOT NULL,
    registrant TEXT NOT NULL,
    fiscal_year TEXT NOT NULL,
    fiscal_period TEXT NOT NULL,
    period_end_date DATE NOT NULL,
    working_capital NUMERIC(22,4),
    current_ratio DOUBLE PRECISION,
    cash_ratio DOUBLE PRECISION,
    quick_ratio DOUBLE PRECISION,
    days_of_inventory_outstanding DOUBLE PRECISION,
    days_of_sales_outstanding DOUBLE PRECISION,
    days_payable_outstanding DOUBLE PRECISION,
    cash_conversion_cycle DOUBLE PRECISION,
    sales_to_working_capital_ratio DOUBLE PRECISION,
    cash_to_current_liabilities_ratio DOUBLE PRECISION,
    working_capital_to_debt_ratio DOUBLE PRECISION,
    cash_flow_adequacy_ratio DOUBLE PRECISION,
    sales_to_current_assets_ratio DOUBLE PRECISION,
    cash_to_current_assets_ratio DOUBLE PRECISION,
    cash_to_working_capital_ratio DOUBLE PRECISION,
    inventory_to_working_capital_ratio DOUBLE PRECISION,
    net_debt NUMERIC(22,4),
    created_by UUID NOT NULL,
    updated_by UUID NOT NULL,
    created_at TIMESTAMPTZ NOT NULL,
    updated_at TIMESTAMPTZ NOT NULL,
    created_at_unix_ms BIGINT NOT NULL,
    updated_at_unix_ms BIGINT NOT NULL,
    PRIMARY KEY (central_index_key, fiscal_year, fiscal_period)
);

CREATE TABLE IF NOT EXISTS public.profitability_ratios (
    symbol TEXT NOT NULL,
    central_index_key TEXT NOT NULL,
    registrant TEXT NOT NULL,
    fiscal_year TEXT NOT NULL,
    fiscal_period TEXT NOT NULL,
    period_end_date DATE NOT NULL,
    ebit NUMERIC(22,4),
    ebitda NUMERIC(22,4),
    profit_margin DOUBLE PRECISION,
    gross_margin DOUBLE PRECISION,
    operating_margin DOUBLE PRECISION,
    operating_cash_flow_margin DOUBLE PRECISION,
    return_on_equity DOUBLE PRECISION,
    return_on_assets DOUBLE PRECISION,
    return_on_debt DOUBLE PRECISION,
    cash_return_on_assets DOUBLE PRECISION,
    cash_turnover_ratio DOUBLE PRECISION,
    created_by UUID NOT NULL,
    updated_by UUID NOT NULL,
    created_at TIMESTAMPTZ NOT NULL,
    updated_at TIMESTAMPTZ NOT NULL,
    created_at_unix_ms BIGINT NOT NULL,
    updated_at_unix_ms BIGINT NOT NULL,
    PRIMARY KEY (central_index_key, fiscal_year, fiscal_period)
);

CREATE TABLE IF NOT EXISTS public.solvency_ratios (
    symbol TEXT NOT NULL,
    central_index_key TEXT NOT NULL,
    registrant TEXT NOT NULL,
    fiscal_year TEXT NOT NULL,
    fiscal_period TEXT NOT NULL,
    period_end_date DATE NOT NULL,
    equity_ratio DOUBLE PRECISION,
    debt_coverage_ratio DOUBLE PRECISION,
    asset_coverage_ratio DOUBLE PRECISION,
    interest_coverage_ratio DOUBLE PRECISION,
    debt_to_equity_ratio DOUBLE PRECISION,
    debt_to_assets_ratio DOUBLE PRECISION,
    debt_to_capital_ratio DOUBLE PRECISION,
    debt_to_income_ratio DOUBLE PRECISION,
    cash_flow_to_debt_ratio DOUBLE PRECISION,
    created_by UUID NOT NULL,
    updated_by UUID NOT NULL,
    created_at TIMESTAMPTZ NOT NULL,
    updated_at TIMESTAMPTZ NOT NULL,
    created_at_unix_ms BIGINT NOT NULL,
    updated_at_unix_ms BIGINT NOT NULL,
    PRIMARY KEY (central_index_key, fiscal_year, fiscal_period)
);

CREATE TABLE IF NOT EXISTS public.valuation_ratios (
    symbol TEXT NOT NULL,
    central_index_key TEXT NOT NULL,
    registrant TEXT NOT NULL,
    fiscal_year TEXT NOT NULL,
    fiscal_period TEXT NOT NULL,
    period_end_date DATE NOT NULL,
    dividends_per_share NUMERIC(22,4),
    dividend_payout_ratio DOUBLE PRECISION,
    book_value_per_share NUMERIC(22,4),
    retention_ratio DOUBLE PRECISION,
    net_fixed_assets NUMERIC(22,4),
    created_by UUID NOT NULL,
    updated_by UUID NOT NULL,
    created_at TIMESTAMPTZ NOT NULL,
    updated_at TIMESTAMPTZ NOT NULL,
    created_at_unix_ms BIGINT NOT NULL,
    updated_at_unix_ms BIGINT NOT NULL,
    PRIMARY KEY (central_index_key, fiscal_year, fiscal_period)
);

CREATE TABLE IF NOT EXISTS public.key_metrics (
    symbol TEXT NOT NULL,
    central_index_key TEXT NOT NULL,
    registrant TEXT NOT NULL,
    fiscal_year TEXT NOT NULL,
    period_end_date DATE NOT NULL,
    earnings_per_share NUMERIC(22,4),
    earnings_per_share_forecast NUMERIC(22,4),
    price_to_earnings_ratio DOUBLE PRECISION,
    forward_price_to_earnings_ratio DOUBLE PRECISION,
    earnings_growth_rate DOUBLE PRECISION,
    price_earnings_to_growth_rate DOUBLE PRECISION,
    book_value_per_share NUMERIC(22,4),
    price_to_book_ratio DOUBLE PRECISION,
    ebitda DOUBLE PRECISION,
    enterprise_value NUMERIC(22,4),
    dividend_yield DOUBLE PRECISION,
    dividend_payout_ratio DOUBLE PRECISION,
    debt_to_equity_ratio DOUBLE PRECISION,
    capital_expenditures NUMERIC(22,4),
    free_cash_flow NUMERIC(22,4),
    return_on_equity NUMERIC(22,4),
    one_year_beta DOUBLE PRECISION,
    three_year_beta DOUBLE PRECISION,
    five_year_beta DOUBLE PRECISION,
    created_by UUID NOT NULL,
    updated_by UUID NOT NULL,
    created_at TIMESTAMPTZ NOT NULL,
    updated_at TIMESTAMPTZ NOT NULL,
    created_at_unix_ms BIGINT NOT NULL,
    updated_at_unix_ms BIGINT NOT NULL,
    PRIMARY KEY (central_index_key, fiscal_year)
);

CREATE TABLE IF NOT EXISTS public.market_caps (
    symbol TEXT NOT NULL,
    central_index_key TEXT NOT NULL,
    registrant TEXT NOT NULL,
    fiscal_year TEXT NOT NULL,
    value NUMERIC(22,4),
    change_in_market_cap NUMERIC(22,4),
    percentage_change_in_market_cap DOUBLE PRECISION,
    shares_outstanding BIGINT,
    change_in_shares_outstanding BIGINT,
    percentage_change_in_shares_outstanding DOUBLE PRECISION,
    created_by UUID NOT NULL,
    updated_by UUID NOT NULL,
    created_at TIMESTAMPTZ NOT NULL,
    updated_at TIMESTAMPTZ NOT NULL,
    created_at_unix_ms BIGINT NOT NULL,
    updated_at_unix_ms BIGINT NOT NULL,
    PRIMARY KEY (central_index_key, fiscal_year)
);

CREATE TABLE IF NOT EXISTS public.employee_counts (
    symbol TEXT NOT NULL,
    central_index_key TEXT NOT NULL,
    registrant TEXT NOT NULL,
    fiscal_year TEXT NOT NULL,
    count INTEGER,
    created_by UUID NOT NULL,
    updated_by UUID NOT NULL,
    created_at TIMESTAMPTZ NOT NULL,
    updated_at TIMESTAMPTZ NOT NULL,
    created_at_unix_ms BIGINT NOT NULL,
    updated_at_unix_ms BIGINT NOT NULL,
    PRIMARY KEY (central_index_key, fiscal_year)
);

CREATE TABLE IF NOT EXISTS public.executive_compensations (
    symbol TEXT NOT NULL,
    central_index_key TEXT NOT NULL,
    registrant TEXT NOT NULL,
    name TEXT NOT NULL,
    position TEXT NOT NULL,
    fiscal_year TEXT NOT NULL,
    salary NUMERIC(22,4),
    bonus NUMERIC(22,4),
    stock_awards NUMERIC(22,4),
    incentive_plan_compensation NUMERIC(22,4),
    other_compensation NUMERIC(22,4),
    total_compensation NUMERIC(22,4),
    created_by UUID NOT NULL,
    updated_by UUID NOT NULL,
    created_at TIMESTAMPTZ NOT NULL,
    updated_at TIMESTAMPTZ NOT NULL,
    created_at_unix_ms BIGINT NOT NULL,
    updated_at_unix_ms BIGINT NOT NULL,
    PRIMARY KEY (central_index_key, name, position, fiscal_year)
);

CREATE TABLE IF NOT EXISTS public.income_statements (
    symbol TEXT NOT NULL,
    central_index_key TEXT NOT NULL,
    registrant TEXT NOT NULL,
    fiscal_year TEXT NOT NULL,
    fiscal_period TEXT NOT NULL,
    period_end_date DATE NOT NULL,
    revenue NUMERIC(22,4),
    cost_of_revenue NUMERIC(22,4),
    gross_profit NUMERIC(22,4),
    research_development_expenses NUMERIC(22,4),
    general_admin_expenses NUMERIC(22,4),
    operating_expenses NUMERIC(22,4),
    operating_income NUMERIC(22,4),
    interest_expense NUMERIC(22,4),
    interest_income NUMERIC(22,4),
    net_income NUMERIC(22,4),
    earnings_per_share_basic NUMERIC(22,4),
    earnings_per_share_diluted NUMERIC(22,4),
    weighted_average_shares_outstanding_basic BIGINT,
    weighted_average_shares_outstanding_diluted BIGINT,
    created_by UUID NOT NULL,
    updated_by UUID NOT NULL,
    created_at TIMESTAMPTZ NOT NULL,
    updated_at TIMESTAMPTZ NOT NULL,
    created_at_unix_ms BIGINT NOT NULL,
    updated_at_unix_ms BIGINT NOT NULL,
    PRIMARY KEY (central_index_key, fiscal_year, fiscal_period)
);

CREATE TABLE IF NOT EXISTS public.balance_sheets (
    symbol TEXT NOT NULL,
    central_index_key TEXT NOT NULL,
    registrant TEXT NOT NULL,
    fiscal_year TEXT NOT NULL,
    fiscal_period TEXT NOT NULL,
    period_end_date DATE NOT NULL,
    cash NUMERIC(22,4),
    marketable_securities_current NUMERIC(22,4),
    accounts_receivable NUMERIC(22,4),
    inventories NUMERIC(22,4),
    non_trade_receivables NUMERIC(22,4),
    other_assets_current NUMERIC(22,4),
    total_assets_current NUMERIC(22,4),
    marketable_securities_non_current NUMERIC(22,4),
    property_plant_equipment NUMERIC(22,4),
    other_assets_non_current NUMERIC(22,4),
    total_assets_non_current NUMERIC(22,4),
    total_assets NUMERIC(22,4),
    accounts_payable NUMERIC(22,4),
    deferred_revenue NUMERIC(22,4),
    short_term_debt NUMERIC(22,4),
    other_liabilities_current NUMERIC(22,4),
    total_liabilities_current NUMERIC(22,4),
    long_term_debt NUMERIC(22,4),
    other_liabilities_non_current NUMERIC(22,4),
    total_liabilities_non_current NUMERIC(22,4),
    total_liabilities NUMERIC(22,4),
    common_stock NUMERIC(22,4),
    retained_earnings NUMERIC(22,4),
    accumulated_other_comprehensive_income NUMERIC(22,4),
    total_shareholders_equity NUMERIC(22,4),
    created_by UUID NOT NULL,
    updated_by UUID NOT NULL,
    created_at TIMESTAMPTZ NOT NULL,
    updated_at TIMESTAMPTZ NOT NULL,
    created_at_unix_ms BIGINT NOT NULL,
    updated_at_unix_ms BIGINT NOT NULL,
    PRIMARY KEY (central_index_key, fiscal_year, fiscal_period)
);

CREATE TABLE IF NOT EXISTS public.cash_flow_statements (
    symbol TEXT NOT NULL,
    central_index_key TEXT NOT NULL,
    registrant TEXT NOT NULL,
    fiscal_year TEXT NOT NULL,
    fiscal_period TEXT NOT NULL,
    period_end_date DATE NOT NULL,
    depreciation NUMERIC(22,4),
    share_based_compensation_expense NUMERIC(22,4),
    deferred_income_tax_expense NUMERIC(22,4),
    other_non_cash_income_expense NUMERIC(22,4),
    change_in_accounts_receivable NUMERIC(22,4),
    change_in_inventories NUMERIC(22,4),
    change_in_non_trade_receivables NUMERIC(22,4),
    change_in_other_assets NUMERIC(22,4),
    change_in_accounts_payable NUMERIC(22,4),
    change_in_deferred_revenue NUMERIC(22,4),
    change_in_other_liabilities NUMERIC(22,4),
    cash_from_operating_activities NUMERIC(22,4),
    purchases_of_marketable_securities NUMERIC(22,4),
    sales_of_marketable_securities NUMERIC(22,4),
    acquisition_of_property NUMERIC(22,4),
    acquisition_of_business NUMERIC(22,4),
    other_investing_activities NUMERIC(22,4),
    cash_from_investing_activities NUMERIC(22,4),
    tax_withholding_for_share_based_compensation NUMERIC(22,4),
    payments_of_dividends NUMERIC(22,4),
    issuance_of_common_stock NUMERIC(22,4),
    repurchase_of_common_stock NUMERIC(22,4),
    issuance_of_long_term_debt NUMERIC(22,4),
    repayment_of_long_term_debt NUMERIC(22,4),
    other_financing_activities NUMERIC(22,4),
    cash_from_financing_activities NUMERIC(22,4),
    change_in_cash NUMERIC(22,4),
    cash_at_end_of_period NUMERIC(22,4),
    income_taxes_paid NUMERIC(22,4),
    interest_paid NUMERIC(22,4),
    created_by UUID NOT NULL,
    updated_by UUID NOT NULL,
    created_at TIMESTAMPTZ NOT NULL,
    updated_at TIMESTAMPTZ NOT NULL,
    created_at_unix_ms BIGINT NOT NULL,
    updated_at_unix_ms BIGINT NOT NULL,
    PRIMARY KEY (central_index_key, fiscal_year, fiscal_period)
);

CREATE TABLE IF NOT EXISTS public.stock_splits (
    symbol TEXT NOT NULL,
    central_index_key TEXT NOT NULL,
    registrant TEXT NOT NULL,
    execution_date DATE NOT NULL,
    multiplier DOUBLE PRECISION,
    created_by UUID NOT NULL,
    updated_by UUID NOT NULL,
    created_at TIMESTAMPTZ NOT NULL,
    updated_at TIMESTAMPTZ NOT NULL,
    created_at_unix_ms BIGINT NOT NULL,
    updated_at_unix_ms BIGINT NOT NULL,
    PRIMARY KEY (central_index_key, execution_date)
);

CREATE TABLE IF NOT EXISTS public.dividends (
    symbol TEXT NOT NULL,
    registrant TEXT NOT NULL,
    type TEXT,
    amount NUMERIC(22,4),
    declaration_date DATE,
    ex_date DATE NOT NULL,
    record_date DATE,
    payment_date DATE,
    created_by UUID NOT NULL,
    updated_by UUID NOT NULL,
    created_at TIMESTAMPTZ NOT NULL,
    updated_at TIMESTAMPTZ NOT NULL,
    created_at_unix_ms BIGINT NOT NULL,
    updated_at_unix_ms BIGINT NOT NULL,
    PRIMARY KEY (symbol, ex_date)
);

CREATE TABLE IF NOT EXISTS public.earnings_releases (
    symbol TEXT NOT NULL,
    central_index_key TEXT NOT NULL,
    registrant_name TEXT NOT NULL,
    market_cap NUMERIC(22,4),
    fiscal_quarter_end_date TEXT NOT NULL,
    earnings_per_share NUMERIC(22,4),
    earnings_per_share_forecast NUMERIC(22,4),
    percentage_surprise DOUBLE PRECISION,
    number_of_forecasts INTEGER,
    conference_call_time TIMESTAMPTZ,
    conference_call_unix_ms BIGINT,
    created_by UUID NOT NULL,
    updated_by UUID NOT NULL,
    created_at TIMESTAMPTZ NOT NULL,
    updated_at TIMESTAMPTZ NOT NULL,
    created_at_unix_ms BIGINT NOT NULL,
    updated_at_unix_ms BIGINT NOT NULL,
    PRIMARY KEY (central_index_key, fiscal_quarter_end_date)
);

CREATE TABLE IF NOT EXISTS public.efficiency_ratios (
    symbol TEXT NOT NULL,
    central_index_key TEXT NOT NULL,
    registrant TEXT NOT NULL,
    fiscal_year TEXT NOT NULL,
    fiscal_period TEXT NOT NULL,
    period_end_date DATE NOT NULL,
    asset_turnover_ratio DOUBLE PRECISION,
    inventory_turnover_ratio DOUBLE PRECISION,
    accounts_receivable_turnover_ratio DOUBLE PRECISION,
    accounts_payable_turnover_ratio DOUBLE PRECISION,
    equity_multiplier DOUBLE PRECISION,
    days_sales_in_inventory DOUBLE PRECISION,
    fixed_asset_turnover_ratio DOUBLE PRECISION,
    days_working_capital DOUBLE PRECISION,
    working_capital_turnover_ratio DOUBLE PRECISION,
    days_cash_on_hand DOUBLE PRECISION,
    capital_intensity_ratio DOUBLE PRECISION,
    sales_to_equity_ratio DOUBLE PRECISION,
    inventory_to_sales_ratio DOUBLE PRECISION,
    investment_turnover_ratio DOUBLE PRECISION,
    sales_to_operating_income_ratio DOUBLE PRECISION,
    created_by UUID NOT NULL,
    updated_by UUID NOT NULL,
    created_at TIMESTAMPTZ NOT NULL,
    updated_at TIMESTAMPTZ NOT NULL,
    created_at_unix_ms BIGINT NOT NULL,
    updated_at_unix_ms BIGINT NOT NULL,
    PRIMARY KEY (central_index_key, fiscal_year, fiscal_period)
);

CREATE TABLE IF NOT EXISTS public.short_interests (
    symbol TEXT NOT NULL,
    title TEXT NOT NULL,
    market_code TEXT NOT NULL,
    settlement_date DATE NOT NULL,
    shorted_securities BIGINT,
    previous_shorted_securities BIGINT,
    change_in_shorted_securities BIGINT,
    percentage_change_in_shorted_securities DOUBLE PRECISION,
    average_daily_volume BIGINT,
    days_to_convert DOUBLE PRECISION,
    is_stock_split BOOLEAN,
    created_by UUID NOT NULL,
    updated_by UUID NOT NULL,
    created_at TIMESTAMPTZ NOT NULL,
    updated_at TIMESTAMPTZ NOT NULL,
    created_at_unix_ms BIGINT NOT NULL,
    updated_at_unix_ms BIGINT NOT NULL,
    PRIMARY KEY (symbol, settlement_date)
);