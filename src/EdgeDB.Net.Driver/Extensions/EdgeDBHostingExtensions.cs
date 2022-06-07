using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB
{
    public static class EdgeDBHostingExtensions
    {
        public static IServiceCollection AddEdgeDB(this IServiceCollection collection, EdgeDBConnection? connection = null, 
            Action<EdgeDBClientPoolConfig>? configure = null)
        {
            var conn = connection ?? EdgeDBConnection.ResolveConnection();
            var config = new EdgeDBClientPoolConfig();
            configure?.Invoke(config);

            collection.AddSingleton(conn);
            collection.AddSingleton(config);
            collection.AddSingleton<EdgeDBClient>();
            return collection;
        }
    }
}
