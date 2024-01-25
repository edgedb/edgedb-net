using EdgeDB.Binary;
using EdgeDB.Binary.Codecs;
using EdgeDB.Binary.Protocol;
using EdgeDB.Binary.Protocol.Common;
using EdgeDB.DataTypes;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Reflection;
using ProtocolExecuteResult = EdgeDB.Binary.Protocol.ExecuteResult;

namespace EdgeDB;

/// <summary>
///     Represents an abstract binary client.
/// </summary>
internal abstract class EdgeDBBinaryClient : BaseEdgeDBClient
{
    private readonly SemaphoreSlim _commandSemaphore;
    private readonly SemaphoreSlim _connectSemaphone;
    private readonly SemaphoreSlim _semaphore;
    internal readonly EdgeDBConnection Connection;
    internal readonly TimeSpan ConnectionTimeout;

    internal readonly ILogger Logger;
    internal readonly TimeSpan MessageTimeout;
    private uint _currentRetries;
    private IProtocolProvider _protocolProvider;
    private CancellationTokenSource _readyCancelTokenSource;

    private TaskCompletionSource _readySource;

    private ICodec? _stateCodec;
    private Guid _stateDescriptorId;

    internal byte[] ServerKey;

    internal int? SuggestedPoolConcurrency
        => _protocolProvider.SuggestedPoolConcurrency;

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
        Logger = clientConfig.Logger ?? NullLogger.Instance;
        Connection = connection;
        ServerKey = new byte[32];
        MessageTimeout = TimeSpan.FromMilliseconds(clientConfig.MessageTimeout);
        ConnectionTimeout = TimeSpan.FromMilliseconds(clientConfig.ConnectionTimeout);
        _stateDescriptorId = CodecBuilder.InvalidCodec;
        ClientConfig = clientConfig;
        _semaphore = new SemaphoreSlim(1, 1);
        _commandSemaphore = new SemaphoreSlim(1, 1);
        _connectSemaphone = new SemaphoreSlim(1, 1);
        _readySource = new TaskCompletionSource();
        _readyCancelTokenSource = new CancellationTokenSource();
        CodecContext = new CodecContext(this);

        _protocolProvider = IProtocolProvider.GetProvider(this);

