using EdgeDB.Binary;
using EdgeDB.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Security;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.BinaryDebugger
{

    internal class DebuggerClient : EdgeDBBinaryClient
    {
        public StreamWriter Writer { get; }
        public FileStream FileStream { get; }
        internal override IBinaryDuplexer Duplexer => _duplexer;

        public override bool IsConnected => _tcpClient.Connected;

        private Stream? _proxy;
        private StreamDuplexer _duplexer;

        private TcpClient _tcpClient;
        private NetworkStream? _stream;
        private SslStream? _secureStream;

        public DebuggerClient(EdgeDBConnection connection, EdgeDBConfig config, ulong? clientId = null)
            : base(connection, config, null!, clientId)
        {
            if (File.Exists("./debug.log"))
                File.Delete("./debug.log");

            _tcpClient = new();
            _duplexer = new(this);
            
            FileStream = File.OpenWrite("./debug.log");
            Writer = new StreamWriter(FileStream, Encoding.UTF8);
        }

        protected override async ValueTask<Stream> GetStreamAsync(CancellationToken token = default)
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

        private readonly struct PacketHeader
        {
            public readonly ServerMessageType Type { get; init; }
            public readonly int Length { get; init; }
            public readonly byte[] Raw { get; init; }
        }

        private PacketHeader? Header { get; set; }
        
        private void OnRead(int read, byte[] buffer, int offset, int count)
        {
            var copyBuffer = new byte[buffer.Length];

            buffer.CopyTo(copyBuffer, 0);
            var reader = new PacketReader(copyBuffer);
            
            if(read == 5)
            {
                var raw = copyBuffer.ToArray();
                // packet header
                Header = new PacketHeader
                {
                    Type = (ServerMessageType)reader.ReadByte(),
                    Length = reader.ReadInt32() - 4,
                    Raw = raw[..5]
                };
                return;
            }

            if (Header.HasValue)
            {
                var memory = copyBuffer.AsMemory()[..read];
                var raw = HexConverter.ToHex(memory.ToArray());
                var packet = PacketSerializer.DeserializePacket(Header.Value.Type, ref memory, Header.Value.Length, this);

                Writer.WriteLine($"READING\n" +
                                $"=======\n" +
                                $"Packet Type: {Header.Value.Type}\n" +
                                $"Raw: {HexConverter.ToHex(Header.Value.Raw)}{raw}\n" +
                                $"{JsonConvert.SerializeObject(packet, Formatting.Indented, new JsonSerializerSettings { MaxDepth = 5 })}\n");

                Header = null;
            }
        }

        private void OnWrite(byte[] buffer, int offset, int count)
        {
            Writer.WriteLine($"WRITING\n" +
                             $"=======\n" +
                             $"Buffer: {EdgeDB.Utils.HexConverter.ToHex(buffer)}\n" +
                             $"Type: {(ClientMessageTypes)buffer[0]}\n" +
                             $"Length: {count}\n" +
                             $"Offset: {offset}\n");
        }

        protected override ValueTask CloseStreamAsync(CancellationToken token = default)
        {
            _proxy?.Close();
            Writer.Flush();
            FileStream.Flush();
            FileStream.Close();

            return ValueTask.CompletedTask;
        }
    }
}
