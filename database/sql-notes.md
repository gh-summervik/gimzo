# Attempts To Determine Common Denominators of Winning Trades

## Seeing runs
```sql
-- Summary per run (grouped by created_by = process/run ID)
SELECT 
    created_by AS run_id,
    COUNT(*) AS trades,
    ROUND(AVG(pnl_percent)::numeric, 2) AS avg_pnl_pct,
    ROUND(AVG(CASE WHEN is_winner THEN 1.0 ELSE 0 END)::numeric, 3) AS win_rate,
    ROUND(
        (AVG(CASE WHEN is_winner THEN 1.0 ELSE 0 END)::numeric * 
         AVG(CASE WHEN is_winner THEN pnl_percent END)::numeric) -
        ((1 - AVG(CASE WHEN is_winner THEN 1.0 ELSE 0 END)::numeric) * 
         AVG(CASE WHEN NOT is_winner THEN -pnl_percent END)::numeric),
        2
    ) AS expectancy,
    ROUND(AVG(CASE WHEN is_winner THEN pnl_percent END)::numeric, 2) AS avg_win,
    ROUND(AVG(CASE WHEN NOT is_winner THEN -pnl_percent END)::numeric, 2) AS avg_loss
FROM public.backtest_trades
GROUP BY created_by
ORDER BY MAX(created_at) DESC;  -- newest run first
```

```sql
-- Old run
SELECT 
    COUNT(*) AS trades,
    ROUND(AVG(pnl_percent)::numeric, 2) AS avg_pnl_pct,
    ROUND(AVG(CASE WHEN is_winner THEN 1.0 ELSE 0 END)::numeric, 3) AS win_rate,
    ROUND(
        (AVG(CASE WHEN is_winner THEN 1.0 ELSE 0 END)::numeric * 
         AVG(CASE WHEN is_winner THEN pnl_percent END)::numeric) -
        ((1 - AVG(CASE WHEN is_winner THEN 1.0 ELSE 0 END)::numeric) * 
         AVG(CASE WHEN NOT is_winner THEN -pnl_percent END)::numeric),
        2
    ) AS expectancy
FROM public.backtest_trades
WHERE created_by = '6f271ba7-a8ff-45e2-b0c9-dd90c770bb5b';

-- New run
SELECT 
    COUNT(*) AS trades,
    ROUND(AVG(pnl_percent)::numeric, 2) AS avg_pnl_pct,
    ROUND(AVG(CASE WHEN is_winner THEN 1.0 ELSE 0 END)::numeric, 3) AS win_rate,
    ROUND(
        (AVG(CASE WHEN is_winner THEN 1.0 ELSE 0 END)::numeric * 
         AVG(CASE WHEN is_winner THEN pnl_percent END)::numeric) -
        ((1 - AVG(CASE WHEN is_winner THEN 1.0 ELSE 0 END)::numeric) * 
         AVG(CASE WHEN NOT is_winner THEN -pnl_percent END)::numeric),
        2
    ) AS expectancy
FROM public.backtest_trades
WHERE created_by = 'e866441f-2540-4a88-899a-b05f1801ded1';
```

## Percent from previous high/low (highest edge usually here)
```sql
WITH buckets AS (
    SELECT 
        width_bucket(percent_from_prev_high, -30, 10, 20) AS bucket,  -- -30% to +10%, 20 buckets
        *
    FROM backtest.trades
    WHERE percent_from_prev_high IS NOT NULL
)
SELECT 
    bucket,
    min(percent_from_prev_high) || ' to ' || max(percent_from_prev_high) AS range_pct,
    count(*) AS trades,
    avg(CASE WHEN is_winner THEN 1.0 ELSE 0 END) AS win_rate,
    avg(pnl_percent) AS avg_return
FROM buckets
GROUP BY bucket
HAVING count(*) >= 50  -- filter noise
ORDER BY avg_return DESC;
```

## RSI + Slope combo
```sql
SELECT 
    floor(rsi / 10) * 10 AS rsi_bucket,
    sign(prior_slope) AS slope_direction,  -- -1, 0, 1
    count(*) AS trades,
    avg(CASE WHEN is_winner THEN 1.0 ELSE 0 END) AS win_rate,
    avg(pnl_percent) AS avg_return
FROM backtest.trades
GROUP BY rsi_bucket, slope_direction
HAVING count(*) >= 100
ORDER BY avg_return DESC;
```