        Logger.ClientProtocolInit(_protocolProvider.Version, string.Join(", ", IProtocolProvider.Providers.Keys));
    }

    /// <summary>
    ///     Gets whether or not this connection is idle.
    /// </summary>
    public virtual bool IsIdle { get; private set; } = true;

    public IReadOnlyDictionary<string, object?> ServerConfig
        => _protocolProvider.ServerConfig;

    internal abstract IBinaryDuplexer Duplexer { get; }

    internal virtual IProtocolProvider ProtocolProvider
        => _protocolProvider;

    internal CodecContext CodecContext { get; }

    internal ref Guid StateDescriptorId
        => ref _stateDescriptorId;

    internal EdgeDBConfig ClientConfig { get; }

    protected CancellationToken DisconnectCancelToken
        => Duplexer.DisconnectToken;

    #region Client pool dispose

    /// <remarks />
    /// <!-- removes the remark about calling base -->
    /// <inheritdoc />
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

    #region Commands/queries

    public async Task SyncAsync(CancellationToken token = default)
    {
        using var linkedToken = CancellationTokenSource.CreateLinkedTokenSource(token, DisconnectCancelToken);

        await _semaphore.WaitAsync(linkedToken.Token).ConfigureAwait(false);

        try
        {
            await _protocolProvider.SendSyncMessageAsync(linkedToken.Token).ConfigureAwait(false);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    internal readonly record struct ExecuteResult(
        ProtocolExecuteResult ProtocolResult,
        ObjectBuilder.PreheatedCodec? PreheatedCodec
    );

    /// <exception cref="EdgeDBException">A general error occored.</exception>
    /// <exception cref="EdgeDBErrorException">The client received an <see cref="IProtocolError" />.</exception>
    /// <exception cref="UnexpectedMessageException">The client received an unexpected message.</exception>
    /// <exception cref="MissingCodecException">A codec could not be found for the given input arguments or the result.</exception>
    internal virtual async Task<ExecuteResult> ExecuteInternalAsync(string query,
        IDictionary<string, object?>? args = null, Cardinality? cardinality = null,
        Capabilities? capabilities = Capabilities.Modifications, IOFormat format = IOFormat.Binary,
        bool isRetry = false, bool implicitTypeName = false,
        Func<ParseResult, Task<ObjectBuilder.PreheatedCodec>>? preheat = null,
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
        var released = false;

        var arguments = new QueryParameters(query, args, capabilities ?? Capabilities.Modifications,
            cardinality ?? Cardinality.Many, format, implicitTypeName);

        try
        {
            var parseResult = await _protocolProvider.ParseQueryAsync(arguments, linkedTokenSource.Token);

            if (preheat is not null)
            {
                var executeTask =
                    _protocolProvider.ExecuteQueryAsync(arguments, parseResult, linkedTokenSource.Token);

#if DEBUG
                    async Task<ObjectBuilder.PreheatedCodec> PreheatWithTrace(ParseResult p)
                    {
                        var stopwatch = Stopwatch.StartNew();
                        var preheated = await preheat(p);
                        stopwatch.Stop();
                        Logger.LogDebug("Preheating of codecs took {@PreheatTime}ms", Math.Round(stopwatch.Elapsed.TotalMilliseconds, 4));
                        return preheated;
                    }

                    var preheatTask = PreheatWithTrace(parseResult);
#else
                var preheatTask = preheat(parseResult);
#endif

                await Task.WhenAll(
                    preheatTask,
                    executeTask
                );

                return new(executeTask.Result, preheatTask.Result);
            }

            return new(await _protocolProvider.ExecuteQueryAsync(arguments, parseResult, linkedTokenSource.Token), null);
        }
        catch (OperationCanceledException ce)
        {
            // only throw if it was the timeout token that caused the operation to cancel
            var wasTimedOut = !token.IsCancellationRequested && !Duplexer.DisconnectToken.IsCancellationRequested;

            // disconnect
            await DisconnectAsync();

            if (wasTimedOut)
                throw new QueryTimeoutException(ClientConfig.MessageTimeout, query, ce);
            else throw;
        }
        catch (EdgeDBException x) when (x.ShouldReconnect && !isRetry)
        {
            await ReconnectAsync(token).ConfigureAwait(false);
            _semaphore.Release();
            released = true;

            return await ExecuteInternalAsync(query, args, cardinality, capabilities, format, true, implicitTypeName,
                preheat, token).ConfigureAwait(false);
        }
        catch (EdgeDBException x) when (x.ShouldRetry && !isRetry)
        {
            _semaphore.Release();
            released = true;

            return await ExecuteInternalAsync(query, args, cardinality, capabilities, format, true, implicitTypeName,
                preheat, token).ConfigureAwait(false);
        }
        catch (Exception x)
        {
            Logger.InternalExecuteFailed(x);

            if (x is EdgeDBErrorException)
                throw;

            throw new EdgeDBException($"Failed to execute query{(isRetry ? " after retrying once" : "")}", x);
        }
        finally
        {
            IsIdle = true;
            if (!released) _semaphore.Release();
        }
    }

    private ValueTask<ObjectBuilder.PreheatedCodec> GetPreheatedCodecAsync<T>(ExecuteResult result, CancellationToken token)
    {
        if (result.PreheatedCodec.HasValue)
            return new ValueTask<ObjectBuilder.PreheatedCodec>(result.PreheatedCodec.Value);

        return new ValueTask<ObjectBuilder.PreheatedCodec>(
            ObjectBuilder.PreheatCodecAsync<T>(this, result.ProtocolResult.OutCodecInfo.Codec, token));
    }

    /// <inheritdoc />
    /// <exception cref="EdgeDBException">A general error occored.</exception>
    /// <exception cref="EdgeDBErrorException">The client received an <see cref="IProtocolError" />.</exception>
    /// <exception cref="UnexpectedMessageException">The client received an unexpected message.</exception>
    /// <exception cref="MissingCodecException">A codec could not be found for the given input arguments or the result.</exception>
    public override async Task ExecuteAsync(string query, IDictionary<string, object?>? args = null,
        Capabilities? capabilities = Capabilities.Modifications, CancellationToken token = default)
        => await ExecuteInternalAsync(query, args, Cardinality.Many, capabilities, IOFormat.None, token: token)
            .ConfigureAwait(false);

    /// <inheritdoc />
    /// <exception cref="EdgeDBException">A general error occored.</exception>
    /// <exception cref="EdgeDBErrorException">The client received an <see cref="IProtocolError" />.</exception>
    /// <exception cref="UnexpectedMessageException">The client received an unexpected message.</exception>
    /// <exception cref="MissingCodecException">A codec could not be found for the given input arguments or the result.</exception>
    /// <exception cref="InvalidOperationException">Target type doesn't match received type.</exception>
    /// <exception cref="TargetInvocationException">Cannot construct a <typeparamref name="TResult" />.</exception>
    public override async Task<IReadOnlyCollection<TResult?>> QueryAsync<TResult>(string query,
        IDictionary<string, object?>? args = null,
        Capabilities? capabilities = Capabilities.Modifications, CancellationToken token = default)
        where TResult : default
    {
        var implicitTypeName = TypeBuilder.TryGetTypeDeserializerInfo(typeof(TResult), out var info) &&
                               info.RequiresTypeName;

        var result = await ExecuteInternalAsync(
            query,
            args,
            Cardinality.Many,
            capabilities,
            implicitTypeName: implicitTypeName,
            preheat: parseResult => ObjectBuilder.PreheatCodecAsync<TResult>(this, parseResult.OutCodecInfo.Codec, token),
            token: token);

        var array = new TResult?[result.ProtocolResult.Data.Length];

        var codec = await GetPreheatedCodecAsync<TResult>(result, token);

        if (result.ProtocolResult.Data.Length <= 7)
        {
            for (var i = 0; i != result.ProtocolResult.Data.Length; i++)
            {
                array[i] = ObjectBuilder.BuildResult<TResult>(this, codec, result.ProtocolResult.Data[i]);
            }
        }
        else
        {
            Parallel.ForEach(result.ProtocolResult.Data, (x, state, i) =>
            {
                array[i] = ObjectBuilder.BuildResult<TResult>(this, codec, x);
            });
        }

        return array.ToImmutableArray();
    }

    /// <inheritdoc />
    /// <exception cref="EdgeDBException">A general error occored.</exception>
    /// <exception cref="EdgeDBErrorException">The client received an <see cref="IProtocolError" />.</exception>
    /// <exception cref="UnexpectedMessageException">The client received an unexpected message.</exception>
    /// <exception cref="MissingCodecException">A codec could not be found for the given input arguments or the result.</exception>
    /// <exception cref="ResultCardinalityMismatchException">The results cardinality was not what the query expected.</exception>
    /// <exception cref="InvalidOperationException">Target type doesn't match received type.</exception>
    /// <exception cref="TargetInvocationException">Cannot construct a <typeparamref name="TResult" />.</exception>
    public override async Task<TResult?> QuerySingleAsync<TResult>(string query,
        IDictionary<string, object?>? args = null,
        Capabilities? capabilities = Capabilities.Modifications, CancellationToken token = default)
        where TResult : default
    {
        var implicitTypeName = TypeBuilder.TryGetTypeDeserializerInfo(typeof(TResult), out var info) &&
                               info.RequiresTypeName;

        var result = await ExecuteInternalAsync(
            query,
            args,
            Cardinality.AtMostOne,
            capabilities,
            implicitTypeName: implicitTypeName,
            preheat: parseResult => ObjectBuilder.PreheatCodecAsync<TResult>(this, parseResult.OutCodecInfo.Codec, token),
            token: token);

        if (result.ProtocolResult.Data.Length > 1)
            throw new ResultCardinalityMismatchException(Cardinality.AtMostOne, Cardinality.Many);

        var codec = await GetPreheatedCodecAsync<TResult>(result, token);

        return result.ProtocolResult.Data.Length == 0
            ? default
            : ObjectBuilder.BuildResult<TResult>(this, codec, result.ProtocolResult.Data[0]);
    }

    /// <inheritdoc />
    /// <exception cref="EdgeDBException">A general error occored.</exception>
    /// <exception cref="EdgeDBErrorException">The client received an <see cref="IProtocolError" />.</exception>
    /// <exception cref="UnexpectedMessageException">The client received an unexpected message.</exception>
    /// <exception cref="MissingCodecException">A codec could not be found for the given input arguments or the result.</exception>
    /// <exception cref="ResultCardinalityMismatchException">The results cardinality was not what the query expected.</exception>
    /// <exception cref="MissingRequiredException">The query didn't return a result.</exception>
    /// <exception cref="InvalidOperationException">Target type doesn't match received type.</exception>
    /// <exception cref="TargetInvocationException">Cannot construct a <typeparamref name="TResult" />.</exception>
    public override async Task<TResult> QueryRequiredSingleAsync<TResult>(string query,
        IDictionary<string, object?>? args = null,
        Capabilities? capabilities = Capabilities.Modifications, CancellationToken token = default)
    {
        var implicitTypeName = TypeBuilder.TryGetTypeDeserializerInfo(typeof(TResult), out var info) &&
                               info.RequiresTypeName;

        var result = await ExecuteInternalAsync(
            query,
            args,
            Cardinality.AtMostOne,
            capabilities,
            implicitTypeName: implicitTypeName,
            preheat: parseResult => ObjectBuilder.PreheatCodecAsync<TResult>(this, parseResult.OutCodecInfo.Codec, token),
            token: token);

        if (result.ProtocolResult.Data.Length is > 1 or 0)
            throw new ResultCardinalityMismatchException(Cardinality.One,
                result.ProtocolResult.Data.Length > 1 ? Cardinality.Many : Cardinality.AtMostOne);


        return result.ProtocolResult.Data.Length != 1
            ? throw new MissingRequiredException()
            : ObjectBuilder.BuildResult<TResult>(this, await GetPreheatedCodecAsync<TResult>(result, token),
                result.ProtocolResult.Data[0])!;
    }

    /// <inheritdoc />
    /// <exception cref="ResultCardinalityMismatchException">The results cardinality was not what the query expected.</exception>
    /// <exception cref="EdgeDBException">A general error occored.</exception>
    /// <exception cref="EdgeDBErrorException">The client received an <see cref="IProtocolError" />.</exception>
    /// <exception cref="UnexpectedMessageException">The client received an unexpected message.</exception>
    /// <exception cref="MissingCodecException">A codec could not be found for the given input arguments or the result.</exception>
    public override async Task<Json> QueryJsonAsync(string query, IDictionary<string, object?>? args = null,
        Capabilities? capabilities = Capabilities.Modifications, CancellationToken token = default)
    {
        var result =
            await ExecuteInternalAsync(query, args, Cardinality.Many, capabilities, IOFormat.Json, token: token);

        return result.ProtocolResult.Data.Length == 1
            ? (string)result.ProtocolResult.OutCodecInfo.Codec.Deserialize(this, in result.ProtocolResult.Data[0])!
            : "[]";
    }

    /// <inheritdoc />
    /// <exception cref="EdgeDBException">A general error occored.</exception>
    /// <exception cref="EdgeDBErrorException">The client received an <see cref="IProtocolError" />.</exception>
    /// <exception cref="UnexpectedMessageException">The client received an unexpected message.</exception>
    /// <exception cref="MissingCodecException">A codec could not be found for the given input arguments or the result.</exception>
    public override async Task<IReadOnlyCollection<Json>> QueryJsonElementsAsync(string query,
        IDictionary<string, object?>? args = null, Capabilities? capabilities = Capabilities.Modifications,
        CancellationToken token = default)
    {
        var result = await ExecuteInternalAsync(query, args, Cardinality.Many, capabilities, IOFormat.JsonElements,
            token: token);

        return result.ProtocolResult.Data.Any()
            ? result.ProtocolResult.Data.Select(x => new Json((string?)result.ProtocolResult.OutCodecInfo.Codec.Deserialize(this, in x)))
                .ToImmutableArray()
            : ImmutableArray<Json>.Empty;
    }

    #endregion

    #region Helper functions

    internal ValueTask OnLogAsync(MessageSeverity severity, ServerErrorCodes code, string message)
    {
        switch (severity)
        {
            case MessageSeverity.Warning:
                Logger.LogWarning("[SERVER: {@Code}]: {@Message}", code, message);
                break;
            case MessageSeverity.Debug:
                Logger.LogDebug("[SERVER: {@Code}]: {@Message}", code, message);
                break;
            case MessageSeverity.Info:
                Logger.LogInformation("[SERVER: {@Code}]: {@Message}", code, message);
                break;
            case MessageSeverity.Notice:
                Logger.LogInformation("[SERVER NOTICE: {@Code}]: {@Message}", code, message);
                break;
        }

        return ValueTask.CompletedTask;
    }

    internal bool TryNegotiateProtocol(in ushort major, in ushort minor)
    {
        Logger.BeginProtocolNegotiation(_protocolProvider.Version, (major, minor));

        if (IProtocolProvider.Providers.TryGetValue((major, minor), out var provider))
        {
            _protocolProvider = provider.Factory(this);
            IProtocolProvider.UpdateProviderFor(this, _protocolProvider);
            return true;
        }

        return false;
    }

    internal void CancelReadyState()
    {
        if (!_readyCancelTokenSource.IsCancellationRequested)
            _readyCancelTokenSource.Cancel();
    }

    protected void TriggerReady() => _readySource.TrySetResult();

    internal ReadOnlyMemory<byte>? SerializeState()
    {
        // TODO: version check this, prevent the state codec
        // from being walked again if the state data hasn't
        // been updated.

        if (_stateCodec is null)
            return null;

        var data = Session.Serialize();

        return _stateCodec.Serialize(this, data);
    }

    internal void UpdateStateCodec(ICodec codec, in Guid stateCodecId)
    {
        _stateCodec = codec;
        _stateDescriptorId = stateCodecId;
    }

    #endregion

    #region Connect/disconnect

    /// <summary>
    ///     Connects and authenticates this client.
    /// </summary>
    /// <remarks>
    ///     This task waits for the underlying connection to receive a
    ///     ready message indicating the client
    ///     can start to preform queries.
    /// </remarks>
    /// <inheritdoc />
    public override async ValueTask ConnectAsync(CancellationToken token = default)
    {
        await _connectSemaphone.WaitAsync(token).ConfigureAwait(false);

        if (IsConnected)
            return;

        var released = false;

        try
        {
            await ConnectInternalAsync(token: token);

            using var linkedToken =
                CancellationTokenSource.CreateLinkedTokenSource(token, _readyCancelTokenSource.Token);

            token.ThrowIfCancellationRequested();

            // run a message loop until the client is ready for commands
            while (!linkedToken.IsCancellationRequested)
            {
                var message = await Duplexer.ReadNextAsync(linkedToken.Token).ConfigureAwait(false) ??
                              throw new UnexpectedDisconnectException();

                try
                {
                    await _protocolProvider.ProcessAsync(in message);
                }
                catch (EdgeDBErrorException x) when (x.ShouldReconnect)
                {
                    if (ClientConfig.RetryMode is ConnectionRetryMode.AlwaysRetry)
                    {
                        if (_currentRetries < ClientConfig.MaxConnectionRetries)
                        {
                            _currentRetries++;

                            Logger.AttemptToReconnect(_currentRetries, ClientConfig.MaxConnectionRetries, x);

                            // do not forward the linked token in this method to the new
                            // reconnection, only supply the external token. We also don't
                            // want to call 'ReconnectAsync' since we queue up a disconnect
                            // and connect request, if this method was called externally
                            // while we handle the error, it would be next in line to attempt
                            // to connect, if that external call completes we would then disconnect
                            // and connect after a successful connection attempt which wouldn't be ideal.
                            await DisconnectAsync(token);

                            _connectSemaphone.Release();

                            await ConnectAsync(token);
                            return;
                        }
                        else
                            Logger.MaxConnectionRetries(ClientConfig.MaxConnectionRetries, x);
                    }

                    throw;
                }

                if (_protocolProvider.Phase is ProtocolPhase.Command)
                {
                    // reset connection attempts
                    _currentRetries = 0;
                    break;
                }
            }

            _readySource.SetResult();

            // call base to notify listeners that we connected.
            await base.ConnectAsync(token);
        }
        finally
        {
            if (!released)
                _connectSemaphone.Release();
        }
    }

    private async Task ConnectInternalAsync(int attempts = 0, CancellationToken token = default)
    {
        try
        {
            if (IsConnected)
                return;

            _readyCancelTokenSource = new CancellationTokenSource();
            _readySource = new TaskCompletionSource();

            Duplexer.Reset();

            Stream? stream;

            try
            {
                stream = await GetStreamAsync(token).ConfigureAwait(false);
            }
            catch (EdgeDBException x) when (x.ShouldReconnect)
            {
                attempts++;
                Logger.AttemptToReconnect((uint)attempts, ClientConfig.MaxConnectionRetries);
                if (attempts == ClientConfig.MaxConnectionRetries)
                    throw new ConnectionFailedException(attempts);
                await ConnectInternalAsync(attempts, token);
                return;
            }

            if (Duplexer is StreamDuplexer streamDuplexer)
                streamDuplexer.Init(stream);


            // send handshake
            await Duplexer.SendAsync(token, _protocolProvider.Handshake()).ConfigureAwait(false);
        }
        catch (EdgeDBException x) when (x.ShouldReconnect)
        {
            if (_currentRetries == ClientConfig.MaxConnectionRetries)
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
    /// <remarks />
    /// <!-- clears the remarks about calling base -->
    /// <inheritdoc />
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
    ///     Closes the stream returned from <see cref="GetStreamAsync" />.
    /// </summary>
    /// <param name="token">A cancellation token used to cancel the asynchronous operation.</param>
    /// <returns>
    ///     A task that represents the asynchronous closing operation of
    ///     the stream.
    /// </returns>
    protected abstract ValueTask CloseStreamAsync(CancellationToken token = default);

    #endregion
}
