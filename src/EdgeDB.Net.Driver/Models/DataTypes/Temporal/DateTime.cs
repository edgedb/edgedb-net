using System.Diagnostics.CodeAnalysis;
using SysDateTime = System.DateTime;

namespace EdgeDB.DataTypes;

/// <summary>
///     A struct representing a timezone-aware moment in time.
/// </summary>
public readonly struct DateTime
{
    /// <summary>
    ///     Gets a <see cref="DateTimeOffset" /> that represents this <see cref="DateTime" />.
    /// </summary>
    public DateTimeOffset DateTimeOffset
        => TemporalCommon.DateTimeOffsetFromMicroseconds(Microseconds, true);

    /// <summary>
    ///     Gets a <see cref="System.DateTime" /> that represents this <see cref="DateTime" />.
    /// </summary>
    public SysDateTime SystemDateTime
        => DateTimeOffset.DateTime;

    /// <summary>
    ///     Gets the microsecond component of this <see cref="DateTime" />; representing the amount
    ///     of microseconds since January 1st 2000, 00:00.
    /// </summary>
    public long Microseconds { get; }

    /// <summary>
    ///     Constructs a new <see cref="DateTime" />.
    /// </summary>
    /// <param name="microseconds">The amount of microseconds since January 1st 2000, 00:00 UTC</param>
    internal DateTime(long microseconds)
    {
        Microseconds = microseconds;
    }

    /// <summary>
    ///     Constructs a new <see cref="DateTime" />.
    /// </summary>
    /// <param name="datetime">
    ///     The <see cref="SysDateTime" /> to use to construct this <see cref="DateTime" />.
    /// </param>
    /// <remarks>
    ///     The supplied <see cref="SysDateTime" /> will be rounded to the nearest microsecond.
    /// </remarks>
    public DateTime(SysDateTime datetime)
        : this((DateTimeOffset)datetime)
    {
    }

    /// <summary>
    ///     Constructs a new <see cref="DateTime" />.
    /// </summary>
    /// <param name="datetime">
    ///     The <see cref="System.DateTimeOffset" /> to use to construct this <see cref="DateTime" />
    /// </param>
    /// <remarks>
    ///     The supplied <see cref="System.DateTimeOffset" /> will be rounded to the nearest microsecond.
    /// </remarks>
    public DateTime(DateTimeOffset datetime)
    {
        Microseconds = TemporalCommon.ToMicroseconds(datetime);
    }

    /// <inheritdoc />
    public override bool Equals([NotNullWhen(true)] object? obj)
    {
        if (obj is not DateTime dt)
            return false;

        return dt.Microseconds == Microseconds;
    }

    /// <inheritdoc />
    public override int GetHashCode()
        => Microseconds.GetHashCode();

    /// <summary>
    ///     Gets a <see cref="DateTime" /> object whos date and time are set to the current UTC time.
    /// </summary>
    public static DateTime Now => new(DateTimeOffset.UtcNow);

    public static implicit operator SysDateTime(DateTime t) => t.DateTimeOffset.DateTime;
    public static implicit operator DateTimeOffset(DateTime t) => t.DateTimeOffset;

    public static implicit operator DateTime(SysDateTime t) => new(t);
    public static implicit operator DateTime(DateTimeOffset t) => new(t);

    public static bool operator ==(DateTime left, DateTime right) => left.Equals(right);
    public static bool operator !=(DateTime left, DateTime right) => !left.Equals(right);
}
