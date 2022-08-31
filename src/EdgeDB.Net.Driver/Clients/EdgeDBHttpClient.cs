using EdgeDB.DataTypes;
using EdgeDB.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB
{
    /// <summary>
    ///     Represents the returned data from a http-based query.
    /// </summary>
    public class HttpQueryResult : IExecuteResult
    {
        /// <summary>
        ///     Gets or sets the data returned from the query.
        /// </summary>
        [JsonProperty("data")]
        public object? Data { get; set; }

        /// <summary>
        ///     Gets or sets the error returned from the query.
        /// </summary>
        [JsonProperty("error")]
        public QueryResultError? Error { get; set; }

        /// <summary>
        ///     Represents a query error received over http
        /// </summary>
        public class QueryResultError : IExecuteError
        {
            /// <summary>
            ///     Gets or sets the error message.
            /// </summary>
            [JsonProperty("message")]
            public string? Message { get; set; }

            /// <summary>
            ///     Gets or sets the type of the error.
            /// </summary>
            [JsonProperty("type")]
            public string? Type { get; set; }

            /// <summary>
            ///     Gets or sets the error code.
            /// </summary>
            [JsonProperty("code")]
            public ServerErrorCodes Code { get; set; }

            string? IExecuteError.Message 
                => Message;

            ServerErrorCodes IExecuteError.ErrorCode 
                => Code;
        }

        bool IExecuteResult.IsSuccess 
            => Error is null;

        IExecuteError? IExecuteResult.ExecutionError 
            => Error;

        Exception? IExecuteResult.Exception 
            => null;

        string? IExecuteResult.ExecutedQuery 
            => null;
    }

    /// <summary>
    ///     Represents a client that can preform queries over HTTP.
    /// </summary>
    public sealed class EdgeDBHttpClient : BaseEdgeDBClient
    {
        /// <summary>
        ///     Fired when a query is executed.
        /// </summary>
        public event Func<HttpQueryResult, ValueTask> QueryExecuted
        {
            add => _onQuery.Add(value);
            remove => _onQuery.Remove(value);
        }

        /// <inheritdoc/>
        /// <remarks>
        ///     This property is always <see langword="true"/>.
        /// </remarks>
        public override bool IsConnected => true;

        /// <summary>
        ///     The URI of the edgedb instance.
        /// </summary>
        public readonly Uri Uri;
        
        private readonly EdgeDBConnection _connection;
        private readonly HttpClient _httpClient;
        private readonly AsyncEvent<Func<HttpQueryResult, ValueTask>> _onQuery = new();
        private readonly ILogger _logger;

        private class QueryPostBody
        {
            [JsonProperty("query")]
            public string? Query { get; set; }

            [JsonProperty("variables")]
            public IDictionary<string, object?>? Variables { get; set; }
        }

        /// <summary>
        ///     Creates a new instance of the http client.
        /// </summary>
        /// <param name="connection">The connection details used to connect to the database.</param>
        /// <param name="config">The configuration for this client.</param>
        /// <param name="clientId">The optional client id of this client. This is used for logging and client pooling.</param>
        public EdgeDBHttpClient(EdgeDBConnection connection, EdgeDBConfig config, ulong clientId)
            : base(clientId)
        {
            _logger = config.Logger ?? Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
            _connection = connection;
            Uri = new($"http{(connection.TLSSecurity != TLSSecurityMode.Insecure ? "s" : "")}://{connection.Hostname}:{connection.Port}/db/{connection.Database}/edgeql");

            var handler = new HttpClientHandler
            {
                ClientCertificateOptions = ClientCertificateOption.Manual,
                SslProtocols = SslProtocols.None,
                ServerCertificateCustomValidationCallback = ValidateServerCertificate
            };
            _httpClient = new(handler);
        }

        private bool ValidateServerCertificate(object sender, X509Certificate? certificate, X509Chain? chain, SslPolicyErrors sslPolicyErrors)
        {
            if (_connection.TLSSecurity == TLSSecurityMode.Insecure)
                return true;

            if (_connection.TLSCertificateAuthority != null)
            {
                var cert = _connection.GetCertificate()!;

                X509Chain chain2 = new();
                chain2.ChainPolicy.ExtraStore.Add(cert);
                chain2.ChainPolicy.VerificationFlags = X509VerificationFlags.AllowUnknownCertificateAuthority;
                chain2.ChainPolicy.RevocationMode = X509RevocationMode.NoCheck;

                bool isValid = chain2.Build(new X509Certificate2(certificate!));
                var chainRoot = chain2.ChainElements[^1].Certificate;
                isValid = isValid && chainRoot.RawData.SequenceEqual(cert.RawData);

                return isValid;
            }
            else
            {
                return sslPolicyErrors == SslPolicyErrors.None;
            }
        }

        /// <remarks>
        ///     This function does nothing for the <see cref="EdgeDBHttpClient"/>.
        /// </remarks>
        /// <param name="token">A cancellation token used to cancel the asynchronous operation.</param>
        /// <inheritdoc/>
        public override ValueTask DisconnectAsync(CancellationToken token = default)
            => default;

        /// <remarks>
        ///     This function does nothing for the <see cref="EdgeDBHttpClient"/>.
        /// </remarks>
        /// <param name="token">A cancellation token used to cancel the asynchronous operation.</param>
        /// <inheritdoc/>
        public override ValueTask ConnectAsync(CancellationToken token = default) 
            => default;

        private async Task<HttpQueryResult> ExecuteInternalAsync(string query, IDictionary<string, object?>? args = null, 
            CancellationToken token = default)
        {
            var content = new StringContent(EdgeDBConfig.JsonSerializer.SerializeObject(new QueryPostBody()
            {
                Query = query,
                Variables = args
            }), Encoding.UTF8, "application/json");

            var httpResult = await _httpClient.PostAsync(Uri, content, token).ConfigureAwait(false);

            if (!httpResult.IsSuccessStatusCode)
                throw new EdgeDBException($"Failed to execute HTTP query, got {httpResult.StatusCode}");

            var json = await httpResult.Content.ReadAsStringAsync(token).ConfigureAwait(false);

            var result = EdgeDBConfig.JsonSerializer.DeserializeObject<HttpQueryResult>(json)!;
            await InvokeResultEventAsync(result).ConfigureAwait(false);
            return result;
        }

        /// <inheritdoc/>
        /// <remarks>
        ///     <paramref name="capabilities"/> has no effect as the HTTP protocol does not support capabilities.
        /// </remarks>
        /// <exception cref="EdgeDBException">The server returned a status code other than 200.</exception>
        public override async Task ExecuteAsync(string query, IDictionary<string, object?>? args = null,
            Capabilities? capabilities = Capabilities.Modifications, CancellationToken token = default)
        {
            await ExecuteInternalAsync(query, args, token).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        /// <remarks>
        ///     <paramref name="capabilities"/> has no effect as the HTTP protocol does not support capabilities.
        /// </remarks>
        /// <exception cref="EdgeDBException">The server returned a status code other than 200.</exception>
        public override async Task<IReadOnlyCollection<TResult?>> QueryAsync<TResult>(string query, IDictionary<string, object?>? args = null,
            Capabilities? capabilities = Capabilities.Modifications, CancellationToken token = default)
            where TResult : default
        {
            var result = await ExecuteInternalAsync(query, args, token);

            if (result.Data is null)
                return Array.Empty<TResult?>();

            var arr = (JArray)result.Data;
            return arr.ToObject<TResult?[]>()!;
        }

        /// <inheritdoc/>
        /// <remarks>
        ///     <paramref name="capabilities"/> has no effect as the HTTP protocol does not support capabilities.
        /// </remarks>
        /// <exception cref="EdgeDBException">The server returned a status code other than 200.</exception>
        public override async Task<TResult> QueryRequiredSingleAsync<TResult>(string query, IDictionary<string, object?>? args = null,
            Capabilities? capabilities = Capabilities.Modifications, CancellationToken token = default)
        {
            var result = await ExecuteInternalAsync(query, args, token);

            if (result.Data is null)
                throw new MissingRequiredException();

            var arr = (JArray)result.Data;

            if (arr.Count is not 1)
                throw new InvalidDataException($"Expected 1 element but got {arr.Count}", new MissingRequiredException());

            return arr[0].ToObject<TResult>()!;
        }

        /// <inheritdoc/>
        /// <remarks>
        ///     <paramref name="capabilities"/> has no effect as the HTTP protocol does not support capabilities.
        /// </remarks>
        /// <exception cref="EdgeDBException">The server returned a status code other than 200.</exception>
        public override async Task<TResult?> QuerySingleAsync<TResult>(string query, IDictionary<string, object?>? args = null,
            Capabilities? capabilities = Capabilities.Modifications, CancellationToken token = default)
            where TResult : default
        {
            var result = await ExecuteInternalAsync(query, args, token);

            if (result.Data is null)
                return default;

            var arr = (JArray)result.Data;

            if (arr.Count > 1)
                throw new ResultCardinalityMismatchException(Cardinality.AtMostOne, Cardinality.Many);

            return arr.Any()
                ? arr[0].ToObject<TResult>(EdgeDBConfig.JsonSerializer)
                : default;
        }

        /// <inheritdoc/>
        /// <remarks>
        ///     <paramref name="capabilities"/> has no effect as the HTTP protocol does not support capabilities.
        /// </remarks>
        public override async Task<Json> QueryJsonAsync(string query, IDictionary<string, object?>? args = null, Capabilities? capabilities = Capabilities.Modifications, CancellationToken token = default)
        {
            var result = await ExecuteInternalAsync(query, args, token);

            if (result.Data is null)
                return default;

            return new(((JToken)result.Data).ToString());
        }

        /// <inheritdoc/>
        /// <remarks>s
        ///     <paramref name="capabilities"/> has no effect as the HTTP protocol does not support capabilities.
        /// </remarks>
        /// <exception cref="ResultCardinalityMismatchException">The result didn't return multiple json elements.</exception>
        public override async Task<IReadOnlyCollection<Json>> QueryJsonElementsAsync(string query, IDictionary<string, object?>? args = null, Capabilities? capabilities = Capabilities.Modifications, CancellationToken token = default)
        {
            var result = await ExecuteInternalAsync(query, args, token);

            if (result.Data is null)
                return Array.Empty<Json>();

            if (result.Data is not JArray jArray)
                throw new ResultCardinalityMismatchException(Cardinality.Many, Cardinality.One);

            return jArray.Select(x => new Json(x.ToString())).ToImmutableArray();
        }

        private async Task InvokeResultEventAsync(HttpQueryResult result)
        {
            try
            {
                await _onQuery.InvokeAsync(result).ConfigureAwait(false);
            }
            catch(Exception x)
            {
                _logger.EventHandlerError(x);
            }
        }
    }
}
