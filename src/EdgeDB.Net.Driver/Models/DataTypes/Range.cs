using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.DataTypes
{
    /// <summary>
    ///     Represents the <see href="https://www.edgedb.com/docs/stdlib/range">Range</see> type in EdgeDB.
    /// </summary>
    /// <typeparam name="T">The inner type of the range.</typeparam>
    public struct Range<T> : IRange
        where T : struct
    {
        /// <summary>
        ///     Gets the lower bound of the range.
        /// </summary>
        public T? Lower { get; init; }

        /// <summary>
        ///     Gets the upper bound of the range.
        /// </summary>
        public T? Upper { get; init; }

        /// <summary>
        ///     Gets whether or not the lower bound is included.
        /// </summary>
        public bool IncludeLower { get; init; }

        /// <summary>
        ///     Gets whether or not the upper bound is included.
        /// </summary>
        public bool IncludeUpper { get; init; }

        /// <summary>
        ///     Gets whether or not the range is empty.
        /// </summary>
        public bool IsEmpty { get; }

        /// <summary>
        ///     Constructs a new range type.
        /// </summary>
        /// <param name="lower">The lower bound of the range.</param>
        /// <param name="upper">The upper bound of the range.</param>
        /// <param name="includeLower">Whether or not to include the lower bound.</param>
        /// <param name="includeUpper">Whether or not to include the upper bound.</param>
        public Range(T? lower, T? upper, bool includeLower = true, bool includeUpper = false)
        {
            Lower = lower;
            Upper = upper;
            IncludeLower = includeLower;
            IncludeUpper = includeUpper;
            IsEmpty = !lower.HasValue && !upper.HasValue;
        }

        /// <summary>
        ///     Gets an empty range.
        /// </summary>
        /// <returns>An empty range.</returns>
        public static Range<T> Empty()
            => new(null, null);

        Type IRange.WrappingType => typeof(T);
    }

    internal interface IRange
    {
        Type WrappingType { get; }
    }
}
