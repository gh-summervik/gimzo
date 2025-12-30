namespace Gimzo.Analysis.Technical.Charts;

public readonly struct AverageTrueRange
{
    public AverageTrueRange(Ohlc[] actions, int period = 14)
    {
        Period = Math.Max(period, 1);
        if (actions.Length < Period)
            return;

        decimal[] trueRanges = new decimal[actions.Length];
        trueRanges[0] = actions[0].High - actions[0].Low;
        for (int i = 1; i < actions.Length; i++)
        {
            decimal hl = actions[i].High - actions[i].Low;                    // high - low
            decimal hpc = Math.Abs(actions[i].High - actions[i - 1].Close);   // high - prev close
            decimal lpc = Math.Abs(actions[i].Low - actions[i - 1].Close);    // low - prev close
            trueRanges[i] = Math.Max(hl, Math.Max(hpc, lpc));                 // take the largest value
        }

        Values = new decimal[actions.Length];
        decimal sum = 0M;
        for (int i = 0; i < Period; i++)
            sum += trueRanges[i];
     
        Values[Period - 1] = sum / Period;

        for (int i = Period; i < actions.Length; i++)
            Values[i] = (Values[i - 1] * (Period - 1) + trueRanges[i]) / Period;
    }

    public int Period { get; }
    public decimal[] Values { get; } = [];
}