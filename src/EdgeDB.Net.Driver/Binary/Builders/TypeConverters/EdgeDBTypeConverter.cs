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

        /// <inheritdoc/>
        public virtual bool CanConvert(Type from, Type to)
            => from == typeof(TSource) && to == typeof(TTarget);

        object? IEdgeDBTypeConverter.ConvertFrom(object? value)
            => ConvertFrom((TTarget?)value);
        object? IEdgeDBTypeConverter.ConvertTo(object? value)
            => ConvertTo((TSource?)value);
        
        Type IEdgeDBTypeConverter.Source => typeof(TSource);
        Type IEdgeDBTypeConverter.Target => typeof(TTarget);
    }

    /// <summary>
    ///     Represents a custom type converter capable of converting
    ///     one type to another.
    /// </summary>
    public interface IEdgeDBTypeConverter
    {
        /// <summary>
        ///     Converts the given target value to the source value.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        object? ConvertFrom(object? value);

        /// <summary>
        ///     Converts the given source value to a the target value.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        object? ConvertTo(object? value);

        /// <summary>
        ///     Checks if the type builder can convert one type to another.
        /// </summary>
        /// <param name="from">The source type.</param>
        /// <param name="to">The target type.</param>
        /// <returns>
        ///     <see langword="true"/> if the source type can be converted to the target type;
        ///     otherwise <see langword="false"/>.
        /// </returns>
        bool CanConvert(Type from, Type to);

        /// <summary>
        ///     Gets the source type of the converter.
        /// </summary>
        Type Source { get; }

        /// <summary>
        ///     Gets the target type of the converter.
        /// </summary>
        Type Target { get; }
    }
}
