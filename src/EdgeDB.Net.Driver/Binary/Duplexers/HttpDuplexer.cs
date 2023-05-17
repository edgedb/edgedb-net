using EdgeDB.Binary.Packets;
using EdgeDB.Utils;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Binary.Duplexers
{
    internal sealed class HttpDuplexer : IBinaryDuplexer
    {
        public const string HTTP_BINARY_CONTENT_TYPE = "application/x.edgedb.v_1_0.binary";

        public bool IsConnected
            => _client.IsConnected;

        public CancellationToken DisconnectToken
            => default;

        private readonly EdgeDBHttpClient _client;
        private readonly Queue<IReceiveable> _packetQueue;
        private readonly SemaphoreSlim _sendSemaphore;
        private readonly SemaphoreSlim _readSemaphore;
        private readonly SemaphoreSlim _contractLock;

        private TaskCompletionSource _packetReadTCS;
        private PacketContract? _packetContract;


        public HttpDuplexer(EdgeDBHttpClient client)
        {
            _client = client;
            _packetQueue   = new(5);
            _packetReadTCS = new();
            _sendSemaphore = new(1, 1);
            _readSemaphore = new(1, 1);
            _contractLock  = new(1, 1);
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

        public async ValueTask SendAsync(CancellationToken token = default, params Sendable[] packets)
        {
            if (!IsConnected)
                throw new EdgeDBException("Cannot send message to a closed connection");

            await _sendSemaphore.WaitAsync(token).ConfigureAwait(false);

            try
            {
                var data = BinaryUtils.BuildPackets(packets);

                var message = new HttpRequestMessage(HttpMethod.Post, _client.Connection.GetExecUri())
                {
                    Content = new ReadOnlyMemoryContent(data)
                };

                message.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _client.AuthorizationToken);
                message.Content.Headers.ContentType = new(HTTP_BINARY_CONTENT_TYPE);
                message.Headers.TryAddWithoutValidation("X-EdgeDB-User", _client.Connection.Username);

                var result = await _client.HttpClient.SendAsync(message, token).ConfigureAwait(false);

                // only perform second iteration if debug log enabled.
                if (_client.Logger.IsEnabled(Microsoft.Extensions.Logging.LogLevel.Debug))
                {
                    foreach (var packet in packets)
                    {
                        _client.Logger.MessageSent(_client.ClientId, packet.Type, packet.Size);
                    }
                }

                result.EnsureSuccessStatusCode();

                if (result.Content.Headers.ContentType is null || result.Content.Headers.ContentType.MediaType != HTTP_BINARY_CONTENT_TYPE)
                    throw new EdgeDBException($"HTTP response content type is not {HTTP_BINARY_CONTENT_TYPE}");

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
                await _contractLock.WaitAsync(token).ConfigureAwait(false);

                totalRead += await stream.ReadAsync(headerBuffer, token).ConfigureAwait(false);

                PacketHeader header;

                unsafe
                {
                    header = *(PacketHeader*)pin.Pointer;
                    header.CorrectLength();
                }

                if(_packetContract != null)
                {
                    if(!_packetContract.IsEmpty)
                    {
                        throw new EdgeDBException("Previous packet failed to be processed fully before the next packet was read");
                    }
                } 

                _packetContract = PacketSerializer.CreateContract(this, stream, ref header, _client.ClientConfig.PacketChunkSize);

                var packet = PacketSerializer.DeserializePacket(header.Type, ref _packetContract, _client);

                if (packet is null)
                    throw new EdgeDBException($"Failed to deserialize packet type {header.Type}");

                _packetQueue.Enqueue(packet);

                _packetReadTCS.TrySetResult();

                _client.Logger.MessageReceived(_client.ClientId, header.Type, header.Length);
            }

            
        }

        void IDisposable.Dispose() {}

        public void OnContractComplete(PacketContract contract)
        {
            if (_packetContract != contract)
            {
                contract.Dispose();
                throw new EdgeDBException("Dangling packet contract was unexpectedly complete.");
            }

            contract.Dispose();
            _contractLock.Release();

        }

        public void OnContractDisconnected(PacketContract contract)
        {
            contract.Dispose();
            _contractLock.Release();
        }
    }
}
