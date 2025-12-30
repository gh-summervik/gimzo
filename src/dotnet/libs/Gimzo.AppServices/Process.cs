namespace Gimzo.AppServices;

public class Process
{
    public static Process Create(string processType, string? inputPath = null,
        string? outputPath = null, params string[] args)
    {
        return new Process()
        {
            ProcessType = processType,
            InputPath = inputPath,
            OutputPath = outputPath,
            StartTime = DateTime.UtcNow,
            Args = args.Length == 0 ? null : string.Join(' ', args)
        };
    }

    private Process()
    {
        ProcessId = Guid.NewGuid();
        ProcessType = "";
    }

    internal Process(Guid processId,
        string processType,
        DateTimeOffset startTime,
        DateTimeOffset? finishTime = null,
        string? inputPath = null,
        string? outputPath = null,
        Guid? parentProcessId = null,
        string? args = null)
    {
        ProcessId = processId;
        ProcessType = processType;
        StartTime = startTime;
        FinishTime = finishTime;
        InputPath = inputPath;
        OutputPath = outputPath;
        ParentProcessId = parentProcessId;
        Args = args;
    }

    public Guid ProcessId { get; init; }
    public string ProcessType { get; init; }
    public DateTimeOffset StartTime { get; init; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? FinishTime { get; set; }
    public string? InputPath { get; init; }
    public string? OutputPath { get; init; }
    public Guid? ParentProcessId { get; init; }
    public string? Args { get; init; }

    internal Infrastructure.Database.DataAccessObjects.Process ToDao(DateTimeOffset? finish = null)
    {
        return new(ProcessId)
        {
            ProcessId = ProcessId,
            ProcessType = ProcessType,
            StartTime = StartTime,
            FinishTime = finish ?? FinishTime,
            InputPath = InputPath,
            OutputPath = OutputPath,
            ParentProcessId = ParentProcessId,
            Args = Args
        };
    }
}
