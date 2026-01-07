using Dapper;
using Npgsql;
using NpgsqlTypes;
using System.Data;
using System.Text.Json;

namespace Gimzo.Infrastructure.Database;

public sealed class DateOnlyTypeHandler : SqlMapper.TypeHandler<DateOnly>
{
    public override DateOnly Parse(object value)
    {
        return value switch
        {
            DateOnly dateOnly => dateOnly,
            DateTime dateTime => DateOnly.FromDateTime(dateTime),
            _ => throw new InvalidCastException($"Unable to convert {value?.GetType()} to DateOnly")
        };
    }
    public override void SetValue(IDbDataParameter parameter, DateOnly value)
    {
        parameter.Value = value.ToDateTime(TimeOnly.MinValue);
        if (parameter is NpgsqlParameter npgsqlParam)
        {
            npgsqlParam.NpgsqlDbType = NpgsqlTypes.NpgsqlDbType.Date;
        }
        else
        {
            parameter.DbType = DbType.Date;
        }
    }
}

public sealed class NullableDateOnlyTypeHandler : SqlMapper.TypeHandler<DateOnly?>
{
    public override DateOnly? Parse(object value)
    {
        if (value is null || value == DBNull.Value)
            return null;

        return value switch
        {
            DateOnly dateOnly => dateOnly,
            DateTime dateTime => DateOnly.FromDateTime(dateTime),
            _ => throw new InvalidCastException($"Unable to convert {value?.GetType()} to DateOnly?")
        };
    }
    public override void SetValue(IDbDataParameter parameter, DateOnly? value)
    {
        if (value.HasValue)
        {
            parameter.Value = value.Value.ToDateTime(TimeOnly.MinValue);
            if (parameter is NpgsqlParameter npgsqlParam)
            {
                npgsqlParam.NpgsqlDbType = NpgsqlTypes.NpgsqlDbType.Date;
            }
            else
            {
                parameter.DbType = DbType.Date;
            }
        }
        else
        {
            parameter.Value = DBNull.Value;
        }
    }
}

public sealed class DictionaryIntDecimalJsonbHandler : SqlMapper.TypeHandler<Dictionary<int, decimal>>
{
    public override void SetValue(IDbDataParameter parameter, Dictionary<int, decimal>? value)
    {
        if (parameter is not NpgsqlParameter npgParam)
            throw new InvalidOperationException("Parameter must be an NpgsqlParameter");

        npgParam.NpgsqlDbType = NpgsqlDbType.Jsonb;

        if (value is null || value.Count == 0)
        {
            npgParam.Value = DBNull.Value;
            return;
        }

        // JSON requires string keys
        var jsonDict = value.ToDictionary(k => k.Key.ToString(), v => v.Value);
        npgParam.Value = JsonSerializer.Serialize(jsonDict);
    }

    public override Dictionary<int, decimal> Parse(object? value)
    {
        if (value is null or DBNull)
            return new Dictionary<int, decimal>();

        var json = (string)value;
        var temp = JsonSerializer.Deserialize<Dictionary<string, decimal>>(json)
                   ?? new Dictionary<string, decimal>();

        var result = new Dictionary<int, decimal>(temp.Count);
        foreach (var kvp in temp)
        {
            if (int.TryParse(kvp.Key, out int key))
                result[key] = kvp.Value;
        }

        return result;
    }
}