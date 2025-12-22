using Gimzo.Common;

namespace Gimzo.Infrastructure.Database.DataAccessObjects;

/// <summary>
/// Represents the base audit trail implementation.
/// This class contains <see cref="CreatedAt"/> and <see cref="ProcessId"/>.
/// </summary>
internal abstract record class AuditBase
{
    private long _createdAtUnixMs;

    public AuditBase() : this(Constants.SystemId)
    {
    }

    public AuditBase(Guid userId)
    {
        CreatedAt = DateTimeOffset.UtcNow;
        CreatedBy = userId;
    }

    /// <summary>
    /// Gets the <see cref="CreatedAt"/> converted to Unix milliseconds.
    /// </summary>
    public long CreatedAtUnixMs
    {
        get => _createdAtUnixMs;
        init => _createdAtUnixMs = value;
    }

    /// <summary>
    /// Gets the DateTimeOffset for when this record was created.
    /// </summary>
    public DateTimeOffset CreatedAt
    {
        get => DateTimeOffset.FromUnixTimeMilliseconds(_createdAtUnixMs);
        init => _createdAtUnixMs = value.ToUnixTimeMilliseconds();
    }

    /// <summary>
    /// Gets the User Id attached to the record, if available.
    /// </summary>
    public Guid CreatedBy { get; init; }

    public virtual bool IsValid() => CreatedAt > DateTimeOffset.MinValue && !CreatedBy.Equals(Guid.Empty);
}

/// <summary>
/// Represents the base DAO implementation.
/// </summary>
internal abstract record class DaoBase : AuditBase
{
    private long _updatedAtUnixMs;

    public DaoBase() : this(Constants.SystemId)
    {
    }

    public DaoBase(Guid userId) : base(userId)
    {
        UpdatedAt = DateTimeOffset.UtcNow;
        UpdatedBy = userId;
    }

    /// <summary>
    /// Gets the <see cref="UpdatedAt"/> converted to Unix milliseconds.
    /// </summary>
    public long UpdatedAtUnixMs
    {
        get => _updatedAtUnixMs;
        init => _updatedAtUnixMs = value;
    }

    /// <summary>
    /// Gets the DateTimeOffset for when this record was last updated.
    /// </summary>
    public DateTimeOffset UpdatedAt
    {
        get => DateTimeOffset.FromUnixTimeMilliseconds(_updatedAtUnixMs);
        init => _updatedAtUnixMs = value.ToUnixTimeMilliseconds();
    }

    /// <summary>
    /// Gets the User Id of the user (i.e., process) who last updated this record.
    /// </summary>
    public Guid UpdatedBy { get; init; }

    public override bool IsValid() => base.IsValid() && 
        UpdatedAt > DateTimeOffset.MinValue && 
        !UpdatedBy.Equals(Guid.Empty);
}
