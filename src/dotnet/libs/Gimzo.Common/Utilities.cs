using Microsoft.Extensions.Logging;
using System.ComponentModel;
using System.Reflection;

namespace Gimzo.Common;

public static class EnumUtilities
{
    public static IEnumerable<string> GetDescriptions<T>() where T : struct, Enum
    {
        MemberInfo[] members = typeof(T).GetMembers(BindingFlags.Public | BindingFlags.Static);
        foreach (MemberInfo member in members)
        {
            var attrs = member.GetCustomAttributes(typeof(DescriptionAttribute), false);
            if (attrs.Length > 0)
            {
                for (int i = 0; i < attrs.Length; i++)
                {
                    string description = ((DescriptionAttribute)attrs[i]).Description;

                    yield return description;
                }
            }
            else
                yield return member.Name;
        }
    }
}

#pragma warning disable CA2254 // Template should be a static expression
public static class LogHelper
{
    public static void LogDebug(ILogger? logger, string message, params object[] args)
    {
        if ((logger?.IsEnabled(LogLevel.Debug) ?? false) && message != null)
            logger.LogDebug(message, args);
    }

    public static void LogInfo(ILogger? logger, string message, params object[] args)
    {
        if ((logger?.IsEnabled(LogLevel.Information) ?? false) && message != null)
            logger.LogInformation(message, args);
    }

    public static void LogWarning(ILogger? logger, string message, params object[] args)
    {
        if ((logger?.IsEnabled(LogLevel.Warning) ?? false) && message != null)
            logger.LogWarning(message, args);
    }

    public static void LogError(ILogger? logger, string message, params object[] args)
    {
        if ((logger?.IsEnabled(LogLevel.Error) ?? false) && message != null)
            logger.LogError(message, args);
    }

    public static void LogError(ILogger? logger, Exception exc, string message, params object[] args)
    {
        if ((logger?.IsEnabled(LogLevel.Error) ?? false) && exc != null && message != null)
            logger.LogError(exc, message, args);
    }
}
#pragma warning restore CA2254 // Template should be a static expression

public static class TimeHelper
{
    private static readonly TimeZoneInfo _easternZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");

    public static DateTime NowEastern => TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, _easternZone);

    public static DateOnly TodayEastern => DateOnly.FromDateTime(NowEastern);
}
