using EdgeDB.Codecs;
using EdgeDB.Models;
using EdgeDB.Utils;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Dynamic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB
{
    /// <summary>
    ///     Represents an abstract binary clinet.
    /// </summary>
    public abstract class EdgeDBBinaryClient : BaseEdgeDBClient, ITransactibleClient
    {
        /// <summary>
        ///     Fired when the client receives a message.
        /// </summary>
        public event Func<IReceiveable, ValueTask> OnMessage
        {
            add => _onMessage.Add(value);
            remove => _onMessage.Remove(value);
        }

        /// <summary>
        ///     Fired when a query is executed.
        /// </summary>
        public event Func<ExecuteResult, ValueTask> QueryExecuted
        {
            add => _queryExecuted.Add(value);
            remove => _queryExecuted.Remove(value);
        }

        /// <summary>
        ///     Fired when the client disconnects.
        /// </summary>
        public event Func<ValueTask> OnDisconnect
        {
            add => _onDisconnect.Add(value);
            remove => _onDisconnect.Remove(value);
        }

        /// <summary>
        ///     Gets whether or not this connection is idle.
        /// </summary>
        public bool IsIdle { get; private set; } = true;

        /// <summary>
        ///     Gets the raw server config.
        /// </summary>
        /// <remarks>
        ///     This dictionary can be empty if the client hasn't connected to the database.
        /// </remarks>
        public IReadOnlyDictionary<string, object?> ServerConfig
            => RawServerConfig.ToImmutableDictionary();

        /// <summary>
        ///     Gets this clients transaction state.
        /// </summary>
        public TransactionState TransactionState { get; private set; }

        internal byte[] ServerKey;
        internal int SuggestedPoolConcurrency;
        internal ILogger Logger;
        internal Dictionary<string, object?> RawServerConfig = new();
        internal readonly ClientPacketDuplexer Duplexer;
        internal readonly TimeSpan MessageTimeout;
        internal readonly TimeSpan ConnectionTimeout;
        internal readonly EdgeDBConnection Connection;

        private readonly AsyncEvent<Func<ValueTask>> _onDisconnect = new();
        private readonly AsyncEvent<Func<IReceiveable, ValueTask>> _onMessage = new();
        private readonly AsyncEvent<Func<ExecuteResult, ValueTask>> _queryExecuted = new();

        protected CancellationToken DisconnectCancelToken
            => Duplexer.DisconnectToken;

        private readonly TaskCompletionSource _readySource;
        private readonly CancellationTokenSource _readyCancelTokenSource;
        private TaskCompletionSource _authCompleteSource;
        private readonly SemaphoreSlim _semaphore;
        private readonly SemaphoreSlim _commandSemaphore;
        private readonly EdgeDBConfig _config;
        private uint _currentRetries;

        /// <summary>
        ///     Creates a new binary client with the provided conection and config.
        /// </summary>
        /// <param name="connection">The connection details used to connect to the database.</param>
        /// <param name="config">The configuration for this client.</param>
        /// <param name="clientId">The optional client id of this client. This is used for logging and client pooling.</param>
        public EdgeDBBinaryClient(EdgeDBConnection connection, EdgeDBConfig config, ulong? clientId = null)
            : base(clientId ?? 0)
        {
            _config = config;
            Logger = config.Logger ?? Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
            _semaphore = new(1, 1);
            _commandSemaphore = new(1, 1);
            Connection = connection;
            _readySource = new();
            _readyCancelTokenSource = new();
            _authCompleteSource = new();
            ServerKey = new byte[32];
            MessageTimeout = TimeSpan.FromMilliseconds(config.MessageTimeout);
            ConnectionTimeout = TimeSpan.FromMilliseconds(config.ConnectionTimeout);
            Duplexer = new ClientPacketDuplexer(this);
        }
        #region Commands/queries

        internal struct RawExecuteResult
        {
            public PrepareComplete PrepareStatement { get; set; }

            public ICodec Deserializer { get; set; }

            public List<Data> Data { get; set; }

            public CommandComplete CompleteStatus { get; set; }
        }

        /// <exception cref="EdgeDBException">A general error occored.</exception>
        /// <exception cref="EdgeDBErrorException">The client received an <see cref="ErrorResponse"/>.</exception>
        /// <exception cref="UnexpectedMessageException">The client received an unexpected message.</exception>
        /// <exception cref="MissingCodecException">A codec could not be found for the given input arguments or the result.</exception>
        internal async Task<RawExecuteResult> ExecuteInternalAsync(string query, IDictionary<string, object?>? args = null, Cardinality? card = null,
            AllowCapabilities? capabilities = AllowCapabilities.ReadOnly)
        {
            await _semaphore.WaitAsync(DisconnectCancelToken).ConfigureAwait(false);

            IsIdle = false;

            ExecuteResult? execResult = null;

            try
            {
                var readyTask = Duplexer.NextAsync(x => x.Type == ServerMessageType.ReadyForCommand);

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

                ReadyForCommand readyForCommand = (ReadyForCommand)await readyTask.ConfigureAwait(false);

                // get the codec for the return type
                var outCodec = PacketSerializer.GetCodec(result.OutputTypedescId);
                var inCodec = PacketSerializer.GetCodec(result.InputTypedescId);
                CommandDataDescription? describer = null;

                // if its not cached or we dont have a default one for it, ask the server to describe it for us
                if (outCodec is null || inCodec is null)
                {
                    describer = (CommandDataDescription)await Duplexer.DuplexAndSyncAsync(new DescribeStatement(), x => x.Type == ServerMessageType.CommandDataDescription);

                    if (outCodec is null)
                    {
                        using var innerReader = new PacketReader(describer.Value.OutputTypeDescriptor.ToArray());
                        outCodec ??= PacketSerializer.BuildCodec(describer.Value.OutputTypeDescriptorId, innerReader);
                    }

                    if (inCodec is null)
                    {
                        using var innerReader = new PacketReader(describer.Value.InputTypeDescriptor.ToArray());
                        inCodec ??= PacketSerializer.BuildCodec(describer.Value.InputTypeDescriptorId, innerReader);
                    }
                }

                if (outCodec is null)
                {
                    throw new MissingCodecException("Couldn't find a valid output codec", result.OutputTypedescId, describer!.Value.OutputTypeDescriptor.ToArray());
                }

                if (inCodec is null)
                {
                    throw new MissingCodecException("Couldn't find a valid input codec", result.InputTypedescId, describer!.Value.InputTypeDescriptor.ToArray());
                }

                if (inCodec is not IArgumentCodec argumentCodec)
                    throw new MissingCodecException($"Cannot encode arguments, {inCodec} is not a registered argument codec", result.InputTypedescId, describer.HasValue
                        ? describer!.Value.InputTypeDescriptor.ToArray()
                        : Array.Empty<byte>());

                List<Data> receivedData = new();

                bool complete = false;
                bool handler(IReceiveable msg)
                {
                    if (complete)
                        return true;

                    switch (msg)
                    {
                        case Data data:
                            receivedData.Add(data);
                            break;
                        case CommandComplete comp:
                            complete = true;
                            return true;
                        case ErrorResponse err:
                            throw new EdgeDBErrorException(err);
                        default:
                            if(msg.Type != ServerMessageType.ReadyForCommand) // could be we received the prepares ready since we have no queue popping
                                throw new UnexpectedMessageException(msg.Type);
                            return false;
                    }
                    return false;
                }

                readyTask = Duplexer.NextAsync(x => x.Type == ServerMessageType.ReadyForCommand);

                var executeResult = await Duplexer.DuplexAndSyncAsync(new Execute() 
                { 
                    Capabilities = result.Capabilities, 
                    Arguments = argumentCodec.SerializeArguments(args) 
                }, handler);

                if (executeResult is ErrorResponse err)
                    throw new EdgeDBErrorException(err);

                if (executeResult is not CommandComplete completePacket)
                    throw new UnexpectedMessageException(ServerMessageType.CommandComplete, executeResult.Type);

                // update transaction state
                readyForCommand = (ReadyForCommand)await readyTask.ConfigureAwait(false);

                TransactionState = readyForCommand.TransactionState;

                execResult = new ExecuteResult(true, null, null, query);

                return new RawExecuteResult
                {
                    CompleteStatus = completePacket,
                    Data = receivedData,
                    Deserializer = outCodec,
                    PrepareStatement = result
                };
            }
            catch (Exception x)
            {
                execResult = x is EdgeDBErrorException err
                    ? (ExecuteResult?)new ExecuteResult(false, err.ErrorResponse, err, query)
                    : (ExecuteResult?)new ExecuteResult(false, null, x, query);

                Logger.InternalExecuteFailed(x);
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
        /// <exception cref="EdgeDBException">A general error occored.</exception>
        /// <exception cref="EdgeDBErrorException">The client received an <see cref="ErrorResponse"/>.</exception>
        /// <exception cref="UnexpectedMessageException">The client received an unexpected message.</exception>
        /// <exception cref="MissingCodecException">A codec could not be found for the given input arguments or the result.</exception>
        public override async Task ExecuteAsync(string query, IDictionary<string, object?>? args = null)
            => await ExecuteInternalAsync(query, args, Cardinality.Many).ConfigureAwait(false);

        /// <inheritdoc/>
        /// <exception cref="EdgeDBException">A general error occored.</exception>
        /// <exception cref="EdgeDBErrorException">The client received an <see cref="ErrorResponse"/>.</exception>
        /// <exception cref="UnexpectedMessageException">The client received an unexpected message.</exception>
        /// <exception cref="MissingCodecException">A codec could not be found for the given input arguments or the result.</exception>
        public override async Task<IReadOnlyCollection<TResult?>> QueryAsync<TResult>(string query, IDictionary<string, object?>? args = null)
            where TResult : default
        {
            var result = await ExecuteInternalAsync(query, args, Cardinality.Many);

            List<TResult?> returnResults = new();

            foreach (var item in result.Data)
            {
                var obj = ObjectBuilder.BuildResult<TResult>(result.PrepareStatement.OutputTypedescId, result.Deserializer.Deserialize(item.PayloadData.ToArray()));
                returnResults.Add(obj);
            }

            return returnResults.ToImmutableArray();
        }

        /// <inheritdoc/>
        /// <exception cref="EdgeDBException">A general error occored.</exception>
        /// <exception cref="EdgeDBErrorException">The client received an <see cref="ErrorResponse"/>.</exception>
        /// <exception cref="UnexpectedMessageException">The client received an unexpected message.</exception>
        /// <exception cref="MissingCodecException">A codec could not be found for the given input arguments or the result.</exception>
        /// <exception cref="ResultCardinalityMismatchException">The results cardinality was not what the query expected.</exception>
        public override async Task<TResult?> QuerySingleAsync<TResult>(string query, IDictionary<string, object?>? args = null)
            where TResult : default
        {
            var result = await ExecuteInternalAsync(query, args, Cardinality.AtMostOne);

            if (result.Data.Count > 1)
                throw new ResultCardinalityMismatchException(Cardinality.AtMostOne, Cardinality.Many);

            var queryResult = result.Data.FirstOrDefault();

            return queryResult.PayloadData is null
                ? default
                : ObjectBuilder.BuildResult<TResult>(result.PrepareStatement.OutputTypedescId, result.Deserializer.Deserialize(queryResult.PayloadData.ToArray()));
        }

        /// <inheritdoc/>
        /// <exception cref="EdgeDBException">A general error occored.</exception>
        /// <exception cref="EdgeDBErrorException">The client received an <see cref="ErrorResponse"/>.</exception>
        /// <exception cref="UnexpectedMessageException">The client received an unexpected message.</exception>
        /// <exception cref="MissingCodecException">A codec could not be found for the given input arguments or the result.</exception>
        /// <exception cref="ResultCardinalityMismatchException">The results cardinality was not what the query expected.</exception>
        /// <exception cref="MissingRequiredException">The query didn't return a result.</exception>
        public override async Task<TResult> QueryRequiredSingleAsync<TResult>(string query, IDictionary<string, object?>? args = null)
        {
            var result = await ExecuteInternalAsync(query, args, Cardinality.AtMostOne);

            if (result.Data.Count is > 1 or 0)
                throw new ResultCardinalityMismatchException(Cardinality.One, result.Data.Count > 1 ? Cardinality.Many : Cardinality.AtMostOne);

            var queryResult = result.Data.FirstOrDefault();

            return queryResult.PayloadData is null
                ? throw new MissingRequiredException()
                : ObjectBuilder.BuildResult<TResult>(result.PrepareStatement.OutputTypedescId, result.Deserializer.Deserialize(queryResult.PayloadData.ToArray()))!;
        }
        #endregion

        #region Packet handling
        private async ValueTask HandlePayloadAsync(IReceiveable payload)
        {
            switch (payload)
            {
                case ErrorResponse err:
                    {
                        Logger.ErrorResponseReceived(err.Severity, err.Message);

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

                default:
                    break;
            }

            Logger.MessageReceived(ClientId, payload.Type);

            try
            {
                await _onMessage.InvokeAsync(payload).ConfigureAwait(false);
            }
            catch (Exception x)
            {
                Logger.EventHandlerError(x);
            }
        }
        #endregion

        #region SASL
        private async Task StartSASLAuthenticationAsync(AuthenticationStatus authStatus)
        {
            // steal the sephamore to stop any query attempts.
            await _semaphore.WaitAsync().ConfigureAwait(false);

            IsIdle = false;

            try
            {
                using var scram = new Scram();

                var method = authStatus.AuthenticationMethods[0];

                if (method is not "SCRAM-SHA-256")
                {
                    throw new ProtocolViolationException("The only supported method is SCRAM-SHA-256");
                }

                var initialMsg = scram.BuildInitialMessage(Connection.Username!, method);

                var initialResult = await Duplexer.DuplexAsync(x => x.Type == ServerMessageType.Authentication, packets: initialMsg).ConfigureAwait(false);

                if (initialResult is ErrorResponse err)
                    throw new EdgeDBErrorException(err);

                if (initialResult is not AuthenticationStatus intiailStatus)
                    throw new UnexpectedMessageException(ServerMessageType.Authentication, initialResult.Type);

                // check the continue
                var (FinalMessage, ExpectedSig) = scram.BuildFinalMessage(intiailStatus, Connection.Password!);

                var finalResult = await Duplexer.DuplexAsync(x => x.Type == ServerMessageType.Authentication, packets: FinalMessage).ConfigureAwait(false);

                if (finalResult is ErrorResponse error)
                    throw new EdgeDBErrorException(error);

                if (finalResult is not AuthenticationStatus finalStatus || finalStatus.AuthStatus != AuthStatus.AuthenticationSASLFinal)
                    throw new UnexpectedMessageException(ServerMessageType.Authentication, finalResult.Type);

                var key = Scram.ParseServerFinalMessage(finalStatus);

                if (!key.SequenceEqual(ExpectedSig))
                {
                    throw new InvalidSignatureException();
                }

                // ok status
                var authOk = await Duplexer.NextAsync(x => x.Type == ServerMessageType.Authentication);

                if (authOk is ErrorResponse er)
                    throw new EdgeDBErrorException(er);

                if (authOk is not AuthenticationStatus status || status.AuthStatus != AuthStatus.AuthenticationOK)
                    throw new UnexpectedMessageException(ServerMessageType.Authentication, authOk.Type);

                _authCompleteSource.TrySetResult();
                _currentRetries = 0;
            }
            catch (Exception x)
            {
                if (_config.RetryMode is ConnectionRetryMode.AlwaysRetry)
                {
                    if (_currentRetries < _config.MaxConnectionRetries)
                    {
                        _currentRetries++;
                        Logger.AttemptToReconnect(_currentRetries, _config.MaxConnectionRetries);
                        await ReconnectAsync();
                    }
                    else
                        Logger.MaxConnectionRetries(_config.MaxConnectionRetries);
                }
                else
                {
                    Logger.AuthenticationFailed(x);
                    throw;
                }
            }
            finally
            {
                _semaphore.Release();
                IsIdle = true;
            }
        }
        #endregion

        #region Helper functions
        private async Task<IReceiveable> PrepareAsync(Prepare packet)
        {
            var result = await Duplexer.DuplexAndSyncAsync(packet, x => x.Type is ServerMessageType.PrepareComplete);

            return result is ErrorResponse err ? throw new EdgeDBErrorException(err) : (IReceiveable)(PrepareComplete)result;
        }

        private void ParseServerSettings(ParameterStatus status)
        {
            try
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

                            if (codec is null)
                            {
                                using var innerReader = new PacketReader(typeDesc);
                                codec = PacketSerializer.BuildCodec(descriptorId, innerReader);

                                if (codec is null)
                                    throw new MissingCodecException("Failed to build codec for system_config", descriptorId, status.Value.ToArray());
                            }

                            // disard length
                            reader.ReadUInt32();

                            var obj = codec.Deserialize(reader)!;

                            RawServerConfig = ((ExpandoObject)obj).ToDictionary(x => x.Key, x => x.Value);
                        }
                        break;

                    default:
                        break;
                }
            }
            catch (Exception x)
            {
                Logger.ServerSettingsParseFailed(x);
            }
        }
        #endregion

        #region Connect/disconnect
        /// <summary>
        ///     Connects and authenticates this client.
        /// </summary>
        public override async ValueTask ConnectAsync()
        {
            _authCompleteSource = new();

            await ConnectInternalAsync();

            // wait for auth promise
            await Task.Run(() => Task.WhenAll(_readySource.Task, _authCompleteSource.Task), _readyCancelTokenSource.Token).ConfigureAwait(false);
        }

        private async Task ConnectInternalAsync()
        {
            try
            {
                var stream = await GetStreamAsync().ConfigureAwait(false);

                Duplexer.Start(stream);

                Duplexer.OnMessage += HandlePayloadAsync;

                // send handshake
                await Duplexer.SendAsync(new ClientHandshake
                {
                    MajorVersion = 1,
                    MinorVersion = 0,
                    ConnectionParameters = new ConnectionParam[]
                    {
                        new ConnectionParam
                        {
                            Name = "user",
                            Value = Connection.Username!
                        },
                        new ConnectionParam
                        {
                            Name = "database",
                            Value = Connection.Database!
                        }
                    }
                }).ConfigureAwait(false);
            }
            catch (EdgeDBException x) when (x.ShouldReconnect)
            {
                if (_currentRetries == _config.MaxConnectionRetries)
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

        /// <inheritdoc/>
        public override async ValueTask DisconnectAsync()
        {
            await Duplexer.DisconnectAsync().ConfigureAwait(false);
            await CloseStreamAsync().ConfigureAwait(false);
        }

        #endregion

        #region Command locks
        internal async Task<IDisposable> AquireCommandLockAsync(CancellationToken token = default)
        {
            var linkedToken = CancellationTokenSource.CreateLinkedTokenSource(DisconnectCancelToken, token);

            await _commandSemaphore.WaitAsync(linkedToken.Token).ConfigureAwait(false);

            return new CommandLock(() => { _commandSemaphore.Release(); });
        }

        private class CommandLock : IDisposable
        {
            private readonly Action _onDispose;

            public CommandLock(Action onDispose)
            {
                _onDispose = onDispose;
            }

            public void Dispose()
                => _onDispose();
        }
        #endregion

        #region Streams
        protected abstract ValueTask<Stream> GetStreamAsync();
        protected abstract ValueTask CloseStreamAsync();
        #endregion

        #region Client pool dispose
        public override async ValueTask<bool> DisposeAsync()
        {
            var shouldDispose = await base.DisposeAsync().ConfigureAwait(false);

            if (shouldDispose)
            {
                await DisconnectAsync();
            }

            return shouldDispose;
        }
        #endregion

        #region ITranactibleClient
        async Task ITransactibleClient.StartTransactionAsync(Isolation isolation, bool readOnly, bool deferrable)
        {
            var isolationMode = isolation switch
            {
                Isolation.Serializable => "serializable",
                _ => throw new EdgeDBException("Unknown isolation mode")
            };

            var readMode = readOnly ? "read only" : "read write";

            var deferMode = $"{(!deferrable ? "not " : "")}deferrable";

            await ExecuteInternalAsync($"start transaction isolation {isolationMode}, {readMode}, {deferMode}", capabilities: null).ConfigureAwait(false);
        }

        async Task ITransactibleClient.CommitAsync()
            => await ExecuteInternalAsync($"commit", capabilities: null).ConfigureAwait(false);

        async Task ITransactibleClient.RollbackAsync()
            => await ExecuteInternalAsync($"rollback", capabilities: null).ConfigureAwait(false);

        TransactionState ITransactibleClient.TransactionState => TransactionState;
        #endregion
    }
}
