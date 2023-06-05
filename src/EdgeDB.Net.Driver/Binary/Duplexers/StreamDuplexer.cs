using EdgeDB.Binary;
using EdgeDB.Binary.Packets;
using EdgeDB.Utils;
using System.Buffers;
using System.Buffers.Binary;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net.Security;
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

#if !LEGACY_BUFFERS
        private readonly Memory<byte> _packetHeaderBuffer;
        private readonly MemoryHandle _packetHeaderHandle;
        private unsafe readonly PacketHeader* _packetHeader;
#endif

        private readonly object _connectivityLock = new();

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

#if !LEGACY_BUFFERS
            _packetHeaderBuffer = new byte[PACKET_HEADER_SIZE];
            _packetHeaderHandle = _packetHeaderBuffer.Pin();
            _packetHeader = (PacketHeader*)_packetHeaderHandle.Pointer;
#endif
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
                if (IsConnected)
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

#if LEGACY_BUFFERS
            var headerBuffer = new byte[PACKET_HEADER_SIZE];
#endif

            try
            {
                if (linkedToken.IsCancellationRequested)
                    return null;

                if (_stream is null)
                    return null;

                if (await BinaryUtils.ReadExactAsync(
                    _stream,
                    _client,
#if LEGACY_BUFFERS
                    headerBuffer,
                    PACKET_HEADER_SIZE,
#else
                    _packetHeaderBuffer,
#endif

                    linkedToken.Token
                ).ConfigureAwait(false) == 0)
                {
                    // disconnected
                    await DisconnectInternalAsync();
                    return null;
                }

                PacketHeader header;

                unsafe
                {
#if LEGACY_BUFFERS
                    fixed (byte* ptr = headerBuffer)
                    {
                        header = *(StreamDuplexer.PacketHeader*)ptr;
                    }
#else
                    header = *_packetHeader;
#endif
                }

                header.CorrectLength();

#if LEGACY_BUFFERS
                var bufferSource = new BufferedSource(ArrayPool<byte>.Shared.Rent(header.Length), header.Length);
#else
                using var memoryOwner = MemoryPool<byte>.Shared.Rent(header.Length);
                var bufferSource = new BufferedSource(memoryOwner.Memory[..header.Length]);
#endif

                if (await BinaryUtils.ReadExactAsync(
                    _stream,
                    _client,
#if LEGACY_BUFFERS
                    bufferSource,
                    header.Length,
#else
                    bufferSource,
#endif
                    linkedToken.Token
                ).ConfigureAwait(false) == 0)
                {
                    await DisconnectInternalAsync();
                    return null;
                }

                _client.Logger.MessageReceived(_client.ClientId, header.Type, header.Length);

                var packet = PacketSerializer.DeserializePacket(header.Type, ref bufferSource, header.Length, _client);

                // check for idle timeout
                if(packet is ErrorResponse err && err.ErrorCode == ServerErrorCodes.IdleSessionTimeoutError)
                {
                    // all connection state needs to be reset for the client here.
                    _client.Logger.IdleDisconnect();

                    await DisconnectInternalAsync();
                    throw new EdgeDBErrorException(err);
                }

                return packet;
            }
            catch (EndOfStreamException)
            {
                // disconnect
                await DisconnectInternalAsync();
                return null;
            }
            catch(EdgeDBErrorException)
            {
                throw;
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
            if (_stream == null || !IsConnected)
            {
                await _client.ReconnectAsync(); //don't pass token here, it may be cancelled due to the disconnect token.
            }

            // check stream after reconnect
            if (_stream is null)
            {
                throw new EdgeDBException("Cannot send message to a force-closed connection");
            }

            using var linkedToken = CancellationTokenSource.CreateLinkedTokenSource(token, _disconnectTokenSource.Token);

            var packetData =
#if LEGACY_BUFFERS
                BinaryUtils.GetByteArray(BinaryUtils.BuildPackets(packets));
#else
                BinaryUtils.BuildPackets(packets);
#endif


            await _stream.WriteAsync(
#if LEGACY_BUFFERS
                packetData.Array,
                packetData.Offset,
                packetData.Count,
#else
                packetData,
#endif
                linkedToken.Token).ConfigureAwait(false);

            // only perform second iteration if debug log enabled.
            if(_client.Logger.IsEnabled(Microsoft.Extensions.Logging.LogLevel.Debug))
            {
                foreach(var packet in packets)
                {
                    _client.Logger.MessageSent(_client.ClientId, packet.Type, packet.Size);
                }
            }
        }

        public async IAsyncEnumerable<DuplexResult> DuplexAsync(
            [EnumeratorCancellation] CancellationToken token = default,
            params Sendable[] packets)
        {
            await SendAsync(token, packets).ConfigureAwait(false);

            using var enumerationFinishToken = new CancellationTokenSource();

            while (!enumerationFinishToken.IsCancellationRequested && !token.IsCancellationRequested)
            {
                var result = await ReadNextAsync(token).ConfigureAwait(false);

                if (result is null)
                    yield break;

                yield return new DuplexResult(result, enumerationFinishToken);
            }

            yield break;
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
#if !LEGACY_BUFFERS
            _packetHeaderHandle.Dispose();
#endif
        }
    }
}
