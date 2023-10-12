using System.Diagnostics.CodeAnalysis;

namespace EdgeDB.DataTypes;

/// <summary>
///     A struct representing a span of time in days.
/// </summary>
/// <remarks>
///     This type is only available in EdgeDB 2.0 or later.
/// </remarks>
public readonly struct DateDuration
{
    /// <summary>
    ///     Gets a <see cref="System.TimeSpan" /> that represents the current <see cref="DateDuration" />.
    /// </summary>
    public TimeSpan TimeSpan
        => TimeSpan.FromDays(Days + Months * 31);

    /// <summary>
    ///     Gets the days component of this <see cref="DateDuration" />.
    /// </summary>
    public int Days { get; }

    /// <summary>
    ///     Gets the months component of this <see cref="DateDuration" />.
    /// </summary>
    public int Months { get; }

    /// <summary>
    ///     Constructs a new <see cref="DateDuration" /> with the given days and months.
    /// </summary>
    /// <param name="days">The amount of days this <see cref="DateDuration" /> has.</param>
    /// <param name="months">The amount of months this <see cref="DateDuration" /> has.</param>
    public DateDuration(int days = 0, int months = 0)
    {
        Days = days;
        Months = months;
    }

    /// <summary>
    ///     Constructs a new <see cref="DateDuration" /> from a given timespan.
    /// </summary>
    /// <param name="timespan">The timespan to use to contruct this <see cref="DateDuration" />.</param>
    public DateDuration(TimeSpan timespan)
    {
        Days = (int)Math.Truncate(timespan.TotalDays) % 31;
        Months = (int)Math.Floor((int)Math.Truncate(timespan.TotalDays) / 31d);
    }

    /// <inheritdoc />
    public override bool Equals([NotNullWhen(true)] object? obj)
    {
        if (obj is not DateDuration dt)
            return false;

        return dt.Days == Days && dt.Months == Months;
    }

    /// <inheritdoc />
    public override int GetHashCode()
        => base.GetHashCode();

    public static implicit operator TimeSpan(DateDuration t) => t.TimeSpan;
    public static implicit operator DateDuration(TimeSpan t) => new(t);

    public static bool operator ==(DateDuration left, DateDuration right) => left.Equals(right);
    public static bool operator !=(DateDuration left, DateDuration right) => !left.Equals(right);
}
