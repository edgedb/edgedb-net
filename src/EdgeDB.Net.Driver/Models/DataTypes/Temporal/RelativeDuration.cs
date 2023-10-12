using System.Diagnostics.CodeAnalysis;

namespace EdgeDB.DataTypes;

/// <summary>
///     A struct representing a relative span of time.
/// </summary>
public readonly struct RelativeDuration
{
    /// <summary>
    ///     Gets a <see cref="System.TimeSpan" /> that represents the current <see cref="RelativeDuration" />.
    /// </summary>
    public TimeSpan TimeSpan
        => TemporalCommon.FromComponents(Microseconds, Days, Months);

    /// <summary>
    ///     Gets the microsecond component of this <see cref="RelativeDuration" />.
    /// </summary>
    public long Microseconds { get; }

    /// <summary>
    ///     Gets the days component of this <see cref="RelativeDuration" />.
    /// </summary>
    public int Days { get; }

    /// <summary>
    ///     Gets the months component of this <see cref="RelativeDuration" />.
    /// </summary>
    public int Months { get; }

    /// <summary>
    ///     Constructs a new <see cref="RelativeDuration" />.
    /// </summary>
    /// <param name="microseconds">The microsecond component.</param>
    /// <param name="days">The days component.</param>
    /// <param name="months">The months component,</param>
    public RelativeDuration(long microseconds = 0, int days = 0, int months = 0)
    {
        Microseconds = microseconds;
        Days = days;
        Months = months;
    }

    /// <summary>
    ///     Constructs a new <see cref="RelativeDuration" />.
    /// </summary>
    /// <param name="timespan">The timespan used to construct this <see cref="RelativeDuration" />.</param>
    /// <remarks>
    ///     The provided <see cref="System.TimeSpan" /> will be rounded to the nearest microsecond.
    /// </remarks>
    public RelativeDuration(TimeSpan timespan)
    {
        var (microseconds, days, months) = TemporalCommon.ToComponents(timespan);

        Months = months;
        Days = days;
        Microseconds = microseconds;
    }

    /// <inheritdoc />
    public override bool Equals([NotNullWhen(true)] object? obj)
    {
        if (obj is not RelativeDuration rd)
            return false;

        return
            Microseconds == rd.Microseconds &&
            Days == rd.Days &&
            Months == rd.Months;
    }

    /// <inheritdoc />
    public override int GetHashCode() => base.GetHashCode();

    public static implicit operator TimeSpan(RelativeDuration t) => t.TimeSpan;
    public static implicit operator RelativeDuration(TimeSpan t) => new(t);

    public static bool operator ==(RelativeDuration left, RelativeDuration right) => left.Equals(right);
    public static bool operator !=(RelativeDuration left, RelativeDuration right) => !left.Equals(right);
}
