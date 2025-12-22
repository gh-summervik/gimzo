using Microsoft.Extensions.Configuration;
using Npgsql;
using System.Data;

namespace Gimzo.Infrastructure.Database;

/// <summary>
/// Represents the definition of a database connection.
/// </summary>
public sealed class DbDef
{
    public const string DefaultParmPrefix = "@";

    /// <summary>
    /// The name (key) of the connection string.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// The connection string (with pooling ensured).
    /// </summary>
    public string ConnectionString { get; }

    /// <summary>
    /// The parameter prefix.
    /// </summary>
    public string ParameterPrefix { get; }

    /// <summary>
    /// Instantiates a new instance of <see cref="DbDef"/>.
    /// </summary>
    public DbDef(string name, string? connectionString,
        int maxConnections = 100,
        string? parameterPrefix = DefaultParmPrefix)
    {
        ArgumentNullException.ThrowIfNull(name);
        ArgumentNullException.ThrowIfNull(connectionString);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(maxConnections, 0);

        ParameterPrefix = parameterPrefix ?? DefaultParmPrefix;
        Name = name;
        ConnectionString = EnsurePooling(connectionString, maxConnections);
    }

    private static string EnsurePooling(string connStr, int maxPoolSize)
    {
        var builder = new NpgsqlConnectionStringBuilder(connStr)
        {
            Pooling = true
        };
     
        if (builder.MaxPoolSize < maxPoolSize)
            builder.MaxPoolSize = maxPoolSize;

        return builder.ConnectionString;
    }

    /// <summary>
    /// Gets an instance of <see cref="IDbConnection"/>.
    /// </summary>
    /// <returns>An instance of <see cref="NpgsqlConnection"/>.</returns>
    public IDbConnection GetConnection() => new NpgsqlConnection(ConnectionString);

    public static IEnumerable<DbDef> GetDbDefs(IConfiguration configuration)
    {
        var connStringSection = configuration.GetSection("ConnectionStrings");

        foreach (var kvp in connStringSection.GetChildren())
        {
            if (kvp.Value is null)
                continue;

            yield return new(kvp.Key, kvp.Value);
        }
    }
}

public sealed class DbDefPair(DbDef command, DbDef query)
{
    public DbDef Command { get; init; } = command ?? throw new ArgumentNullException(nameof(command));
    public DbDef Query { get; init; } = query ?? throw new ArgumentNullException(nameof(query));
    public string[] GetNames => [Command.Name, Query.Name];
    public IDbConnection GetCommandConnection() => Command.GetConnection();
    public IDbConnection GetQueryConnection() => Query.GetConnection();
    public bool ConnectionStringsMatch => Command.ConnectionString.Equals(Query.ConnectionString);
    public bool HasName(string name) => Command.Name.Equals(name) || Query.Name.Equals(name);
    public static IEnumerable<DbDefPair> GetPairs(IConfiguration configuration)
    {
        var dbDefs = DbDef.GetDbDefs(configuration).ToArray();

        if (dbDefs.Length > 0)
        {
            foreach (var cmdDef in dbDefs.Where(k => !k.Name.EndsWith("Read", StringComparison.OrdinalIgnoreCase)))
            {
                var readDef = dbDefs.FirstOrDefault(k =>
                    k.Name.StartsWith(cmdDef.Name, StringComparison.OrdinalIgnoreCase) &&
                    k.Name.EndsWith("Read", StringComparison.OrdinalIgnoreCase));

                readDef ??= cmdDef;

                yield return new DbDefPair(cmdDef, readDef);
            }
        }
    }
}
