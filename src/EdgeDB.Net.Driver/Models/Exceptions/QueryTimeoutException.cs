using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB
{
    /// <summary>
    ///     Represents an exception thrown when a query operation times out.
    /// </summary>
    public class QueryTimeoutException : EdgeDBException
    {
        /// <summary>
        ///     Gets the query that caused the operation to time out.
        /// </summary>
        public string Query { get; }

        /// <summary>
        ///     Gets the configured timeout duration allocated for the
        ///     query when it was timed out.
        /// </summary>
        public uint TimeoutDuration { get; }

        internal QueryTimeoutException(uint timeout, string query, OperationCanceledException ce)
            : base("The query operation timed out", ce)
        {
            TimeoutDuration = timeout;
            Query = query;
        }
    }
}
