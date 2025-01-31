namespace Examples

open Microsoft.Extensions.Logging

type IExample =
    abstract member ExecuteAsync: clientPool: Gel.GelClientPool * logger: ILogger -> System.Threading.Tasks.Task
