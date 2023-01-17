using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.DataTypes
{
    public readonly struct Duration
    {
        public TimeSpan TimeSpan
            => TemporalCommon.TimeSpanFromMicroseconds(Microseconds);

        public readonly long Microseconds;

        internal Duration(long microseconds)
        {
            Microseconds = microseconds;   
        }

        public Duration(TimeSpan time)
            : this(TemporalCommon.ToMicroseconds(time))
        { }

        /// <inheritdoc/>
        public override bool Equals([NotNullWhen(true)] object? obj)
        {
            if (obj is not Duration d)
                return false;

            return d.Microseconds == Microseconds;
        }

        /// <inheritdoc/>
        public override int GetHashCode() => Microseconds.GetHashCode();

        public static implicit operator TimeSpan(Duration t) => t.TimeSpan;
        public static implicit operator Duration(TimeSpan t) => new(t);

        public static bool operator ==(Duration left, Duration right) => left.Equals(right);
        public static bool operator !=(Duration left, Duration right) => !left.Equals(right);
    }
}
