using Gimzo.Analysis.Technical.Charts;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gimzo.AppServices.Backtests;

public class BacktestResults
{
    public BacktestResults()
    {
        Ledger = new();
    }

    public Ledger Ledger { get; }
    public decimal? MaximumFavorableExursion { get; set; }
    public decimal? MaximumAdverseExcursion { get; set;  }
    
}

public record BacktestDetails
{
    public PriceExtreme? PreviousHigh { get; internal set; }
    public PriceExtreme? PreviousLow { get; internal set; }
    public CashLedgerEntry? Open { get; internal set; }
    public CashLedgerEntry? Close { get; internal set; }
    public decimal? DistanceFromEntryToPrevHigh => PreviousHigh is null || Open is null ? null
        : Math.Abs(PreviousHigh.Value.Price - Open.TradePrice);
    public decimal? DistanceFromEntryToPrevLow => PreviousLow is null || Open is null ? null
        : Math.Abs(PreviousLow.Value.Price - Open.TradePrice);
}

