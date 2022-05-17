using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.ExampleApp.Examples
{
    public class BasicQueryFunctions : IExample
    {
        public ILogger? Logger { get; set; }

        public async Task ExecuteAsync(EdgeDBClient client)
        {
            // We can preform basic scalar queries with our client.
            // forach query function that can return a result, we must specify
            // the return type as a generic parameter.
            // This function will always return a collection of the specified result.
            var str = await client.QueryAsync<string>("select \"Hello, DotNet!\"");
            Logger?.LogInformation("QueryAsync test: {Query}", str);

            // There are also different query functions for different usecases:

            // Lets use QuerySingle to return a single object from the query.
            // This function will throw if the query result contains more than one result.
            // This function will not throw if no result is returned.
            var singleStr = await client.QuerySingleAsync<string>("select \"Hello, DotNet!\"");
            Logger?.LogInformation("QuerySingleAsync test: {Query}", singleStr);

            // We can use QueryRequiredSingle to return a single required result.
            // This function will throw if the query result contains more than one result.
            // This function will throw if no result is returned. 
            var singleRequiredStr = await client.QueryRequiredSingleAsync<string>("select \"Hello, DotNet!\"");
            Logger?.LogInformation("QueryRequiredSingleAsync test: {Query}", singleRequiredStr);

            // If we want to execute a query but do not want/need its result we can use the Execute method.
            // This is useful for insert/update queries.
            await client.ExecuteAsync("select \"Hello, DotNet!\"");

            // Each function maps to a cardinality mode:
            // QueryAsync -> Cardinality.Many
            // QuerySingleAsync -> Cardinality.AtMostOne
            // QueryRequiredSingleAsyn -> Cardinality.One
            // ExecuteAsync -> Cardinality.Many
        }
    }
}