## MAE/MFE filter (shows if winners run further relative to drawdown)
```sql
SELECT 
    floor(mfe_percent / 5) * 5 AS mfe_bucket,
    floor(mae_percent / 2) * 2 AS mae_bucket,
    avg(CASE WHEN is_winner THEN 1.0 ELSE 0 END) AS win_rate,
    count(*) AS trades
FROM backtest.trades
GROUP BY mfe_bucket, mae_bucket
HAVING count(*) >= 50
ORDER BY win_rate DESC;
```

## Relative volume
```sql
SELECT 
    floor(relative_volume * 10)/10 AS rel_vol_bucket,  -- 0.0-0.1, 0.1-0.2 etc.
    avg(CASE WHEN is_winner THEN 1.0 ELSE 0 END) AS win_rate,
    count(*)
FROM backtest.trades
GROUP BY rel_vol_bucket
ORDER BY win_rate DESC;
```
---

## Percent from previous high (pullback depth often strongest edge)

```sql
-- Pullbacks (negative % = below previous high)
WITH pullbacks AS (
    SELECT 
        width_bucket(entry_percent_from_prev_high, -100, 0, 20) AS bucket,
        entry_percent_from_prev_high,
        is_winner,
        pnl_percent
    FROM public.backtest_trades
    WHERE entry_percent_from_prev_high < 0
)
SELECT 
    bucket,
    round(min(entry_percent_from_prev_high)::numeric, 1) || '% to ' || 
    round(max(entry_percent_from_prev_high)::numeric, 1) || '%' AS range_pct,
    count(*) AS trades,
    round(avg(CASE WHEN is_winner THEN 1.0 ELSE 0 END)::numeric, 3) AS win_rate,
    round(avg(pnl_percent)::numeric, 2) AS avg_pnl_pct
FROM pullbacks
GROUP BY bucket
HAVING count(*) >= 100
ORDER BY avg_pnl_pct DESC;
```

```sql
-- Breakouts (positive % = above previous high)
WITH breakouts AS (
    SELECT 
        width_bucket(entry_percent_from_prev_high, 0, 100, 20) AS bucket,  -- adjust upper to 50000 if needed
        entry_percent_from_prev_high,
        is_winner,
        pnl_percent
    FROM public.backtest_trades
    WHERE entry_percent_from_prev_high > 0
)
SELECT 
    bucket,
    round(min(entry_percent_from_prev_high)::numeric, 1) || '% to ' || 
    round(max(entry_percent_from_prev_high)::numeric, 1) || '%' AS range_pct,
    count(*) AS trades,
    round(avg(CASE WHEN is_winner THEN 1.0 ELSE 0 END)::numeric, 3) AS win_rate,
    round(avg(pnl_percent)::numeric, 2) AS avg_pnl_pct
FROM breakouts
GROUP BY bucket
HAVING count(*) >= 100
ORDER BY avg_pnl_pct DESC;
```

```sql
WITH pullbacks AS (
    SELECT 
        width_bucket(entry_percent_from_prev_high, -100, 0, 20) AS bucket,
        entry_percent_from_prev_high,
        is_winner,
        pnl_percent
    FROM public.backtest_trades
    WHERE entry_percent_from_prev_high < 0
)
SELECT 
    bucket,
    round(min(entry_percent_from_prev_high)::numeric, 1) || '% to ' || 
    round(max(entry_percent_from_prev_high)::numeric, 1) || '%' AS range_pct,
    count(*) AS trades,
    round(avg(CASE WHEN is_winner THEN 1.0 ELSE 0 END)::numeric, 3) AS win_rate,
    round(avg(CASE WHEN is_winner THEN pnl_percent END)::numeric, 2) AS avg_win,
    round(avg(CASE WHEN NOT is_winner THEN -pnl_percent END)::numeric, 2) AS avg_loss,
    round(
        (avg(CASE WHEN is_winner THEN 1.0 ELSE 0 END)::numeric * 
         avg(CASE WHEN is_winner THEN pnl_percent END)::numeric) -
        ((1 - avg(CASE WHEN is_winner THEN 1.0 ELSE 0 END)::numeric) * 
         avg(CASE WHEN NOT is_winner THEN -pnl_percent END)::numeric),
        2
    ) AS expectancy
FROM pullbacks
GROUP BY bucket
HAVING count(*) >= 100
ORDER BY expectancy DESC;
```

