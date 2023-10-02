using System.Net.Sockets;

namespace EdgeDB;

/// <summary>
///     Represents a temporary connection failiure exception.
/// </summary>
public sealed class ConnectionFailedTemporarilyException : EdgeDBException
{
    /// <summary>
    ///     Constructs a new <see cref="ConnectionFailedTemporarilyException" /> with the specified socket error.
    /// </summary>
    /// <param name="error">The underlying socket error that caused this exception to be thrown.</param>
    public ConnectionFailedTemporarilyException(SocketError error)
        : base("The connection could not be established at this time", true, true)
    {
        SocketError = error;
    }

    /// <summary>
    ///     Gets the socket error that caused the connection to fail.
    /// </summary>
    public SocketError SocketError { get; }
}
