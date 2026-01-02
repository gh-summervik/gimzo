using System.ComponentModel;

namespace Gimzo.Common;

public static class Constants
{
    public const int DefaultChartLookback = 60;
    public const int DefaultAverageTrueRangePeriod = 14;

    /// <summary>
    /// The number of digits to the right of the decimal point in money calculations.
    /// </summary>
    public const int MoneyPrecision = 4;

    public const string SystemIdText = "10010000-0001-1000-0010-100000100000";

    public static Guid SystemId => Guid.Parse(SystemIdText);

    public static class DbKeys
    {
        public const string Gimzo = "Gimzo";
        public const string GimzoRead = "Gimzo-Read";

        public static IEnumerable<string> GetAll()
        {
            foreach (var p in typeof(DbKeys).GetFields(System.Reflection.BindingFlags.Public |
                System.Reflection.BindingFlags.Static))
                yield return p.GetValue(null)?.ToString() ?? p.Name;
        }
    }
}

public enum LogicalOperator
{
    [Description("AND")]
    And = 0,
    [Description("OR")]
    Or = 1
}