```sql
WITH breakouts AS (
    SELECT 
        width_bucket(entry_percent_from_prev_high, 0, 100, 20) AS bucket,
        entry_percent_from_prev_high,
        is_winner,
        pnl_percent
    FROM public.backtest_trades
    WHERE entry_percent_from_prev_high > 0
)
SELECT 
    bucket,
    round(min(entry_percent_from_prev_high)::numeric, 1) || '% to ' || 
    round(max(entry_percent_from_prev_high)::numeric, 1) || '%' AS range_pct,
    count(*) AS trades,
    round(avg(CASE WHEN is_winner THEN 1.0 ELSE 0 END)::numeric, 3) AS win_rate,
    round(avg(CASE WHEN is_winner THEN pnl_percent END)::numeric, 2) AS avg_win,
    round(avg(CASE WHEN NOT is_winner THEN -pnl_percent END)::numeric, 2) AS avg_loss,
    round(
        (avg(CASE WHEN is_winner THEN 1.0 ELSE 0 END)::numeric * 
         avg(CASE WHEN is_winner THEN pnl_percent END)::numeric) -
        ((1 - avg(CASE WHEN is_winner THEN 1.0 ELSE 0 END)::numeric) * 
         avg(CASE WHEN NOT is_winner THEN -pnl_percent END)::numeric),
        2
    ) AS expectancy
FROM breakouts
GROUP BY bucket
HAVING count(*) >= 100
ORDER BY expectancy DESC;
```

```sql
WITH target_pullbacks AS (
    SELECT *
    FROM public.backtest_trades
    WHERE entry_percent_from_prev_high >= -20
      AND entry_percent_from_prev_high < -5
      AND entry_rsi IS NOT NULL
)
SELECT 
    floor(entry_rsi / 5) * 5 AS rsi_bucket,
    count(*) AS trades,
    round(avg(CASE WHEN is_winner THEN 1.0 ELSE 0 END)::numeric, 3) AS win_rate,
    round(avg(CASE WHEN is_winner THEN pnl_percent END)::numeric, 2) AS avg_win,
    round(avg(CASE WHEN NOT is_winner THEN -pnl_percent END)::numeric, 2) AS avg_loss,
    round(
        (avg(CASE WHEN is_winner THEN 1.0 ELSE 0 END)::numeric * 
         avg(CASE WHEN is_winner THEN pnl_percent END)::numeric) -
        ((1 - avg(CASE WHEN is_winner THEN 1.0 ELSE 0 END)::numeric) * 
         avg(CASE WHEN NOT is_winner THEN -pnl_percent END)::numeric),
        2
    ) AS expectancy
FROM target_pullbacks
GROUP BY rsi_bucket
HAVING count(*) >= 100
ORDER BY expectancy DESC;
```

```sql
WITH target AS (
    SELECT *
    FROM public.backtest_trades
    WHERE entry_percent_from_prev_high >= -20
      AND entry_percent_from_prev_high < -5
      AND entry_rsi IS NOT NULL
      AND entry_rsi BETWEEN -5 AND 5  -- focus on strongest RSI ~0 bucket
)
SELECT 
    floor(entry_relative_volume * 5) / 5 AS rel_vol_bucket,
    count(*) AS trades,
    round(avg(CASE WHEN is_winner THEN 1.0 ELSE 0 END)::numeric, 3) AS win_rate,
    round(avg(CASE WHEN is_winner THEN pnl_percent END)::numeric, 2) AS avg_win,
    round(avg(CASE WHEN NOT is_winner THEN -pnl_percent END)::numeric, 2) AS avg_loss,
    round(
        (avg(CASE WHEN is_winner THEN 1.0 ELSE 0 END)::numeric * 
         avg(CASE WHEN is_winner THEN pnl_percent END)::numeric) -
        ((1 - avg(CASE WHEN is_winner THEN 1.0 ELSE 0 END)::numeric) * 
         avg(CASE WHEN NOT is_winner THEN -pnl_percent END)::numeric),
        2
    ) AS expectancy
FROM target
GROUP BY rel_vol_bucket
HAVING count(*) >= 100
ORDER BY expectancy DESC;
```

