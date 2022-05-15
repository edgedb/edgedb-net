using System.Reflection;

namespace EdgeDB.ExampleApp
{
    public interface IExample
    {
        Logger? Logger { get; set; }

        Task ExecuteAsync(EdgeDBClient client);

        static async Task ExecuteAllAsync(EdgeDBClient client)
        {
            var log = Logger.GetLogger<IExample>();

            var examples = Assembly.GetExecutingAssembly().GetTypes().Where(x => x.IsAssignableTo(typeof(IExample)) && x != typeof(IExample));

            foreach (var example in examples)
            {
                log.Log($"Running {example.Name}...", LogPostfix.Examples);
                try
                {
                    var inst = (IExample)Activator.CreateInstance(example)!;
                    inst.Logger = Logger.GetLogger(example);
                    await inst.ExecuteAsync(client).ConfigureAwait(false);
                    log.Log($"{example.Name} complete!", LogPostfix.Examples);
                }
                catch (Exception x)
                {
                    log.Error($"Example {example.Name} failed", LogPostfix.Examples, x);
                }
            }

            log.Info("Examples complete", LogPostfix.Examples);
        }
    }
}
