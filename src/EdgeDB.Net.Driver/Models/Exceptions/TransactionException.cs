namespace EdgeDB;

/// <summary>
///     Represents an exception that occurs within transactions.
/// </summary>
public class TransactionException : EdgeDBException
{
    /// <summary>
    ///     Constructs a new <see cref="TransactionException" /> with a specified error message.
    /// </summary>
    /// <param name="message">The error message describing why this exception was thrown.</param>
    /// <param name="innerException">An optional inner exception.</param>
    public TransactionException(string message, Exception? innerException = null)
        : base(message, innerException)
    {
    }
}