```sql
WITH target AS (
    SELECT *
    FROM public.backtest_trades
    WHERE entry_percent_from_prev_high >= -20
      AND entry_percent_from_prev_high < -5
      AND entry_rsi BETWEEN -5 AND 5
      AND entry_relative_volume >= 1.6  -- top volume buckets
      AND entry_prior_slope IS NOT NULL
)
SELECT 
    floor(entry_prior_slope * 10) / 10 AS slope_bucket,
    count(*) AS trades,
    round(avg(CASE WHEN is_winner THEN 1.0 ELSE 0 END)::numeric, 3) AS win_rate,
    round(avg(CASE WHEN is_winner THEN pnl_percent END)::numeric, 2) AS avg_win,
    round(avg(CASE WHEN NOT is_winner THEN -pnl_percent END)::numeric, 2) AS avg_loss,
    round(
        (avg(CASE WHEN is_winner THEN 1.0 ELSE 0 END)::numeric * 
         avg(CASE WHEN is_winner THEN pnl_percent END)::numeric) -
        ((1 - avg(CASE WHEN is_winner THEN 1.0 ELSE 0 END)::numeric) * 
         avg(CASE WHEN NOT is_winner THEN -pnl_percent END)::numeric),
        2
    ) AS expectancy
FROM target
GROUP BY slope_bucket
HAVING count(*) >= 50  -- lower threshold for combo
ORDER BY expectancy DESC;
```

```sql
WITH target AS (
    SELECT *
    FROM public.backtest_trades
    WHERE entry_percent_from_prev_high >= -20
      AND entry_percent_from_prev_high < -5
      AND entry_rsi BETWEEN -5 AND 5
      AND entry_relative_volume >= 1.6
      AND entry_prior_slope > 0.3  -- top positive slope
      AND entry_ma_distances ? '20'  -- has MA20
)
SELECT 
    floor((entry_ma_distances->>'20')::numeric / 2) * 2 AS ma20_bucket,
    count(*) AS trades,
    round(avg(CASE WHEN is_winner THEN 1.0 ELSE 0 END)::numeric, 3) AS win_rate,
    round(avg(CASE WHEN is_winner THEN pnl_percent END)::numeric, 2) AS avg_win,
    round(avg(CASE WHEN NOT is_winner THEN -pnl_pct END)::numeric, 2) AS avg_loss,
    round(
        (avg(CASE WHEN is_winner THEN 1.0 ELSE 0 END)::numeric * 
         avg(CASE WHEN is_winner THEN pnl_percent END)::numeric) -
        ((1 - avg(CASE WHEN is_winner THEN 1.0 ELSE 0 END)::numeric) * 
         avg(CASE WHEN NOT is_winner THEN -pnl_percent END)::numeric),
        2
    ) AS expectancy
FROM target
GROUP BY ma20_bucket
HAVING count(*) >= 30
ORDER BY expectancy DESC;
```

```sql
WITH target AS (
    SELECT *
    FROM public.backtest_trades
    WHERE entry_percent_from_prev_high >= -20
      AND entry_percent_from_prev_high < -5
      AND entry_rsi BETWEEN -5 AND 5
      AND entry_relative_volume >= 1.6
      AND entry_prior_slope > 0.3
      AND entry_ma_distances ? '21'
)
SELECT 
    floor((entry_ma_distances->>'21')::numeric / 2) * 2 AS ma21_bucket,
    count(*) AS trades,
    round(avg(CASE WHEN is_winner THEN 1.0 ELSE 0 END)::numeric, 3) AS win_rate,
    round(avg(CASE WHEN is_winner THEN pnl_percent END)::numeric, 2) AS avg_win,
    round(avg(CASE WHEN NOT is_winner THEN -pnl_percent END)::numeric, 2) AS avg_loss,
    round(
        (avg(CASE WHEN is_winner THEN 1.0 ELSE 0 END)::numeric * 
         avg(CASE WHEN is_winner THEN pnl_percent END)::numeric) -
        ((1 - avg(CASE WHEN is_winner THEN 1.0 ELSE 0 END)::numeric) * 
         avg(CASE WHEN NOT is_winner THEN -pnl_percent END)::numeric),
        2
    ) AS expectancy
FROM target
GROUP BY ma21_bucket
HAVING count(*) >= 30
ORDER BY expectancy DESC;
```

