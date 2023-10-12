namespace EdgeDB;

/// <summary>
///     Represents an exception that occurs when the server signature is incorrect.
/// </summary>
public class InvalidSignatureException : EdgeDBException
{
    /// <summary>
    ///     Constructs a new <see cref="InvalidSignatureException" />.
    /// </summary>
    public InvalidSignatureException()
        : base("The received signature didn't match the expected one")
    {
    }
}
