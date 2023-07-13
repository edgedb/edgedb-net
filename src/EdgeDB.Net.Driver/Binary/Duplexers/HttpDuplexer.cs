using EdgeDB.Binary.Protocol;
using EdgeDB.Binary.Protocol.Common;
using EdgeDB.Utils;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace EdgeDB.Binary.Duplexers
{
    internal sealed class HttpDuplexer : IBinaryDuplexer
    {
        private static readonly Regex _contentTypeRegex = new(@"application\/x\.edgedb\.v_(\d+)_(\d+)\.binary");

        public bool IsConnected
            => _client.IsConnected;

        public CancellationToken DisconnectToken
            => default;

        public IProtocolProvider ProtocolProvider
            => _client.ProtocolProvider;

        public string ContentTypeHeader
            => $"application/x.edgedb.v_{ProtocolProvider.Version.Major}_{ProtocolProvider.Version.Minor}.binary";

        private readonly EdgeDBHttpClient _client;
        private readonly Queue<IReceiveable> _packetQueue;
        private readonly SemaphoreSlim _sendSemaphore;
        private readonly SemaphoreSlim _readSemaphore;
        private TaskCompletionSource _packetReadTCS;

        public HttpDuplexer(EdgeDBHttpClient client)
        {
            _client = client;
            _packetQueue = new(5);
            _packetReadTCS = new();
            _sendSemaphore = new(1, 1);
            _readSemaphore = new(1, 1);
        }

        public void Reset()
        {
            _packetReadTCS = new();
            _packetQueue.Clear();
        }

        public ValueTask DisconnectAsync(CancellationToken token = default(CancellationToken)) => ValueTask.CompletedTask;

        public async IAsyncEnumerable<DuplexResult> DuplexAsync([EnumeratorCancellation] CancellationToken token = default, params Sendable[] packets)
        {
            await _readSemaphore.WaitAsync(token).ConfigureAwait(false);

            try
            {
                await SendAsync(token, packets).ConfigureAwait(false);

                using var enumerationFinishToken = new CancellationTokenSource();

                while (_packetQueue.TryDequeue(out var packet) && !enumerationFinishToken.IsCancellationRequested)
                {
                    yield return new DuplexResult(packet, enumerationFinishToken);
                }

                yield break;
            }
            finally
            {
                _readSemaphore.Release();
            }
        }

        public async Task<IReceiveable?> ReadNextAsync(CancellationToken token = default)
        {
            if (!IsConnected)
                throw new EdgeDBException("Cannot read from a closed connection");

            await _readSemaphore.WaitAsync(token).ConfigureAwait(false);

            try
            {
                IReceiveable? packet;

                while (!_packetQueue.TryDequeue(out packet))
                    await _packetReadTCS.Task.ConfigureAwait(false);

                _packetReadTCS = new();

                return packet;
            }
            finally
            {
                _readSemaphore.Release();
            }
        }

        private Task<HttpResponseMessage> SendInternalAsync(CancellationToken token = default, params Sendable[] packets)
        {
            var data = BinaryUtils.BuildPackets(packets);

            var message = new HttpRequestMessage(HttpMethod.Post, _client.Connection.GetExecUri())
            {
                Content = new ReadOnlyMemoryContent(data)
            };

            message.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _client.AuthorizationToken);
            message.Content.Headers.ContentType = new(ContentTypeHeader);
            message.Headers.TryAddWithoutValidation("X-EdgeDB-User", _client.Connection.Username);

            return _client.HttpClient.SendAsync(message, token);
        }

        public async ValueTask SendAsync(CancellationToken token = default, params Sendable[] packets)
        {
            if (!IsConnected)
                throw new EdgeDBException("Cannot send message to a closed connection");

            await _sendSemaphore.WaitAsync(token).ConfigureAwait(false);

            try
            {
                var result = await SendInternalAsync(token, packets).ConfigureAwait(false);

                // only perform second iteration if debug log enabled.
                if (_client.Logger.IsEnabled(Microsoft.Extensions.Logging.LogLevel.Debug))
                {
                    foreach (var packet in packets)
                    {
                        _client.Logger.MessageSent(_client.ClientId, packet.Type, packet.Size);
                    }
                }

                var attempts = 0;

            process_result:

                result.EnsureSuccessStatusCode();

                if (result.Content.Headers.ContentType?.MediaType is null)
                    throw new EdgeDBException($"HTTP response content type is not present");

                if (result.Content.Headers.ContentType.MediaType != ContentTypeHeader)
                {
                    var match = _contentTypeRegex.Match(result.Content.Headers.ContentType.MediaType);

                    if(match.Success)
                    {
                        var major = ushort.Parse(match.Groups[1].Value);
                        var minor = ushort.Parse(match.Groups[2].Value);

                        if (!_client.TryNegotiateProtocol(in major, in minor))
                            throw new EdgeDBException($"The protocol requirements of the server cannot be met, theirs: {major}.{minor}, ours: {ProtocolProvider.Version}");

                        // resend with new provider
                        attempts++;

                        if (attempts > _client.ClientConfig.MaxConnectionRetries)
                            throw new EdgeDBException($"Failed to negotiate with server after {attempts} attempts");

                        result = await SendInternalAsync(token, packets);
                        goto process_result;
                    }
                    else
                        throw new EdgeDBException($"HTTP response content type is unknown/unsupported: {result.Content.Headers.ContentType.MediaType}");
                }
                    

                var stream = await result.Content.ReadAsStreamAsync().ConfigureAwait(false);
                var length = result.Content.Headers.ContentLength;

                if (!length.HasValue)
                    throw new EdgeDBException("HTTP response content length is not specified");

                await ReadPacketsAsync(stream, length.Value, token).ConfigureAwait(false);
            }
            finally
            {
                _sendSemaphore.Release();
            }
        }

        private async Task ReadPacketsAsync(Stream stream, long length, CancellationToken token)
        {
            long totalRead = 0;
            Memory<byte> headerBuffer = new byte[5];
            using var pin = headerBuffer.Pin(); // pin the header buffer so we can use its pointer to link a struct

            while (totalRead < length)
            {
                totalRead += await stream.ReadAsync(headerBuffer, token).ConfigureAwait(false);

                PacketHeader header;

                unsafe
                {
                    header = *(PacketHeader*)pin.Pointer;
                }

                header.CorrectLength();

                using var memoryOwner = MemoryPool<byte>.Shared.Rent(header.Length);
                var buffer = memoryOwner.Memory[..header.Length];

                totalRead += await stream.ReadAsync(buffer, token).ConfigureAwait(false);

                var packetFactory = ProtocolProvider.GetPacketFactory(header.Type);

                if (packetFactory is null)
                {
                    // unknow/unsupported packet
                    _client.Logger.UnknownPacket($"{header.Type}{{0x{(byte)header.Type}}}:{header.Length}");

                    throw new UnexpectedMessageException($"Unknown/unsupported message type {header.Type}");
                }

                var packet = PacketSerializer.DeserializePacket(in packetFactory, in buffer);

                if (packet is null)
                    throw new EdgeDBException($"Failed to deserialize packet type {header.Type}");

                _packetQueue.Enqueue(packet);

                _client.Logger.MessageReceived(_client.ClientId, header.Type, buffer.Length);
            }

            _packetReadTCS.TrySetResult();
        }

        void IDisposable.Dispose() {}
    }
}
