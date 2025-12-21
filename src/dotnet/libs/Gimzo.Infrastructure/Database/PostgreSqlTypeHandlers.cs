using Dapper;
using Npgsql;
using System.Data;

namespace Gimzo.Infrastructure.Database;

public class DateOnlyTypeHandler : SqlMapper.TypeHandler<DateOnly>
{
    public override DateOnly Parse(object value)
    {
        return value switch
        {
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

public class NullableDateOnlyTypeHandler : SqlMapper.TypeHandler<DateOnly?>
{
    public override DateOnly? Parse(object value)
    {
        return value switch
        {
            null => null,
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
