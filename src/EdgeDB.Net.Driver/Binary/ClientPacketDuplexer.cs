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
    internal sealed class ClientPacketDuplexer
    {
        public const int PACKET_HEADER_SIZE = 5;
        public bool IsConnected
            => _stream != null && (_client?.IsConnected ?? false);

        public bool IsReading
            => _readTask != null && _readTask.Status == TaskStatus.Running;

        public event Func<IReceiveable, ValueTask> OnMessage
        {
            add => _onMessage.Add(value);
            remove => _onMessage.Remove(value);
        }

        public event Func<ValueTask> OnDisconnected
        {
            add => _onDisconnected.Add(value);
            remove => _onDisconnected.Remove(value);
        }

        public CancellationToken DisconnectToken
            => _disconnectTokenSource.Token;

        private Stream? _stream;
        private Task? _readTask;
        private CancellationTokenSource _disconnectTokenSource;
        private readonly EdgeDBBinaryClient _client;
        private readonly AsyncEvent<Func<ValueTask>> _onDisconnected = new();
        private readonly AsyncEvent<Func<IReceiveable, ValueTask>> _onMessage = new();
        private readonly SemaphoreSlim _duplexLock;
        private readonly SemaphoreSlim _onMessageLock;

        public ClientPacketDuplexer(EdgeDBBinaryClient client)
        {
            _client = client;
            _disconnectTokenSource = new();
            _duplexLock = new(1, 1);
            _onMessageLock = new(1, 1);
        }

        public async Task ResetAsync()
        {
            if(_readTask != null && !_readTask.IsCanceled)
                await _readTask!;
            _disconnectTokenSource = new();
        }

        public void Start(Stream stream)
        {
            _stream = stream;
            _readTask = Task.Run(async () => await ReadAsync());
        }

        public async ValueTask DisconnectAsync(CancellationToken token = default)
        {
            try
            {
                await SendAsync(token, packets: new Terminate()).ConfigureAwait(false);
            }
            catch(EdgeDBException) { } // assume its because the connection is closed.
            
            _disconnectTokenSource.Cancel();
            await _onDisconnected.InvokeAsync().ConfigureAwait(false);
        }

        private CancellationToken GetTimeoutToken()
            => new CancellationTokenSource(_client.MessageTimeout).Token;

        private async Task ReadAsync()
        {
            Memory<byte> packetHeaderBuffer = new byte[PACKET_HEADER_SIZE];
            
            try
            {
                while (!_disconnectTokenSource.IsCancellationRequested)
                {
                    if (_stream == null)
                        return;
                    
                    if (await ReadExactAsync(packetHeaderBuffer, _disconnectTokenSource.Token).ConfigureAwait(false) == 0)
                    {
                        // disconnected
                        _disconnectTokenSource?.Cancel();
                        await _onDisconnected.InvokeAsync().ConfigureAwait(false);
                        return;
                    }

                    // read the length
                    var type = (ServerMessageType)packetHeaderBuffer.Span[0];
                    var length = (int)BinaryPrimitives.ReadUInt32BigEndian(packetHeaderBuffer.Span[1..5]) - 4;

                    using var memoryOwner = MemoryPool<byte>.Shared.Rent(length);
                    var buffer = memoryOwner.Memory[..length];
                    
                    if (await ReadExactAsync(buffer, _disconnectTokenSource.Token).ConfigureAwait(false) == 0)
                    {
                        // disconnected
                        _disconnectTokenSource?.Cancel();
                        await _onDisconnected.InvokeAsync().ConfigureAwait(false);
                        return;
                    }

                    var msg = PacketSerializer.DeserializePacket(type, ref buffer, length, _client);

                    if (msg != null)
                    {
                        await _onMessageLock.WaitAsync().ConfigureAwait(false);
                        try
                        {
                            await _onMessage.InvokeAsync(msg).ConfigureAwait(false);
                        }
                        finally
                        {
                            _onMessageLock.Release();
                        }
                    }
                }
            }
            catch(Exception x) when (x is not OperationCanceledException or TaskCanceledException)
            {
                _client.Logger.ReadException(x);
            }
        }

        private async ValueTask<int> ReadExactAsync(Memory<byte> buffer, CancellationToken token)
        {
            var targetLength = buffer.Length;

            int numRead = 0;

            while(numRead < targetLength)
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
        
        public async Task<IReceiveable> DuplexAsync(Predicate<IReceiveable>? predicate = null, bool alwaysReturnError = true, CancellationToken token = default, params Sendable[] packets)
        {
            var linkedToken = CancellationTokenSource.CreateLinkedTokenSource(token, _disconnectTokenSource.Token);

            // enter the duplex lock
            await _duplexLock.WaitAsync(linkedToken.Token).ConfigureAwait(false);

            try
            {
                var receiveTask = NextAsync(predicate, alwaysReturnError: alwaysReturnError, token: token);

                await SendAsync(token, packets).ConfigureAwait(false);

                return await receiveTask;
            }
            finally
            {
                _duplexLock.Release();
            }
        }

        public async ValueTask SendAsync(CancellationToken token = default, params Sendable[] packets)
        {
            var linkedToken = CancellationTokenSource.CreateLinkedTokenSource(token, _disconnectTokenSource.Token);

            if (_stream == null || !IsConnected)
                throw new EdgeDBException("Cannot send message to a closed connection");
            
            await _stream.WriteAsync(BuildPackets(packets), linkedToken.Token).ConfigureAwait(false);
        }

        private ReadOnlyMemory<byte> BuildPackets(Sendable[] packets)
        {
            var size = packets.Sum(x => x.GetSize());
            var writer = new PacketWriter(size);

            for (int i = 0; i != packets.Length; i++)
            {
                packets[i].Write(ref writer, _client);
            }
            
            return writer.GetBytes();
        }

        public async Task<IReceiveable> NextAsync(Predicate<IReceiveable>? predicate = null, bool alwaysReturnError = true, CancellationToken token = default)
        {
            // stop all modifications to the data event.
            await _onMessageLock.WaitAsync(token).ConfigureAwait(false);

            bool released = false;

            try
            {
                var tcs = new TaskCompletionSource<IReceiveable>();

                List<IReceiveable> got = new();

                var handler = (IReceiveable t) =>
                {
                    got.Add(t);

                    // always return errors.
                    if (alwaysReturnError && t.Type == ServerMessageType.ErrorResponse)
                    {
                        tcs.TrySetResult(t);
                        return ValueTask.CompletedTask;
                    }

                    try
                    {
                        if (predicate != null)
                        {
                            if (predicate(t))
                                tcs.TrySetResult(t);
                        }
                        else
                            tcs.TrySetResult(t);

                        return ValueTask.CompletedTask;
                    }
                    catch (Exception x)
                    {
                        tcs.TrySetException(x);
                        return ValueTask.CompletedTask;
                    }
                };

                var linkedToken = CancellationTokenSource.CreateLinkedTokenSource(token, _disconnectTokenSource.Token, GetTimeoutToken());

                linkedToken.Token.Register(() => tcs.TrySetCanceled(linkedToken.Token));

                _onMessage.Add(handler);

                _onMessageLock.Release();
                released = true;

                try
                {
                    return await tcs.Task.ConfigureAwait(false);
                }
                finally
                {
                    _onMessage.Remove(handler);
                }
            }
            finally
            {
                if (!released)
                    _onMessageLock.Release();
            }
        }
    }

    internal static class DuplexerExtensions
    {
        public static ValueTask SendAsync(this ClientPacketDuplexer duplexer, Sendable packet, CancellationToken token = default)
            => duplexer.SendAsync(token, packet);

        public static ValueTask SendAndSyncAsync(this ClientPacketDuplexer duplexer, Sendable packet,
            CancellationToken token = default)
            => duplexer.SendAsync(token, packet, new Sync());

        public static Task<IReceiveable> DuplexAndSyncAsync(this ClientPacketDuplexer duplexer, Sendable packet,
            Predicate<IReceiveable>? predicate = null, bool alwaysReturnError = true, CancellationToken token = default)
            => duplexer.DuplexAsync(predicate, alwaysReturnError, token, packet, new Sync());
        public static Task<IReceiveable> DuplexAsync(this ClientPacketDuplexer duplexer, Sendable packet,
            Predicate<IReceiveable>? predicate = null, bool alwaysReturnError = true, CancellationToken token = default)
            => duplexer.DuplexAsync(predicate, alwaysReturnError, token, packet);
    }
}
