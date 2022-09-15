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
    public readonly struct Annotation
    {
        public int Size => Encoding.UTF8.GetByteCount(Name) + Encoding.UTF8.GetByteCount(Value);

        /// <summary>
        ///     Gets the name of this annotation.
        /// </summary>
        public string Name { get; init; }

        /// <summary>
        ///     Gets the value of the annotation (in json format).
        /// </summary>
        public string Value { get; init; }

        internal Annotation(string name, string value)
        {
            Name = name;
            Value = value;
        }
    }
}
