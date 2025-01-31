using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace EdgeDB;

/// <summary>
///     A class containing extension methods for DI.
/// </summary>
public static class EdgeDBHostingExtensions
{
    /// <summary>
    ///     Adds a <see cref="GelClientPool" /> singleton to a <see cref="IServiceCollection" />.
    /// </summary>
    /// <param name="collection">The source collection to add a <see cref="GelClientPool" /> to.</param>
    /// <param name="connection">An optional connection arguments for the client.</param>
    /// <param name="clientConfig">
    ///     An optional configuration delegate for configuring the <see cref="GelClientPool" />.
    /// </param>
    /// <returns>
    ///     The source <see cref="IServiceCollection" /> with <see cref="GelClientPool" /> added as a singleton.
    /// </returns>
    public static IServiceCollection AddEdgeDB(this IServiceCollection collection, GelConnection? connection = null,
        Action<EdgeDBClientPoolConfig>? clientConfig = null)
    {
        var conn = connection ?? GelConnection.Create();

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
        collection.AddSingleton<GelClientPool>();

        return collection;
    }
}
