using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.DataTypes
{
    /// <summary>
    ///     A struct representing a date without a timezone.
    /// </summary>
    public readonly struct LocalDate : IComparable<LocalDate>, IComparable
    {
        /// <summary>
        ///     Gets a <see cref="System.DateOnly"/> that represents the current <see cref="LocalDate"/>.
        /// </summary>
        public DateOnly DateOnly
            => DateOnly.FromDateTime(TemporalCommon.EdgeDBEpocDateTimeUTC.AddDays(_days).DateTime);

        /// <summary>
        ///     Gets the days component of this <see cref="LocalDate"/>; representing the number of
        ///     days since January 1st 2000.
        /// </summary>
        public int Days
            => _days;

        private readonly int _days;

        /// <summary>
        ///     Constructs a new <see cref="LocalDate"/>.
        /// </summary>
        /// <param name="days">The number of days since January 1st 2000.</param>
        public LocalDate(int days)
        {
            _days = days;
        }

        /// <summary>
        ///     Constructs a new <see cref="LocalDate"/>
        /// </summary>
        /// <param name="date">The <see cref="System.DateOnly"/> used to construct this <see cref="LocalDate"/>.</param>
        public LocalDate(DateOnly date)
            : this(TemporalCommon.ToDays(date))
        { }
        
        /// <inheritdoc/>
        public override bool Equals([NotNullWhen(true)] object? obj)
        {
            if (obj is not LocalDate d)
                return false;

            return d._days == _days;
        }

        /// <inheritdoc/>
        public override int GetHashCode() => _days.GetHashCode();

        public int CompareTo(LocalDate other)
        {
            return DateOnly.CompareTo(other.DateOnly);
        }

        public int CompareTo(object? obj)
        {
            if (obj is null)
                return 1;

            if (obj is not LocalDate date)
                throw new ArgumentException("Argument type must be a LocalDate");

            return CompareTo(date);
        }

        public static implicit operator DateOnly(LocalDate t) => t.DateOnly;
        public static implicit operator LocalDate(DateOnly t) => new(t);

        public static bool operator ==(LocalDate left, LocalDate right) => left.Equals(right);
        public static bool operator !=(LocalDate left, LocalDate right) => !left.Equals(right);
    }
}
