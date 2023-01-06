using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SysDateTime = System.DateTime;
using static EdgeDB.DataTypes.DateTime;

namespace EdgeDB.DataTypes
{
    /// <summary>
    ///     A struct representing the <c>std::local_datetime</c> type in EdgeDB.
    /// </summary>
    public readonly struct LocalDateTime
    {
        /// <summary>
        ///     Gets the <see cref="DateTimeOffset"/> that represents the
        ///     current <see cref="DateTime"/>.
        /// </summary>
        public DateTimeOffset DateTimeOffset
            => TemporalCommon.DateTimeFromMicroseconds(Microseconds);

        public readonly long Microseconds;

        internal LocalDateTime(long microsecond)
        {
            Microseconds = microsecond;
        }

        /// <summary>
        ///     Creates a new <see cref="LocalDateTime"/>.
        /// </summary>
        /// <param name="datetime">The value of the date time.</param>
        public LocalDateTime(SysDateTime datetime)
            : this((DateTimeOffset)datetime)
        {

        }

        /// <summary>
        ///     Creates a new <see cref="LocalDateTime"/>.
        /// </summary>
        /// <param name="datetime">The value of the date time.</param>
        public LocalDateTime(DateTimeOffset datetime)
        {
            Microseconds = TemporalCommon.ToMicroseconds(datetime);
        }

        /// <summary>
        ///     Gets a <see cref="LocalDateTime"/> object whos date and time are set to the current UTC time.
        /// </summary>
        public static LocalDateTime Now => new(DateTimeOffset.UtcNow);

        public static implicit operator SysDateTime(LocalDateTime t) => t.DateTimeOffset.DateTime;
        public static implicit operator DateTimeOffset(LocalDateTime t) => t.DateTimeOffset;

        public static implicit operator LocalDateTime(SysDateTime t) => new(t);
        public static implicit operator LocalDateTime(DateTimeOffset t) => new(t);
    }
}
