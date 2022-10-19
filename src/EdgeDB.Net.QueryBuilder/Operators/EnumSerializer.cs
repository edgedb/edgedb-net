using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB
{
    [AttributeUsage(AttributeTargets.Enum)]
    public class EnumSerializerAttribute : Attribute
    {
        internal readonly SerializationMethod Method;

        /// <summary>
        ///     Marks this enum to be serialized in a certian way.
        /// </summary>
        /// <param name="method">The serialization method to use when serializing this enum.</param>
        public EnumSerializerAttribute(SerializationMethod method)
        {
            Method = method;
        }
    }

    public enum SerializationMethod
    {
        /// <summary>
        ///     Converts the name of the enums value ex: Day to the lowercase representation "day" as a string.
        /// </summary>
        Lower,

        /// <summary>
        ///     Converts the value of the enum to the numeric value.
        /// </summary>
        Numeric,
    }
}
