using EdgeDB.Binary;
using EdgeDB.Binary.Packets;
using EdgeDB.Utils;
using System.Buffers;
using System.Buffers.Binary;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace EdgeDB.Binary
{
    internal sealed class StreamDuplexer : IBinaryDuplexer
    {
        public const int PACKET_HEADER_SIZE = 5;
        public bool IsConnected
            => _stream != null && (_client?.IsConnected ?? false);

        public bool IsReading
            => _readLock.CurrentCount == 0;

        public event Func<ValueTask> OnDisconnected
        {
            add => _onDisconnected.Add(value);
            remove => _onDisconnected.Remove(value);
        }

        public CancellationToken DisconnectToken
            => _disconnectTokenSource.Token;

        private readonly EdgeDBBinaryClient _client;
        private readonly AsyncEvent<Func<ValueTask>> _onDisconnected = new();
        private readonly AsyncEvent<Func<IReceiveable, ValueTask>> _onMessage = new();
        private readonly SemaphoreSlim _duplexLock;
        private readonly SemaphoreSlim _onMessageLock;
        private readonly SemaphoreSlim _readLock;

        private readonly Memory<byte> _packetHeaderBuffer;
        private readonly MemoryHandle _packetHeaderHandle;
        private unsafe readonly PacketHeader* _packetHeader;

        private Stream? _stream;
        private CancellationTokenSource _disconnectTokenSource;

        [StructLayout(LayoutKind.Explicit, Pack = 0, Size = 5)]
        internal struct PacketHeader
        {
            [FieldOffset(0)]
            public readonly ServerMessageType Type;

            [FieldOffset(1)]
            public int Length;

            public void CorrectLength()
            {
                BinaryUtils.CorrectEndianness(ref Length);
                // remove the length of "Length" from the length of the packet
                Length -= 4;
            }
        }

        public unsafe StreamDuplexer(EdgeDBBinaryClient client)
        {
            _client = client;
            _disconnectTokenSource = new();
            _duplexLock = new(1, 1);
            _onMessageLock = new(1, 1);
            _readLock = new(1, 1);

            _packetHeaderBuffer = new byte[PACKET_HEADER_SIZE];
            _packetHeaderHandle = _packetHeaderBuffer.Pin();
            _packetHeader = (PacketHeader*)_packetHeaderHandle.Pointer;
        }

        public void Init(Stream stream)
        {
            _stream = stream;
        }

        public void Reset()
        {
            _disconnectTokenSource = new();
        }

        public async ValueTask DisconnectAsync(CancellationToken token = default)
        {
            try
            {
                if(IsConnected)
                    await SendAsync(token, packets: new Terminate()).ConfigureAwait(false);
            }
            catch (EdgeDBException) { } // assume its because the connection is closed.

            await DisconnectInternalAsync();
        }

        public async Task<IReceiveable?> ReadNextAsync(CancellationToken token = default)
        {
            using var timeoutTokenSource = GetTimeoutToken();
            using var linkedToken = CancellationTokenSource.CreateLinkedTokenSource(token, _disconnectTokenSource.Token, timeoutTokenSource.Token);

            await _readLock.WaitAsync(linkedToken.Token).ConfigureAwait(false);

            try
            {
                if (linkedToken.IsCancellationRequested)
                    return null;

                if (await ReadExactAsync(_packetHeaderBuffer, linkedToken.Token).ConfigureAwait(false) == 0)
                {
                    // disconnected
                    await DisconnectInternalAsync();
                    return null;
                }

                PacketHeader header;

                unsafe
                {
                    header = *_packetHeader;
                    header.CorrectLength();
                }

                using var memoryOwner = MemoryPool<byte>.Shared.Rent(header.Length);
                var buffer = memoryOwner.Memory[..header.Length];

                if (await ReadExactAsync(buffer, linkedToken.Token).ConfigureAwait(false) == 0)
                {
                    await DisconnectInternalAsync();
                    return null;
                }

                _client.Logger.MessageReceived(_client.ClientId, header.Type);
                return PacketSerializer.DeserializePacket(header.Type, ref buffer, header.Length, _client);
            }
            catch (Exception x) when (x is not OperationCanceledException or TaskCanceledException)
            {
                _client.Logger.ReadException(x);
                throw;
            }
            finally
            {
                _readLock.Release();
            }
        }
        
        public async ValueTask SendAsync(CancellationToken token = default, params Sendable[] packets)
        {
            using var linkedToken = CancellationTokenSource.CreateLinkedTokenSource(token, _disconnectTokenSource.Token);

            if (_stream == null || !IsConnected)
                throw new EdgeDBException("Cannot send message to a closed connection");

            await _stream.WriteAsync(BinaryUtils.BuildPackets(packets), linkedToken.Token).ConfigureAwait(false);
        }
        
        public async IAsyncEnumerable<DuplexResult> DuplexAsync(
            [EnumeratorCancellation] CancellationToken token = default,
            params Sendable[] packets)
        {
            await SendAsync(token, packets).ConfigureAwait(false);
            
            using var enumerationFinishToken = new CancellationTokenSource();

            while(!enumerationFinishToken.IsCancellationRequested && !token.IsCancellationRequested)
            {
                var result = await ReadNextAsync(token).ConfigureAwait(false);

                if (result is null)
                    yield break;

                yield return new DuplexResult(result, enumerationFinishToken);
            }

            yield break;
        }
        
        private async ValueTask<int> ReadExactAsync(Memory<byte> buffer, CancellationToken token)
        {
            var targetLength = buffer.Length;

            int numRead = 0;

            while (numRead < targetLength)
            {
                var buff = numRead == 0
                    ? buffer
                    : buffer[numRead..];

                var read = await _stream!.ReadAsync(buff, token);

                if (read == 0) // disconnected
                    return 0;

                numRead += read;
            }

            return numRead;
        }
        
        private CancellationTokenSource GetTimeoutToken()
            => new(_client.MessageTimeout);

        private async ValueTask DisconnectInternalAsync()
        {
            _disconnectTokenSource?.Cancel();
            await _onDisconnected.InvokeAsync().ConfigureAwait(false);
        }

        public void Dispose()
        {
            _disconnectTokenSource.Dispose();
            _packetHeaderHandle.Dispose();
        }
    }
}
