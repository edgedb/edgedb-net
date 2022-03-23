using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Models
{
    /// <summary>
    ///     Represents a generic execution result of a command.
    /// </summary>
    public struct ExecuteResult : IExecuteResult
    {
        /// <inheritdoc/>
        public bool IsSuccess { get; private set; }

        /// <inheritdoc/>
        public ErrorResponse? Error { get; private set; }

        /// <inheritdoc/>
        public Exception? Exception { get; private set; }

        /// <inheritdoc/>
        public string? ExecutedQuery { get; private set; }

        internal ExecuteResult(bool success, ErrorResponse? error, Exception? exc, string? executedQuery)
        {
            IsSuccess = success;
            Error = error;
            Exception = exc;
            ExecutedQuery = executedQuery;
        }
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
        ErrorResponse? Error { get; }

        /// <summary>
        ///     Gets the exception (if any) that the command threw when executing.
        /// </summary>
        Exception? Exception { get; }

        /// <summary>
        ///     Gets the executed query string.
        /// </summary>
        string? ExecutedQuery { get; }
    }
}
