using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB
{
    /// <summary>
    ///     Represents a connection failure that cannot be retried.
    /// </summary>
    public sealed class ConnectionFailedException : EdgeDBException
    {
        /// <summary>
        ///     Gets the number of attempts the client made to reconnect.
        /// </summary>
        public int Attempts { get; }
        public ConnectionFailedException(int attempts)
            : base($"The connection failed to be established after {attempts} attempt(s)", false, false)
        {
            Attempts = attempts;
        }
    }
}
