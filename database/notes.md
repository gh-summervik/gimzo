```sql
WITH perfs AS (
SELECT
symbol,
(end_close - start_close) / start_close * 100 AS performance_pct,
ROW_NUMBER() OVER (ORDER BY (end_close - start_close) / start_close * 100 DESC) AS best_rank,
ROW_NUMBER() OVER (ORDER BY (end_close - start_close) / start_close * 100 ASC) AS worst_rank
FROM (
SELECT
symbol,
FIRST_VALUE(close) OVER (PARTITION BY symbol ORDER BY date_eod ASC) AS start_close,
LAST_VALUE(close) OVER (PARTITION BY symbol ORDER BY date_eod ASC ROWS BETWEEN UNBOUNDED PRECEDING AND UNBOUNDED FOLLOWING) AS end_close,
ROW_NUMBER() OVER (PARTITION BY symbol ORDER BY date_eod ASC) AS rn
FROM eod_prices
WHERE date_eod >= '2025-12-15'
) sub
WHERE rn = 1 AND start_close <> 0
)
SELECT
symbol,
performance_pct,
CASE
WHEN best_rank <= 5 THEN 'best'
WHEN worst_rank <= 5 THEN 'worst'
END AS type,
CASE
WHEN best_rank <= 5 THEN best_rank
ELSE worst_rank
END AS rank_num
FROM perfs
WHERE best_rank <= 5 OR worst_rank <= 5
ORDER BY type DESC, rank_num ASC;
```


```sql
SELECT symbol, COUNT(*) AS Count, 200 AS Min, 200 - COUNT(*) AS Delta
FROM public.eod_prices
GROUP BY symbol
HAVING COUNT(*) < 200;
```