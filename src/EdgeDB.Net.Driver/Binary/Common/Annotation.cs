using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Binary
{
    /// <summary>
    ///     Represents an annotation within a packet.
    /// </summary>
    internal readonly struct Annotation
    {
        internal int Size => Encoding.UTF8.GetByteCount(Name) + Encoding.UTF8.GetByteCount(Value);

        /// <summary>
        ///     The name of this annotation.
        /// </summary>
        public readonly string Name;

        /// <summary>
        ///     The value of the annotation (in json format).
        /// </summary>
        public readonly string Value;

        internal Annotation(string name, string value)
        {
            Name = name;
            Value = value;
        }
    }
}
