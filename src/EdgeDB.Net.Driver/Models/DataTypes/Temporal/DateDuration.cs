using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.DataTypes
{
    public readonly struct DateDuration
    {
        public TimeSpan TimeSpan
            => TimeSpan.FromDays(_days + _months * 31);

        public int Days
            => _days;

        public int Months
            => _months;

        private readonly int _days;
        private readonly int _months;

        public DateDuration(int days = 0, int months = 0)
        {
            _days = days;
            _months = months;
        }

        public DateDuration(TimeSpan timespan)
        {
            _days = (int)Math.Truncate(timespan.TotalDays) % 31;
            _months = (int)Math.Floor((int)Math.Truncate(timespan.TotalDays) / 31d);
        }

        public static implicit operator TimeSpan(DateDuration t) => t.TimeSpan;
        public static implicit operator DateDuration(TimeSpan t) => new(t);
    }
}
