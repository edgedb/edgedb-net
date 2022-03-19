using EdgeDB.Codecs;
using EdgeDB.Dumps;
using EdgeDB.Models;
using EdgeDB.Utils;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Collections.Immutable;
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
    public sealed class EdgeDBTcpClient : IAsyncDisposable
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

        private readonly AsyncEvent<Func<Task>> _onDisconnect = new();
        private readonly AsyncEvent<Func<IReceiveable, Task>> _onMessage = new();
        private readonly AsyncEvent<Func<ExecuteResult, Task>> _queryExecuted = new();

        private NetworkStream? _stream;
        private SslStream? _secureStream;
        private CancellationTokenSource _disconnectCancelToken;
        private readonly TcpClient _client;
        private readonly EdgeDBConnection _connection;
        private readonly TaskCompletionSource _readySource;
        private readonly CancellationTokenSource _readyCancelTokenSource;
        private readonly SemaphoreSlim _sephamore;
        private readonly EdgeDBConfig _config;

        private ulong _currentMessageId;
        private readonly ConcurrentStack<IReceiveable> _messageStack;
        private int _maxMessageStackSize = 25;

        /// <summary>
        ///     Creates a new TCP client with the provided conection and config.
        /// </summary>
        /// <param name="connection">The connection details used to connect to the database.</param>
        /// <param name="config">The configuration for this client.</param>
        public EdgeDBTcpClient(EdgeDBConnection connection, EdgeDBConfig config)
        {
            _config = config;
            Logger = config.Logger ?? Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
            _sephamore = new(1,1);
            _disconnectCancelToken = new();
            _connection = connection;
            _client = new();
            _readySource = new();
            _readyCancelTokenSource = new();
            _messageStack = new();
            ServerKey = new byte[32];
            ServerConfig = new Dictionary<string, object?>();
        }

        public async Task<Stream?> DumpDatabaseAsync()
        {
            await _sephamore.WaitAsync(_disconnectCancelToken.Token).ConfigureAwait(false);

            IsIdle = false;

            try
            {
                await SendMessageAsync(new Dump());
                await SendMessageAsync(new Sync());

                var dump = await WaitForMessageOrError(ServerMessageType.DumpHeader, timeout: TimeSpan.FromSeconds(15));

                if (dump is ErrorResponse err)
                    throw new EdgeDBErrorException(err);

                if (dump is not DumpHeader dumpHeader)
                    throw new EdgeDBException($"Expected dump header but got {dump.Type}");

                var stream = new MemoryStream();
                var writer = new DumpWriter(stream);

                writer.WriteDumpHeader(dumpHeader);

                bool complete = false;

                while (!complete)
                {
                    var msg = await NextMessageAsync(x => true, timeout: TimeSpan.FromSeconds(15));
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
                            throw new EdgeDBException($"Got unexpected message type when dumping: {msg.Type}");

                    }
                }
                stream.Position = 0;
                return stream;
            }
            catch (Exception x) when (x is OperationCanceledException or TaskCanceledException)
            {
                throw new EdgeDBException("Database dump timed out", x);
            }
            finally
            {
                IsIdle = true;
                _sephamore.Release();
            }
        }

        /// <summary>
        ///     Executes a query and returns the result.
        /// </summary>
        /// <param name="query">The query string to execute.</param>
        /// <param name="arguments">Any arguments used in the query.</param>
        /// <param name="cardinality">The optional cardinality of the query.</param>
        /// <returns>
        ///     An execute result containing the result of the query.
        /// </returns>
        /// <exception cref="EdgeDBErrorException">An error occured within the query and the database returned an error result.</exception>
        /// <exception cref="EdgeDBException">An error occored when reading, writing, or parsing the results.</exception>
        public Task<object?> ExecuteAsync(string query, IDictionary<string, object?>? arguments = null, Cardinality? card = null)
            => ExecuteAsync<object>(query, arguments, card);

        /// <summary>
        ///     Executes a query and returns the result.
        /// </summary>
        /// <typeparam name="TResult">The return type of the query.</typeparam>
        /// <param name="query">The query string to execute.</param>
        /// <param name="arguments">Any arguments used in the query.</param>
        /// <param name="cardinality">The optional cardinality of the query.</param>
        /// <returns>
        ///     The result of the query operation as <typeparamref name="TResult"/>.
        /// </returns>
        /// <exception cref="EdgeDBErrorException">An error occured within the query and the database returned an error result.</exception>
        /// <exception cref="EdgeDBException">An error occored when reading, writing, or parsing the results.</exception>
        public async Task<TResult?> ExecuteAsync<TResult>(string query, IDictionary<string, object?>? arguments = null, Cardinality? card = null)
        {
            await _sephamore.WaitAsync(_disconnectCancelToken.Token).ConfigureAwait(false);

            IsIdle = false;

            try
            {
                var prepareResult = await PrepareAsync(new Prepare
                {
                    Capabilities = AllowCapabilities.ReadOnly, // TODO: change this
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
                    throw new EdgeDBException($"Got unexpected result from prepare: {prepareResult.Type}");
                }

                card ??= result.Cardinality;

                // get the codec for the return type
                var outCodec = PacketSerializer.GetCodec(result.OutputTypedescId);
                var inCodec = PacketSerializer.GetCodec(result.InputTypedescId);
                CommandDataDescription? describer = null;

                // if its not cached or we dont have a default one for it, ask the server to describe it for us
                if (outCodec == null || inCodec == null)
                {
                    await SendMessageAsync(new DescribeStatement()).ConfigureAwait(false);
                    await SendMessageAsync(new Sync()).ConfigureAwait(false);
                    describer = await WaitForMessageAsync<CommandDataDescription>(ServerMessageType.CommandDataDescription).ConfigureAwait(false);

                    if (!describer.HasValue)
                        throw new EdgeDBException("Failed to get a descriptor");

                    using (var innerReader = new PacketReader(describer.Value.OutputTypeDescriptor.ToArray()))
                    {
                        outCodec ??= PacketSerializer.BuildCodec(describer.Value.OutputTypeDescriptorId, innerReader);
                    }

                    using (var innerReader = new PacketReader(describer.Value.InputTypeDescriptor.ToArray()))
                    {
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

                // convert our arguments if any
                await SendMessageAsync(new Execute() { Capabilities = result.Capabilities, Arguments = argumentCodec.SerializeArguments(arguments) }).ConfigureAwait(false);
                await SendMessageAsync(new Sync()).ConfigureAwait(false);

                List<Data> receivedData = new();

                Task addData(IReceiveable msg)
                {
                    if (msg.Type == ServerMessageType.Data)
                        receivedData.Add((Data)msg);

                    return Task.CompletedTask;
                }

                OnMessage += addData;

                // wait for complete or error
                var completionTask = WaitForMessageAsync<CommandComplete>(ServerMessageType.CommandComplete);
                var errorTask = WaitForMessageAsync<ErrorResponse>(ServerMessageType.ErrorResponse);
                var executionResult = await Task.WhenAny(
                    completionTask,
                    errorTask).ConfigureAwait(false);

                OnMessage -= addData;

                object? queryResult = null;

                if (receivedData.Any()) // TODO: optimize?
                {
                    if (receivedData.Count == 1 && (card == Cardinality.AtMostOne || card == Cardinality.One))
                    {
                        using (var reader = new PacketReader(receivedData[0].PayloadData.ToArray()))
                        {
                            queryResult = outCodec.Deserialize(reader);
                        }
                    }
                    else
                    {
                        object?[] data = new object[receivedData.Count];

                        for (int i = 0; i != data.Length; i++)
                        {
                            using (var reader = new PacketReader(receivedData[i].PayloadData.ToArray()))
                            {
                                data[i] = outCodec.Deserialize(reader);
                            }
                        }

                        queryResult = data;
                    }
                }

                CommandComplete? completeResult = completionTask.IsCompleted ? completionTask.Result : null;
                ErrorResponse? errorResult = errorTask.IsCompleted ? errorTask.Result : null;

                if (completeResult.HasValue)
                {
                    Logger.LogInformation("Executed query with {}: {}", completeResult.Value.Status, completeResult.Value.UsedCapabilities);
                    return ObjectBuilder.BuildResult<TResult>(result.OutputTypedescId, queryResult);
                }
                else if (errorResult.HasValue)
                {
                    throw new EdgeDBErrorException(errorResult.Value);
                }
                else throw new EdgeDBException("Didn't receive a result or error response trying to execute a query.");
            }
            catch (Exception x)
            {
                Logger.LogWarning("Failed to execute: {}", x);
                throw new EdgeDBException($"Failed to execute query: {x}", x);
            }
            finally
            {
                IsIdle = true;
                _sephamore.Release();
            }
        }

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

                    var result = _secureStream.Read(typeBuf);

                    if (result == 0)
                    {
                        // disconnected
                        _disconnectCancelToken.Cancel();
                        _readyCancelTokenSource.Cancel();
                        await _onDisconnect.InvokeAsync().ConfigureAwait(false);
                        return;
                    }

                    var msg = PacketSerializer.DeserializePacket((ServerMessageType)typeBuf[0], _secureStream, this);

                    if (msg != null)
                    {
                        msg.Id = _currentMessageId;
                        _messageStack.Push(msg.Clone());

                        Interlocked.Increment(ref _currentMessageId);

                        while (_messageStack.Count > _maxMessageStackSize && _messageStack.TryPop(out var _))
                        { }

                        await HandlePayloadAsync(msg).ConfigureAwait(false);
                    }
                }
            }
            catch (Exception x)
            {
                Logger.LogCritical("Failed to read and deserialize packet: {}", x);
            }
        }

        private async Task HandlePayloadAsync(IReceiveable payload)
        {
            Logger.LogDebug("Got message type {}", payload.Type);

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
                case AuthenticationStatus authStatus when authStatus.AuthStatus == AuthStatus.AuthenticationRequiredSASLMessage:
                    _ = Task.Run(async () => await StartSASLAuthenticationAsync(authStatus).ConfigureAwait(false));
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

            try
            {
                await _onMessage.InvokeAsync(payload).ConfigureAwait(false);
            }
            catch (Exception x)
            {
                Logger.LogCritical("Exception in message handler: {}", x);
            }
        }

        private async Task<IReceiveable> PrepareAsync(Prepare packet)
        {
            await SendMessageAsync(packet).ConfigureAwait(false);
            await SendMessageAsync(new Sync()).ConfigureAwait(false);
            var timeoutTask = Task.Delay(15000);
            var resultTask = WaitForMessageAsync<PrepareComplete>(ServerMessageType.PrepareComplete);
            var errorTask = WaitForMessageAsync<ErrorResponse>(ServerMessageType.ErrorResponse);

            var result = await Task.WhenAny(resultTask, errorTask, timeoutTask).ConfigureAwait(false);

            if (result == timeoutTask)
                throw new TimeoutException("Didn't receive PrepareComplete result after 15 seconds");

            if (resultTask.IsCompleted)
                return await resultTask;
            else return await errorTask;
        }

        private void ParseServerSettings(ParameterStatus status)
        {
            switch (status.Name)
            {
                case "suggested_pool_concurrency":
                    SuggestedPoolConcurrency = int.Parse(Encoding.UTF8.GetString(status.Value.ToArray()));
                    break;

                case "system_config":
                    using(var reader = new PacketReader(status.Value.ToArray()))
                    {
                        var length = reader.ReadInt32() - 16;
                        var descriptorId = reader.ReadGuid();
                        var typeDesc = reader.ReadBytes(length);

                        ICodec? codec = PacketSerializer.GetCodec(descriptorId);

                        if(codec == null)
                        {
                            using (var innerReader = new PacketReader(typeDesc))
                            {
                                codec = PacketSerializer.BuildCodec(descriptorId, innerReader);

                                if (codec == null)
                                    throw new Exception("Failed to build codec for system config");
                            }
                        }

                        // disard length
                        reader.ReadUInt32();

                        ServerConfig = ImmutableDictionary.CreateRange((codec.Deserialize(reader) as IDictionary<string, object?>)!);
                    }
                    break;
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

            Logger.LogDebug("Sent message type {}", message.Type);
        }

        private async Task StartSASLAuthenticationAsync(AuthenticationStatus authStatus)
        {
            // steal the sephamore to stop any query attempts.
            await _sephamore.WaitAsync().ConfigureAwait(false);

            try
            {
                using (var scram = new Scram())
                {
                    var method = authStatus.AuthenticationMethods[0];

                    if (method != "SCRAM-SHA-256")
                    {
                        // TODO: add converter for string methods?
                        throw new NotSupportedException("The only supported method is SCRAM-SHA-256");
                    }

                    var initialMsg = scram.BuildInitialMessage(_connection.Username!, method);

                    await SendMessageAsync(initialMsg).ConfigureAwait(false);

                    // wait for continue or timeout
                    var initialResult = await WaitForMessageOrError(ServerMessageType.Authentication, from: authStatus, timeout: TimeSpan.FromSeconds(15));

                    if (initialResult is ErrorResponse err)
                        throw new EdgeDBErrorException(err);

                    if (initialResult is not AuthenticationStatus intiailStatus)
                        throw new EdgeDBException($"Unexpected message type {initialResult.Type}");

                    // check the continue
                    var final = scram.BuildFinalMessage(intiailStatus, _connection.Password!);

                    await SendMessageAsync(final.FinalMessage);

                    var finalResult = await WaitForMessageOrError(ServerMessageType.Authentication, from: initialResult, timeout: TimeSpan.FromSeconds(15));

                    if (finalResult is ErrorResponse error)
                        throw new EdgeDBErrorException(error);

                    if (finalResult is not AuthenticationStatus finalStatus)
                        throw new EdgeDBException($"Unexpected message type {finalResult.Type}");

                    var key = scram.ParseServerFinalMessage(finalStatus);

                    if (!key.SequenceEqual(final.ExpectedSig))
                    {
                        Logger.LogWarning("Received signature didn't match expected scram signature");
                        await DisconnectAsync();
                    }
                }
            }
            catch(Exception x)
            {
                Logger.LogError(x, "Failed to complete authentication");
                await DisconnectAsync();
            }
            finally
            {
                _sephamore.Release();
            }
        }

        /// <summary>
        ///     Connects and authenticates this client.
        /// </summary>
        public async Task ConnectAsync()
        {
            _disconnectCancelToken = new();

            await _client.ConnectAsync(_connection.Hostname!, _connection.Port).ConfigureAwait(false);

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

            _ = Task.Run(async () => await ReceiveAsync().ConfigureAwait(false));

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

            // wait for auth promise
            await Task.Run(() => _readySource.Task, _readyCancelTokenSource.Token);
        }

        private Task<IReceiveable> WaitForMessageOrError(ServerMessageType type, IReceiveable? from = null, CancellationToken? token = null, TimeSpan? timeout = null)
            => NextMessageAsync(x => x.Type == type || x.Type == ServerMessageType.ErrorResponse, from, timeout, token: token);

        private async Task<TMessage?> WaitForMessageAsync<TMessage>(ServerMessageType type, IReceiveable? from = null, CancellationToken? token = null, TimeSpan? timeout = null) where TMessage : IReceiveable
            => (TMessage)(await NextMessageAsync(x => x.Type == type, from, timeout, token: token));

        private async Task<IReceiveable> NextMessageAsync(Predicate<IReceiveable> predicate, IReceiveable? from = null, TimeSpan? timeout = null, CancellationToken? token = null)
        {
            var fromId = from != null ? from.Id + 1 : _currentMessageId;

            var tcs = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            IReceiveable? returnValue = default;
            Func<IReceiveable, Task> handler = (msg) =>
            {
                if (predicate(msg))
                {
                    returnValue = msg;
                    tcs.TrySetResult();
                }

                return Task.CompletedTask;
            };

            var cancelSource = new CancellationTokenSource();

            if (token.HasValue)
                token.Value.Register(() => cancelSource.Cancel());

            _disconnectCancelToken.Token.Register(() => cancelSource.Cancel());

            if (timeout.HasValue)
                cancelSource.CancelAfter(timeout.Value);

            cancelSource.Token.Register(() => tcs.TrySetCanceled());

            foreach (var msg in _messageStack.Where(x => x.Id >= fromId))
                await handler(msg);

            if (!tcs.Task.IsCompleted)
            {
                OnMessage += handler;
                await Task.Run(() => tcs.Task, cancelSource.Token).ConfigureAwait(false);
                OnMessage -= handler;
            }

            return returnValue!;
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
            if (!_config.RequireCertificateMatch)
                return true;

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

        /// <summary>
        ///     Disconnects and disposes the client.
        /// </summary>
        public async ValueTask DisposeAsync()
        {
            try { await DisconnectAsync(); } catch { }
            _client.Dispose();
            await (_stream?.DisposeAsync() ?? ValueTask.CompletedTask);
            await (_secureStream?.DisposeAsync() ?? ValueTask.CompletedTask);
        }
    }
}