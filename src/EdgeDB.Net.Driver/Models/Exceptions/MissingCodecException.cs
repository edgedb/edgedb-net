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
        ///     Constructs a new <see cref="MissingCodecException"/> with the specified error message.
        /// </summary>
        /// <param name="message">The error message describing why this exception was thrown.</param>
        public MissingCodecException(string message)
            : base(message)
        {
        }
    }
}
