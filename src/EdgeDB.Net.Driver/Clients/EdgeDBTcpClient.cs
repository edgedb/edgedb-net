using EdgeDB.Binary;
using EdgeDB.Binary.Codecs;
using EdgeDB.Utils;
using Microsoft.Extensions.Logging;
using System.Collections.Immutable;
using System.Dynamic;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;

namespace EdgeDB
{
    /// <summary>
    ///     Represents a TCP client used to interact with EdgeDB.
    /// </summary>
    internal sealed class EdgeDBTcpClient : EdgeDBBinaryClient, ITransactibleClient
    {
        /// <inheritdoc/>
        public override bool IsConnected
            => _tcpClient.Connected && _secureStream != null;
        
        /// <summary>
        ///     Gets this clients transaction state.
        /// </summary>
        public TransactionState TransactionState { get; private set; }

        internal override IBinaryDuplexer Duplexer
            => _duplexer;

        // might be incomplete frame
        internal bool DangerousIsDataAvailable
            => _stream?.DataAvailable ?? false;

        private readonly StreamDuplexer _duplexer;
        

        private TcpClient _tcpClient;
        private NetworkStream? _stream;
        private SslStream? _secureStream;

        /// <summary>
        ///     Creates a new TCP client with the provided conection and config.
        /// </summary>
        /// <param name="connection">The connection details used to connect to the database.</param>
        /// <param name="clientConfig">The configuration for this client.</param>
        /// <param name="clientPoolHolder">The client pool holder for this client.</param>
        /// <param name="clientId">The optional client id of this client. This is used for logging and client pooling.</param>
        public EdgeDBTcpClient(EdgeDBConnection connection, EdgeDBConfig clientConfig, IDisposable clientPoolHolder, ulong? clientId = null) 
            : base(connection, clientConfig, clientPoolHolder, clientId)
        {
            _tcpClient = new();
            _duplexer = new(this);
            _duplexer.OnDisconnected += HandleDuplexerDisconnectAsync;

        }
        private async ValueTask HandleDuplexerDisconnectAsync()
        {
            // if we receive a disconnect from the duplexer we should call our disconnect methods to property close down our client.
            await CloseStreamAsync().ConfigureAwait(false);
            await OnDisconnectInternal.InvokeAsync(this);
        }

        protected override async ValueTask<Stream> GetStreamAsync(CancellationToken token)
        {
            _tcpClient = new TcpClient();

            using var timeoutToken = CancellationTokenSource.CreateLinkedTokenSource(DisconnectCancelToken, token);

            timeoutToken.CancelAfter(ConnectionTimeout);

            try
            {
                await _tcpClient.ConnectAsync(Connection.Hostname!, Connection.Port, timeoutToken.Token).ConfigureAwait(false);
            }
            catch (OperationCanceledException x) when (timeoutToken.IsCancellationRequested)
            {
                throw new TimeoutException("The connection timed out", x);
            }
            catch (SocketException x)
            {
                switch (x.SocketErrorCode)
                {
                    case SocketError.ConnectionRefused
                        or SocketError.ConnectionAborted
                        or SocketError.ConnectionReset
                        or SocketError.HostNotFound
                        or SocketError.NotInitialized:
                        throw new ConnectionFailedTemporarilyException(x.SocketErrorCode);
                    default:
                        throw;
                }
            }

            _stream = _tcpClient.GetStream();

            _secureStream = new SslStream(_stream, false, Connection.ValidateServerCertificateCallback, null);

            var options = new SslClientAuthenticationOptions()
            {
                AllowRenegotiation = true,
                ApplicationProtocols = new List<SslApplicationProtocol>
                    {
                        new SslApplicationProtocol("edgedb-binary")
                    },
                TargetHost = Connection.Hostname,
                EnabledSslProtocols = System.Security.Authentication.SslProtocols.None,
                CertificateRevocationCheckMode = X509RevocationMode.NoCheck,
            };

            await _secureStream.AuthenticateAsClientAsync(options).ConfigureAwait(false);

            return _secureStream;
        }

        protected override ValueTask CloseStreamAsync(CancellationToken token = default)
        {
            _tcpClient.Close();
            return ValueTask.CompletedTask;
        }

        protected override void UpdateTransactionState(TransactionState state)
        {
            TransactionState = state;
        }

        /// <inheritdoc/>
        public override async ValueTask<bool> DisposeAsync()
        {
            var shouldDispose = await base.DisposeAsync();

            if (shouldDispose)
            {
                _stream?.Dispose();

                if (_secureStream is not null)
                    await _secureStream.DisposeAsync();
            }

            return shouldDispose;
        }

        #region ITransactibleClient
        /// <inheritdoc/>
        async Task ITransactibleClient.StartTransactionAsync(Isolation isolation, bool readOnly, bool deferrable, CancellationToken token)
        {
            var isolationMode = isolation switch
            {
                Isolation.Serializable => "serializable",
                _ => throw new EdgeDBException("Unknown isolation mode")
            };

            var readMode = readOnly ? "read only" : "read write";

            var deferMode = $"{(!deferrable ? "not " : "")}deferrable";

            await ExecuteInternalAsync($"start transaction isolation {isolationMode}, {readMode}, {deferMode}", capabilities: Capabilities.Transaction, token: token).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        async Task ITransactibleClient.CommitAsync(CancellationToken token)
            => await ExecuteInternalAsync($"commit", capabilities: Capabilities.Transaction, token: token).ConfigureAwait(false);

        /// <inheritdoc/>
        async Task ITransactibleClient.RollbackAsync(CancellationToken token)
            => await ExecuteInternalAsync($"rollback", capabilities: Capabilities.Transaction, token: token).ConfigureAwait(false);

        /// <inheritdoc/>
        TransactionState ITransactibleClient.TransactionState => TransactionState;
        #endregion
    }
}
