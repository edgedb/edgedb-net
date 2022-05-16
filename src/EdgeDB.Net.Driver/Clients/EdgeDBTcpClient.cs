using EdgeDB.Codecs;
using EdgeDB.Models;
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
    public sealed class EdgeDBTcpClient : EdgeDBBinaryClient
    {
        public override bool IsConnected
            => TcpClient.Connected && _secureStream != null;


        internal TcpClient TcpClient;
        private NetworkStream? _stream;
        private SslStream? _secureStream;

        public EdgeDBTcpClient(EdgeDBConnection connection, EdgeDBConfig config, ulong? clientId = null) 
            : base(connection, config, clientId)
        {
            TcpClient = new();
        }

        public override async ValueTask<Stream> GetStreamAsync()
        {
            TcpClient = new TcpClient();

            var timeoutToken = CancellationTokenSource.CreateLinkedTokenSource(DisconnectCancelToken);

            timeoutToken.CancelAfter(ConnectionTimeout);

            try
            {
                await TcpClient.ConnectAsync(Connection.Hostname!, Connection.Port, timeoutToken.Token).ConfigureAwait(false);
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
                        throw new ConnectionFailedTemporarilyException();

                    default:
                        throw;
                }
            }

            _stream = TcpClient.GetStream();

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

        public override ValueTask CloseStreamAsync()
        {
            TcpClient.Close();
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