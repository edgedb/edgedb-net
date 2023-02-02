using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.DataTypes
{
    /// <summary>
    ///     A struct representing a relative span of time.
    /// </summary>
    public readonly struct RelativeDuration
    {
        /// <summary>
        ///     Gets a <see cref="System.TimeSpan"/> that represents the current <see cref="RelativeDuration"/>.
        /// </summary>
        public TimeSpan TimeSpan
            => TemporalCommon.FromComponents(_microseconds, _days, _months);

        /// <summary>
        ///     Gets the microsecond component of this <see cref="RelativeDuration"/>.
        /// </summary>
        public long Microseconds
            => _microseconds;

        /// <summary>
        ///     Gets the days component of this <see cref="RelativeDuration"/>.
        /// </summary>
        public int Days
            => _days;

        /// <summary>
        ///     Gets the months component of this <see cref="RelativeDuration"/>.
        /// </summary>
        public int Months
            => _months;

        private readonly long _microseconds;
        private readonly int _days;
        private readonly int _months;

        /// <summary>
        ///     Constructs a new <see cref="RelativeDuration"/>.
        /// </summary>
        /// <param name="microseconds">The microsecond component.</param>
        /// <param name="days">The days component.</param>
        /// <param name="months">The months component,</param>
        public RelativeDuration(long microseconds = 0, int days = 0, int months = 0)
        {
            _microseconds = microseconds;
            _days = days;
            _months = months;
        }

        /// <summary>
        ///     Constructs a new <see cref="RelativeDuration"/>.
        /// </summary>
        /// <param name="timespan">The timespan used to construct this <see cref="RelativeDuration"/>.</param>
        /// <remarks>
        ///     The provided <see cref="System.TimeSpan"/> will be rounded to the nearest microsecond.
        /// </remarks>
        public RelativeDuration(TimeSpan timespan)
        {
            var (microseconds, days, months) = TemporalCommon.ToComponents(timespan);

            _months = months;
            _days = days;
            _microseconds = microseconds;
        }

        /// <inheritdoc/>
        public override bool Equals([NotNullWhen(true)] object? obj)
        {
            if (obj is not RelativeDuration rd)
                return false;

            return
                _microseconds == rd._microseconds &&
                _days == rd._days &&
                _months == rd._months;
        }

        /// <inheritdoc/>
        public override int GetHashCode() => base.GetHashCode();

        public static implicit operator TimeSpan(RelativeDuration t) => t.TimeSpan;
        public static implicit operator RelativeDuration(TimeSpan t) => new(t);

        public static bool operator ==(RelativeDuration left, RelativeDuration right) => left.Equals(right);
        public static bool operator !=(RelativeDuration left, RelativeDuration right) => !left.Equals(right);
    }
}
