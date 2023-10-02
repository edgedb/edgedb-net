namespace EdgeDB;

/// <summary>
///     Represents a connection failure that cannot be retried.
/// </summary>
public sealed class ConnectionFailedException : EdgeDBException
{
    /// <summary>
    ///     Constructs a new <see cref="ConnectionFailedException" /> with the number
    ///     of connection attempts made.
    /// </summary>
    /// <param name="attempts">The number of attempts made to connect.</param>
    public ConnectionFailedException(int attempts)
        : base($"The connection failed to be established after {attempts} attempt(s)")
    {
        Attempts = attempts;
    }

    /// <summary>
    ///     Gets the number of attempts the client made to reconnect.
    /// </summary>
    public int Attempts { get; }
}
