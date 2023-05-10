using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.DataTypes
{
    /// <summary>
    ///     A struct representing a time without a timezone.
    /// </summary>
    public readonly struct LocalTime : IComparable<LocalTime>, IComparable
    {
        /// <summary>
        ///     Gets a <see cref="System.TimeOnly"/> that represents this <see cref="LocalTime"/>.
        /// </summary>
        public TimeOnly TimeOnly
            => TemporalCommon.TimeOnlyFromMicroseconds(_microseconds);

        /// <summary>
        ///     Gets a <see cref="System.TimeSpan"/> that represents this <see cref="LocalTime"/>.
        /// </summary>
        public TimeSpan TimeSpan
            => TimeOnly.ToTimeSpan();

        /// <summary>
        ///     Gets the microsecond component of this <see cref="LocalTime"/>; representing the
        ///     number of microseconds since midnight.
        /// </summary>
        public long Microseconds
            => _microseconds;

        private readonly long _microseconds;

        /// <summary>
        ///     Constructs a new <see cref="LocalTime"/>.
        /// </summary>
        /// <param name="microseconds">The number of microseconds since midnight.</param>
        public LocalTime(long microseconds)
        {
            _microseconds = microseconds;
        }

        /// <summary>
        ///     Constructs a new <see cref="LocalTime"/>.
        /// </summary>
        /// <param name="timeOnly">The <see cref="System.TimeOnly"/> used to construct this <see cref="LocalTime"/>.</param>
        /// <remarks>
        ///     The provided <see cref="System.TimeOnly"/> will be rounded to the nearest microsecond.
        /// </remarks>
        public LocalTime(TimeOnly timeOnly)
            : this(TemporalCommon.ToMicroseconds(timeOnly))
        { }

        /// <summary>
        ///     Constructs a new <see cref="LocalTime"/>.
        /// </summary>
        /// <param name="timespan">The <see cref="System.TimeSpan"/> used to construct this <see cref="LocalTime"/>.</param>
        /// <remarks>
        ///     The provided <see cref="System.TimeSpan"/> will be rounded to the nearest microsecond.
        /// </remarks>
        public LocalTime(TimeSpan timespan)
            : this(TemporalCommon.ToMicroseconds(timespan))
        { }

        /// <inheritdoc/>
        public override bool Equals([NotNullWhen(true)] object? obj)
        {
            if (obj is not LocalTime d)
                return false;

            return d._microseconds == _microseconds;
        }

        /// <inheritdoc/>
        public override int GetHashCode() => _microseconds.GetHashCode();

        public int CompareTo(LocalTime other)
        {
            return TimeSpan.CompareTo(other.TimeSpan);
        }

        public int CompareTo(object? obj)
        {
            if (obj is null)
                return 1;

            if (obj is not LocalTime time)
                throw new ArgumentException("Argument type must be a LocalTime");

            return CompareTo(time);
        }

        public static implicit operator TimeOnly(LocalTime t) => t.TimeOnly;
        public static implicit operator TimeSpan(LocalTime t) => t.TimeSpan;
        
        public static implicit operator LocalTime(TimeOnly t) => new(t);
        public static implicit operator LocalTime(TimeSpan t) => new(t);

        public static bool operator ==(LocalTime left, LocalTime right) => left.Equals(right);
        public static bool operator !=(LocalTime left, LocalTime right) => !left.Equals(right);
    }
}
