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
        public Guid CodecId { get; }
        public byte[] CodecDescriptor { get; }

        public MissingCodecException(string message, Guid id, byte[] descriptor)
            : base(message)
        {
            CodecId = id;
            CodecDescriptor = descriptor;
        }
    }
}
