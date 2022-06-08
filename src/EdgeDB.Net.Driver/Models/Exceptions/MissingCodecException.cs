using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB
{
    /// <summary>
    ///     Represents an exception that occurs when the client doesn't 
    ///     have a codec for incoming or outgoing data.
    /// </summary>
    public class MissingCodecException : EdgeDBException
    {
        /// <summary>
        ///     Gets the unique identifier for the codec.
        /// </summary>
        public Guid CodecId { get; }

        /// <summary>
        ///     Gets the codec's descriptor
        /// </summary>
        public byte[] CodecDescriptor { get; }

        public MissingCodecException(string message, Guid id, byte[] descriptor)
            : base(message)
        {
            CodecId = id;
            CodecDescriptor = descriptor;
        }
    }
}
