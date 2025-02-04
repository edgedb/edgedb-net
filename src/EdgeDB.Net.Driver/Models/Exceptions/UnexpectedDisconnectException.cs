namespace EdgeDB;

/// <summary>
///     Represents an exception that was caused by an unexpected disconnection from EdgeDB.
/// </summary>
public class UnexpectedDisconnectException : GelException
{
    internal UnexpectedDisconnectException()
        : base("The connection was unexpectedly closed")
    {
    }
}
