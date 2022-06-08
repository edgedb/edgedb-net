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
        public ConnectionFailedTemporarilyException(SocketError error)
            : base("The connection could not be established at this time", true, true)
        {
            SocketError = error;
        }
    }
}
