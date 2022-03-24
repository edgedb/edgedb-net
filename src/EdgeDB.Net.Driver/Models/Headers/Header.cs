using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Models
{
    /// <summary>
    ///     Represents a dynamic header received in a <see cref="IReceiveable"/>.
    /// </summary>
    public struct Header
    {
        /// <summary>
        ///     Gets the code of the header.
        /// </summary>
        public ushort Code { get; internal set; }

        /// <summary>
        ///     Gets the value stored within this header.
        /// </summary>
        public byte[] Value { get; internal set; }

        /// <summary>
        ///     Converts this headers value to a UTF8 encoded string
        /// </summary>
        public override string ToString()
        {
            return Encoding.UTF8.GetString(Value);
        }
    }
}
