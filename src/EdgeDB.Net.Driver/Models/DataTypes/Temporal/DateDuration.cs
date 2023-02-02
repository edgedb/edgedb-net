using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.DataTypes
{
    /// <summary>
    ///     A struct representing a span of time in days.
    /// </summary>
    /// <remarks>
    ///     This type is only available in EdgeDB 2.0 or later.
    /// </remarks>
    public readonly struct DateDuration
    {
        /// <summary>
        ///     Gets a <see cref="System.TimeSpan"/> that represents the current <see cref="DateDuration"/>.
        /// </summary>
        public TimeSpan TimeSpan
            => TimeSpan.FromDays(_days + _months * 31);

        /// <summary>
        ///     Gets the days component of this <see cref="DateDuration"/>.
        /// </summary>
        public int Days
            => _days;

        /// <summary>
        ///     Gets the months component of this <see cref="DateDuration"/>.
        /// </summary>
        public int Months
            => _months;

        private readonly int _days;
        private readonly int _months;

        /// <summary>
        ///     Constructs a new <see cref="DateDuration"/> with the given days and months.
        /// </summary>
        /// <param name="days">The amount of days this <see cref="DateDuration"/> has.</param>
        /// <param name="months">The amount of months this <see cref="DateDuration"/> has.</param>
        public DateDuration(int days = 0, int months = 0)
        {
            _days = days;
            _months = months;
        }

        /// <summary>
        ///     Constructs a new <see cref="DateDuration"/> from a given timespan.
        /// </summary>
        /// <param name="timespan">The timespan to use to contruct this <see cref="DateDuration"/>.</param>
        public DateDuration(TimeSpan timespan)
        {
            _days = (int)Math.Truncate(timespan.TotalDays) % 31;
            _months = (int)Math.Floor((int)Math.Truncate(timespan.TotalDays) / 31d);
        }

        /// <inheritdoc/>
        public override bool Equals([NotNullWhen(true)] object? obj)
        {
            if (obj is not DateDuration dt)
                return false;

            return dt._days == _days && dt._months == _months;
        }

        /// <inheritdoc/>
        public override int GetHashCode()
            => base.GetHashCode();

        public static implicit operator TimeSpan(DateDuration t) => t.TimeSpan;
        public static implicit operator DateDuration(TimeSpan t) => new(t);

        public static bool operator ==(DateDuration left, DateDuration right) => left.Equals(right);
        public static bool operator !=(DateDuration left, DateDuration right) => !left.Equals(right);
    }
}
