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
        private readonly SemaphoreSlim _contractLock;

        private readonly Memory<byte> _packetHeaderBuffer;
        private readonly MemoryHandle _packetHeaderHandle;
        private unsafe readonly PacketHeader* _packetHeader;

        private readonly object _connectivityLock = new();

        private Stream? _stream;
        private CancellationTokenSource _disconnectTokenSource;
        private PacketContract? _packetContract;

        public unsafe StreamDuplexer(EdgeDBBinaryClient client)
        {
            _client = client;
            _disconnectTokenSource = new();
            _duplexLock = new(1, 1);
            _onMessageLock = new(1, 1);
            _readLock = new(1, 1);
            _contractLock = new(1, 1);

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
            await _contractLock.WaitAsync(linkedToken.Token).ConfigureAwait(false);

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

                _client.Logger.MessageReceived(_client.ClientId, header.Type, header.Length);

                var contract = PacketSerializer.CreateContract(this, _stream!, ref header, _client.ClientConfig.PacketChunkSize);

                var packet = PacketSerializer.DeserializePacket(header.Type, ref contract, _client);

                // check for idle timeout
                if(packet is ErrorResponse err && err.ErrorCode == ServerErrorCodes.IdleSessionTimeoutError)
                {
                    // all connection state needs to be reset for the client here.
                    _client.Logger.IdleDisconnect();

                    await DisconnectInternalAsync();
                    throw new EdgeDBErrorException(err);
                }

                _packetContract = contract;

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

            await _stream.WriteAsync(BinaryUtils.BuildPackets(packets), linkedToken.Token).ConfigureAwait(false);

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

        private async ValueTask<int> ReadExactAsync(Memory<byte> buffer, CancellationToken token)
        {
            var sw = Stopwatch.StartNew();

            try
            {
#if NET7_0_OR_GREATER
                await _stream!.ReadExactlyAsync(buffer, token).ConfigureAwait(false);
                return buffer.Length;
#else
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
#endif
            }
            finally
            {
                sw.Stop();

                var delta = sw.ElapsedMilliseconds / _client.MessageTimeout.TotalMilliseconds;
                if (delta >= 0.75)
                {
                    if (delta < 1)
                        _client.Logger.MessageTimeoutDeltaWarning((int)(delta * 100), (int)_client.MessageTimeout.TotalMilliseconds);
                    else
                        _client.Logger.MessageTimeoutDeltaError((int)(delta * 100), (int)_client.MessageTimeout.TotalMilliseconds);
                }
            }

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
