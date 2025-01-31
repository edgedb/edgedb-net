namespace Gel;

/// <summary>
///     An enum representing the retry mode when connecting new clients.
/// </summary>
public enum ConnectionRetryMode
{
    /// <summary>
    ///     The client should retry to connect up to the specified <see cref="GelClientConfig.MaxConnectionRetries" />.
    /// </summary>
    AlwaysRetry,

    /// <summary>
    ///     The client should error and not retry to connect.
    /// </summary>
    NeverRetry
}
