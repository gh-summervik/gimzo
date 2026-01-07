CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

DROP TABLE IF EXISTS public.backtest_trades CASCADE;

CREATE TABLE public.backtest_trades (
    trade_id UUID NOT NULL PRIMARY KEY,
    backtest_type TEXT NOT NULL,
    symbol TEXT NOT NULL,
    entry_date DATE NOT NULL,
    exit_date DATE NOT NULL,
    entry_price NUMERIC(22,4) NOT NULL,
    exit_price NUMERIC(22,4) NOT NULL,

    pnl_percent DOUBLE PRECISION NOT NULL,
    is_winner BOOLEAN NOT NULL,
    duration_days INTEGER NOT NULL,

    mfe_price NUMERIC(22,4),
    mae_price NUMERIC(22,4),
    mfe_percent DOUBLE PRECISION,
    mae_percent DOUBLE PRECISION,

    -- Entry context
    entry_prev_high_price NUMERIC(22,4),
    entry_prev_low_price NUMERIC(22,4),
    entry_percent_from_prev_high DOUBLE PRECISION,
    entry_percent_from_prev_low DOUBLE PRECISION,
    entry_atr NUMERIC(22,4),
    entry_avg_volume DOUBLE PRECISION,
    entry_relative_volume DOUBLE PRECISION,
    entry_num_up_days INTEGER,
    entry_num_down_days INTEGER,
    entry_num_green_days INTEGER,
    entry_num_red_days INTEGER,
    entry_prior_slope DOUBLE PRECISION,
    entry_rsi DOUBLE PRECISION,
    entry_ma_distances JSONB,

    -- Exit context
    exit_prev_high_price NUMERIC(22,4),
    exit_prev_low_price NUMERIC(22,4),
    exit_percent_from_prev_high DOUBLE PRECISION,
    exit_percent_from_prev_low DOUBLE PRECISION,
    exit_atr NUMERIC(22,4),
    exit_avg_volume DOUBLE PRECISION,
    exit_relative_volume DOUBLE PRECISION,
    exit_num_up_days INTEGER,
    exit_num_down_days INTEGER,
    exit_num_green_days INTEGER,
    exit_num_red_days INTEGER,
    exit_prior_slope DOUBLE PRECISION,
    exit_rsi DOUBLE PRECISION,
    exit_ma_distances JSONB,

    exit_reason TEXT,

    created_by UUID NOT NULL,
    updated_by UUID NOT NULL,
    created_at TIMESTAMPTZ NOT NULL,
    updated_at TIMESTAMPTZ NOT NULL,
    created_at_unix_ms BIGINT NOT NULL,
    updated_at_unix_ms BIGINT NOT NULL
);

-- Recommended indexes
CREATE INDEX idx_trades_is_winner ON public.backtest_trades(is_winner);
CREATE INDEX idx_trades_entry_percent_from_prev_high ON public.backtest_trades(entry_percent_from_prev_high);
CREATE INDEX idx_trades_entry_percent_from_prev_low ON public.backtest_trades(entry_percent_from_prev_low);
CREATE INDEX idx_trades_entry_rsi ON public.backtest_trades(entry_rsi);
CREATE INDEX idx_trades_entry_prior_slope ON public.backtest_trades(entry_prior_slope);
CREATE INDEX idx_trades_entry_relative_volume ON public.backtest_trades(entry_relative_volume);
CREATE INDEX idx_trades_backtest_type ON public.backtest_trades(backtest_type);
CREATE INDEX idx_trades_exit_date ON public.backtest_trades(exit_date);