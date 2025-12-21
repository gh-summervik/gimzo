using Microsoft.Extensions.Logging;

namespace Gimzo.Infrastructure.Logging;

public sealed class LogItem
{
    internal LogItem()
    {
    }

    public LogItem(string? message, LogLevel logLevel = LogLevel.Information,
        string? scope = null, Guid? processId = null) : this()
    {
        LogLevel = logLevel;
        Message = message;
        Scope = scope;
        CreatedBy = processId;
    }

    public LogItem(Exception exception, string? scope = null, Guid? processId = null,
        LogLevel logLevel = LogLevel.Critical)
        : this(exception.Message, logLevel, scope, processId)
    {
        Exception = exception;
    }

    public LogItem(int eventId, string? eventName = null, Guid? processId = null)
        : this(new EventId(eventId, eventName), processId) { }

    public LogItem(EventId eventId, Guid? processId = null)
    {
        EventId = eventId;
        CreatedBy = processId;
    }

    public LogLevel LogLevel { get; init; } = LogLevel.Information;
    public EventId EventId { get; init; }
    public string? Message { get; init; }
    public Exception? Exception { get; init; }
    public string? Scope { get; init; }
    public DateTimeOffset TimestampUtc { get; init; } = DateTimeOffset.UtcNow;
    public Guid? CreatedBy { get; init; }

    public override string ToString() => EventId.Equals(default)
            ? Exception?.Message ?? Message ?? "None"
            : EventId.ToString();
}