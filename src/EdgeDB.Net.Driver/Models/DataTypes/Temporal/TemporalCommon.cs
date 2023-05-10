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
        public static readonly DateTimeOffset EdgeDBEpocDateTimeUTC = new(2000, 1, 1, 0, 0, 0, TimeSpan.Zero);

        private const long MicrosecondsPerDay = 86400000000;
        private const long MicrosecondsPerMonth = MicrosecondsPerDay * 31;
        private const int TicksPerMicrosecond = 10;

        public static DateTimeOffset DateTimeOffsetFromMicroseconds(long microseconds, bool preserveTimezone = false)
        {
            var time = 
#if NET7_0_OR_GREATER
                EdgeDBEpocDateTimeUTC.AddMicroseconds(microseconds);
#else
                EdgeDBEpocDateTimeUTC.AddMilliseconds(microseconds / 1000d); // maintains the precision as we divide by a double
#endif
            return preserveTimezone
                ? time.ToOffset(TimeZoneInfo.Local.GetUtcOffset(time))
                : time;
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
            var offset = datetime - EdgeDBEpocDateTimeUTC;

            return
#if NET7_0_OR_GREATER
                (long)Math.Round(offset.TotalMicroseconds);
#else
                (long)Math.Round((double)offset.Ticks / TicksPerMicrosecond);
#endif
        }

        public static long ToMicroseconds(TimeOnly time)
        {
            return ToMicroseconds(time.ToTimeSpan());
        }

        public static long ToMicroseconds(TimeSpan timespan)
        {
            return
#if NET7_0_OR_GREATER
                (long)Math.Round(timespan.TotalMicroseconds);
#else
                (long)Math.Round(timespan.TotalMilliseconds * 1000);
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
            return (int)Math.Round((date.ToDateTime(TimeOnly.MinValue) - EdgeDBEpocDateTimeUTC).TotalDays);
        }
    }
}
