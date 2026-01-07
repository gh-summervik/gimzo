CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

DROP TABLE IF EXISTS public.company_valuations CASCADE;
DROP TABLE IF EXISTS public.sic_money_flow CASCADE;

CREATE TABLE IF NOT EXISTS public.company_valuations(
    symbol TEXT NOT NULL,
    date_eval DATE NOT NULL,
    absolute_value INTEGER NOT NULL,
    percentile_rank INTEGER NOT NULL,
    created_by UUID NOT NULL,
    updated_by UUID NOT NULL,
    created_at TIMESTAMPTZ NOT NULL,
    updated_at TIMESTAMPTZ NOT NULL,
    created_at_unix_ms BIGINT NOT NULL,
    updated_at_unix_ms BIGINT NOT NULL,
    PRIMARY KEY (symbol, date_eval)
);

CREATE TABLE IF NOT EXISTS public.sic_money_flow(
    sic_code TEXT NOT NULL,
    date_eval DATE NOT NULL,
    flow_billions NUMERIC(22,4) NOT NULL,
    rank INTEGER NOT NULL,
    created_by UUID NOT NULL,
    updated_by UUID NOT NULL,
    created_at TIMESTAMPTZ NOT NULL,
    updated_at TIMESTAMPTZ NOT NULL,
    created_at_unix_ms BIGINT NOT NULL,
    updated_at_unix_ms BIGINT NOT NULL,
    PRIMARY KEY (sic_code, date_eval)
);

