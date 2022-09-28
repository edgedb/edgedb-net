using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB
{
    /// <summary>
    ///     Represents a temporary connection failiure exception.
    /// </summary>
    public sealed class ConnectionFailedTemporarilyException : EdgeDBException
    {
        /// <summary>
        ///     Gets the socket error that caused the connection to fail.
        /// </summary>
        public SocketError SocketError { get; }

        /// <summary>
        ///     Constructs a new <see cref="ConnectionFailedTemporarilyException"/> with the specified socket error.
        /// </summary>
        /// <param name="error">The underlying socket error that caused this exception to be thrown.</param>
        public ConnectionFailedTemporarilyException(SocketError error)
            : base("The connection could not be established at this time", true, true)
        {
            SocketError = error;
        }
    }
}
