using System.Diagnostics.CodeAnalysis;
using SysDateTime = System.DateTime;

namespace EdgeDB.DataTypes;

/// <summary>
///     A struct representing a date and time without a timezone.
/// </summary>
public readonly struct LocalDateTime
{
    /// <summary>
    ///     Gets a <see cref="System.DateTimeOffset" /> that represents this <see cref="LocalDateTime" />.
    /// </summary>
    public DateTimeOffset DateTimeOffset
        => TemporalCommon.DateTimeOffsetFromMicroseconds(Microseconds, true);

    /// <summary>
    ///     Gets a <see cref="SysDateTime" /> that represents this <see cref="LocalDateTime" />
    /// </summary>
    public SysDateTime DateTime
        => DateTimeOffset.DateTime;

    /// <summary>
    ///     Gets the microsecond component of this <see cref="LocalDateTime" />; representing the
    ///     amount of microseconds since January 1st 2000, 00:00.
    /// </summary>
    public long Microseconds { get; }

    /// <summary>
    ///     Constucts a new <see cref="LocalDateTime" />.
    /// </summary>
    /// <param name="microsecond">The number of microseconds since January 1st 2000, 00:00.</param>
    public LocalDateTime(long microsecond)
    {
        Microseconds = microsecond;
    }

    /// <summary>
    ///     Constucts a new <see cref="LocalDateTime" />.
    /// </summary>
    /// <param name="datetime">The <see cref="SysDateTime" /> used to construct this <see cref="LocalDateTime" />.</param>
    /// <remarks>
    ///     The provided <see cref="SysDateTime" /> will be rounded to the nearest microsecond.
    /// </remarks>
    public LocalDateTime(SysDateTime datetime)
        : this((DateTimeOffset)datetime)
    {
    }

    /// <summary>
    ///     Constucts a new <see cref="LocalDateTime" />.
    /// </summary>
    /// <param name="datetime">The <see cref="System.DateTimeOffset" /> used to construct this <see cref="LocalDateTime" />.</param>
    /// <remarks>
    ///     The provided <see cref="System.DateTimeOffset" /> will be rounded to the nearest microsecond.
    /// </remarks>
    public LocalDateTime(DateTimeOffset datetime)
    {
        Microseconds = TemporalCommon.ToMicroseconds(datetime);
    }

    /// <inheritdoc />
    public override bool Equals([NotNullWhen(true)] object? obj)
    {
        if (obj is not LocalDateTime d)
            return false;

        return d.Microseconds == Microseconds;
    }

    /// <inheritdoc />
    public override int GetHashCode() => Microseconds.GetHashCode();

    /// <summary>
    ///     Gets a <see cref="LocalDateTime" /> object whos date and time are set to the current UTC time.
    /// </summary>
    public static LocalDateTime Now => new(DateTimeOffset.UtcNow);

    public static implicit operator SysDateTime(LocalDateTime t) => t.DateTimeOffset.DateTime;
    public static implicit operator DateTimeOffset(LocalDateTime t) => t.DateTimeOffset;

    public static implicit operator LocalDateTime(SysDateTime t) => new(t);
    public static implicit operator LocalDateTime(DateTimeOffset t) => new(t);

    public static bool operator ==(LocalDateTime left, LocalDateTime right) => left.Equals(right);
    public static bool operator !=(LocalDateTime left, LocalDateTime right) => !left.Equals(right);
}
