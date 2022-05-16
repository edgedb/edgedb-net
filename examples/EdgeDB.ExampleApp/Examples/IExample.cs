using Microsoft.Extensions.Logging;
using System.Reflection;

namespace EdgeDB.ExampleApp
{
    public interface IExample
    {
        Logger? Logger { get; set; }

        Task ExecuteAsync(EdgeDBClient client);

        static async Task ExecuteAllAsync(EdgeDBClient client, ILogger logger)
        {
            var examples = Assembly.GetExecutingAssembly().GetTypes().Where(x => x.IsAssignableTo(typeof(IExample)) && x != typeof(IExample));

            foreach (var example in examples)
            {
                logger.LogInformation("Running {example}..", $"{example.Name}.cs");
                try
                {
                    var inst = (IExample)Activator.CreateInstance(example)!;
                    inst.Logger = Logger.GetLogger(example);
                    await inst.ExecuteAsync(client).ConfigureAwait(false);
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
}
