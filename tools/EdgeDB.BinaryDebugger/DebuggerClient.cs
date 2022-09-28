using EdgeDB.Binary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.BinaryDebugger
{
    internal class DebuggerClient : EdgeDBBinaryClient
    {
        public StreamWriter Writer { get; }
        public FileStream FileStream { get; }

        public override bool IsConnected => _tcpClient.Connected;

        private Stream? _proxy;

        private bool _readingPacket;
        private ServerMessageType _packetType;
        private bool _isReadingBody;
        private int _packetLength;
        private List<byte>? _packetBody;

        private TcpClient _tcpClient;
        private NetworkStream? _stream;
        private SslStream? _secureStream;

        public DebuggerClient(EdgeDBConnection connection, EdgeDBConfig config, ulong? clientId = null)
            : base(connection, config, null!, clientId)
        {
            if (File.Exists("./debug.log"))
                File.Delete("./debug.log");

            _tcpClient = new();

            FileStream = File.OpenWrite("./debug.log");
            Writer = new StreamWriter(FileStream, Encoding.UTF8);
        }

        protected override async ValueTask<Stream> GetStreamAsync(CancellationToken token = default)
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

            _proxy = new StreamProxy(_secureStream, OnRead, OnWrite);
            return _proxy;
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

        private void OnRead(int read, byte[] buffer, int offset, int count)
        {
            if (!_readingPacket && count == 1)
            {
                _readingPacket = true;
                _packetType = (ServerMessageType)buffer[0];
                return;
            }

            if (!_isReadingBody && count == 4)
            {
                _isReadingBody = true;
                _packetLength = BitConverter.ToInt32(buffer.Reverse().ToArray());
                _packetBody = new();
                return;

            }

            if (_isReadingBody)
            {
                _packetBody!.AddRange(buffer);

                if (_packetBody.Count >= _packetLength - 4)
                {
                    Writer.WriteLine($"READING\n" +
                             $"=======\n" +
                             $"Packet: {_packetType}\n" +
                             $"Length: {_packetLength}\n" +
                             $"Data: {Utils.HexConverter.ToHex(_packetBody.ToArray())}\n" +
                             $"Raw: {_packetType:X}{Utils.HexConverter.ToHex(BitConverter.GetBytes(_packetLength).Reverse().ToArray())}{Utils.HexConverter.ToHex(_packetBody.ToArray())}\n");

                    _isReadingBody = false;
                    _readingPacket = false;
                }
            }
        }

        private void OnWrite(byte[] buffer, int offset, int count)
        {
            Writer.WriteLine($"WRITING\n" +
                             $"=======\n" +
                             $"Buffer: {EdgeDB.Utils.HexConverter.ToHex(buffer)}\n" +
                             $"Length: {count}\n" +
                             $"Offset: {offset}\n");
        }

        protected override ValueTask CloseStreamAsync(CancellationToken token = default)
        {
            _proxy?.Close();
            FileStream.Flush();
            FileStream.Close();

            return ValueTask.CompletedTask;
        }
    }
}
