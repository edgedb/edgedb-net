using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.DataTypes
{
    /// <summary>
    ///     A struct representing a span of time.
    /// </summary>
    public readonly struct Duration
    {
        /// <summary>
        ///     Gets a <see cref="System.TimeSpan"/> that represents the current <see cref="Duration"/>.
        /// </summary>
        public TimeSpan TimeSpan
            => TemporalCommon.TimeSpanFromMicroseconds(_microseconds);

        /// <summary>
        ///     Gets the microsecond component of this <see cref="Duration"/>.
        /// </summary>
        public long Microseconds
            => _microseconds;

        private readonly long _microseconds;

        /// <summary>
        ///     Constructs a new <see cref="Duration"/>.
        /// </summary>
        /// <param name="microseconds">The microsecond component of this duration.</param>
        public Duration(long microseconds)
        {
            _microseconds = microseconds;   
        }

        /// <summary>
        ///     Constructs a new <see cref="Duration"/>.
        /// </summary>
        /// <param name="timespan">A timespan used to contruct this duration.</param>
        /// <remarks>
        ///     The provided <see cref="System.TimeSpan"/> will be rounded to the nearest microsecond.
        /// </remarks>
        public Duration(TimeSpan timespan)
            : this(TemporalCommon.ToMicroseconds(timespan))
        { }

        /// <inheritdoc/>
        public override bool Equals([NotNullWhen(true)] object? obj)
        {
            if (obj is not Duration d)
                return false;

            return d._microseconds == _microseconds;
        }

        /// <inheritdoc/>
        public override int GetHashCode() => _microseconds.GetHashCode();

        public static implicit operator TimeSpan(Duration t) => t.TimeSpan;
        public static implicit operator Duration(TimeSpan t) => new(t);

        public static bool operator ==(Duration left, Duration right) => left.Equals(right);
        public static bool operator !=(Duration left, Duration right) => !left.Equals(right);
    }
}
