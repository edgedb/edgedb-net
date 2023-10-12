using System.Diagnostics.CodeAnalysis;

namespace EdgeDB.DataTypes;

/// <summary>
///     A struct representing a span of time.
/// </summary>
public readonly struct Duration
{
    /// <summary>
    ///     Gets a <see cref="System.TimeSpan" /> that represents the current <see cref="Duration" />.
    /// </summary>
    public TimeSpan TimeSpan
        => TemporalCommon.TimeSpanFromMicroseconds(Microseconds);

    /// <summary>
    ///     Gets the microsecond component of this <see cref="Duration" />.
    /// </summary>
    public long Microseconds { get; }

    /// <summary>
    ///     Constructs a new <see cref="Duration" />.
    /// </summary>
    /// <param name="microseconds">The microsecond component of this duration.</param>
    public Duration(long microseconds)
    {
        Microseconds = microseconds;
    }

    /// <summary>
    ///     Constructs a new <see cref="Duration" />.
    /// </summary>
    /// <param name="timespan">A timespan used to contruct this duration.</param>
    /// <remarks>
    ///     The provided <see cref="System.TimeSpan" /> will be rounded to the nearest microsecond.
    /// </remarks>
    public Duration(TimeSpan timespan)
        : this(TemporalCommon.ToMicroseconds(timespan))
    {
    }

    /// <inheritdoc />
    public override bool Equals([NotNullWhen(true)] object? obj)
    {
        if (obj is not Duration d)
            return false;

        return d.Microseconds == Microseconds;
    }

    /// <inheritdoc />
    public override int GetHashCode() => Microseconds.GetHashCode();

    public static implicit operator TimeSpan(Duration t) => t.TimeSpan;
    public static implicit operator Duration(TimeSpan t) => new(t);

    public static bool operator ==(Duration left, Duration right) => left.Equals(right);
    public static bool operator !=(Duration left, Duration right) => !left.Equals(right);
}
