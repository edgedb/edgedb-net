using Microsoft.Extensions.Logging;

namespace EdgeDB.ExampleApp;

public interface IExample
{
    ILogger? Logger { get; set; }

    Task ExecuteAsync(GelClientPool clientPool);

    static async Task ExecuteAllAsync(GelClientPool clientPool, ILogger logger, ILoggerFactory factory)
    {
        var examples = typeof(IExample).Assembly.GetTypes()
            .Where(x => x.IsAssignableTo(typeof(IExample)) && x != typeof(IExample));

        foreach (var example in examples)
        {
            logger.LogInformation("Running {example}..", $"{example.Name}.cs");
            try
            {
                var inst = (IExample)Activator.CreateInstance(example)!;
                inst.Logger = factory.CreateLogger(example.Name);
                await inst.ExecuteAsync(clientPool).ConfigureAwait(false);
                logger.LogInformation("{example} complete!", $"{example.Name}.cs");
            }
            catch (Exception x)
            {
                logger.LogError(x, "Example {example} failed", $"{example.Name}.cs");
            }
        }

        logger.LogInformation("Examples complete!");
    }
}
