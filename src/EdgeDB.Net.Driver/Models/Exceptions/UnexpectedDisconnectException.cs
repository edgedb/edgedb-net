namespace EdgeDB;

/// <summary>
///     Represents an exception that was caused by an unexpected disconnection from EdgeDB.
/// </summary>
public class UnexpectedDisconnectException : EdgeDBException
{
    internal UnexpectedDisconnectException()
        : base("The connection was unexpectedly closed")
    {
    }
}
