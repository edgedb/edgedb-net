using EdgeDB.TypeConverters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class EdgeDBTypeConverterAttribute : Attribute
    {
        internal IEdgeDBTypeConverter Converter;

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
}
