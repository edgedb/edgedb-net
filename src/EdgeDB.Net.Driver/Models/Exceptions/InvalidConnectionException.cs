using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB
{
    /// <summary>
    ///     Represents an error with the provided connection details.
    /// </summary>
    public class InvalidConnectionException : EdgeDBException
    {
        /// <summary>
        ///     Constructs a new <see cref="InvalidConnectionException"/> with the specified error message.
        /// </summary>
        /// <param name="message">The error message describing why this exception was thrown.</param>
        public InvalidConnectionException(string message)
            : base(message, false, false)
        {

        }
    }
}
