using EdgeDB.Binary.Codecs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.DataTypes
{
    internal class TemporalCommon
    {
        public static readonly DateTimeOffset EdgeDBEpocDateTime = new(2000, 1, 1, 0, 0, 0, TimeSpan.Zero);

        private const long MicrosecondsPerDay = 86400000000;
        private const long MicrosecondsPerMonth = MicrosecondsPerDay * 31;

        public static DateTimeOffset DateTimeFromMicroseconds(long microseconds)
        {
            return
#if NET7_0_OR_GREATER
                EdgeDBEpocDateTime.AddMicroseconds(microseconds);
#else
                EdgeDBEpocDateTime.AddMilliseconds(microseconds / 1000d); // maintains the precision as we divide by a double
#endif
        }

        public static TimeOnly TimeOnlyFromMicroseconds(long microseconds)
            => TimeOnly.FromTimeSpan(TimeSpanFromMicroseconds(microseconds));

        public static TimeSpan TimeSpanFromMicroseconds(long microseconds)
        {
            return
#if NET7_0_OR_GREATER
                TimeSpan.FromMicroseconds(microseconds);
#else
                TimeSpan.FromMilliseconds(microseconds / 1000d); // maintains the precision as we divide by a double
#endif
        }

        public static TimeSpan FromComponents(long microseconds, int days, int months)
        {
            return TimeSpan.FromDays(days + months * 31) +
#if NET7_0_OR_GREATER
                TimeSpan.FromMicroseconds(microseconds);
#else
                TimeSpan.FromMilliseconds(microseconds / 1000d);
#endif
        }

        public static long ToMicroseconds(DateTimeOffset datetime)
        {
            var offset = datetime - EdgeDBEpocDateTime;

            return
#if NET7_0_OR_GREATER
                (long)Math.Truncate(offset.TotalMicroseconds);
#else
                (long)Math.Truncate(offset.TotalMilliseconds * 1000);
#endif
        }

        public static long ToMicroseconds(TimeOnly time)
        {
            return
#if NET7_0_OR_GREATER
                time.Microsecond;
#else
                ToMicroseconds(time.ToTimeSpan());
#endif
        }

        public static long ToMicroseconds(TimeSpan timespan)
        {
            return
#if NET7_0_OR_GREATER
                (long)Math.Truncate(timespan.TotalMicroseconds);
#else
                (long)Math.Truncate(timespan.TotalMilliseconds * 1000);
#endif
        }

        public static (long microseconds, int days, int months) ToComponents(TimeSpan timespan)
        {
            var microseconds = ToMicroseconds(timespan);

            var days = 0;
            var months = 0;

            for (; microseconds >= MicrosecondsPerMonth; microseconds -= MicrosecondsPerMonth)
            {
                months++;
            }

            for (; microseconds >= MicrosecondsPerDay; microseconds -= MicrosecondsPerDay)
            {
                days++;
            }

            return (microseconds, days, months);
        }

        public static int ToDays(DateOnly date)
        {
            return (int)Math.Truncate((date.ToDateTime(TimeOnly.MinValue) - EdgeDBEpocDateTime).TotalDays);
        }
    }
}
