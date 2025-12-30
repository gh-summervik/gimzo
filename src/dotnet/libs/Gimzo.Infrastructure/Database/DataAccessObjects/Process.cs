namespace Gimzo.Infrastructure.Database.DataAccessObjects;

internal sealed record Process : DaoBase
{
    private long _startTimeUnixMs;
    private long? _finishTimeUnixMs;

    public Process() : base()
    {
        ProcessId = Guid.Empty;
        ProcessType = "";
    }

    public Process(Guid userId) : base(userId)
    {
        ProcessId = userId;
        ProcessType = "";
    }

    public Guid ProcessId { get; init; }
    public string ProcessType { get; init; }
    public DateTimeOffset StartTime
    {
        get => DateTimeOffset.FromUnixTimeMilliseconds(_startTimeUnixMs);
        init => _startTimeUnixMs = value.ToUnixTimeMilliseconds();
    }
    public DateTimeOffset? FinishTime
    {
        get => _finishTimeUnixMs.HasValue ? DateTimeOffset.FromUnixTimeMilliseconds(_finishTimeUnixMs.GetValueOrDefault()) : null;
        set => _finishTimeUnixMs = value?.ToUnixTimeMilliseconds();
    }
    public long StartTimeUnixMs { get => _startTimeUnixMs; set => _startTimeUnixMs = value; }
    public long? FinishTimeUnixMs { get => _finishTimeUnixMs; set => _finishTimeUnixMs = value; }
    public string? InputPath { get; init; }
    public string? OutputPath { get; init; }
    public Guid? ParentProcessId { get; init; }
    public string? Args { get; init; }
    public override bool IsValid() => base.IsValid() &&
        !ProcessId.Equals(Guid.Empty);
}
