using EdgeDB.Binary.Packets;

namespace EdgeDB
{
    /// <summary>
    ///     Represents a generic execution result of a command.
    /// </summary>
    public readonly struct ExecuteResult : IExecuteResult
    {
        /// <inheritdoc/>
        public bool IsSuccess { get; internal init; }

        /// <inheritdoc/>
        public ErrorResponse? ErrorResponse { get; internal init; }

        /// <inheritdoc/>
        public Exception? Exception { get; internal init; }

        /// <inheritdoc/>
        public string? ExecutedQuery { get; internal init; }

        internal ExecuteResult(bool success, ErrorResponse? error, Exception? exc, string? executedQuery)
        {
            IsSuccess = success;
            ErrorResponse = error;
            Exception = exc;
            ExecutedQuery = executedQuery;
        }

        IExecuteError? IExecuteResult.ExecutionError => ErrorResponse ?? null;

    }

    /// <summary>
    ///     An interface representing a generic execution result.
    /// </summary>
    public interface IExecuteResult
    {
        /// <summary>
        ///     Gets whether or not the command executed successfully.
        /// </summary>
        bool IsSuccess { get; }

        /// <summary>
        ///     Gets the error (if any) that the command received.
        /// </summary>
        IExecuteError? ExecutionError { get; }

        /// <summary>
        ///     Gets the exception (if any) that the command threw when executing.
        /// </summary>
        Exception? Exception { get; }

        /// <summary>
        ///     Gets the executed query string.
        /// </summary>
        string? ExecutedQuery { get; }
    }

    public interface IExecuteError
    {
        string? Message { get; }

        ServerErrorCodes ErrorCode { get; }
    }
}
