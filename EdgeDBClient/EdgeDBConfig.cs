using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB
{
    public class EdgeDBConfig
    {
        /// <summary>
        ///     Gets or sets whether or not to allow an unsecure connection when connecting over TCP.
        /// </summary>
        public bool AllowUnsecureConnection { get; set; } = false;

        /// <summary>
        ///     Gets or sets the default client pool size.
        /// </summary>
        public int DefaultPoolSize { get; set; } = 50;

        /// <summary>
        ///     Gets or sets the logger used for logging messages from the driver.
        /// </summary>
        public ILogger? Logger { get; set; }
    }
}
