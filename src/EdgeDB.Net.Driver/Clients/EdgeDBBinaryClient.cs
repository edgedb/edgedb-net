using EdgeDB.Binary;
using EdgeDB.Binary.Packets;
using EdgeDB.Binary.Codecs;
using EdgeDB.Utils;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Dynamic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB
{
    /// <summary>
    ///     Represents an abstract binary client.
    /// </summary>
    internal abstract class EdgeDBBinaryClient : BaseEdgeDBClient, ITransactibleClient
    {
        /// <summary>
        ///     The major version of the protocol that this client supports.
        /// </summary>
        public const int PROTOCOL_MAJOR_VERSION = 1;

        /// <summary>
        ///     The minor version of the protocol that this client supports.
        /// </summary>
        public const int PROTOCOL_MINOR_VERSION = 0;

        #region Events
        /// <summary>
        ///     Fired when the client receives a message.
        /// </summary>
        public event Func<IReceiveable, ValueTask> OnMessage
        {
            add => _onMessage.Add(value);
            remove => _onMessage.Remove(value);
        }

        /// <summary>
        ///     Fired when the client receives a <see cref="LogMessage"/>.
        /// </summary>
        public event Func<LogMessage, ValueTask> OnServerLog
        {
            add => _onServerLog.Add(value);
            remove => _onServerLog.Remove(value);
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
        public new event Func<ValueTask> OnDisconnect
        {
            add => OnDisconnectInternal.Add((c) => value());
            remove => OnDisconnectInternal.Remove((c) => value());
        }
        #endregion
        
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
        
        protected CancellationToken DisconnectCancelToken
            => Duplexer.DisconnectToken;
            
        private readonly AsyncEvent<Func<IReceiveable, ValueTask>> _onMessage = new();
        private readonly AsyncEvent<Func<ExecuteResult, ValueTask>> _queryExecuted = new();
        private readonly AsyncEvent<Func<LogMessage, ValueTask>> _onServerLog = new();

        private ICodec? _stateCodec;
        private Guid _stateDescriptorId;
        private TaskCompletionSource _readySource;
        private TaskCompletionSource _authCompleteSource;
        private CancellationTokenSource _readyCancelTokenSource;
        private readonly SemaphoreSlim _semaphore;
        private readonly SemaphoreSlim _commandSemaphore;
        private readonly SemaphoreSlim _connectSemaphone;
        private readonly EdgeDBConfig _config;
        private uint _currentRetries;

        /// <summary>
        ///     Creates a new binary client with the provided conection and config.
        /// </summary>
        /// <param name="connection">The connection details used to connect to the database.</param>
        /// <param name="config">The configuration for this client.</param>
        /// <param name="clientPoolHolder">The client pool holder for this client.</param>
        /// <param name="clientId">The optional client id of this client. This is used for logging and client pooling.</param>
        public EdgeDBBinaryClient(EdgeDBConnection connection, EdgeDBConfig config, IDisposable clientPoolHolder, ulong? clientId = null)
            : base(clientId ?? 0, clientPoolHolder)
        {
            Logger = config.Logger ?? Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
            Connection = connection;
            ServerKey = new byte[32];
            MessageTimeout = TimeSpan.FromMilliseconds(config.MessageTimeout);
            ConnectionTimeout = TimeSpan.FromMilliseconds(config.ConnectionTimeout);
            Duplexer = new ClientPacketDuplexer(this);
            Duplexer.OnDisconnected += HandleDuplexerDisconnectAsync;
            _stateDescriptorId = CodecBuilder.InvalidCodec;
            _config = config;
            _semaphore = new(1, 1);
            _commandSemaphore = new(1, 1);
            _connectSemaphone = new(1, 1);
            _readySource = new();
            _readyCancelTokenSource = new();
            _authCompleteSource = new();
        }

        #region Commands/queries
        internal readonly struct RawExecuteResult
        {
            public readonly ICodec Deserializer;
            public readonly Data[] Data;

            public RawExecuteResult(ICodec codec, List<Data> data)
            {
                Data = data.ToArray();
                Deserializer = codec;
            }
        }

        /// <exception cref="EdgeDBException">A general error occored.</exception>
        /// <exception cref="EdgeDBErrorException">The client received an <see cref="ErrorResponse"/>.</exception>
        /// <exception cref="UnexpectedMessageException">The client received an unexpected message.</exception>
        /// <exception cref="MissingCodecException">A codec could not be found for the given input arguments or the result.</exception>
        internal async Task<RawExecuteResult> ExecuteInternalAsync<TResult>(string query, IDictionary<string, object?>? args = null, Cardinality? cardinality = null,
            Capabilities? capabilities = Capabilities.Modifications, IOFormat format = IOFormat.Binary, bool isRetry = false, 
            CancellationToken token = default)
        {
            // if the current client is not connected, reconnect it
            if (!Duplexer.IsConnected)
                await ReconnectAsync(token);

            var linkedToken = CancellationTokenSource.CreateLinkedTokenSource(token, DisconnectCancelToken).Token;

            // safe to allow taskcancelledexception at this point since no data has been sent to the server.
            // dont pass linked token as we want to check our connection state below
            await _semaphore.WaitAsync(token).ConfigureAwait(false);

            await _readySource.Task;
            
            IsIdle = false;
            bool released = false;
            ExecuteResult? execResult = null;

            try
            {
                var cacheKey = CodecBuilder.GetCacheHashKey(query, cardinality ?? Cardinality.Many, format);

                var serializedState = Session.Serialize();
                
                if (!CodecBuilder.TryGetCodecs(cacheKey, out var inCodecInfo, out var outCodecInfo))
                {
                    bool parseHandlerPredicate(IReceiveable? packet)
                    {
                        switch (packet)
                        {
                            case ErrorResponse err when err.ErrorCode is not ServerErrorCodes.StateMismatchError:
                                throw new EdgeDBErrorException(err);
                            case CommandDataDescription descriptor:
                                {
                                    outCodecInfo = new(descriptor.OutputTypeDescriptorId,
                                        CodecBuilder.BuildCodec(descriptor.OutputTypeDescriptorId, descriptor.OutputTypeDescriptorBuffer));

                                    inCodecInfo = new(descriptor.InputTypeDescriptorId,
                                        CodecBuilder.BuildCodec(descriptor.InputTypeDescriptorId, descriptor.InputTypeDescriptorBuffer));

                                    CodecBuilder.UpdateKeyMap(cacheKey, descriptor.InputTypeDescriptorId, descriptor.OutputTypeDescriptorId);
                                }
                                break;
                            case StateDataDescription stateDescriptor:
                                {
                                    _stateCodec = CodecBuilder.BuildCodec(stateDescriptor.TypeDescriptorId, stateDescriptor.TypeDescriptorBuffer);
                                    _stateDescriptorId = stateDescriptor.TypeDescriptorId;
                                }
                                break;
                            case ReadyForCommand ready:
                                TransactionState = ready.TransactionState;
                                return true;
                            default:
                                break;
                        }

                        return false;
                    }

                    var stateBuf = _stateCodec?.Serialize(serializedState)!;

                    var result = await Duplexer.DuplexAndSyncAsync(new Parse
                    {
                        Capabilities = capabilities,
                        Query = query,
                        Format = format,
                        ExpectedCardinality = cardinality ?? Cardinality.Many,
                        ExplicitObjectIds = _config.ExplicitObjectIds,
                        StateTypeDescriptorId = _stateDescriptorId,
                        StateData = stateBuf,
                        ImplicitLimit = _config.ImplicitLimit,
                        ImplicitTypeNames = true, // used for type builder
                        ImplicitTypeIds = true,  // used for type builder
                    }, parseHandlerPredicate, alwaysReturnError: false, token: token).ConfigureAwait(false);

                    if (outCodecInfo is null)
                        throw new MissingCodecException("Couldn't find a valid output codec");

                    if (inCodecInfo is null)
                        throw new MissingCodecException("Couldn't find a valid input codec");
                }

                if (inCodecInfo.Codec is not IArgumentCodec argumentCodec)
                    throw new MissingCodecException($"Cannot encode arguments, {inCodecInfo.Codec} is not a registered argument codec");

                List<Data> receivedData = new();

                bool handler(IReceiveable msg)
                {
                    switch (msg)
                    {
                        case Data data:
                            receivedData.Add(data);
                            break;
                        case ErrorResponse err when err.ErrorCode is not ServerErrorCodes.ParameterTypeMismatchError:
                            throw new EdgeDBErrorException(err);
                        case ReadyForCommand ready:
                            TransactionState = ready.TransactionState;
                            return true;
                    }

                    return false;
                }

                var executeResult = await Duplexer.DuplexAndSyncAsync(new Execute() 
                {
                    Capabilities = capabilities,
                    Query = query,
                    Format = format,
                    ExpectedCardinality = cardinality ?? Cardinality.Many,
                    ExplicitObjectIds = _config.ExplicitObjectIds,
                    StateTypeDescriptorId = _stateDescriptorId,
                    StateData = _stateCodec?.Serialize(serializedState),
                    ImplicitTypeNames = true, // used for type builder
                    ImplicitTypeIds = true,  // used for type builder
                    Arguments = argumentCodec?.SerializeArguments(args) ,
                    ImplicitLimit = _config.ImplicitLimit,
                    InputTypeDescriptorId = inCodecInfo.Id,
                    OutputTypeDescriptorId = outCodecInfo.Id,
                }, handler, alwaysReturnError: false, token: linkedToken).ConfigureAwait(false);

                executeResult.ThrowIfErrrorResponse();

                execResult = new ExecuteResult(true, null, null, query);

                return new RawExecuteResult(outCodecInfo.Codec!, receivedData);
            }
            catch (OperationCanceledException)
            {
                // disconnect
                await DisconnectAsync(default);
                throw;
            }
            catch (EdgeDBException x) when (x.ShouldReconnect && !isRetry)
            {
                await ReconnectAsync(token).ConfigureAwait(false);
                _semaphore.Release();
                released = true;

                return await ExecuteInternalAsync<TResult>(query, args, cardinality, capabilities, format, true, token).ConfigureAwait(false);
            }
            catch (EdgeDBException x) when (x.ShouldRetry && !isRetry)
            {
                _semaphore.Release();
                released = true;

                return await ExecuteInternalAsync<TResult>(query, args, cardinality, capabilities, format, true, token).ConfigureAwait(false);
            }
            catch (Exception x)
            {
                execResult = x is EdgeDBErrorException err
                    ? new ExecuteResult(false, err.ErrorResponse, err, query)
                    : new ExecuteResult(false, null, x, query);

                Logger.InternalExecuteFailed(x);

                throw new EdgeDBException($"Failed to execute query{(isRetry ? " after retrying once" : "")}", x);
            }
            finally
            {
                if(execResult.HasValue)
                    _ = Task.Run(async () => await _queryExecuted.InvokeAsync(execResult!.Value).ConfigureAwait(false), token);
                IsIdle = true;
                if(!released) _semaphore.Release();
            }
        }

        /// <inheritdoc/>
        /// <exception cref="EdgeDBException">A general error occored.</exception>
        /// <exception cref="EdgeDBErrorException">The client received an <see cref="ErrorResponse"/>.</exception>
        /// <exception cref="UnexpectedMessageException">The client received an unexpected message.</exception>
        /// <exception cref="MissingCodecException">A codec could not be found for the given input arguments or the result.</exception>
        public override async Task ExecuteAsync(string query, IDictionary<string, object?>? args = null,
            Capabilities? capabilities = Capabilities.Modifications, CancellationToken token = default)
            => await ExecuteInternalAsync<object>(query, args, Cardinality.Many, capabilities, token: token).ConfigureAwait(false);

        /// <inheritdoc/>
        /// <exception cref="EdgeDBException">A general error occored.</exception>
        /// <exception cref="EdgeDBErrorException">The client received an <see cref="ErrorResponse"/>.</exception>
        /// <exception cref="UnexpectedMessageException">The client received an unexpected message.</exception>
        /// <exception cref="MissingCodecException">A codec could not be found for the given input arguments or the result.</exception>
        /// <exception cref="InvalidOperationException">Target type doesn't match received type.</exception>
        /// <exception cref="TargetInvocationException">Cannot construct a <typeparamref name="TResult"/>.</exception>
        public override async Task<IReadOnlyCollection<TResult?>> QueryAsync<TResult>(string query, IDictionary<string, object?>? args = null,
            Capabilities? capabilities = Capabilities.Modifications, CancellationToken token = default)
            where TResult : default
        {
            var result = await ExecuteInternalAsync<TResult>(query, args, Cardinality.Many, capabilities, token: token);

            List<TResult?> returnResults = new();

            for(int i = 0; i != result.Data.Length; i++)
            {
                var obj = ObjectBuilder.BuildResult<TResult>(result.Deserializer, ref result.Data[i]);
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
        /// <exception cref="InvalidOperationException">Target type doesn't match received type.</exception>
        /// <exception cref="TargetInvocationException">Cannot construct a <typeparamref name="TResult"/>.</exception>
        public override async Task<TResult?> QuerySingleAsync<TResult>(string query, IDictionary<string, object?>? args = null,
            Capabilities? capabilities = Capabilities.Modifications, CancellationToken token = default)
            where TResult : default
        {
            var result = await ExecuteInternalAsync<TResult>(query, args, Cardinality.AtMostOne, capabilities, token: token);

            if (result.Data.Length > 1)
                throw new ResultCardinalityMismatchException(Cardinality.AtMostOne, Cardinality.Many);

            var queryResult = result.Data.FirstOrDefault();

            return queryResult.PayloadBuffer is null
                ? default
                : ObjectBuilder.BuildResult<TResult>(result.Deserializer, ref result.Data[0]);
        }

        /// <inheritdoc/>
        /// <exception cref="EdgeDBException">A general error occored.</exception>
        /// <exception cref="EdgeDBErrorException">The client received an <see cref="ErrorResponse"/>.</exception>
        /// <exception cref="UnexpectedMessageException">The client received an unexpected message.</exception>
        /// <exception cref="MissingCodecException">A codec could not be found for the given input arguments or the result.</exception>
        /// <exception cref="ResultCardinalityMismatchException">The results cardinality was not what the query expected.</exception>
        /// <exception cref="MissingRequiredException">The query didn't return a result.</exception>
        /// <exception cref="InvalidOperationException">Target type doesn't match received type.</exception>
        /// <exception cref="TargetInvocationException">Cannot construct a <typeparamref name="TResult"/>.</exception>
        public override async Task<TResult> QueryRequiredSingleAsync<TResult>(string query, IDictionary<string, object?>? args = null,
            Capabilities? capabilities = Capabilities.Modifications, CancellationToken token = default)
        {
            var result = await ExecuteInternalAsync<TResult>(query, args, Cardinality.AtMostOne, capabilities, token: token);

            if (result.Data.Length is > 1 or 0)
                throw new ResultCardinalityMismatchException(Cardinality.One, result.Data.Length > 1 ? Cardinality.Many : Cardinality.AtMostOne);

            var queryResult = result.Data.FirstOrDefault();
            
            return queryResult.PayloadBuffer is null
                ? throw new MissingRequiredException()
                : ObjectBuilder.BuildResult<TResult>(result.Deserializer, ref result.Data[0])!;
        }

        /// <inheritdoc/>
        /// <exception cref="ResultCardinalityMismatchException">The results cardinality was not what the query expected.</exception>
        /// <exception cref="EdgeDBException">A general error occored.</exception>
        /// <exception cref="EdgeDBErrorException">The client received an <see cref="ErrorResponse"/>.</exception>
        /// <exception cref="UnexpectedMessageException">The client received an unexpected message.</exception>
        /// <exception cref="MissingCodecException">A codec could not be found for the given input arguments or the result.</exception>
        public override async Task<DataTypes.Json> QueryJsonAsync(string query, IDictionary<string, object?>? args = null, Capabilities? capabilities = Capabilities.Modifications, CancellationToken token = default)
        {
            var result = await ExecuteInternalAsync<object>(query, args, Cardinality.Many, capabilities, IOFormat.Json, token: token);
            
            return result.Data.Length == 1
                ? (string)result.Deserializer.Deserialize(result.Data[0].PayloadBuffer)!
                : "[]";
        }

        /// <inheritdoc/>
        /// <exception cref="EdgeDBException">A general error occored.</exception>
        /// <exception cref="EdgeDBErrorException">The client received an <see cref="ErrorResponse"/>.</exception>
        /// <exception cref="UnexpectedMessageException">The client received an unexpected message.</exception>
        /// <exception cref="MissingCodecException">A codec could not be found for the given input arguments or the result.</exception>
        public override async Task<IReadOnlyCollection<DataTypes.Json>> QueryJsonElementsAsync(string query, IDictionary<string, object?>? args = null, Capabilities? capabilities = Capabilities.Modifications, CancellationToken token = default)
        {
            var result = await ExecuteInternalAsync<object>(query, args, Cardinality.AtMostOne, capabilities, IOFormat.Json, token: token);

            return result.Data.Any()
                ? result.Data.Select(x => new DataTypes.Json((string?)result.Deserializer.Deserialize(x.PayloadBuffer))).ToImmutableArray()
                : ImmutableArray<DataTypes.Json>.Empty;
        }
        #endregion

        #region Packet handling
        private async ValueTask HandlePayloadAsync(IReceiveable payload)
        {
            switch (payload)
            {
                case ServerHandshake handshake:
                    if (handshake.MajorVersion != PROTOCOL_MAJOR_VERSION || handshake.MinorVersion < PROTOCOL_MINOR_VERSION)
                    {
                        Logger.ProtocolMajorMismatch($"{handshake.MajorVersion}.{handshake.MinorVersion}", $"{PROTOCOL_MAJOR_VERSION}.{PROTOCOL_MINOR_VERSION}");
                        await DisconnectAsync().ConfigureAwait(false);
                    }
                    else if (handshake.MajorVersion == PROTOCOL_MAJOR_VERSION && handshake.MinorVersion > PROTOCOL_MINOR_VERSION)
                        Logger.ProtocolMinorMismatch($"{handshake.MajorVersion}.{handshake.MinorVersion}", $"{PROTOCOL_MAJOR_VERSION}.{PROTOCOL_MINOR_VERSION}");
                    break;
                case ErrorResponse err:
                    Logger.ErrorResponseReceived(err.Severity, err.Message);
                    if (!_readyCancelTokenSource.IsCancellationRequested)
                        _readyCancelTokenSource.Cancel();
                    break;
                case AuthenticationStatus authStatus:
                    if (authStatus.AuthStatus == AuthStatus.AuthenticationRequiredSASLMessage)
                        _ = Task.Run(async () => await StartSASLAuthenticationAsync(authStatus).ConfigureAwait(false));
                    else if (authStatus.AuthStatus == AuthStatus.AuthenticationOK)
                        _authCompleteSource.TrySetResult();
                    break;
                case ServerKeyData keyData:
                    ServerKey = keyData.KeyBuffer;
                    break;
                case StateDataDescription stateDescriptor:
                    _stateCodec = CodecBuilder.BuildCodec(stateDescriptor.TypeDescriptorId, stateDescriptor.TypeDescriptorBuffer);
                    _stateDescriptorId = stateDescriptor.TypeDescriptorId;
                    break;
                case ParameterStatus parameterStatus:
                    ParseServerSettings(parameterStatus);
                    break;
                case ReadyForCommand cmd when !DisconnectCancelToken.IsCancellationRequested:
                    TransactionState = cmd.TransactionState;
                    _readySource.TrySetResult();
                    break;
                case LogMessage log:
                    try
                    {
                        await _onServerLog.InvokeAsync(log).ConfigureAwait(false);
                    }
                    catch(Exception x)
                    {
                        Logger.EventHandlerError(x);
                    }
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
            //await _semaphore.WaitAsync().ConfigureAwait(false);

            bool released = false;
            IsIdle = false;

            try
            {
                using var scram = new Scram();

                var method = authStatus.AuthenticationMethods![0];

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
            catch(EdgeDBException x) when (x.ShouldReconnect)
            {
                _semaphore.Release();
                released = true;
                await ReconnectAsync().ConfigureAwait(false);
                throw;
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
                if(!released) _semaphore.Release();
                IsIdle = true;
            }
        }
        #endregion

        #region Helper functions
        private void ParseServerSettings(ParameterStatus status)
        {
            try
            {
                switch (status.Name)
                {
                    case "suggested_pool_concurrency":
                        var str = Encoding.UTF8.GetString(status.ValueBuffer);
                        if (!int.TryParse(str, out SuggestedPoolConcurrency))
                        {
                            throw new FormatException("suggested_pool_concurrency type didn't match the expected type of int");
                        }
                        break;

                    case "system_config":
                        var reader = new PacketReader(status.ValueBuffer);
                        var length = reader.ReadInt32() - 16;
                        var descriptorId = reader.ReadGuid();
                        reader.ReadBytes(length, out var typeDesc);

                        var codec = CodecBuilder.GetCodec(descriptorId);

                        if (codec is null)
                        {
                            var innerReader = new PacketReader(ref typeDesc);
                            codec = CodecBuilder.BuildCodec(descriptorId, ref innerReader);

                            if (codec is null)
                                throw new MissingCodecException("Failed to build codec for system_config");
                        }

                        // disard length
                        reader.Skip(4);

                        var obj = codec.Deserialize(ref reader)!;

                        RawServerConfig = ((ExpandoObject)obj).ToDictionary(x => x.Key, x => x.Value);
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
        /// <remarks>
        ///     This task waits for the underlying connection to receive a 
        ///     <see cref="ReadyForCommand"/> message indicating the client 
        ///     can start to preform queries.
        /// </remarks>
        /// <inheritdoc/>
        public override async ValueTask ConnectAsync(CancellationToken token = default)
        {
            await _connectSemaphone.WaitAsync(token);

            try
            {
                _authCompleteSource = new();

                await ConnectInternalAsync(token: token);

                var linkedToken = CancellationTokenSource.CreateLinkedTokenSource(token, _readyCancelTokenSource.Token);

                token.ThrowIfCancellationRequested();

                // wait for auth promise
                await Task.Run(() => Task.WhenAll(_readySource.Task, _authCompleteSource.Task), linkedToken.Token).ConfigureAwait(false);

                // call base to notify listeners that we connected.
                await base.ConnectAsync(token);
            }
            finally
            {
                _connectSemaphone.Release();
            }
        }

        private async Task ConnectInternalAsync(int attempts = 0, CancellationToken token = default)
        {
            try
            {
                if (IsConnected)
                    return;

                _readySource = new();
                _readyCancelTokenSource = new();
                _authCompleteSource = new();

                await Duplexer.ResetAsync();

                Stream? stream;

                try
                {
                    stream = await GetStreamAsync(token).ConfigureAwait(false);
                }
                catch(EdgeDBException x) when (x.ShouldReconnect)
                {
                    attempts++;
                    Logger.AttemptToReconnect((uint)attempts, _config.MaxConnectionRetries);
                    if (attempts == _config.MaxConnectionRetries)
                        throw new ConnectionFailedException(attempts);
                    else
                    {
                        await ConnectInternalAsync(attempts, token);
                        return;
                    }
                } 

                Duplexer.Start(stream);

                Duplexer.OnMessage += HandlePayloadAsync;

                // send handshake
                await Duplexer.SendAsync(new ClientHandshake
                {
                    MajorVersion = PROTOCOL_MAJOR_VERSION,
                    MinorVersion = PROTOCOL_MINOR_VERSION,
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
                }, token).ConfigureAwait(false);
            }
            catch (EdgeDBException x) when (x.ShouldReconnect)
            {
                if (_currentRetries == _config.MaxConnectionRetries)
                {
                    throw;
                }

                _currentRetries++;
                await ConnectInternalAsync(token: token).ConfigureAwait(false);
            }
        }

        /// <summary>
        ///     Disconnects and reconnects the current client.
        /// </summary>
        /// <param name="token">A cancellation token used to cancel the asynchronous operation.</param>
        /// <returns>A task representing the asynchronous disconnect and reconnection operations.</returns>
        public async Task ReconnectAsync(CancellationToken token = default)
        {
            Duplexer.OnMessage -= HandlePayloadAsync;
            await DisconnectAsync(token).ConfigureAwait(false);
            await ConnectAsync(token).ConfigureAwait(false);
        }

        /// <summary>
        ///     Disconnects this client from the database.
        /// </summary>
        /// <remarks/> <!-- clears the remarks about calling base -->
        /// <inheritdoc/>
        public override ValueTask DisconnectAsync(CancellationToken token = default)
            => Duplexer.DisconnectAsync(token);

        private async ValueTask HandleDuplexerDisconnectAsync()
        {
            // if we receive a disconnect from the duplexer we should call our disconnect methods to property close down our client.
            await CloseStreamAsync().ConfigureAwait(false);
            await base.DisconnectAsync();
        }

        #endregion

        #region Command locks
        internal async Task<IDisposable> AquireCommandLockAsync(CancellationToken token = default)
        {
            var linkedToken = CancellationTokenSource.CreateLinkedTokenSource(DisconnectCancelToken, token);

            await _commandSemaphore.WaitAsync(linkedToken.Token).ConfigureAwait(false);

            return new CommandLock(() => { _commandSemaphore.Release(); });
        }

        private readonly struct CommandLock : IDisposable
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
        /// <summary>
        ///     Gets a stream that the binary client can write and read from.
        /// </summary>
        /// <param name="token">A cancellation token used to cancel the asynchronous operation.</param>
        /// <returns>
        ///     A task representing the asynchronous operation of opening or 
        ///     initializing a stream; the result of the task is a stream the 
        ///     binary client can use to read from/write to the database with.
        /// </returns>
        protected abstract ValueTask<Stream> GetStreamAsync(CancellationToken token = default);

        /// <summary>
        ///     Closes the stream returned from <see cref="GetStreamAsync"/>.
        /// </summary>
        /// <param name="token">A cancellation token used to cancel the asynchronous operation.</param>
        /// <returns>
        ///     A task that represents the asynchronous closing operation of 
        ///     the stream.
        /// </returns>
        protected abstract ValueTask CloseStreamAsync(CancellationToken token = default);
        #endregion

        #region Client pool dispose
        /// <remarks/> <!-- removes the remark about calling base -->
        /// <inheritdoc/>
        public override async ValueTask<bool> DisposeAsync()
        {
            var shouldDispose = await base.DisposeAsync().ConfigureAwait(false);

            if (shouldDispose && IsConnected)
            {
                await DisconnectAsync();
            }

            return shouldDispose;
        }
        #endregion

        #region ITransactibleClient
        /// <inheritdoc/>
        async Task ITransactibleClient.StartTransactionAsync(Isolation isolation, bool readOnly, bool deferrable, CancellationToken token)
        {
            var isolationMode = isolation switch
            {
                Isolation.Serializable => "serializable",
                _ => throw new EdgeDBException("Unknown isolation mode")
            };

            var readMode = readOnly ? "read only" : "read write";

            var deferMode = $"{(!deferrable ? "not " : "")}deferrable";

            await ExecuteInternalAsync<object>($"start transaction isolation {isolationMode}, {readMode}, {deferMode}", capabilities: Capabilities.Transaction, token: token).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        async Task ITransactibleClient.CommitAsync(CancellationToken token)
            => await ExecuteInternalAsync<object>($"commit", capabilities: Capabilities.Transaction, token: token).ConfigureAwait(false);

        /// <inheritdoc/>
        async Task ITransactibleClient.RollbackAsync(CancellationToken token)
            => await ExecuteInternalAsync<object>($"rollback", capabilities: Capabilities.Transaction, token: token).ConfigureAwait(false);

        /// <inheritdoc/>
        TransactionState ITransactibleClient.TransactionState => TransactionState;
        #endregion
    }
}
