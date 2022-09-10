using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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
            var conn = connection ?? EdgeDBConnection.ResolveEdgeDBTOML();

            collection.AddSingleton(conn);
            collection.AddSingleton<EdgeDBClientPoolConfig>((provider) =>
            {
                var config = new EdgeDBClientPoolConfig();
                configure?.Invoke(config);

                if (config.Logger is null)
                {
                    config.Logger = provider.GetService<ILoggerFactory>()?.CreateLogger("EdgeDB");
                }
                return config;
            });
            collection.AddSingleton<EdgeDBClient>();

            return collection;
        }
    }
}
