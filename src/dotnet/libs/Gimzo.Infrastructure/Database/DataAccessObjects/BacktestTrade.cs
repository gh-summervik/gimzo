namespace Gimzo.Infrastructure.Database.DataAccessObjects;

internal sealed record BacktestTrade : DaoBase
{
    public BacktestTrade() : base()
    {
        TradeId = Guid.NewGuid();
        BacktestType = "";
        Symbol = "";
    }

    public BacktestTrade(Guid userId) : base(userId)
    {
        TradeId = Guid.NewGuid();
        BacktestType = "";
        Symbol = "";
    }

    public Guid TradeId { get; init; }
    public string BacktestType { get; init; }
    public string Symbol { get; init; }
    public DateOnly EntryDate { get; init; }
    public DateOnly ExitDate { get; init; }
    public decimal EntryPrice { get; init; }
    public decimal ExitPrice { get; init; }

    public double PnlPercent { get; init; }
    public bool IsWinner { get; init; }
    public int DurationDays { get; init; }

    public decimal? MfePrice { get; init; }
    public decimal? MaePrice { get; init; }
    public double? MfePercent { get; init; }
    public double? MaePercent { get; init; }

    // Entry context
    public decimal? EntryPrevHighPrice { get; init; }
    public decimal? EntryPrevLowPrice { get; init; }
    public double? EntryPercentFromPrevHigh { get; init; }
    public double? EntryPercentFromPrevLow { get; init; }
    public decimal? EntryAtr { get; init; }
    public double? EntryAvgVolume { get; init; }
    public double? EntryRelativeVolume { get; init; }
    public int? EntryNumUpDays { get; init; }
    public int? EntryNumDownDays { get; init; }
    public int? EntryNumGreenDays { get; init; }
    public int? EntryNumRedDays { get; init; }
    public double? EntryPriorSlope { get; init; }
    public double? EntryRsi { get; init; }
    public Dictionary<int, decimal> EntryMaDistances { get; init; } = new Dictionary<int, decimal>();

    // Exit context
    public decimal? ExitPrevHighPrice { get; init; }
    public decimal? ExitPrevLowPrice { get; init; }
    public double? ExitPercentFromPrevHigh { get; init; }
    public double? ExitPercentFromPrevLow { get; init; }
    public decimal? ExitAtr { get; init; }
    public double? ExitAvgVolume { get; init; }
    public double? ExitRelativeVolume { get; init; }
    public int? ExitNumUpDays { get; init; }
    public int? ExitNumDownDays { get; init; }
    public int? ExitNumGreenDays { get; init; }
    public int? ExitNumRedDays { get; init; }
    public double? ExitPriorSlope { get; init; }
    public double? ExitRsi { get; init; }
    public Dictionary<int, decimal> ExitMaDistances { get; init; } = new Dictionary<int, decimal>();

    public string? ExitReason { get; init; }

    public override bool IsValid() => base.IsValid() &&
        TradeId != Guid.Empty &&
        !string.IsNullOrWhiteSpace(BacktestType) &&
        !string.IsNullOrWhiteSpace(Symbol) &&
        EntryDate != default &&
        ExitDate != default &&
        EntryPrice > 0 &&
        ExitPrice > 0;
}