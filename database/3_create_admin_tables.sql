CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

CREATE TABLE IF NOT EXISTS public.processes(
    process_id UUID NOT NULL,
    process_type TEXT NOT NULL,
    start_time TIMESTAMPTZ NOT NULL,
    finish_time TIMESTAMPTZ,
    start_time_unix_ms BIGINT NOT NULL,
    finish_time_unix_ms BIGINT,
    input_path TEXT,
    output_path TEXT,
    parent_process_id UUID,
    created_by UUID NOT NULL,
    updated_by UUID NOT NULL,
    created_at TIMESTAMPTZ NOT NULL,
    updated_at TIMESTAMPTZ NOT NULL,
    created_at_unix_ms BIGINT NOT NULL,
    updated_at_unix_ms BIGINT NOT NULL,
    PRIMARY KEY (process_id)
);

CREATE TABLE IF NOT EXISTS public.ignored_symbols(
    symbol TEXT NOT NULL,
    reason TEXT NOT NULL,
    expiration DATE,                -- when the symbol should come off the list.
    created_by UUID NOT NULL,
    updated_by UUID NOT NULL,
    created_at TIMESTAMPTZ NOT NULL,
    updated_at TIMESTAMPTZ NOT NULL,
    created_at_unix_ms BIGINT NOT NULL,
    updated_at_unix_ms BIGINT NOT NULL,
    PRIMARY KEY (symbol)
);