using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.DataTypes
{
    public readonly struct RelativeDuration
    {
        public TimeSpan TimeSpan
            => TemporalCommon.FromComponents(Microseconds, Days, Months);

        public readonly long Microseconds;
        public readonly int Days;
        public readonly int Months;

        internal RelativeDuration(long microseconds, int days, int months)
        {
            Microseconds = microseconds;
            Days = days;
            Months = months;
        }

        public RelativeDuration(TimeSpan timespan)
        {
            var (microseconds, days, months) = TemporalCommon.ToComponents(timespan);

            Months = months;
            Days = days;
            Microseconds = microseconds;
        }

        public static implicit operator TimeSpan(RelativeDuration t) => t.TimeSpan;
        public static implicit operator RelativeDuration(TimeSpan t) => new(t);
    }
}
