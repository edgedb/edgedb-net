using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Binary
{
    /// <summary>
    ///     Represents a dynamic header received in a <see cref="IReceiveable"/>.
    /// </summary>
    public readonly struct Header
    {
        /// <summary>
        ///     Gets the code of the header.
        /// </summary>
        public ushort Code { get; init; }

        /// <summary>
        ///     Gets the value stored within this header.
        /// </summary>
        public byte[] Value { get; init; }

        internal Header(ushort code, byte[] value)
        {
            Code = code;
            Value = value;
        }

        /// <summary>
        ///     Converts this headers value to a UTF8 encoded string
        /// </summary>
        public override string ToString()
            => Encoding.UTF8.GetString(Value);
    }
}
