using EdgeDB.Codecs;
using EdgeDB.Dumps;
using EdgeDB.Models;
using EdgeDB.Utils;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Collections.Immutable;
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
    public sealed class EdgeDBTcpClient : IEdgeDBQueryable, IAsyncDisposable
    {
        /// <summary>
        ///     Fired when the client receives a message.
        /// </summary>
        public event Func<IReceiveable, Task> OnMessage
        {
            add => _onMessage.Add(value);
            remove => _onMessage.Remove(value);
        }

        /// <summary>
        ///     Fired when a query is executed.
        /// </summary>
        public event Func<ExecuteResult, Task> QueryExecuted 
        {
            add => _queryExecuted.Add(value);
            remove => _queryExecuted.Remove(value);
        }

        /// <summary>
        ///     Fired when the client disconnects.
        /// </summary>
        public event Func<Task> OnDisconnect
        {
            add => _onDisconnect.Add(value);
            remove => _onDisconnect.Remove(value);
        }

        internal event Func<EdgeDBTcpClient, Task<bool>> OnDisposed
        {
            add => _onDisposed.Add(value);
            remove => _onDisposed.Remove(value);
        }

        /// <summary>
        ///     Gets whether or not this connection is idle.
        /// </summary>
        public bool IsIdle { get; private set; } = true;

        /// <summary>
        ///     Gets the raw server config.
        /// </summary>
        public IReadOnlyDictionary<string, object?> ServerConfig;

        /// <summary>
        ///     Gets this clients transaction state.
        /// </summary>
        public TransactionState TransactionState { get; private set; }

        internal byte[] ServerKey;
        internal int SuggestedPoolConcurrency;
        internal ILogger Logger;

        internal ulong ClientId
            => _clientId;

        private readonly AsyncEvent<Func<Task>> _onDisconnect = new();
        private readonly AsyncEvent<Func<EdgeDBTcpClient, Task<bool>>> _onDisposed = new();
        private readonly AsyncEvent<Func<IReceiveable, Task>> _onMessage = new();
        private readonly AsyncEvent<Func<ExecuteResult, Task>> _queryExecuted = new();

        private NetworkStream? _stream;
        private SslStream? _secureStream;
        private CancellationTokenSource _disconnectCancelToken;
        private TcpClient _client;
        private TaskCompletionSource _readySource;
        private CancellationTokenSource _readyCancelTokenSource;
        private TaskCompletionSource _authCompleteSource;
        private Task? _receiveTask;
        private readonly EdgeDBConnection _connection;
        private readonly SemaphoreSlim _semaphore;
        private readonly SemaphoreSlim _receiveSemaphore;
        private readonly EdgeDBConfig _config;
        private readonly TimeSpan _messageTimeout;
        private readonly TimeSpan _connectionTimeout;

        private ulong _currentMessageId;
        private readonly ConcurrentQueue<IReceiveable> _messageStack;
        private int _maxMessageStackSize = 25;

        private readonly ulong _clientId;
        private bool _isDisposed = false;
        private uint _currentRetries = 0;

        /// <summary>
        ///     Creates a new TCP client with the provided conection and config.
        /// </summary>
        /// <param name="connection">The connection details used to connect to the database.</param>
        /// <param name="config">The configuration for this client.</param>
        public EdgeDBTcpClient(EdgeDBConnection connection, EdgeDBConfig config)
        {
            _config = config;
            Logger = config.Logger ?? Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
            _semaphore = new(1, 1);
            _receiveSemaphore = new(1, 1);
            _disconnectCancelToken = new();
            _connection = connection;
            _client = new();
            _readySource = new();
            _readyCancelTokenSource = new();
            _authCompleteSource = new();
            _messageStack = new();
            ServerKey = new byte[32];
            ServerConfig = new Dictionary<string, object?>();
            _messageTimeout = TimeSpan.FromMilliseconds(config.MessageTimeout);
            _connectionTimeout = TimeSpan.FromMilliseconds(config.ConnectionTimeout);
        }

        public EdgeDBTcpClient(EdgeDBConnection connection, EdgeDBConfig config, ulong clientId) : this(connection, config)
        {
            _clientId = clientId;
        }

        #region Commands/queries

        public Task<Transaction> TransactionAsync(TransactionSettings? settings = null)
            => Transaction.EnterTransactionAsync(this, settings);

        /// <summary>
        ///     Dumps the current database to a stream.
        /// </summary>
        /// <returns>A stream containing the entire dumped database.</returns>
        /// <exception cref="EdgeDBErrorException">The server sent an error message during the dumping process.</exception>
        /// <exception cref="EdgeDBException">The server sent a mismatched packet.</exception>
        public async Task<Stream?> DumpDatabaseAsync()
        {
            await _semaphore.WaitAsync(_disconnectCancelToken.Token).ConfigureAwait(false);

            IsIdle = false;

            try
            {
                await SendMessageAsync(new Dump());
                await SendMessageAsync(new Sync());

                var dump = await WaitForMessageOrError(ServerMessageType.DumpHeader, timeout: _messageTimeout);

                if (dump is ErrorResponse err)
                    throw new EdgeDBErrorException(err);

                if (dump is not DumpHeader dumpHeader)
                    throw new UnexpectedMessageException(ServerMessageType.DumpHeader, dump.Type);

                var stream = new MemoryStream();
                var writer = new DumpWriter(stream);

                writer.WriteDumpHeader(dumpHeader);

                bool complete = false;

                while (!complete)
                {
                    var msg = await NextMessageAsync(x => true, timeout: _messageTimeout);
                    switch (msg)
                    {
                        case CommandComplete:
                            complete = true;
                            break;
                        case DumpBlock block:
                            {
                                writer.WriteDumpBlock(block);
                            }
                            break;
                        case ErrorResponse error:
                            {
                                throw new EdgeDBErrorException(error);
                            }
                        default:
                            throw new UnexpectedMessageException(msg.Type);

                    }
                }
                stream.Position = 0;
                return stream;
            }
            catch (Exception x) when (x is OperationCanceledException or TaskCanceledException)
            {
                throw new TimeoutException("Database dump timed out", x);
            }
            finally
            {
                IsIdle = true;
                _semaphore.Release();
            }
        }

        /// <summary>
        ///     Restores the database based on a database dump stream.
        /// </summary>
        /// <param name="stream">The stream containing the database dump.</param>
        /// <returns>The command complete packet received after restoring the database.</returns>
        /// <exception cref="EdgeDBException">
        ///     The server sent an invalid packet or the restore operation couldn't proceed 
        ///     due to the database not being empty.
        /// </exception>
        /// <exception cref="EdgeDBErrorException">The server sent an error during the restore operation.</exception>
        public async Task<CommandComplete> RestoreDatabaseAsync(Stream stream)
        {
            var reader = new DumpReader();

            var count = await QuerySingleAsync<long>("select count(schema::Module filter not .builtin and not .name = \"default\") + count(schema::Object filter .name like \"default::%\")");

            // steal semaphore to block any queries
            await _semaphore.WaitAsync().ConfigureAwait(false);

            try
            {
                if (count > 0)
                    throw new InvalidOperationException("Cannot restore: Database isn't empty");

                var packets = reader.ReadDatabaseDump(stream);

                // send restore
                await SendMessageAsync(packets.Restore);

                var result = await WaitForMessageOrError(ServerMessageType.RestoreReady, timeout: _messageTimeout);

                if (result is ErrorResponse err)
                    throw new EdgeDBErrorException(err);

                if (result is not RestoreReady)
                    throw new UnexpectedMessageException(ServerMessageType.RestoreReady, result.Type);

                foreach (var block in packets.Blocks)
                {
                    await SendMessageAsync(block).ConfigureAwait(false);
                }

                await SendMessageAsync(new RestoreEOF());

                result = await WaitForMessageOrError(ServerMessageType.CommandComplete, from: result, timeout: _messageTimeout);

                if (result is ErrorResponse error)
                    throw new EdgeDBErrorException(error);

                if (result is not CommandComplete complete)
                    throw new UnexpectedMessageException(ServerMessageType.CommandComplete, result.Type);

                return complete;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        internal struct RawExecuteResult
        {
            public PrepareComplete PrepareStatement { get; set; }
            public ICodec Deserializer { get; set; }
            public List<Data> Data { get; set; }
            public CommandComplete CompleteStatus { get; set; }
        }

        internal async Task<RawExecuteResult> ExecuteInternalAsync(string query, IDictionary<string, object?>? args = null, Cardinality? card = null,
            AllowCapabilities? capabilities = AllowCapabilities.ReadOnly)
        {
            await _semaphore.WaitAsync(_disconnectCancelToken.Token).ConfigureAwait(false);

            IsIdle = false;

            ExecuteResult? execResult = null;

            try
            {
                var prepareResult = await PrepareAsync(new Prepare
                {
                    Capabilities = capabilities,
                    Command = query,
                    Format = IOFormat.Binary,
                    ExpectedCardinality = card ?? Cardinality.Many,
                    ExplicitObjectIds = true,
                    ImplicitTypeNames = true,
                    ImplicitTypeIds = true,
                }).ConfigureAwait(false);

                if (prepareResult is ErrorResponse error)
                {
                    throw new EdgeDBErrorException(error);
                }

                if (prepareResult is not PrepareComplete result)
                {
                    throw new UnexpectedMessageException(ServerMessageType.PrepareComplete, prepareResult.Type);
                }

                ReadyForCommand readyForCommand = default;

                readyForCommand = await WaitForMessageAsync<ReadyForCommand>(ServerMessageType.ReadyForCommand, result, timeout: _messageTimeout).ConfigureAwait(false);

                ulong fromId = ((IReceiveable)readyForCommand).Id;

                // get the codec for the return type
                var outCodec = PacketSerializer.GetCodec(result.OutputTypedescId);
                var inCodec = PacketSerializer.GetCodec(result.InputTypedescId);
                CommandDataDescription? describer = null;

                // if its not cached or we dont have a default one for it, ask the server to describe it for us
                if (outCodec == null || inCodec == null)
                {
                    await SendMessageAsync(new DescribeStatement()).ConfigureAwait(false);
                    await SendMessageAsync(new Sync()).ConfigureAwait(false);
                    describer = await WaitForMessageAsync<CommandDataDescription>(ServerMessageType.CommandDataDescription, from: readyForCommand).ConfigureAwait(false);
                    readyForCommand = await WaitForMessageAsync<ReadyForCommand>(ServerMessageType.ReadyForCommand, describer, timeout: _messageTimeout).ConfigureAwait(false);

                    fromId = ((IReceiveable)readyForCommand).Id;

                    if (outCodec == null)
                    {
                        using (var innerReader = new PacketReader(describer.Value.OutputTypeDescriptor.ToArray()))
                        {
                            outCodec ??= PacketSerializer.BuildCodec(describer.Value.OutputTypeDescriptorId, innerReader);
                        }
                    }

                    if (inCodec == null)
                    {
                        using (var innerReader = new PacketReader(describer.Value.InputTypeDescriptor.ToArray()))
                        {
                            inCodec ??= PacketSerializer.BuildCodec(describer.Value.InputTypeDescriptorId, innerReader);
                        }
                    }
                }

                if (outCodec == null)
                {
                    throw new MissingCodecException("Couldn't find a valid output codec", result.OutputTypedescId, describer!.Value.OutputTypeDescriptor.ToArray());
                }

                if (inCodec == null)
                {
                    throw new MissingCodecException("Couldn't find a valid input codec", result.InputTypedescId, describer!.Value.InputTypeDescriptor.ToArray());
                }

                if (inCodec is not IArgumentCodec argumentCodec)
                    throw new MissingCodecException($"Cannot encode arguments, {inCodec} is not a registered argument codec", result.InputTypedescId, describer.HasValue
                        ? describer!.Value.InputTypeDescriptor.ToArray()
                        : Array.Empty<byte>());

                // convert our arguments if any
                await SendMessageAsync(new Execute() { Capabilities = result.Capabilities, Arguments = argumentCodec.SerializeArguments(args) }).ConfigureAwait(false);
                await SendMessageAsync(new Sync()).ConfigureAwait(false);

                List<Data> receivedData = new();

                CommandComplete? completePacket = null;

                var prevFrom = fromId;

                while (completePacket == null)
                {
                    var msg = await NextMessageAsync(from: fromId, timeout: _messageTimeout);
                    prevFrom = fromId;
                    fromId = msg.Id;

                    switch (msg)
                    {
                        case Data data:
                            receivedData.Add(data);
                            break;
                        case CommandComplete comp:
                            completePacket = comp;
                            break;
                        case ErrorResponse err:
                            throw new EdgeDBErrorException(err);
                        default:
                            throw new UnexpectedMessageException(msg.Type);
                    }
                }

                execResult = new ExecuteResult(true, null, null, query);

                return new RawExecuteResult
                {
                    CompleteStatus = completePacket.Value,
                    Data = receivedData,
                    Deserializer = outCodec,
                    PrepareStatement = result
                };
            }
            catch (Exception x)
            {
                if (x is EdgeDBErrorException err)
                    execResult = new ExecuteResult(false, err.ErrorResponse, err, query);
                else
                    execResult = new ExecuteResult(false, null, x, query);

                Logger.LogWarning("Failed to execute: {}", x);
                throw new EdgeDBException($"Failed to execute query: {x}", x);
            }
            finally
            {
                _ = Task.Run(async () => await _queryExecuted.InvokeAsync(execResult!.Value).ConfigureAwait(false));
                IsIdle = true;
                _semaphore.Release();
            }
        }

        /// <inheritdoc/>
        public async Task ExecuteAsync(string query, IDictionary<string, object?>? args = null)
        {
            await ExecuteInternalAsync(query, args, Cardinality.Many).ConfigureAwait(false);
        }

        public async Task<IReadOnlyCollection<TResult?>> QueryAsync<TResult>(string query, IDictionary<string, object?>? args = null)
        {
            var result = await ExecuteInternalAsync(query, args, Cardinality.Many);

            List<TResult?> returnResults = new();

            foreach(var item in result.Data)
            {
                var obj = ObjectBuilder.BuildResult<TResult>(result.PrepareStatement.OutputTypedescId, result.Deserializer.Deserialize(item.PayloadData.ToArray()));
                returnResults.Add(obj);
            }

            return returnResults.ToImmutableArray();
        }

        public async Task<TResult?> QuerySingleAsync<TResult>(string query, IDictionary<string, object?>? args = null)
        {
            var result = await ExecuteInternalAsync(query, args, Cardinality.AtMostOne);

            if (result.Data.Count > 1)
                throw new ResultCardinalityMismatchException(Cardinality.AtMostOne, Cardinality.Many);

            var queryResult = result.Data.FirstOrDefault();

            if (queryResult.PayloadData == null)
                return default;

            return ObjectBuilder.BuildResult<TResult>(result.PrepareStatement.OutputTypedescId, result.Deserializer.Deserialize(queryResult.PayloadData.ToArray()));
        }

        public async Task<TResult> QueryRequiredSingleAsync<TResult>(string query, IDictionary<string, object?>? args = null)
        {
            var result = await ExecuteInternalAsync(query, args, Cardinality.AtMostOne);

            if (result.Data.Count > 1 || result.Data.Count == 0)
                throw new ResultCardinalityMismatchException(Cardinality.One, result.Data.Count > 1 ? Cardinality.Many : Cardinality.AtMostOne);

            var queryResult = result.Data.FirstOrDefault();

            if (queryResult.PayloadData == null)
                throw new MissingRequiredException();

            return ObjectBuilder.BuildResult<TResult>(result.PrepareStatement.OutputTypedescId, result.Deserializer.Deserialize(queryResult.PayloadData.ToArray()))!;
        }

        #endregion

        #region Receive/Send packet functions
        private async Task ReceiveAsync()
        {
            try
            {
                byte[] buffer = new byte[1024];

                while (true)
                {
                    if (!_client.Connected)
                        return;

                    if (_disconnectCancelToken.IsCancellationRequested)
                        return;

                    if (_secureStream == null)
                        return;

                    var typeBuf = new byte[1];

                    var result = await _secureStream.ReadAsync(typeBuf, _disconnectCancelToken.Token).ConfigureAwait(false);

                    if (result == 0)
                    {
                        // disconnected
                        _readyCancelTokenSource?.Cancel();
                        await _onDisconnect.InvokeAsync().ConfigureAwait(false);
                        return;
                    }

                    var msg = PacketSerializer.DeserializePacket((ServerMessageType)typeBuf[0], _secureStream, this);

                    if (msg != null)
                    {
                        await _receiveSemaphore.WaitAsync().ConfigureAwait(false);

                        try
                        {
                            msg.Id = _currentMessageId;
                            _messageStack.Enqueue(msg.Clone());



                            Interlocked.Increment(ref _currentMessageId);

                            while (_messageStack.Count > _maxMessageStackSize && _messageStack.TryDequeue(out var _))
                            { }

                            await HandlePayloadAsync(msg).ConfigureAwait(false);
                        }
                        finally
                        {
                            _receiveSemaphore.Release();
                        }

                    }
                }
            }
            catch (OperationCanceledException) { } // disconnect
            catch (IOException) { } // disconnect
            catch (Exception x)
            {
                Logger.LogCritical("Failed to read and deserialize packet: {}", x);
            }
        }

        private async Task HandlePayloadAsync(IReceiveable payload)
        {
            var msg = "[ {} ] Got message type {}";


            switch (payload)
            {
                case ErrorResponse err:
                    {
                        Logger.LogError("Got error level: {}\nMessage: {}\nHeaders:\n{}",
                            err.Severity,
                            err.Message,
                            string.Join("\n", err.Headers.Select(x => $"  0x{BitConverter.ToString(BitConverter.GetBytes(x.Code).Reverse().ToArray()).Replace("-", "")}: {x}")));

                        if (!_readyCancelTokenSource.IsCancellationRequested)
                            _readyCancelTokenSource.Cancel();
                    }
                    break;
                case AuthenticationStatus authStatus:
                    if (authStatus.AuthStatus == AuthStatus.AuthenticationRequiredSASLMessage)
                        _ = Task.Run(async () => await StartSASLAuthenticationAsync(authStatus).ConfigureAwait(false));
                    else if (authStatus.AuthStatus == AuthStatus.AuthenticationOK)
                        _authCompleteSource.TrySetResult();

                    break;
                case ServerKeyData keyData:
                    {
                        ServerKey = keyData.Key.ToArray();
                    }
                    break;
                case ParameterStatus parameterStatus:
                    ParseServerSettings(parameterStatus);
                    break;
                case ReadyForCommand cmd:
                    TransactionState = cmd.TransactionState;
                    _readySource.TrySetResult();
                    break;
            }

            Logger.LogDebug(msg, _clientId, payload.Type);

            try
            {
                await _onMessage.InvokeAsync(payload).ConfigureAwait(false);
            }
            catch (Exception x)
            {
                Logger.LogCritical("Exception in message handler: {}", x);
            }
        }

        private async Task SendMessageAsync<T>(T message) where T : Sendable
        {
            if (_secureStream == null)
                throw new EdgeDBException("Cannot send message to a closed connection");

            using var ms = new MemoryStream();
            using (var writer = new PacketWriter(ms))
            {
                message.Write(writer, this);
            }

            await _secureStream.WriteAsync(ms.ToArray()).ConfigureAwait(false);

            Logger.LogDebug("[ {} ] Sent message type {}", _clientId, message.Type);
        }

        #endregion

        #region Helper functions

        private async Task StartSASLAuthenticationAsync(AuthenticationStatus authStatus)
        {
            // steal the sephamore to stop any query attempts.
            await _semaphore.WaitAsync().ConfigureAwait(false);

            IsIdle = false;

            try
            {
                using (var scram = new Scram())
                {
                    var method = authStatus.AuthenticationMethods[0];

                    if (method != "SCRAM-SHA-256")
                    {
                        throw new ProtocolViolationException("The only supported method is SCRAM-SHA-256");
                    }

                    var initialMsg = scram.BuildInitialMessage(_connection.Username!, method);

                    await SendMessageAsync(initialMsg).ConfigureAwait(false);

                    // wait for continue or timeout
                    var initialResult = await WaitForMessageOrError(ServerMessageType.Authentication, from: authStatus, timeout: _messageTimeout);

                    if (initialResult is ErrorResponse err)
                        throw new EdgeDBErrorException(err);

                    if (initialResult is not AuthenticationStatus intiailStatus)
                        throw new UnexpectedMessageException(ServerMessageType.Authentication, initialResult.Type);

                    // check the continue
                    var final = scram.BuildFinalMessage(intiailStatus, _connection.Password!);

                    await SendMessageAsync(final.FinalMessage);

                    var finalResult = await WaitForMessageOrError(ServerMessageType.Authentication, from: initialResult, timeout: _messageTimeout);

                    if (finalResult is ErrorResponse error)
                        throw new EdgeDBErrorException(error);

                    if (finalResult is not AuthenticationStatus finalStatus || finalStatus.AuthStatus != AuthStatus.AuthenticationSASLFinal)
                        throw new UnexpectedMessageException(ServerMessageType.Authentication, finalResult.Type);

                    var key = scram.ParseServerFinalMessage(finalStatus);

                    if (!key.SequenceEqual(final.ExpectedSig))
                    {
                        throw new InvalidSignatureException();
                    }

                    // ok status
                    var authOk = await WaitForMessageOrError(ServerMessageType.Authentication, from: finalResult, timeout: _messageTimeout);

                    if (authOk is ErrorResponse er)
                        throw new EdgeDBErrorException(er);

                    if (authOk is not AuthenticationStatus status || status.AuthStatus != AuthStatus.AuthenticationOK)
                        throw new UnexpectedMessageException(ServerMessageType.Authentication, authOk.Type);

                    _authCompleteSource.TrySetResult();
                    _currentRetries = 0;
                }
            }
            catch(Exception x)
            {
                if (_config.RetryMode == ConnectionRetryMode.AlwaysRetry)
                {
                    if (_currentRetries < _config.MaxConnectionRetries)
                    {
                        _currentRetries++;
                        Logger.LogWarning(x, "Attempting to reconnect, {}/{}", _currentRetries, _config.MaxConnectionRetries);
                        await ReconnectAsync();
                    }
                    else
                        Logger.LogError(x, "Max connection attemts reached");
                }
                else
                {
                    Logger.LogError(x, "Failed to complete authentication");
                    throw;
                }
            }
            finally
            {
                _semaphore.Release();
                IsIdle = true;
            }
        }

        private async Task<IReceiveable> PrepareAsync(Prepare packet)
        {
            var next = _currentMessageId;
            await SendMessageAsync(packet).ConfigureAwait(false);
            await SendMessageAsync(new Sync()).ConfigureAwait(false);

            var result = await WaitForMessageOrError(ServerMessageType.PrepareComplete, from: next, timeout: _messageTimeout).ConfigureAwait(false);

            if (result is ErrorResponse err)
                throw new EdgeDBErrorException(err);

            return (PrepareComplete)result;
        }

        private void ParseServerSettings(ParameterStatus status)
        {
            switch (status.Name)
            {
                case "suggested_pool_concurrency":
                    var str = Encoding.UTF8.GetString(status.Value.ToArray());
                    if (!int.TryParse(str, out SuggestedPoolConcurrency))
                    {
                        throw new FormatException("suggested_pool_concurrency type didn't match the expected type of int");
                    }
                    break;

                case "system_config":
                    using (var reader = new PacketReader(status.Value.ToArray()))
                    {
                        var length = reader.ReadInt32() - 16;
                        var descriptorId = reader.ReadGuid();
                        var typeDesc = reader.ReadBytes(length);

                        ICodec? codec = PacketSerializer.GetCodec(descriptorId);

                        if (codec == null)
                        {
                            using (var innerReader = new PacketReader(typeDesc))
                            {
                                codec = PacketSerializer.BuildCodec(descriptorId, innerReader);

                                if (codec == null)
                                    throw new MissingCodecException("Failed to build codec for system_config", descriptorId, status.Value.ToArray());
                            }
                        }

                        // disard length
                        reader.ReadUInt32();

                        ServerConfig = ImmutableDictionary.CreateRange((codec.Deserialize(reader) as IDictionary<string, object?>)!);
                    }
                    break;
            }
        }

        private Task<IReceiveable> WaitForMessageOrError(ServerMessageType type, ulong from, CancellationToken token = default, TimeSpan? timeout = null)
            => NextMessageAsync(x => x.Type == type || x.Type == ServerMessageType.ErrorResponse, from, timeout, token: token);
        private Task<IReceiveable> WaitForMessageOrError(ServerMessageType type, IReceiveable? from = null, CancellationToken token = default, TimeSpan? timeout = null)
            => NextMessageAsync(from, predicate: x => x.Type == type || x.Type == ServerMessageType.ErrorResponse, timeout, token: token);

        private async Task<TMessage> WaitForMessageAsync<TMessage>(ServerMessageType type, IReceiveable? from, CancellationToken token = default, TimeSpan? timeout = null) where TMessage : IReceiveable
            => (TMessage)(await NextMessageAsync(from, x => x.Type == type, timeout, token: token));

        private Task<IReceiveable> NextMessageAsync(IReceiveable? from, Predicate<IReceiveable>? predicate = null, TimeSpan? timeout = null, CancellationToken token = default)
            => NextMessageAsync(predicate: predicate, from: from?.Id, timeout, token);
        private async Task<IReceiveable> NextMessageAsync(Predicate<IReceiveable>? predicate = null, ulong? from = null, TimeSpan? timeout = null, CancellationToken token = default)
        {
            var targetId = from.HasValue ? from.Value + 1 : _currentMessageId;

            var tcs = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            IReceiveable? returnValue = default;
            bool found = false;
            Func<IReceiveable, Task> handler = (msg) =>
            {
                if (predicate == null)
                {
                    if (msg.Id == targetId)
                    {
                        if (!found)
                            returnValue = msg;
                        found = true;
                        tcs.TrySetResult();
                    }
                    else if (msg.Id > targetId)
                    {
                        // get the target and return
                        if (!found)
                            returnValue = _messageStack.First(x => x.Id == targetId);
                        found = true;
                        tcs.TrySetResult();
                    }

                }
                else if (predicate(msg))
                {
                    if (!found)
                        returnValue = msg;
                    found = true;
                    tcs.TrySetResult();
                }

                return Task.CompletedTask;
            };

            await _receiveSemaphore.WaitAsync().ConfigureAwait(false);

            try
            {
                if (predicate != null)
                {
                    var col = _messageStack.Where(x => x.Id >= targetId).OrderBy(x => x.Id).ToArray();

                    foreach (var msg in col)
                    {
                        await handler(msg);
                        if (returnValue != null)
                            break;
                    }
                }
                else
                {
                    var msg = _messageStack.FirstOrDefault(x => x.Id == targetId);
                    if (msg != null)
                        return msg;
                }

                OnMessage += handler;
            }
            finally
            {
                _receiveSemaphore.Release();
            }

            var cancelSource = CancellationTokenSource.CreateLinkedTokenSource(token, _disconnectCancelToken.Token);

            if (timeout.HasValue)
                cancelSource.CancelAfter(timeout.Value);

            cancelSource.Token.Register(() => tcs.TrySetCanceled());

            _disconnectCancelToken.Token.ThrowIfCancellationRequested();

            if (!tcs.Task.IsCompleted)
            {
                await tcs.Task.ConfigureAwait(false);
            }

            OnMessage -= handler;


            return returnValue!;
        }

        #endregion

        #region Connect/disconnect

        /// <summary>
        ///     Connects and authenticates this client.
        /// </summary>
        public async Task ConnectAsync()
        {
            _disconnectCancelToken?.Cancel();
            
            _disconnectCancelToken = new();
            _authCompleteSource = new();

            await ConnectInternalAsync();

            // wait for auth promise
            
            await Task.Run(() => Task.WhenAll(_readySource.Task, _authCompleteSource.Task), _readyCancelTokenSource.Token).ConfigureAwait(false);
        }

        private async Task ConnectInternalAsync()
        {
            try
            {
                _disconnectCancelToken?.Cancel();
                _disconnectCancelToken = new();
                _client = new TcpClient();

                var timeoutToken = CancellationTokenSource.CreateLinkedTokenSource(_disconnectCancelToken.Token);

                timeoutToken.CancelAfter(_connectionTimeout);

                try
                {
                    await _client.ConnectAsync(_connection.Hostname!, _connection.Port, timeoutToken.Token).ConfigureAwait(false);
                }
                catch(OperationCanceledException x) when (timeoutToken.IsCancellationRequested)
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

                _stream = _client.GetStream();

                _secureStream = new SslStream(_stream, false, new RemoteCertificateValidationCallback(ValidateServerCertificate), null);

                var options = new SslClientAuthenticationOptions()
                {
                    AllowRenegotiation = true,
                    ApplicationProtocols = new List<SslApplicationProtocol>
                    {
                        new SslApplicationProtocol("edgedb-binary")
                    },
                    TargetHost = _connection.Hostname,
                    EnabledSslProtocols = System.Security.Authentication.SslProtocols.None,
                    CertificateRevocationCheckMode = X509RevocationMode.NoCheck,
                };

                await _secureStream.AuthenticateAsClientAsync(options).ConfigureAwait(false);

                _receiveTask = Task.Run(async () => await ReceiveAsync().ConfigureAwait(false));

                // send handshake
                await SendMessageAsync(new ClientHandshake
                {
                    MajorVersion = 1,
                    MinorVersion = 0,
                    ConnectionParameters = new ConnectionParam[]
                    {
                    new ConnectionParam
                    {
                        Name = "user",
                        Value = _connection.Username!
                    },
                    new ConnectionParam
                    {
                        Name = "database",
                        Value = _connection.Database!
                    }
                    }
                }).ConfigureAwait(false);
            }
            catch (EdgeDBException x) when (x.ShouldReconnect)
            {
                if(_currentRetries == _config.MaxConnectionRetries)
                {
                    throw;
                }

                _currentRetries++;
                await ConnectInternalAsync().ConfigureAwait(false);
            }
        }

        private async Task ReconnectAsync()
        {
            await DisconnectAsync();
            await ConnectInternalAsync();
        }

        /// <summary>
        ///     Disconnects this client from the database.
        /// </summary>
        public async Task DisconnectAsync()
        {
            await SendMessageAsync(new Terminate()).ConfigureAwait(false);
            _client.Close();
            _disconnectCancelToken.Cancel();
        }

        private bool ValidateServerCertificate(object sender, X509Certificate? certificate, X509Chain? chain, SslPolicyErrors sslPolicyErrors)
        {
            if (_connection.TLSSecurity == TLSSecurityMode.Insecure)
                return true;

            if (_connection.TLSCertificateAuthority != null)
            {
                var rawCert = _connection.TLSCertData!;
                var cleaned = rawCert.Replace("\n", "").Replace("\r", "");
                var certData = Regex.Match(cleaned, @"-----BEGIN CERTIFICATE-----(.*?)-----END CERTIFICATE-----");

                if (!certData.Success)
                    return false;

                var cert = new X509Certificate2(Encoding.ASCII.GetBytes(_connection.TLSCertData!));

                X509Chain chain2 = new X509Chain();
                chain2.ChainPolicy.ExtraStore.Add(cert);
                chain2.ChainPolicy.VerificationFlags = X509VerificationFlags.AllowUnknownCertificateAuthority;
                chain2.ChainPolicy.RevocationMode = X509RevocationMode.NoCheck;

                bool isValid = chain2.Build(new X509Certificate2(certificate!));
                var chainRoot = chain2.ChainElements[chain2.ChainElements.Count - 1].Certificate;
                isValid = isValid && chainRoot.RawData.SequenceEqual(cert.RawData);

                return isValid;
            }
            else
            {
                return sslPolicyErrors == SslPolicyErrors.None;
            }            
        }

        #endregion

        #region Client pool dispose

        public async ValueTask DisposeAsync()
        {
            if (_isDisposed)
                return;

            bool shouldDispose = true;

            if (_onDisposed.HasSubscribers)
            {
                var results = await _onDisposed.InvokeAsync(this).ConfigureAwait(false);
                shouldDispose = results.Any(x => x);
            }

            if (shouldDispose)
            {
                await DisconnectAsync();
                _stream?.Dispose();

                if (_secureStream != null)
                    await _secureStream.DisposeAsync();
            }
        }

        #endregion
    }
}