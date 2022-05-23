using EdgeDB.Models;
using EdgeDB.Utils;
using System.Collections.Concurrent;

namespace EdgeDB
{
    internal class ClientPacketDuplexer
    {
        public bool Connected
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
            if(_readTask != null)
                await _readTask!;
            _disconnectTokenSource = new();
        }

        public void Start(Stream stream)
        {
            _stream = stream;
            _readTask = Task.Run(async () => await ReadAsync());
        }

        public async ValueTask DisconnectAsync()
        {
            await SendAsync(packets: new Terminate()).ConfigureAwait(false);
            _disconnectTokenSource.Cancel();
        }

        private CancellationToken GetTimeoutToken()
            => new CancellationTokenSource(_client.MessageTimeout).Token;

        private async Task ReadAsync()
        {
            byte[] packetHeaderBuffer = new byte[5];

            try
            {
                while (Connected)
                {
                    if (_disconnectTokenSource.IsCancellationRequested)
                        return;

                    if (_stream == null)
                        return;

                    var result = await _stream.ReadAsync(packetHeaderBuffer, _disconnectTokenSource.Token).ConfigureAwait(false);

                    if (result != 5)
                    {
                        // disconnected
                        _disconnectTokenSource?.Cancel();
                        await _onDisconnected.InvokeAsync().ConfigureAwait(false);
                        return;
                    }

                    // read the length
                    var type = (ServerMessageType)packetHeaderBuffer[0];
                    var length = (BitConverter.IsLittleEndian
                        ? BitConverter.ToInt32(packetHeaderBuffer.Reverse().ToArray(), 0)
                        : BitConverter.ToInt32(packetHeaderBuffer, 1)) - 4; // length includes self

                    byte[] buffer = new byte[length];
                    if (await _stream.ReadAsync(buffer, _disconnectTokenSource.Token).ConfigureAwait(false) != length)
                    {
                        // disconnected
                        _disconnectTokenSource?.Cancel();
                        await _onDisconnected.InvokeAsync().ConfigureAwait(false);
                        return;
                    }

                    var msg = PacketSerializer.DeserializePacket(type, buffer, _client);

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

        public async Task<IReceiveable> DuplexAsync(Predicate<IReceiveable>? predicate = null, CancellationToken token = default, params Sendable[] packets)
        {
            var linkedToken = CancellationTokenSource.CreateLinkedTokenSource(token, _disconnectTokenSource.Token);

            // enter the duplex lock
            await _duplexLock.WaitAsync(linkedToken.Token).ConfigureAwait(false);

            try
            {
                var receiveTask = NextAsync(predicate, token: token);

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

            if (_stream == null || !Connected)
                throw new EdgeDBException("Cannot send message to a closed connection");

            using var ms = new MemoryStream();
            foreach (var packet in packets)
            {
                using var writer = new PacketWriter(ms);
                packet.Write(writer, _client);
            }

            await _stream.WriteAsync(ms.ToArray(), linkedToken.Token).ConfigureAwait(false);
        }

        public async Task<IReceiveable> NextAsync(Predicate<IReceiveable>? predicate = null, Action? readyCallback = null, CancellationToken token = default)
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
                    if (t.Type == ServerMessageType.ErrorResponse)
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

                readyCallback?.Invoke();

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
            Predicate<IReceiveable>? predicate = null, CancellationToken token = default)
            => duplexer.DuplexAsync(predicate, token, packet, new Sync());
        public static Task<IReceiveable> DuplexAsync(this ClientPacketDuplexer duplexer, Sendable packet,
            Predicate<IReceiveable>? predicate = null, CancellationToken token = default)
            => duplexer.DuplexAsync(predicate, token, packet);
    }
}
