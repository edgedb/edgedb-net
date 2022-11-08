module CancelQueries

open Examples
open EdgeDB
open Microsoft.Extensions.Logging
open System.Threading.Tasks
open System.Threading
open System

type CancelQueries() =
    interface IExample with
        member this.ExecuteAsync(client: EdgeDBClient, logger: ILogger) =
            task {
                let tokenSource = new CancellationTokenSource()
                tokenSource.CancelAfter(TimeSpan.FromTicks(5))

                try
                    let! result = client.QueryRequiredSingleAsync<string>("select 'Hello, .NET'")
                    result |> ignore
                with
                    | :? OperationCanceledException -> logger.LogInformation("Got task cancelled exception")
                    | e ->
                        logger.LogError(e, "Got unexpected exception")
                        raise e
            }