```sql
WITH target AS (
    SELECT *
    FROM public.backtest_trades
    WHERE entry_percent_from_prev_high >= -20
      AND entry_percent_from_prev_high < -5
      AND entry_rsi BETWEEN -5 AND 5
      AND entry_relative_volume >= 1.6
      AND entry_prior_slope > 0.1  -- loosen slope for more trades
      AND entry_ma_distances ? '21'
)
SELECT 
    floor((entry_ma_distances->>'21')::numeric / 2) * 2 AS ma21_bucket,
    count(*) AS trades,
    round(avg(CASE WHEN is_winner THEN 1.0 ELSE 0 END)::numeric, 3) AS win_rate,
    round(avg(CASE WHEN is_winner THEN pnl_percent END)::numeric, 2) AS avg_win,
    round(avg(CASE WHEN NOT is_winner THEN -pnl_percent END)::numeric, 2) AS avg_loss,
    round(
        (avg(CASE WHEN is_winner THEN 1.0 ELSE 0 END)::numeric * 
         avg(CASE WHEN is_winner THEN pnl_percent END)::numeric) -
        ((1 - avg(CASE WHEN is_winner THEN 1.0 ELSE 0 END)::numeric) * 
         avg(CASE WHEN NOT is_winner THEN -pnl_percent END)::numeric),
        2
    ) AS expectancy
FROM target
GROUP BY ma21_bucket
HAVING count(*) >= 50
ORDER BY expectancy DESC;
```

```sql
WITH target AS (
    SELECT *
    FROM public.backtest_trades
    WHERE entry_percent_from_prev_high >= -20
      AND entry_percent_from_prev_high < -5
      AND entry_rsi BETWEEN -5 AND 5
      AND entry_relative_volume >= 1.6
      AND entry_prior_slope > 0.1
      AND entry_ma_distances ? '21'
      AND (entry_ma_distances->>'21')::numeric > 0  -- above MA21
)
SELECT 
    floor((entry_ma_distances->>'21')::numeric) AS ma21_bucket,
    count(*) AS trades,
    round(avg(CASE WHEN is_winner THEN 1.0 ELSE 0 END)::numeric, 3) AS win_rate,
    round(avg(CASE WHEN is_winner THEN pnl_percent END)::numeric, 2) AS avg_win,
    round(avg(CASE WHEN NOT is_winner THEN -pnl_percent END)::numeric, 2) AS avg_loss,
    round(
        (avg(CASE WHEN is_winner THEN 1.0 ELSE 0 END)::numeric * 
         avg(CASE WHEN is_winner THEN pnl_percent END)::numeric) -
        ((1 - avg(CASE WHEN is_winner THEN 1.0 ELSE 0 END)::numeric) * 
         avg(CASE WHEN NOT is_winner THEN -pnl_percent END)::numeric),
        2
    ) AS expectancy
FROM target
GROUP BY ma21_bucket
HAVING count(*) >= 30
ORDER BY expectancy DESC;
```

```sql
WITH target AS (
    SELECT *
    FROM public.backtest_trades
    WHERE entry_percent_from_prev_high >= -20
      AND entry_percent_from_prev_high < -5
      AND entry_rsi BETWEEN -5 AND 5
      AND entry_relative_volume >= 1.6
      AND entry_prior_slope > 0.1
      AND entry_ma_distances ? '21'
      AND (entry_ma_distances->>'21')::numeric >= 4  -- top buckets
)
SELECT 
    floor((entry_ma_distances->>'21')::numeric * 2) / 2 AS ma21_fine_bucket,
    count(*) AS trades,
    round(avg(CASE WHEN is_winner THEN 1.0 ELSE 0 END)::numeric, 3) AS win_rate,
    round(avg(CASE WHEN is_winner THEN pnl_percent END)::numeric, 2) AS avg_win,
    round(avg(CASE WHEN NOT is_winner THEN -pnl_percent END)::numeric, 2) AS avg_loss,
    round(
        (avg(CASE WHEN is_winner THEN 1.0 ELSE 0 END)::numeric * 
         avg(CASE WHEN is_winner THEN pnl_percent END)::numeric) -
        ((1 - avg(CASE WHEN is_winner THEN 1.0 ELSE 0 END)::numeric) * 
         avg(CASE WHEN NOT is_winner THEN -pnl_percent END)::numeric),
        2
    ) AS expectancy
FROM target
GROUP BY ma21_fine_bucket
HAVING count(*) >= 20
ORDER BY expectancy DESC;
```

