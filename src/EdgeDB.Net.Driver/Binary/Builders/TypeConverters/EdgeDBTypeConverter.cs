using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.TypeConverters
{
    /// <summary>
    ///     Represents a generic client-side type converter.
    /// </summary>
    /// <typeparam name="TSource">The client-side type which the converter is responsible for converting.</typeparam>
    /// <typeparam name="TTarget">The database-side type which the converter is responsible for converting to.</typeparam>
    public abstract class EdgeDBTypeConverter<TSource, TTarget> : IEdgeDBTypeConverter
    {
        /// <summary>
        ///     Converts the given <typeparamref name="TTarget"/> to a <typeparamref name="TSource"/>.
        /// </summary>
        /// <param name="value">The value to convert to a <typeparamref name="TSource"/>.</param>
        /// <returns>
        ///     An instance of <typeparamref name="TSource"/>; or <see langword="default"/>.
        /// </returns>
        public abstract TSource? ConvertFrom(TTarget? value);

        /// <summary>
        ///     Converts the given <typeparamref name="TSource"/> to a <typeparamref name="TTarget"/>.
        /// </summary>
        /// <param name="value">The value to convert to a <typeparamref name="TTarget"/>.</param>
        /// <returns>An instance of <typeparamref name="TTarget"/>; or <see langword="default"/>.</returns>
        public abstract TTarget? ConvertTo(TSource? value);

        object? IEdgeDBTypeConverter.ConvertFrom(object? value)
            => ConvertFrom((TTarget?)value);
        object? IEdgeDBTypeConverter.ConvertTo(object? value)
            => ConvertTo((TSource?)value);
        bool IEdgeDBTypeConverter.CanConvert(System.Type from, System.Type to)
            => from == typeof(TSource) && to == typeof(TTarget);
    }

    internal interface IEdgeDBTypeConverter
    {
        object? ConvertFrom(object? value);
        object? ConvertTo(object? value);

        bool CanConvert(Type from, Type to);
    }
}
