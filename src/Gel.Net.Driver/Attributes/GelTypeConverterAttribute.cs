using Gel.TypeConverters;

namespace Gel;

/// <summary>
///     Marks the current property to be deserialized/serialized with a specific
///     <see cref="GelTypeConverter{TSource,TTarget}" />.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class GelTypeConverterAttribute : Attribute
{
    internal IGelTypeConverter Converter;

    /// <summary>
    ///     Initializes the <see cref="GelTypeConverterAttribute" /> with the
    ///     specified <see cref="GelTypeConverter{TSource, TTarget}" />.
    /// </summary>
    /// <param name="converterType">The type of the converter.</param>
    /// <exception cref="ArgumentException">
    ///     <paramref name="converterType" /> is not a valid
    ///     <see cref="GelTypeConverter{TSource, TTarget}" />.
    /// </exception>
    public GelTypeConverterAttribute(Type converterType)
    {
        if (converterType.GetInterface(nameof(IGelTypeConverter)) is null)
        {
            throw new ArgumentException("Converter type must implement IGelTypeConverter");
        }

        if (converterType.IsAbstract || converterType.IsInterface)
        {
            throw new ArgumentException("Converter type must be a concrete type");
        }

        Converter = (IGelTypeConverter)Activator.CreateInstance(converterType)!;
    }
}
