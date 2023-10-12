module BasicQueryFunctions

open Examples
open EdgeDB
open Microsoft.Extensions.Logging

type BasicQueryFunctions() =
    interface IExample with
        member this.ExecuteAsync(client: EdgeDBClient, logger: ILogger) =
            task {
                // We can preform basic scalar queries with our client.
                // forach query function that can return a result, we must specify
                // the return type as a generic parameter.
                // This function will always return a collection of the specified result.
                let! str = client.QueryAsync<string>("select 'Hello, .NET!'")
                logger.LogInformation("QueryAsync test: {Query}", str)

                // There are also different query functions for different usecases:

                // Lets use QuerySingle to return a single object from the query.
                // This function will throw if the query result contains more than one result.
                // This function will not throw if no result is returned.
                let! singleStr = client.QuerySingleAsync<string>("select 'Hello, .NET!'")
                logger.LogInformation("QuerySingleAsync test: {Query}", singleStr)

                // We can use QueryRequiredSingle to return a single required result.
                // This function will throw if the query result contains more than one result.
                // This function will throw if no result is returned.
                let! singleRequiredStr = client.QueryRequiredSingleAsync<string>("select 'Hello, DotNet!'")
                logger.LogInformation("QueryRequiredSingleAsync test: {Query}", singleRequiredStr)

                // If we want to execute a query but do not want/need its result we can use the Execute method.
                // This is useful for insert/update queries.
                client.ExecuteAsync("select 'Hello, .NET!'")
                |> Async.AwaitTask
                |> Async.Ignore
                |> ignore

            // Each function maps to a cardinality mode:
            // QueryAsync -> Cardinality.Many
            // QuerySingleAsync -> Cardinality.AtMostOne
            // QueryRequiredSingleAsyn -> Cardinality.One
            // ExecuteAsync -> Cardinality.Many
            }