```sql
WITH target AS (
    SELECT *
    FROM public.backtest_trades
    WHERE entry_percent_from_prev_high >= -20
      AND entry_percent_from_prev_high < -5
      AND entry_rsi BETWEEN -5 AND 5
      AND entry_relative_volume >= 1.6
      AND entry_prior_slope > 0.1
      AND entry_ma_distances ? '21'
      AND (entry_ma_distances->>'21')::numeric >= 4
)
SELECT 
    floor(duration_days / 5) * 5 AS duration_bucket,
    count(*) AS trades,
    round(avg(CASE WHEN is_winner THEN 1.0 ELSE 0 END)::numeric, 3) AS win_rate,
    round(avg(CASE WHEN is_winner THEN pnl_percent END)::numeric, 2) AS avg_win,
    round(avg(CASE WHEN NOT is_winner THEN -pnl_percent END)::numeric, 2) AS avg_loss,
    round(
        (avg(CASE WHEN is_winner THEN 1.0 ELSE 0 END)::numeric * 
         avg(CASE WHEN is_winner THEN pnl_percent END)::numeric) -
        ((1 - avg(CASE WHEN is_winner THEN 1.0 ELSE 0 END)::numeric) * 
         avg(CASE WHEN NOT is_winner THEN -pnl_percent END)::numeric),
        2
    ) AS expectancy
FROM target
GROUP BY duration_bucket
HAVING count(*) >= 10
ORDER BY expectancy DESC;
```

```sql
WITH target AS (
    SELECT *
    FROM public.backtest_trades
    WHERE entry_percent_from_prev_high >= -20
      AND entry_percent_from_prev_high < -5
      AND entry_rsi BETWEEN -5 AND 5
      AND entry_relative_volume >= 1.6
      AND entry_prior_slope > 0.1
      AND entry_ma_distances ? '21'
      AND (entry_ma_distances->>'21')::numeric >= 4
      AND duration_days BETWEEN 15 AND 40
      AND entry_atr IS NOT NULL
)
SELECT 
    floor(entry_atr / 0.5) * 0.5 AS atr_bucket,
    count(*) AS trades,
    round(avg(CASE WHEN is_winner THEN 1.0 ELSE 0 END)::numeric, 3) AS win_rate,
    round(avg(CASE WHEN is_winner THEN pnl_percent END)::numeric, 2) AS avg_win,
    round(avg(CASE WHEN NOT is_winner THEN -pnl_percent END)::numeric, 2) AS avg_loss,
    round(
        (avg(CASE WHEN is_winner THEN 1.0 ELSE 0 END)::numeric * 
         avg(CASE WHEN is_winner THEN pnl_percent END)::numeric) -
        ((1 - avg(CASE WHEN is_winner THEN 1.0 ELSE 0 END)::numeric) * 
         avg(CASE WHEN NOT is_winner THEN -pnl_percent END)::numeric),
        2
    ) AS expectancy
FROM target
GROUP BY atr_bucket
HAVING count(*) >= 10
ORDER BY expectancy DESC;
```

