namespace EdgeDB
{
    /// <summary>
    ///     Represents a generic execution result of a command.
    /// </summary>
    [Obsolete("This class is no longer used anywhere within the binding and will be removed in a future version.")]
    public readonly struct ExecuteResult : IExecuteResult
    {
        /// <inheritdoc/>
        public bool IsSuccess { get; internal init; }
        
        /// <inheritdoc/>
        public Exception? Exception { get; internal init; }

        /// <inheritdoc/>
        public string? ExecutedQuery { get; internal init; }

        internal ExecuteResult(bool success, Exception? exc, string? executedQuery)
        {
            IsSuccess = success;
            Exception = exc;
            ExecutedQuery = executedQuery;
        }

        IExecuteError? IExecuteResult.ExecutionError => null;

    }

    /// <summary>
    ///     An interface representing a generic execution result.
    /// </summary>
    [Obsolete("This interface is no longer used anywhere within the binding and will be removed in a future version.")]
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

    /// <summary>
    ///     Represents a generic execution error.
    /// </summary>
    [Obsolete("This interface is no longer used anywhere within the binding and will be removed in a future version.")]
    public interface IExecuteError
    {
        /// <summary>
        ///     Gets the error message.
        /// </summary>
        string? Message { get; }

        /// <summary>
        ///     Gets the error code.
        /// </summary>
        ServerErrorCodes ErrorCode { get; }
    }
}
