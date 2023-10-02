using EdgeDB.Binary;
using EdgeDB.Binary.Duplexers;
using EdgeDB.Utils;
using System.Net;
using System.Net.Http.Headers;
using System.Text;

namespace EdgeDB;

/// <summary>
///     Represents a client that can preform queries over HTTP.
/// </summary>
internal sealed class EdgeDBHttpClient : EdgeDBBinaryClient
{
    public const string HTTP_TOKEN_AUTH_METHOD = "SCRAM-SHA-256";
    private readonly HttpDuplexer _duplexer;

    internal readonly HttpClient HttpClient;

    private bool _authed;

    internal EdgeDBHttpClient(EdgeDBConnection connection, EdgeDBConfig config, IDisposable poolHolder, ulong clientId)
        : base(connection, config, poolHolder, clientId)
    {
        var manager = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = connection.ValidateServerCertificateCallback
        };

        HttpClient = new HttpClient(manager);
        _duplexer = new HttpDuplexer(this);
    }

    public override bool IsConnected
        => _authed;

    public string? AuthorizationToken { get; private set; }

    internal override IBinaryDuplexer Duplexer
        => _duplexer;

    private async Task AuthenticateAsync()
    {
        if (Connection.Password is null)
            throw new ConfigurationException("A password is required for HTTP authentication");

        using var scram = new Scram();

        var first = scram.BuildInitialMessage(Connection.Username);

        var firstMessage = new HttpRequestMessage(HttpMethod.Get, Connection.GetAuthUri());

        firstMessage.Headers.Authorization = new AuthenticationHeaderValue(
            HTTP_TOKEN_AUTH_METHOD,
            $"data={Convert.ToBase64String(Encoding.UTF8.GetBytes(first))}");

        var result = await HttpClient.SendAsync(firstMessage);

        var authenticate = result.Headers.GetValues("www-authenticate").First();

        if (!authenticate.StartsWith(HTTP_TOKEN_AUTH_METHOD))
            throw new ProtocolViolationException($"The only supported auth method is {HTTP_TOKEN_AUTH_METHOD}");

        authenticate = authenticate[(HTTP_TOKEN_AUTH_METHOD.Length + 1)..];

        var keys = ParseKeys(authenticate);

        var (final, sig) = scram.BuildFinalMessage(Encoding.UTF8.GetString(Convert.FromBase64String(keys["data"])),
            Connection.Password);

        var finalMessage = new HttpRequestMessage(HttpMethod.Get, Connection.GetAuthUri());

        finalMessage.Headers.Authorization = new AuthenticationHeaderValue(
            HTTP_TOKEN_AUTH_METHOD,
            $"sid={keys["sid"]} data={Convert.ToBase64String(Encoding.UTF8.GetBytes(final))}");

        result = await HttpClient.SendAsync(finalMessage);
        result.EnsureSuccessStatusCode();

        AuthorizationToken = await result.Content.ReadAsStringAsync();
        _authed = true;

        TriggerReady();
    }

    private static Dictionary<string, string> ParseKeys(string input)
        => input.Split(',')
            .Select(value => value.Split('='))
            .ToDictionary(pair => pair[0].Trim(), pair => pair[1]);

    public override async ValueTask ConnectAsync(CancellationToken token = default)
    {
        // preform authentication
        if (!_authed)
            await AuthenticateAsync().ConfigureAwait(false);
    }

    protected override ValueTask<Stream> GetStreamAsync(CancellationToken token = default) =>
        throw new NotSupportedException();

    protected override ValueTask CloseStreamAsync(CancellationToken token = default) =>
        throw new NotSupportedException();
}
