using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SysDateTime = System.DateTime;
using static EdgeDB.DataTypes.DateTime;
using System.Diagnostics.CodeAnalysis;

namespace EdgeDB.DataTypes
{
    /// <summary>
    ///     A struct representing the <c>std::local_datetime</c> type in EdgeDB.
    /// </summary>
    public readonly struct LocalDateTime
    {
        /// <summary>
        ///     Gets the <see cref="DateTimeOffset"/> that represents the
        ///     current <see cref="LocalDateTime"/>.
        /// </summary>
        public DateTimeOffset DateTimeOffset
            => TemporalCommon.DateTimeOffsetFromMicroseconds(Microseconds, true);

        /// <summary>
        ///     Gets the <see cref="SysDateTime"/> that represents the
        ///     current <see cref="LocalDateTime"/>
        /// </summary>
        public SysDateTime DateTime
            => DateTimeOffset.DateTime;

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

        /// <inheritdoc/>
        public override bool Equals([NotNullWhen(true)] object? obj)
        {
            if (obj is not LocalDateTime d)
                return false;

            return d.Microseconds == Microseconds;
        }

        /// <inheritdoc/>
        public override int GetHashCode() => Microseconds.GetHashCode();

        /// <summary>
        ///     Gets a <see cref="LocalDateTime"/> object whos date and time are set to the current UTC time.
        /// </summary>
        public static LocalDateTime Now => new(DateTimeOffset.UtcNow);

        public static implicit operator SysDateTime(LocalDateTime t) => t.DateTimeOffset.DateTime;
        public static implicit operator DateTimeOffset(LocalDateTime t) => t.DateTimeOffset;

        public static implicit operator LocalDateTime(SysDateTime t) => new(t);
        public static implicit operator LocalDateTime(DateTimeOffset t) => new(t);

        public static bool operator ==(LocalDateTime left, LocalDateTime right) => left.Equals(right);
        public static bool operator !=(LocalDateTime left, LocalDateTime right) => !left.Equals(right);
    }
}
