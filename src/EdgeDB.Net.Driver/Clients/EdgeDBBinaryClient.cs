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
    internal abstract class EdgeDBBinaryClient : BaseEdgeDBClient
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
        public virtual bool IsIdle { get; private set; } = true;

        /// <summary>
        ///     Gets the raw server config.
        /// </summary>
        /// <remarks>
        ///     This dictionary can be empty if the client hasn't connected to the database.
        /// </remarks>
        public IReadOnlyDictionary<string, object?> ServerConfig
            => RawServerConfig.ToImmutableDictionary();
        
        internal abstract IBinaryDuplexer Duplexer { get; }

        internal byte[] ServerKey;
        internal int SuggestedPoolConcurrency;
        internal Dictionary<string, object?> RawServerConfig = new();
        
        internal readonly ILogger Logger;
        internal readonly TimeSpan MessageTimeout;
        internal readonly TimeSpan ConnectionTimeout;
        internal readonly EdgeDBConnection Connection;
        
        protected CancellationToken DisconnectCancelToken
            => Duplexer.DisconnectToken;
            
        private readonly AsyncEvent<Func<ExecuteResult, ValueTask>> _queryExecuted = new();
        private readonly AsyncEvent<Func<LogMessage, ValueTask>> _onServerLog = new();

        private ICodec? _stateCodec;
        private Guid _stateDescriptorId;
        private TaskCompletionSource _readySource;
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
        /// <param name="clientConfig">The configuration for this client.</param>
        /// <param name="clientPoolHolder">The client pool holder for this client.</param>
        /// <param name="clientId">The optional client id of this client. This is used for logging and client pooling.</param>
        public EdgeDBBinaryClient(
            EdgeDBConnection connection,
            EdgeDBConfig clientConfig,
            IDisposable clientPoolHolder,
            ulong? clientId = null)
            : base(clientId ?? 0, clientPoolHolder)
        {
            Logger = clientConfig.Logger ?? Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
            Connection = connection;
            ServerKey = new byte[32];
            MessageTimeout = TimeSpan.FromMilliseconds(clientConfig.MessageTimeout);
            ConnectionTimeout = TimeSpan.FromMilliseconds(clientConfig.ConnectionTimeout);
            _stateDescriptorId = CodecBuilder.InvalidCodec;
            _config = clientConfig;
            _semaphore = new(1, 1);
            _commandSemaphore = new(1, 1);
            _connectSemaphone = new(1, 1);
            _readySource = new();
            _readyCancelTokenSource = new();
        }

        #region Commands/queries

        public async Task SyncAsync(CancellationToken token = default)
        {
            // if the current client is not connected, reconnect it
            if (!Duplexer.IsConnected)
                await ReconnectAsync(token);

            using var linkedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(token, DisconnectCancelToken);

            await _semaphore.WaitAsync(linkedTokenSource.Token).ConfigureAwait(false);

            try
            {
                var result = await Duplexer.DuplexSingleAsync(new Sync(), linkedTokenSource.Token).ConfigureAwait(false);
                var rfc = result?.ThrowIfErrorOrNot<ReadyForCommand>();

                if(rfc.HasValue)
                {
                    UpdateTransactionState(rfc.Value.TransactionState);
                }    
            }
            finally
            {
                _semaphore.Release();
            }
        }

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
        internal virtual async Task<RawExecuteResult> ExecuteInternalAsync(string query, IDictionary<string, object?>? args = null, Cardinality? cardinality = null,
            Capabilities? capabilities = Capabilities.Modifications, IOFormat format = IOFormat.Binary, bool isRetry = false, bool implicitTypeName = false,
            CancellationToken token = default)
        {
            // if the current client is not connected, reconnect it
            if (!Duplexer.IsConnected)
                await ReconnectAsync(token);

            using var linkedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(token, DisconnectCancelToken);

            // safe to allow taskcancelledexception at this point since no data has been sent to the server.
            await _semaphore.WaitAsync(linkedTokenSource.Token).ConfigureAwait(false);

            await _readySource.Task;
            
            IsIdle = false;
            bool released = false;
            ExecuteResult? execResult = null;
            ErrorResponse? error = null;

            bool gotStateDescriptor = false;
            bool successfullyParsed = false;
            int parseAttempts = 0;

            try
            {
                var cacheKey = CodecBuilder.GetCacheHashKey(query, cardinality ?? Cardinality.Many, format);

                var serializedState = Session.Serialize();
                var stateBuf = _stateCodec?.Serialize(serializedState)!;
                
                if (!CodecBuilder.TryGetCodecs(cacheKey, out var inCodecInfo, out var outCodecInfo))
                {                    
                    while (!successfullyParsed)
                    {
                        if(parseAttempts > 2)
                        {
                            throw error.HasValue
                                ? new EdgeDBException($"Failed to parse query after {parseAttempts} attempts", new EdgeDBErrorException(error.Value, query))
                                : new EdgeDBException($"Failed to parse query after {parseAttempts} attempts");
                        }

                        await foreach (var result in Duplexer.DuplexAndSyncAsync(new Parse
                        {
                            Capabilities = capabilities,
                            Query = query,
                            Format = format,
                            ExpectedCardinality = cardinality ?? Cardinality.Many,
                            ExplicitObjectIds = _config.ExplicitObjectIds,
                            StateTypeDescriptorId = _stateDescriptorId,
                            StateData = stateBuf,
                            ImplicitLimit = _config.ImplicitLimit,
                            ImplicitTypeNames = implicitTypeName, // used for type builder
                            ImplicitTypeIds = true,  // used for type builder
                        }, linkedTokenSource.Token))
                        {
                            switch (result.Packet)
                            {
                                case ErrorResponse err when err.ErrorCode is not ServerErrorCodes.StateMismatchError:
                                    error = err;
                                    break;
                                case ErrorResponse err when err.ErrorCode is ServerErrorCodes.StateMismatchError:
                                    {
                                        // we should have received a state descriptor at this point,
                                        // if we have not, this is a issue with the client implementation
                                        if (!gotStateDescriptor)
                                        {
                                            throw new EdgeDBException("Failed to properly encode state data, this is a bug.");
                                        }

                                        // we can safely retry by finishing this duplex and starting a
                                        // new one with the 'while' loop.
                                        result.Finish();
                                    }
                                    break;
                                case CommandDataDescription descriptor:
                                    {
                                        outCodecInfo = new(descriptor.OutputTypeDescriptorId,
                                            CodecBuilder.BuildCodec(this, descriptor.OutputTypeDescriptorId, descriptor.OutputTypeDescriptorBuffer));

                                        inCodecInfo = new(descriptor.InputTypeDescriptorId,
                                            CodecBuilder.BuildCodec(this, descriptor.InputTypeDescriptorId, descriptor.InputTypeDescriptorBuffer));

                                        CodecBuilder.UpdateKeyMap(cacheKey, descriptor.InputTypeDescriptorId, descriptor.OutputTypeDescriptorId);
                                    }
                                    break;
                                case StateDataDescription stateDescriptor:
                                    {
                                        _stateCodec = CodecBuilder.BuildCodec(this, stateDescriptor.TypeDescriptorId, stateDescriptor.TypeDescriptorBuffer);
                                        _stateDescriptorId = stateDescriptor.TypeDescriptorId;
                                        gotStateDescriptor = true;
                                        stateBuf = _stateCodec?.Serialize(serializedState)!;
                                    }
                                    break;
                                case ReadyForCommand ready:
                                    UpdateTransactionState(ready.TransactionState);
                                    result.Finish();
                                    successfullyParsed = true;
                                    break;
                                default:
                                    break;
                            }
                        }
                        
                        parseAttempts++;
                    }
                    
                    
                    if (error.HasValue)
                        throw new EdgeDBErrorException(error.Value, query);

                    if (outCodecInfo is null)
                        throw new MissingCodecException("Couldn't find a valid output codec");

                    if (inCodecInfo is null)
                        throw new MissingCodecException("Couldn't find a valid input codec");
                }

                if (inCodecInfo.Codec is not IArgumentCodec argumentCodec)
                    throw new MissingCodecException($"Cannot encode arguments, {inCodecInfo.Codec} is not a registered argument codec");

                List<Data> receivedData = new();
                bool executeSuccess = false;

                do
                {
                    await foreach (var result in Duplexer.DuplexAndSyncAsync(new Execute()
                    {
                        Capabilities = capabilities,
                        Query = query,
                        Format = format,
                        ExpectedCardinality = cardinality ?? Cardinality.Many,
                        ExplicitObjectIds = _config.ExplicitObjectIds,
                        StateTypeDescriptorId = _stateDescriptorId,
                        StateData = _stateCodec?.Serialize(serializedState),
                        ImplicitTypeNames = implicitTypeName, // used for type builder
                        ImplicitTypeIds = true,  // used for type builder
                        Arguments = argumentCodec.SerializeArguments(args),
                        ImplicitLimit = _config.ImplicitLimit,
                        InputTypeDescriptorId = inCodecInfo.Id,
                        OutputTypeDescriptorId = outCodecInfo.Id,
                    }, linkedTokenSource.Token))
                    {
                        switch (result.Packet)
                        {
                            case Data data:
                                receivedData.Add(data);
                                break;
                            case StateDataDescription stateDescriptor:
                                {
                                    _stateCodec = CodecBuilder.BuildCodec(this, stateDescriptor.TypeDescriptorId, stateDescriptor.TypeDescriptorBuffer);
                                    _stateDescriptorId = stateDescriptor.TypeDescriptorId;
                                    gotStateDescriptor = true;
                                    stateBuf = _stateCodec?.Serialize(serializedState)!;
                                }
                                break;
                            case ErrorResponse err when err.ErrorCode is ServerErrorCodes.StateMismatchError:
                                {
                                    // we should have received a state descriptor at this point,
                                    // if we have not, this is a issue with the client implementation
                                    if (!gotStateDescriptor)
                                    {
                                        throw new EdgeDBException("Failed to properly encode state data, this is a bug.");
                                    }

                                    // we can safely retry by finishing this duplex and starting a
                                    // new one with the 'while' loop.
                                    result.Finish();
                                }
                                break;
                            case ErrorResponse err when err.ErrorCode is not ServerErrorCodes.ParameterTypeMismatchError:
                                error = err;
                                break;
                            case ReadyForCommand ready:
                                UpdateTransactionState(ready.TransactionState);
                                result.Finish();
                                executeSuccess = true;
                                break;
                        }
                    }
                }
                while (!successfullyParsed && !executeSuccess);

                if (error.HasValue)
                    throw new EdgeDBErrorException(error.Value, query);

                execResult = new ExecuteResult(true, null, query);

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

                return await ExecuteInternalAsync(query, args, cardinality, capabilities, format, true, implicitTypeName, token).ConfigureAwait(false);
            }
            catch (EdgeDBException x) when (x.ShouldRetry && !isRetry)
            {
                _semaphore.Release();
                released = true;

                return await ExecuteInternalAsync(query, args, cardinality, capabilities, format, true, implicitTypeName, token).ConfigureAwait(false);
            }
            catch (Exception x)
            {
                execResult = x is EdgeDBErrorException err
                    ? new ExecuteResult(false, err, query)
                    : new ExecuteResult(false, x, query);

                Logger.InternalExecuteFailed(x);

                if (x is EdgeDBErrorException)
                    throw;

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
            => await ExecuteInternalAsync(query, args, Cardinality.Many, capabilities, token: token).ConfigureAwait(false);

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
            var implicitTypeName = TypeBuilder.TryGetTypeDeserializerInfo(typeof(TResult), out var info) && info.RequiresTypeName;

            var result = await ExecuteInternalAsync(
                query,
                args,
                Cardinality.Many,
                capabilities,
                implicitTypeName: implicitTypeName,
                token: token);

            var array = new TResult?[result.Data.Length];

            for(int i = 0; i != result.Data.Length; i++)
            {
                var obj = ObjectBuilder.BuildResult<TResult>(result.Deserializer, ref result.Data[i]);
                array[i] = obj;
            }

            return array.ToImmutableArray();
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
            var implicitTypeName = TypeBuilder.TryGetTypeDeserializerInfo(typeof(TResult), out var info) && info.RequiresTypeName;
            
            var result = await ExecuteInternalAsync(
                query,
                args,
                Cardinality.AtMostOne,
                capabilities,
                implicitTypeName: implicitTypeName,
                token: token);

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
            var implicitTypeName = TypeBuilder.TryGetTypeDeserializerInfo(typeof(TResult), out var info) && info.RequiresTypeName;
            
            var result = await ExecuteInternalAsync(
                query,
                args,
                Cardinality.AtMostOne,
                capabilities,
                implicitTypeName: implicitTypeName,
                token: token);

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
            var result = await ExecuteInternalAsync(query, args, Cardinality.Many, capabilities, IOFormat.Json, token: token);
            
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
            var result = await ExecuteInternalAsync(query, args, Cardinality.Many, capabilities, IOFormat.JsonElements, token: token);

            return result.Data.Any()
                ? result.Data.Select(x => new DataTypes.Json((string?)result.Deserializer.Deserialize(x.PayloadBuffer))).ToImmutableArray()
                : ImmutableArray<DataTypes.Json>.Empty;
        }
        #endregion

        #region Packet handling
        private async ValueTask HandlePacketAsync(IReceiveable payload)
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
                        await StartSASLAuthenticationAsync(authStatus).ConfigureAwait(false);
                    else if(authStatus.AuthStatus != AuthStatus.AuthenticationOK)
                        throw new UnexpectedMessageException("Expected AuthenticationRequiredSASLMessage, got " + authStatus.AuthStatus);
                    break;
                case ServerKeyData keyData:
                    ServerKey = keyData.KeyBuffer;
                    break;
                case StateDataDescription stateDescriptor:
                    _stateCodec = CodecBuilder.BuildCodec(this,  stateDescriptor.TypeDescriptorId, stateDescriptor.TypeDescriptorBuffer);
                    _stateDescriptorId = stateDescriptor.TypeDescriptorId;
                    break;
                case ParameterStatus parameterStatus:
                    ParseServerSettings(parameterStatus);
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
        }
        #endregion

        #region SASL
        private async Task StartSASLAuthenticationAsync(AuthenticationStatus authStatus)
        {
            IsIdle = false;

            try
            {
                using var scram = new Scram();

                var method = authStatus.AuthenticationMethods![0];

                if (method is not "SCRAM-SHA-256")
                {
                    throw new ProtocolViolationException("The only supported method is SCRAM-SHA-256");
                }

                var initialMsg = scram.BuildInitialMessagePacket(Connection.Username!, method);
                byte[] expectedSig = Array.Empty<byte>();

                await foreach(var result in Duplexer.DuplexAsync(initialMsg))
                {
                    switch (result.Packet)
                    {
                        case AuthenticationStatus authResult:
                            {
                                switch (authResult.AuthStatus)
                                {
                                    case AuthStatus.AuthenticationSASLContinue:
                                        {
                                            var (msg, sig) = scram.BuildFinalMessagePacket(in authResult, Connection.Password!);
                                            expectedSig = sig;

                                            await Duplexer.SendAsync(packets: msg).ConfigureAwait(false);
                                        }
                                        break;
                                    case AuthStatus.AuthenticationSASLFinal:
                                        {
                                            var key = Scram.ParseServerFinalMessage(authResult);

                                            if (!key.SequenceEqual(expectedSig))
                                            {
                                                throw new InvalidSignatureException();
                                            }
                                        }
                                        break;
                                    case AuthStatus.AuthenticationOK:
                                        {
                                            _currentRetries = 0;
                                            result.Finish();
                                        }
                                        break;
                                    default:
                                        throw new UnexpectedMessageException($"Expected coninue or final but got {authResult.AuthStatus}");
                                }
                            }
                            break;
                        case ErrorResponse err:
                            throw new EdgeDBErrorException(err);
                    }
                }
            }
            catch(EdgeDBException x) when (x.ShouldReconnect)
            {
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
                IsIdle = true;
            }
        }
        #endregion

        #region Helper functions
        protected void TriggerReady()
        {
            _readySource.TrySetResult();
        }

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
                            codec = CodecBuilder.BuildCodec(this, descriptorId, ref innerReader);

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
            await _connectSemaphone.WaitAsync(token).ConfigureAwait(false);

            try
            {
                await ConnectInternalAsync(token: token);

                using var linkedToken = CancellationTokenSource.CreateLinkedTokenSource(token, _readyCancelTokenSource.Token);

                token.ThrowIfCancellationRequested();

                // run a message loop until the client is ready for commands
                while (!linkedToken.IsCancellationRequested)
                {
                    var message = await Duplexer.ReadNextAsync(linkedToken.Token).ConfigureAwait(false);

                    if (message is null)
                        throw new UnexpectedDisconnectException();

                    if (message is ReadyForCommand)
                        break;

                    await HandlePacketAsync(message).ConfigureAwait(false);
                }

                _readySource.SetResult();

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

                _readyCancelTokenSource = new();
                _readySource = new();

                Duplexer.Reset();

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

                if(Duplexer is StreamDuplexer streamDuplexer)
                    streamDuplexer.Init(stream);

                // send handshake
                await Duplexer.SendAsync(token, new ClientHandshake
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
                }).ConfigureAwait(false);
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
        #endregion

        #region Command locks
        internal async Task<IDisposable> AquireCommandLockAsync(CancellationToken token = default)
        {
            using var linkedToken = CancellationTokenSource.CreateLinkedTokenSource(DisconnectCancelToken, token);

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

                if (shouldDispose)
                {
                    Duplexer.Dispose();
                    _readyCancelTokenSource.Dispose();
                }
            }

            return shouldDispose;
        }
        #endregion
    }
}
