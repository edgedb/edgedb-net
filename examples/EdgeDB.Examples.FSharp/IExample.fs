namespace Examples

open Microsoft.Extensions.Logging

type IExample =
    abstract member ExecuteAsync: client:EdgeDB.EdgeDBClient * logger:ILogger -> System.Threading.Tasks.Task
