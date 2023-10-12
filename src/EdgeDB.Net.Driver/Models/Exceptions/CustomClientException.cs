namespace EdgeDB;

/// <summary>
///     Represents a generic error with custom clients.
/// </summary>
public sealed class CustomClientException : EdgeDBException
{
    /// <summary>
    ///     Constructs a new <see cref="CustomClientException" /> with the specified error message.
    /// </summary>
    /// <param name="message">The error message describing why this exception was thrown.</param>
    public CustomClientException(string message)
        : base(message)
    {
    }
}
