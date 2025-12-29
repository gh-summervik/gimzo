namespace Gimzo.AppServices;

public class Process
{
    public static Process Create(string processType, string? inputPath = null,
        string? outputPath = null)
    {
        return new Process()
        {
            ProcessType = processType,
            InputPath = inputPath,
            OutputPath = outputPath,
            StartTime = DateTime.UtcNow
        };
    }

    internal Process()
    {
        ProcessId = Guid.NewGuid();
        ProcessType = "";
    }

    internal Process(Guid processId,
        string processType,
        DateTime startTime,
        DateTime? finishTime = null,
        string? inputPath = null,
        string? outputPath = null,
        Guid? parentProcessId = null)
    {
        ProcessId = processId;
        ProcessType = processType;
        StartTime = startTime;
        FinishTime = finishTime;
        InputPath = inputPath;
        OutputPath = outputPath;
        ParentProcessId = parentProcessId;
    }

    public Guid ProcessId { get; init; }
    public string ProcessType { get; init; }
    public DateTime StartTime { get; init; } = DateTime.UtcNow;
    public DateTime? FinishTime { get; init; }
    public string? InputPath { get; init; }
    public string? OutputPath { get; init; }
    public Guid? ParentProcessId { get; init; }
}
