using EdgeDB.TypeConverters;

namespace EdgeDB;

/// <summary>
///     Marks the current property to be deserialized/serialized with a specific
///     <see cref="EdgeDBTypeConverter{TSource,TTarget}" />.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class EdgeDBTypeConverterAttribute : Attribute
{
    internal IEdgeDBTypeConverter Converter;

    /// <summary>
    ///     Initializes the <see cref="EdgeDBTypeConverterAttribute" /> with the
    ///     specified <see cref="EdgeDBTypeConverter{TSource, TTarget}" />.
    /// </summary>
    /// <param name="converterType">The type of the converter.</param>
    /// <exception cref="ArgumentException">
    ///     <paramref name="converterType" /> is not a valid
    ///     <see cref="EdgeDBTypeConverter{TSource, TTarget}" />.
    /// </exception>
    public EdgeDBTypeConverterAttribute(Type converterType)
    {
        if (converterType.GetInterface(nameof(IEdgeDBTypeConverter)) is null)
        {
            throw new ArgumentException("Converter type must implement IEdgeDBTypeConverter");
        }

        if (converterType.IsAbstract || converterType.IsInterface)
        {
            throw new ArgumentException("Converter type must be a concrete type");
        }

        Converter = (IEdgeDBTypeConverter)Activator.CreateInstance(converterType)!;
    }
}
