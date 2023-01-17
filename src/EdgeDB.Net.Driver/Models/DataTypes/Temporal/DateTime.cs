using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SysDateTime = System.DateTime;

namespace EdgeDB.DataTypes
{
    /// <summary>
    ///     A struct representing the <c>std::datetime</c> type in EdgeDB.
    /// </summary>
    public readonly struct DateTime
    {
        /// <summary>
        ///     Gets the <see cref="DateTimeOffset"/> that represents the
        ///     current <see cref="DateTime"/>.
        /// </summary>
        public DateTimeOffset DateTimeOffset
            => TemporalCommon.DateTimeOffsetFromMicroseconds(Microseconds, true);

        public readonly long Microseconds;

        internal DateTime(long microseconds)
        {
            Microseconds = microseconds;
        }

        /// <summary>
        ///     Creates a new <see cref="DateTime"/>.
        /// </summary>
        /// <param name="datetime">The value of the date time.</param>
        public DateTime(SysDateTime datetime)
            : this((DateTimeOffset)datetime)
        { }

        /// <summary>
        ///     Creates a new <see cref="DateTime"/>.
        /// </summary>
        /// <param name="datetime">The value of the date time.</param>
        public DateTime(DateTimeOffset datetime)
        {
            Microseconds = TemporalCommon.ToMicroseconds(datetime);
        }

        /// <inheritdoc/>
        public override bool Equals([NotNullWhen(true)] object? obj)
        {
            if (obj is not DateTime dt)
                return false;

            return dt.Microseconds == Microseconds;
        }

        /// <inheritdoc/>
        public override int GetHashCode()
            => Microseconds.GetHashCode();

        /// <summary>
        ///     Gets a <see cref="DateTime"/> object whos date and time are set to the current UTC time.
        /// </summary>
        public static DateTime Now => new(DateTimeOffset.UtcNow);

        public static implicit operator SysDateTime(DateTime t) => t.DateTimeOffset.DateTime;
        public static implicit operator DateTimeOffset(DateTime t) => t.DateTimeOffset;

        public static implicit operator DateTime(SysDateTime t) => new(t);
        public static implicit operator DateTime(DateTimeOffset t) => new(t);

        public static bool operator ==(DateTime left, DateTime right) => left.Equals(right);
        public static bool operator !=(DateTime left, DateTime right) => !left.Equals(right);
    }
}
