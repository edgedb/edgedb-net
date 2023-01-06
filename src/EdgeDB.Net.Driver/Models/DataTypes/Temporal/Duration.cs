using System;
using System.Collections.Generic;
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

        public static implicit operator TimeSpan(Duration t) => t.TimeSpan;
        public static implicit operator Duration(TimeSpan t) => new(t);
    }
}
