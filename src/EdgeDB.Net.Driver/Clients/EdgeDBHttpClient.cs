using EdgeDB.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB
{
    public class HttpQueryResult : IExecuteResult
    {
        [JsonProperty("data")]
        public object? Data { get; set; }

        [JsonProperty("error")]
        public QueryResultError? Error { get; set; }

        public class QueryResultError : IExecuteError
        {
            [JsonProperty("message")]
            public string? Message { get; set; }
            [JsonProperty("type")]
            public string? Type { get; set; }
            [JsonProperty("code")]
            public uint Code { get; set; }

            string? IExecuteError.Message => Message;

            uint IExecuteError.ErrorCode => Code;
        }

        bool IExecuteResult.IsSuccess => Error == null;

        IExecuteError? IExecuteResult.ExecutionError => Error;

        Exception? IExecuteResult.Exception => null;

        string? IExecuteResult.ExecutedQuery => null;
    }

    public class EdgeDBHttpClient : BaseEdgeDBClient
    {
        public event Func<HttpQueryResult, Task> QueryExecuted
        {
            add => _onQuery.Add(value);
            remove => _onQuery.Remove(value);
        }

        public override bool IsConnected => true;
        public readonly Uri Uri;
        
        private readonly EdgeDBConfig _config;
        private readonly EdgeDBConnection _connection;
        private readonly HttpClient _httpClient;
        private readonly AsyncEvent<Func<HttpQueryResult, Task>> _onQuery = new();
        private readonly ILogger _logger;

        private class QueryPostBody
        {
            [JsonProperty("query")]
            public string? Query { get; set; }

            [JsonProperty("variables")]
            public IDictionary<string, object?>? Variables { get; set; }
        }

        public EdgeDBHttpClient(EdgeDBConnection connection, EdgeDBConfig config, ulong clientId)
            : base(clientId)
        {
            _logger = config.Logger ?? Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
            _config = config;
            _connection = connection;
            Uri = new($"http{(connection.TLSSecurity != TLSSecurityMode.Insecure ? "s" : "")}://{connection.Hostname}:{connection.Port}/db/{connection.Database}/edgeql");
            
            var handler = new HttpClientHandler();
            handler.ClientCertificateOptions = ClientCertificateOption.Manual;
            handler.SslProtocols = SslProtocols.None;
            handler.ServerCertificateCustomValidationCallback = ValidateServerCertificate;
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

        public override ValueTask DisconnectAsync()
            => default;

        public override ValueTask ConnectAsync() 
            => default;

        private async Task<HttpQueryResult> ExecuteInternalAsync(string query, IDictionary<string, object?>? args = null)
        {
            var content = new StringContent(JsonConvert.SerializeObject(new QueryPostBody()
            {
                Query = query,
                Variables = args
            }, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore }), Encoding.UTF8, "application/json");

            var httpResult = await _httpClient.PostAsync(Uri, content).ConfigureAwait(false);

            if (!httpResult.IsSuccessStatusCode)
                throw new EdgeDBException($"Failed to execute HTTP query, got {httpResult.StatusCode}");

            var json = await httpResult.Content.ReadAsStringAsync().ConfigureAwait(false);

            var result = JsonConvert.DeserializeObject<HttpQueryResult>(json)!;
            await InvokeResultEventAsync(result).ConfigureAwait(false);
            return result;
        }

        public override async Task ExecuteAsync(string query, IDictionary<string, object?>? args = null)
        {
            await ExecuteInternalAsync(query, args).ConfigureAwait(false);
        }

        public override async Task<IReadOnlyCollection<TResult?>> QueryAsync<TResult>(string query, IDictionary<string, object?>? args = null)
            where TResult : default
        {
            var result = await ExecuteInternalAsync(query, args);

            if (result.Data == null)
                return Array.Empty<TResult?>();

            var arr = (JArray)result.Data;
            return arr.ToObject<TResult?[]>()!;
        }

        public override async Task<TResult> QueryRequiredSingleAsync<TResult>(string query, IDictionary<string, object?>? args = null)
        {
            var result = await ExecuteInternalAsync(query, args);

            if (result.Data == null)
                throw new MissingRequiredException();

            var arr = (JArray)result.Data;

            if (arr.Count != 1)
                throw new InvalidDataException($"Expected 1 element but got {arr.Count}", new MissingRequiredException());

            return arr[0].ToObject<TResult>()!;
        }

        public override async Task<TResult?> QuerySingleAsync<TResult>(string query, IDictionary<string, object?>? args = null)
            where TResult : default
        {
            var result = await ExecuteInternalAsync(query, args);

            if (result.Data == null)
                return default;

            var arr = (JArray)result.Data;

            if (arr.Count != 1)
                throw new InvalidDataException($"Expected 1 element but got {arr.Count}", new MissingRequiredException());

            return arr[0].ToObject<TResult>();
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
