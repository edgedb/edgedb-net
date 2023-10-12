using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace EdgeDB;

/// <summary>
///     A class containing extension methods for DI.
/// </summary>
public static class EdgeDBHostingExtensions
{
    /// <summary>
    ///     Adds a <see cref="EdgeDBClient" /> singleton to a <see cref="IServiceCollection" />.
    /// </summary>
    /// <param name="collection">The source collection to add a <see cref="EdgeDBClient" /> to.</param>
    /// <param name="connection">An optional connection arguments for the client.</param>
    /// <param name="clientConfig">
    ///     An optional configuration delegate for configuring the <see cref="EdgeDBClient" />.
    /// </param>
    /// <returns>
    ///     The source <see cref="IServiceCollection" /> with <see cref="EdgeDBClient" /> added as a singleton.
    /// </returns>
    public static IServiceCollection AddEdgeDB(this IServiceCollection collection, EdgeDBConnection? connection = null,
        Action<EdgeDBClientPoolConfig>? clientConfig = null)
    {
        var conn = connection ?? EdgeDBConnection.ResolveEdgeDBTOML();

        collection.AddSingleton(conn);
        collection.AddSingleton<EdgeDBClientPoolConfig>(provider =>
        {
            var config = new EdgeDBClientPoolConfig();
            clientConfig?.Invoke(config);

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
