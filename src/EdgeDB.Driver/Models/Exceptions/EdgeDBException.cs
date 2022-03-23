using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB
{
    /// <summary>
    ///     Represents a generic exception that occured with the edgedb library.
    /// </summary>
    public class EdgeDBException : Exception
    {
        internal bool ShouldRetry { get; }
        internal bool ShouldReconnect { get; }

        public EdgeDBException(bool shouldRetry = false, bool shouldReconnect = false) 
        {
            ShouldRetry = shouldRetry;
            ShouldReconnect = shouldReconnect;
        }

        public EdgeDBException(string? message, bool shouldRetry = false, bool shouldReconnect = false) : base(message) 
        {
            ShouldRetry = shouldRetry;
            ShouldReconnect = shouldReconnect;
        }

        public EdgeDBException(string? message, Exception? innerException, bool shouldRetry = false, bool shouldReconnect = false) : base(message, innerException) 
        {
            ShouldRetry = shouldRetry;
            ShouldReconnect = shouldReconnect;
        }
    }
}
