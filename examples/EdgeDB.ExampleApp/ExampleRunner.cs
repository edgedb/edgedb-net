using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.ExampleApp
{
    public class ExampleRunner
    {
        private readonly EdgeDBClient _client;
        private readonly ILogger _logger;
        private readonly ILoggerFactory _loggerFactory;
        public ExampleRunner(EdgeDBClient client, ILogger<ExampleRunner> logger, ILoggerFactory factory)
        {
            _client = client;
            _logger = logger;
            _loggerFactory = factory;
        }

        public async Task StartAsync()
        {
            await IExample.ExecuteAllAsync(_client, _logger, _loggerFactory).ConfigureAwait(false);
        }
    }
}
