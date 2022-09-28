using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB
{
    /// <summary>
    ///     Represents a generic configuration error.
    /// </summary>
    public sealed class ConfigurationException : EdgeDBException
    {
        /// <summary>
        ///     Creates a new <see cref="ConfigurationException"/>.
        /// </summary>
        /// <param name="message">The configuration error message.</param>
        public ConfigurationException(string message) : base(message) { }

        /// <summary>
        ///     Creates a new <see cref="ConfigurationException"/>.
        /// </summary>
        /// <param name="message">The configuration error message.</param>
        /// <param name="inner">An inner exception.</param>
        public ConfigurationException(string message, Exception inner) : base(message, inner) { }
    }
}
