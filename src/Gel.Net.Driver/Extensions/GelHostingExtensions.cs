using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Gel;

/// <summary>
///     A class containing extension methods for DI.
/// </summary>
public static class GelHostingExtensions
{
    /// <summary>
    ///     Adds a <see cref="GelClientPool" /> singleton to a <see cref="IServiceCollection" />.
    /// </summary>
    /// <param name="collection">The source collection to add a <see cref="GelClientPool" /> to.</param>
    /// <param name="connection">An optional connection arguments for the client.</param>
    /// <param name="clientPoolConfig">
    ///     An optional configuration delegate for configuring the <see cref="GelClientPool" />.
    /// </param>
    /// <returns>
    ///     The source <see cref="IServiceCollection" /> with <see cref="GelClientPool" /> added as a singleton.
    /// </returns>
    public static IServiceCollection AddGel(this IServiceCollection collection, GelConnection? connection = null,
        Action<GelClientPoolConfig>? clientPoolConfig = null)
    {
        var conn = connection ?? GelConnection.Create();

        collection.AddSingleton(conn);
        collection.AddSingleton<GelClientPoolConfig>(provider =>
        {
            var config = new GelClientPoolConfig();
            clientPoolConfig?.Invoke(config);

            if (config.Logger is null)
            {
                config.Logger = provider.GetService<ILoggerFactory>()?.CreateLogger("Gel");
            }

            return config;
        });
        collection.AddSingleton<GelClientPool>();

        return collection;
    }
}
