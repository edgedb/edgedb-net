namespace Gel;

/// <summary>
///     Represents a generic exception that occured with the edgedb library.
/// </summary>
public class GelException : Exception
{
    /// <summary>
    ///     Constructs a new <see cref="GelException" />.
    /// </summary>
    /// <param name="shouldRetry">Whether or not this exception is retryable.</param>
    /// <param name="shouldReconnect">Whether or not the client who caught this exception should reconnect.</param>
    public GelException(bool shouldRetry = false, bool shouldReconnect = false)
    {
        ShouldRetry = shouldRetry;
        ShouldReconnect = shouldReconnect;
    }

    /// <summary>
    ///     Constructs a new <see cref="GelException" /> with the specified error message.
    /// </summary>
    /// <param name="message">The error message describing why this exception was thrown.</param>
    /// <param name="shouldRetry">Whether or not this exception is retryable.</param>
    /// <param name="shouldReconnect">Whether or not the client who caught this exception should reconnect.</param>
    public GelException(string? message, bool shouldRetry = false, bool shouldReconnect = false) : base(message)
    {
        ShouldRetry = shouldRetry;
        ShouldReconnect = shouldReconnect;
    }

    /// <summary>
    ///     Constructs a new <see cref="GelException" /> with the specified error message
    ///     and inner exception.
    /// </summary>
    /// <param name="message">The error message describing why this exception was thrown.</param>
    /// <param name="innerException">The inner exception.</param>
    /// <param name="shouldRetry">Whether or not this exception is retryable.</param>
    /// <param name="shouldReconnect">Whether or not the client who caught this exception should reconnect.</param>
    public GelException(string? message, Exception? innerException, bool shouldRetry = false,
        bool shouldReconnect = false) : base(message, innerException)
    {
        ShouldRetry = shouldRetry;
        ShouldReconnect = shouldReconnect;
    }

    internal bool ShouldRetry { get; }
    internal bool ShouldReconnect { get; }
}
