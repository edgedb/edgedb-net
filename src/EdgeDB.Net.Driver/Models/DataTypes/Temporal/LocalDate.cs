using System.Diagnostics.CodeAnalysis;

namespace EdgeDB.DataTypes;

/// <summary>
///     A struct representing a date without a timezone.
/// </summary>
public readonly struct LocalDate
{
    /// <summary>
    ///     Gets a <see cref="System.DateOnly" /> that represents the current <see cref="LocalDate" />.
    /// </summary>
    public DateOnly DateOnly
        => DateOnly.FromDateTime(TemporalCommon.EdgeDBEpocDateTimeUTC.AddDays(Days).DateTime);

    /// <summary>
    ///     Gets the days component of this <see cref="LocalDate" />; representing the number of
    ///     days since January 1st 2000.
    /// </summary>
    public int Days { get; }

    /// <summary>
    ///     Constructs a new <see cref="LocalDate" />.
    /// </summary>
    /// <param name="days">The number of days since January 1st 2000.</param>
    public LocalDate(int days)
    {
        Days = days;
    }

    /// <summary>
    ///     Constructs a new <see cref="LocalDate" />
    /// </summary>
    /// <param name="date">The <see cref="System.DateOnly" /> used to construct this <see cref="LocalDate" />.</param>
    public LocalDate(DateOnly date)
        : this(TemporalCommon.ToDays(date))
    {
    }

    /// <inheritdoc />
    public override bool Equals([NotNullWhen(true)] object? obj)
    {
        if (obj is not LocalDate d)
            return false;

        return d.Days == Days;
    }

    /// <inheritdoc />
    public override int GetHashCode() => Days.GetHashCode();


    public static implicit operator DateOnly(LocalDate t) => t.DateOnly;
    public static implicit operator LocalDate(DateOnly t) => new(t);

    public static bool operator ==(LocalDate left, LocalDate right) => left.Equals(right);
    public static bool operator !=(LocalDate left, LocalDate right) => !left.Equals(right);
}
