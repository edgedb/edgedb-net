using EdgeDB.TypeConverters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB
{
    internal sealed class EdgeDBPropertyInfo
    {
        public string PropertyName
            => _property.Name;

        public string EdgeDBName
            => AttributeName ?? TypeBuilder.SchemaNamingStrategy.Convert(_property);

        public string? AttributeName
            => _propertyAttribute?.Name;

        public IEdgeDBTypeConverter? CustomConverter
            => _typeConverter?.Converter;

        public Type Type
            => _property.PropertyType;

        public bool IsIgnored
            => _ignore is not null;

        private readonly EdgeDBPropertyAttribute? _propertyAttribute;
        private readonly EdgeDBTypeConverterAttribute? _typeConverter;
        private readonly EdgeDBIgnoreAttribute? _ignore;
        private readonly PropertyInfo _property;
        public EdgeDBPropertyInfo(PropertyInfo propInfo)
        {
            _property = propInfo;
            _propertyAttribute = propInfo.GetCustomAttribute<EdgeDBPropertyAttribute>();
            _typeConverter = propInfo.GetCustomAttribute<EdgeDBTypeConverterAttribute>();
            _ignore = propInfo.GetCustomAttribute<EdgeDBIgnoreAttribute>();
        }

        public object? ConvertToPropertyType(object? value)
        {
            if (value is null)
                return ReflectionUtils.GetDefault(Type);

            // check if we can use the custom converter
            if (CustomConverter is not null && CustomConverter.CanConvert(Type, value.GetType()))
            {
                return CustomConverter.ConvertFrom(value);
            }

            return ObjectBuilder.ConvertTo(Type, value);
        }

        public void ConvertAndSetValue(object instance, object? value)
        {
            var converted = ConvertToPropertyType(value);
            _property.SetValue(instance, converted);
        }
    }
}
