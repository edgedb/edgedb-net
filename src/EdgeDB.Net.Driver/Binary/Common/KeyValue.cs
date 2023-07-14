using EdgeDB.Binary.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Binary
{
    /// <summary>
    ///     Represents a dynamic key-value pair received in a <see cref="IReceiveable"/>.
    /// </summary>
    internal readonly struct KeyValue
    {
        /// <summary>
        ///     Gets the key code.
        /// </summary>
        public ushort Code { get; init; }

        /// <summary>
        ///     Gets the value stored within this keyvalue.
        /// </summary>
        public byte[] Value { get; init; }

        internal KeyValue(ushort code, byte[] value)
        {
            Code = code;
            Value = value;
        }

        /// <summary>
        ///     Converts this headers value to a UTF8 encoded string
        /// </summary>
        public override string ToString()
            => Encoding.UTF8.GetString(Value);

        internal int ToInt()
            => int.Parse(ToString());
    }
}
