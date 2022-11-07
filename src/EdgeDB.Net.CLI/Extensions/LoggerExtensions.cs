using Microsoft.Extensions.Logging;
using Serilog.Extensions.Logging;
using System;
namespace EdgeDB
{
    public static class LoggerExtensions
    {
        public static ILogger CreateClientLogger(this Serilog.ILogger logger)
        {
            return new SerilogLoggerFactory(logger).CreateLogger<EdgeDBClient>();
        }
    }
}

