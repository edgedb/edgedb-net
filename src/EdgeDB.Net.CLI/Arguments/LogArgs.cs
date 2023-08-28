using CommandLine;
using Microsoft.Extensions.Logging;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.CLI.Arguments
{
    /// <summary>
    ///     A class containing logger arguments.
    /// </summary>
    public class LogArgs
    {
        /// <summary>
        ///     Gets or sets the log level for the default logger.
        /// </summary>
        [Option("loglevel", HelpText = "Configure the log level")]
        public LogEventLevel LogLevel { get; set; } = LogEventLevel.Information;
    }
}
