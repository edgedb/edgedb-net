using EdgeDB.Codecs;
using EdgeDB.Models;
using EdgeDB.Utils;
using Microsoft.Extensions.Logging;
using System.Collections.Immutable;
using System.Dynamic;
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
        internal TcpClient TcpClient;
        internal Dictionary<string, object?> RawServerConfig = new();
        internal readonly ClientPacketDuplexer Duplexer;
        internal readonly TimeSpan MessageTimeout;

        internal ulong ClientId { get; }

        private readonly AsyncEvent<Func<Task>> _onDisconnect = new();
        private readonly AsyncEvent<Func<EdgeDBTcpClient, Task<bool>>> _onDisposed = new();
        private readonly AsyncEvent<Func<IReceiveable, Task>> _onMessage = new();
        private readonly AsyncEvent<Func<ExecuteResult, Task>> _queryExecuted = new();

        private NetworkStream? _stream;
        private SslStream? _secureStream;
        private CancellationToken DisconnectCancelToken
            => Duplexer.DisconnectToken;
        private readonly TaskCompletionSource _readySource;
        private readonly CancellationTokenSource _readyCancelTokenSource;
        private TaskCompletionSource _authCompleteSource;
        private readonly EdgeDBConnection _connection;
        private readonly SemaphoreSlim _semaphore;
        private readonly SemaphoreSlim _commandSemaphore;
        private readonly EdgeDBConfig _config;
        private readonly TimeSpan _connectionTimeout;
        private uint _currentRetries;

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
            _commandSemaphore = new(1, 1);
            _connection = connection;
            TcpClient = new();
            _readySource = new();
            _readyCancelTokenSource = new();
            _authCompleteSource = new();
            ServerKey = new byte[32];
            MessageTimeout = TimeSpan.FromMilliseconds(config.MessageTimeout);
            _connectionTimeout = TimeSpan.FromMilliseconds(config.ConnectionTimeout);
            Duplexer = new ClientPacketDuplexer(this);
        }

        public EdgeDBTcpClient(EdgeDBConnection connection, EdgeDBConfig config, ulong clientId) : this(connection, config)
        {
            ClientId = clientId;
        }

        #region Commands/queries

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
                if (outCodec == null || inCodec == null)
                {
                    describer = (CommandDataDescription)await Duplexer.DuplexAndSyncAsync(new DescribeStatement(), x => x.Type == ServerMessageType.CommandDataDescription);

                    if (outCodec == null)
                    {
                        using var innerReader = new PacketReader(describer.Value.OutputTypeDescriptor.ToArray());
                        outCodec ??= PacketSerializer.BuildCodec(describer.Value.OutputTypeDescriptorId, innerReader);
                    }

                    if (inCodec == null)
                    {
                        using var innerReader = new PacketReader(describer.Value.InputTypeDescriptor.ToArray());
                        inCodec ??= PacketSerializer.BuildCodec(describer.Value.InputTypeDescriptorId, innerReader);
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
                            throw new UnexpectedMessageException(msg.Type);
                    }
                    return false;
                }

                readyTask = Duplexer.NextAsync(x => x.Type == ServerMessageType.ReadyForCommand);

                var completePacket = (CommandComplete)await Duplexer.DuplexAndSyncAsync(new Execute() { Capabilities = result.Capabilities, Arguments = argumentCodec.SerializeArguments(args) }, handler);

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
        public async Task ExecuteAsync(string query, IDictionary<string, object?>? args = null)
            => await ExecuteInternalAsync(query, args, Cardinality.Many).ConfigureAwait(false);

        public async Task<IReadOnlyCollection<TResult?>> QueryAsync<TResult>(string query, IDictionary<string, object?>? args = null)
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

        public async Task<TResult?> QuerySingleAsync<TResult>(string query, IDictionary<string, object?>? args = null)
        {
            var result = await ExecuteInternalAsync(query, args, Cardinality.AtMostOne);

            if (result.Data.Count > 1)
                throw new ResultCardinalityMismatchException(Cardinality.AtMostOne, Cardinality.Many);

            var queryResult = result.Data.FirstOrDefault();

            return queryResult.PayloadData == null
                ? default
                : ObjectBuilder.BuildResult<TResult>(result.PrepareStatement.OutputTypedescId, result.Deserializer.Deserialize(queryResult.PayloadData.ToArray()));
        }

        public async Task<TResult> QueryRequiredSingleAsync<TResult>(string query, IDictionary<string, object?>? args = null)
        {
            var result = await ExecuteInternalAsync(query, args, Cardinality.AtMostOne);

            if (result.Data.Count is > 1 or 0)
                throw new ResultCardinalityMismatchException(Cardinality.One, result.Data.Count > 1 ? Cardinality.Many : Cardinality.AtMostOne);

            var queryResult = result.Data.FirstOrDefault();

            return queryResult.PayloadData == null
                ? throw new MissingRequiredException()
                : ObjectBuilder.BuildResult<TResult>(result.PrepareStatement.OutputTypedescId, result.Deserializer.Deserialize(queryResult.PayloadData.ToArray()))!;
        }

        #endregion

        #region Receive/Send packet functions

        private async Task HandlePayloadAsync(IReceiveable payload)
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

                    var initialResult = await Duplexer.DuplexAsync(x => x.Type == ServerMessageType.Authentication, packets: initialMsg).ConfigureAwait(false);

                    if (initialResult is ErrorResponse err)
                        throw new EdgeDBErrorException(err);

                    if (initialResult is not AuthenticationStatus intiailStatus)
                        throw new UnexpectedMessageException(ServerMessageType.Authentication, initialResult.Type);

                    // check the continue
                    var (FinalMessage, ExpectedSig) = scram.BuildFinalMessage(intiailStatus, _connection.Password!);

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
            }
            catch (Exception x)
            {
                if (_config.RetryMode == ConnectionRetryMode.AlwaysRetry)
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

        private async Task<IReceiveable> PrepareAsync(Prepare packet)
        {
            var result = await Duplexer.DuplexAndSyncAsync(packet, x => x.Type == ServerMessageType.PrepareComplete);

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
        public async Task ConnectAsync()
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
                TcpClient = new TcpClient();

                var timeoutToken = CancellationTokenSource.CreateLinkedTokenSource(DisconnectCancelToken);

                timeoutToken.CancelAfter(_connectionTimeout);

                try
                {
                    await TcpClient.ConnectAsync(_connection.Hostname!, _connection.Port, timeoutToken.Token).ConfigureAwait(false);
                }
                catch (OperationCanceledException x) when (timeoutToken.IsCancellationRequested)
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

                _stream = TcpClient.GetStream();

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

                Duplexer.Start(_secureStream);

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

        /// <summary>
        ///     Disconnects this client from the database.
        /// </summary>
        public async Task DisconnectAsync()
        {
            await Duplexer.DisconnectAsync().ConfigureAwait(false);
            TcpClient.Close();
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

                X509Chain chain2 = new();
                chain2.ChainPolicy.ExtraStore.Add(cert);
                chain2.ChainPolicy.VerificationFlags = X509VerificationFlags.AllowUnknownCertificateAuthority;
                chain2.ChainPolicy.RevocationMode = X509RevocationMode.NoCheck;

                bool isValid = chain2.Build(new X509Certificate2(certificate!));
                var chainRoot = chain2.ChainElements[^1].Certificate;
                isValid = isValid && chainRoot.RawData.SequenceEqual(cert.RawData);

                return isValid;
            }
            else
            {
                return sslPolicyErrors == SslPolicyErrors.None;
            }
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

        #region Client pool dispose

        public async ValueTask DisposeAsync()
        {
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