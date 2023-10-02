using EdgeDB.TypeConverters;
using System.Reflection;

namespace EdgeDB;

internal sealed class EdgeDBPropertyInfo
{
    private readonly EdgeDBIgnoreAttribute? _ignore;

    private readonly EdgeDBPropertyAttribute? _propertyAttribute;
    private readonly IEdgeDBTypeConverter? _typeConverter;
    private readonly EdgeDBTypeConverterAttribute? _typeConverterAttribute;

    public EdgeDBPropertyInfo(PropertyInfo propInfo)
    {
        PropertyInfo = propInfo;
        _propertyAttribute = propInfo.GetCustomAttribute<EdgeDBPropertyAttribute>();
        _typeConverterAttribute = propInfo.GetCustomAttribute<EdgeDBTypeConverterAttribute>();
        _ignore = propInfo.GetCustomAttribute<EdgeDBIgnoreAttribute>();

        if (TypeBuilder.TypeConverters.TryGetValue(PropertyInfo.PropertyType, out var converter))
            _typeConverter = converter;
    }

    public PropertyInfo PropertyInfo { get; }

    public string PropertyName
        => PropertyInfo.Name;

    public string EdgeDBName
        => AttributeName ?? TypeBuilder.SchemaNamingStrategy.Convert(PropertyInfo);

    public string? AttributeName
        => _propertyAttribute?.Name;

    public IEdgeDBTypeConverter? CustomConverter
        => _typeConverterAttribute?.Converter ?? _typeConverter;

    public Type Type
        => PropertyInfo.PropertyType;

    public bool IsIgnored
        => _ignore is not null;

    public object? ConvertToPropertyType(object? value)
    {
        if (value is null)
            return ReflectionUtils.GetDefault(Type);

        // check if we can use the custom converter
        if (CustomConverter is not null && CustomConverter.CanConvert(Type, value.GetType()))
        {
            CustomConverter.ValidateTargetType();

            return CustomConverter.ConvertFrom(value);
        }

        return ObjectBuilder.ConvertTo(Type, value);
    }

    public void ConvertAndSetValue(object instance, object? value)
    {
        var converted = ConvertToPropertyType(value);
        PropertyInfo.SetValue(instance, converted);
    }
}
