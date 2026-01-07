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

public static class Maths
{
    /// <summary>
    /// Calculates the least-squares linear regression slope over the last <paramref name="period"/> values.
    /// Values[0] = oldest, Values[^1] = newest.
    /// Returns double.NaN if insufficient data or denominator zero.
    /// </summary>
    public static double CalculateSlope(IReadOnlyList<decimal> values, int period)
    {
        ArgumentNullException.ThrowIfNull(values);
        if (period < 2 || period > values.Count)
            return double.NaN;

        int startIndex = values.Count - period;
        double sumX = 0;
        double sumY = 0;
        double sumXY = 0;
        double sumX2 = 0;

        for (int i = 0; i < period; i++)
        {
            double x = i; // 0 = oldest in window, period-1 = newest
            double y = (double)values[startIndex + i];

            sumX += x;
            sumY += y;
            sumXY += x * y;
            sumX2 += x * x;
        }

        double n = period;
        double denominator = n * sumX2 - sumX * sumX;
        if (denominator == 0)
            return double.NaN;

        return (n * sumXY - sumX * sumY) / denominator;
    }
}

public static class OsHelper
{
    public static bool IsWindows() => System.Runtime.InteropServices.RuntimeInformation.
        IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows);

    public static bool IsLinux() => System.Runtime.InteropServices.RuntimeInformation.
        IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Linux);

    public static bool IsMacOs() => System.Runtime.InteropServices.RuntimeInformation.
        IsOSPlatform(System.Runtime.InteropServices.OSPlatform.FreeBSD);
}


#pragma warning disable CA2254 // Template should be a static expression
public static class LogHelper
{
    public static void LogDebug(ILogger? logger, string message, params object[] args)
    {
        if ((logger?.IsEnabled(LogLevel.Debug) ?? false) && message is not null)
            logger.LogDebug(message, args);
    }

    public static void LogInfo(ILogger? logger, string message, params object[] args)
    {
        if ((logger?.IsEnabled(LogLevel.Information) ?? false) && message is not null)
            logger.LogInformation(message, args);
    }

    public static void LogWarning(ILogger? logger, string message, params object[] args)
    {
        if ((logger?.IsEnabled(LogLevel.Warning) ?? false) && message is not null)
            logger.LogWarning(message, args);
    }

    public static void LogError(ILogger? logger, string message, params object[] args)
    {
        if ((logger?.IsEnabled(LogLevel.Error) ?? false) && message is not null)
            logger.LogError(message, args);
    }

    public static void LogError(ILogger? logger, Exception exc, string message, params object[] args)
    {
        if ((logger?.IsEnabled(LogLevel.Error) ?? false) && exc is not null && message is not null)
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