```sql
WITH target AS (
    SELECT *
    FROM public.backtest_trades
    WHERE entry_percent_from_prev_high >= -20
      AND entry_percent_from_prev_high < -5
      AND entry_rsi BETWEEN -5 AND 5
      AND entry_relative_volume >= 1.6
      AND entry_prior_slope > 0.1
      AND entry_ma_distances ? '21'
      AND (entry_ma_distances->>'21')::numeric >= 4
      AND duration_days BETWEEN 15 AND 40
      AND entry_atr >= 2.0
      AND mae_percent IS NOT NULL
)
SELECT 
    floor(mae_percent / 5) * 5 AS mae_bucket,
    count(*) AS trades,
    round(avg(CASE WHEN is_winner THEN 1.0 ELSE 0 END)::numeric, 3) AS win_rate,
    round(avg(pnl_percent)::numeric, 2) AS avg_pnl_pct,
    round(
        (avg(CASE WHEN is_winner THEN 1.0 ELSE 0 END)::numeric * 
         avg(CASE WHEN is_winner THEN pnl_percent END)::numeric) -
        ((1 - avg(CASE WHEN is_winner THEN 1.0 ELSE 0 END)::numeric) * 
         avg(CASE WHEN NOT is_winner THEN -pnl_percent END)::numeric),
        2
    ) AS expectancy
FROM target
GROUP BY mae_bucket
HAVING count(*) >= 5  -- small samples ok here
ORDER BY expectancy DESC;
```

```sql
WITH target AS (
    SELECT *
    FROM public.backtest_trades
    WHERE entry_percent_from_prev_high >= -20
      AND entry_percent_from_prev_high < -5
      AND entry_rsi BETWEEN -5 AND 5
      AND entry_relative_volume >= 1.6
      AND entry_prior_slope > 0.1
      AND entry_ma_distances ? '21'
      AND (entry_ma_distances->>'21')::numeric >= 4
      AND duration_days BETWEEN 15 AND 40
      AND entry_atr >= 2.0
      AND mae_percent BETWEEN 0 AND 5
      AND mfe_percent IS NOT NULL
)
SELECT 
    floor(mfe_percent / 5) * 5 AS mfe_bucket,
    count(*) AS trades,
    round(avg(CASE WHEN is_winner THEN 1.0 ELSE 0 END)::numeric, 3) AS win_rate,
    round(avg(pnl_percent)::numeric, 2) AS avg_pnl_pct,
    round(
        (avg(CASE WHEN is_winner THEN 1.0 ELSE 0 END)::numeric * 
         avg(CASE WHEN is_winner THEN pnl_percent END)::numeric) -
        ((1 - avg(CASE WHEN is_winner THEN 1.0 ELSE 0 END)::numeric) * 
         avg(CASE WHEN NOT is_winner THEN -pnl_percent END)::numeric),
        2
    ) AS expectancy
FROM target
GROUP BY mfe_bucket
HAVING count(*) >= 5
ORDER BY expectancy DESC;
```

```sql
WITH target AS (
    SELECT *
    FROM public.backtest_trades
    WHERE entry_percent_from_prev_high >= -20
      AND entry_percent_from_prev_high < -5
      AND entry_rsi BETWEEN -5 AND 5
      AND entry_relative_volume >= 1.6
      AND entry_prior_slope > 0.1
      AND entry_ma_distances ? '21'
      AND (entry_ma_distances->>'21')::numeric >= 4
      AND duration_days BETWEEN 15 AND 40
      AND entry_atr >= 2.0
      AND mae_percent BETWEEN 0 AND 5
      AND mfe_percent >= 5
      AND exit_reason IS NOT NULL
)
SELECT 
    exit_reason,
    count(*) AS trades,
    round(avg(CASE WHEN is_winner THEN 1.0 ELSE 0 END)::numeric, 3) AS win_rate,
    round(avg(pnl_percent)::numeric, 2) AS avg_pnl_pct,
    round(
        (avg(CASE WHEN is_winner THEN 1.0 ELSE 0 END)::numeric * 
         avg(CASE WHEN is_winner THEN pnl_percent END)::numeric) -
        ((1 - avg(CASE WHEN is_winner THEN 1.0 ELSE 0 END)::numeric) * 
         avg(CASE WHEN NOT is_winner THEN -pnl_percent END)::numeric),
        2
    ) AS expectancy
FROM target
GROUP BY exit_reason
HAVING count(*) >= 3  -- lower for exit_reason
ORDER BY expectancy DESC;
```

```sql
SELECT DISTINCT jsonb_object_keys(entry_ma_distances)
FROM public.backtest_trades
LIMIT 20;
```

```sql
SELECT DISTINCT jsonb_object_keys(entry_ma_distances)
FROM public.backtest_trades
LIMIT 20;
```
