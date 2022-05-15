namespace EdgeDB.ExampleApp.Examples
{
    public class DumpAndRestore : IExample
    {
        public Logger? Logger { get; set; }

        public Task ExecuteAsync(EdgeDBClient client) => Task.CompletedTask;
    }
}
