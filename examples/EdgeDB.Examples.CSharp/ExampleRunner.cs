using Microsoft.Extensions.Logging;

namespace Gel.ExampleApp;

public class ExampleRunner
{
    private readonly GelClientPool _clientPool;
    private readonly ILogger _logger;
    private readonly ILoggerFactory _loggerFactory;

    public ExampleRunner(GelClientPool clientPool, ILogger<ExampleRunner> logger, ILoggerFactory factory)
    {
        _clientPool = clientPool;
        _logger = logger;
        _loggerFactory = factory;
    }

    public async Task StartAsync() =>
        await IExample.ExecuteAllAsync(_clientPool, _logger, _loggerFactory).ConfigureAwait(false);
}
