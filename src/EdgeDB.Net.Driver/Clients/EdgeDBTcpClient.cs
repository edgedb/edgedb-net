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
    internal sealed class EdgeDBTcpClient : EdgeDBBinaryClient
    {
        /// <inheritdoc/>
        public override bool IsConnected
            => _tcpClient.Connected && _secureStream != null;


        private TcpClient _tcpClient;
        private NetworkStream? _stream;
        private SslStream? _secureStream;

        /// <summary>
        ///     Creates a new TCP client with the provided conection and config.
        /// </summary>
        /// <param name="connection">The connection details used to connect to the database.</param>
        /// <param name="config">The configuration for this client.</param>
        /// <param name="clientPoolHolder">The client pool holder for this client.</param>
        /// <param name="clientId">The optional client id of this client. This is used for logging and client pooling.</param>
        public EdgeDBTcpClient(EdgeDBConnection connection, EdgeDBConfig config, IDisposable clientPoolHolder, ulong? clientId = null) 
            : base(connection, config, clientPoolHolder, clientId)
        {
            _tcpClient = new();
        }

        protected override async ValueTask<Stream> GetStreamAsync(CancellationToken token)
        {
            _tcpClient = new TcpClient();

            var timeoutToken = CancellationTokenSource.CreateLinkedTokenSource(DisconnectCancelToken, token);

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

            _secureStream = new SslStream(_stream, false, new RemoteCertificateValidationCallback(ValidateServerCertificate), null);

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

        protected override ValueTask CloseStreamAsync(CancellationToken token)
        {
            _tcpClient.Close();
            return ValueTask.CompletedTask;
        }

        private bool ValidateServerCertificate(object sender, X509Certificate? certificate, X509Chain? chain, SslPolicyErrors sslPolicyErrors)
        {
            if (Connection.TLSSecurity is TLSSecurityMode.Insecure)
                return true;

            if (Connection.TLSCertificateAuthority is not null)
            {
                var cert = Connection.GetCertificate()!;

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
                return sslPolicyErrors is SslPolicyErrors.None;
            }
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
    }
}
