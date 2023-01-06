using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.DataTypes
{
    public readonly struct LocalTime
    {
        public TimeOnly TimeOnly
            => TemporalCommon.TimeOnlyFromMicroseconds(Microseconds);

        public TimeSpan TimeSpan
            => TimeOnly.ToTimeSpan();

        public readonly long Microseconds;

        internal LocalTime(long microseconds)
        {
            Microseconds = microseconds;
        }

        public LocalTime(TimeOnly time)
            : this(TemporalCommon.ToMicroseconds(time))
        {
            
        }

        public LocalTime(TimeSpan time)
            : this(TemporalCommon.ToMicroseconds(time))
        {
            
        }

        public static implicit operator TimeOnly(LocalTime t) => t.TimeOnly;
        public static implicit operator TimeSpan(LocalTime t) => t.TimeSpan;
        
        public static implicit operator LocalTime(TimeOnly t) => new(t);
        public static implicit operator LocalTime(TimeSpan t) => new(t);
    }
}
