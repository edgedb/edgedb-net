using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB
{
    /// <summary>
    ///     Marks this class or struct as a valid type to use when serializing/deserializing.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class EdgeDBType : Attribute
    {
        internal readonly string? Name;

        /// <summary>
        ///     Marks this as a valid target to use when serializing/deserializing.
        /// </summary>
        /// <param name="name">The name of the type in the edgedb schema.</param>
        public EdgeDBType(string name)
        {
            Name = name;
        }

        /// <summary>
        ///     Marks this as a valid target to use when serializing/deserializing.
        /// </summary>
        public EdgeDBType() { }
    }
}
