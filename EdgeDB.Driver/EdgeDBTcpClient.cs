using EdgeDB.Codecs;
using EdgeDB.Models;
using EdgeDB.Utils;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace EdgeDB 
{
    public sealed class EdgeDBTcpClient : IAsyncDisposable
    {
        public event Func<IReceiveable, Task> OnMessage
        {
            add => _onMessage.Add(value);
            remove => _onMessage.Remove(value);
        }

        public event Func<Task> OnDisconnect
        {
            add => _onDisconnect.Add(value);
            remove => _onDisconnect.Remove(value);
        }

        private AsyncEvent<Func<Task>> _onDisconnect = new();
        private AsyncEvent<Func<IReceiveable, Task>> _onMessage = new();

        // server config
        internal byte[] ServerKey;
        internal int SuggestedPoolConcurrency;
        internal IDictionary<string, object?> ServerConfig;

        internal bool IsIdle { get; private set; }
        internal ILogger Logger;

        private readonly TcpClient _client;
        private NetworkStream? _stream;
        private SslStream? _secureStream;
        private readonly EdgeDBConnection _connection;
        private CancellationTokenSource _disconnectCancelToken;
        private TaskCompletionSource<AuthenticationStatus> _authenticationStatusSource;
        private TaskCompletionSource _readySource;
        private CancellationTokenSource _readyCancelTokenSource;
        private SemaphoreSlim _sephamore;

        // TODO: config?
        public EdgeDBTcpClient(EdgeDBConnection connection, ILogger? logger = null)
        {
            Logger = logger ?? Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
            _sephamore = new(1,1);
            _disconnectCancelToken = new();
            _connection = connection;
            _client = new();
            _authenticationStatusSource = new();
            _readySource = new();
            _readyCancelTokenSource = new();
            ServerKey = new byte[32];
            ServerConfig = new Dictionary<string, object?>();
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

                    if(result == 0)
                    {
                        // disconnected
                        _disconnectCancelToken.Cancel();
                        _readyCancelTokenSource.Cancel();
                        await _onDisconnect.InvokeAsync().ConfigureAwait(false);
                        return;
                    }

                    var msg = PacketSerializer.DeserializePacket((ServerMessageTypes)typeBuf[0], _secureStream, this);

                    if (msg != null)
                        await HandlePayloadAsync(msg).ConfigureAwait(false);
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
                case AuthenticationStatus authStatus:
                    {
                        switch (authStatus.AuthStatus)
                        {
                            case AuthStatus.AuthenticationRequiredSASLMessage:
                                {
                                    _ = Task.Run(async () => await StartSASLAuthenticationAsync(authStatus).ConfigureAwait(false));
                                }
                                break;
                            case AuthStatus.AuthenticationSASLContinue or AuthStatus.AuthenticationSASLFinal:
                                {
                                    _authenticationStatusSource.SetResult(authStatus);
                                    _authenticationStatusSource = new();
                                }
                                break;
                            case AuthStatus.AuthenticationOK:
                                {
                                    _authenticationStatusSource.TrySetResult(authStatus);
                                }
                                break;
                            default:
                                throw new InvalidDataException($"Got unknown auth status: {authStatus.AuthStatus:X}");
                        }
                    }
                    break;
                case ServerKeyData keyData:
                    {
                        ServerKey = keyData.Key;
                    }
                    break;
                case ParameterStatus parameterStatus:
                    ParseServerSettings(parameterStatus);
                    break;
                case ReadyForCommand cmd:
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
            var resultTask = WaitForMessageAsync<PrepareComplete>(ServerMessageTypes.PrepareComplete);
            var errorTask = WaitForMessageAsync<ErrorResponse>(ServerMessageTypes.ErrorResponse);

            var result = await Task.WhenAny(resultTask, errorTask, timeoutTask).ConfigureAwait(false);

            if (result == timeoutTask)
                throw new TimeoutException("Didn't receive PrepareComplete result after 15 seconds");

            if (resultTask.IsCompleted)
                return await resultTask;
            else return await errorTask;
        }

        public async Task<ExecuteResult> ExecuteAsync(string query, IDictionary<string, object?>? arguments = null, Cardinality card = Cardinality.Many)
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
                    ExpectedCardinality = card
                }).ConfigureAwait(false);

                if(prepareResult is ErrorResponse error)
                {
                    return new ExecuteResult
                    {
                        Error = error,
                        IsSuccess = false,
                        ExecutedQuery = query + $"\nParameters: {(arguments == null ? "none" : $"[{string.Join(", ", arguments.Select(x => $"{x.Key}: {x.Value}"))}]")}"
                    };
                }

                if(prepareResult is not PrepareComplete result)
                {
                    throw new InvalidDataException($"Got unexpected result from prepare: {prepareResult.Type}");
                }

                // get the codec for the return type
                var outCodec = PacketSerializer.GetCodec(result.OutputTypedescId);
                var inCodec = PacketSerializer.GetCodec(result.InputTypedescId);

                // if its not cached or we dont have a default one for it, ask the server to describe it for us
                if (outCodec == null || inCodec == null)
                {
                    await SendMessageAsync(new DescribeStatement()).ConfigureAwait(false);
                    await SendMessageAsync(new Sync()).ConfigureAwait(false);
                    var describer = await WaitForMessageAsync<CommandDataDescription>(ServerMessageTypes.CommandDataDescription).ConfigureAwait(false);

                    using (var innerReader = new PacketReader(describer.OutputTypeDescriptor))
                    {
                        outCodec ??= PacketSerializer.BuildCodec(describer.OutputTypeDescriptorId, innerReader);
                    }

                    using(var innerReader = new PacketReader(describer.InputTypeDescriptor))
                    {
                        inCodec ??= PacketSerializer.BuildCodec(describer.InputTypeDescriptorId, innerReader);
                    }
                }

                if (outCodec == null)
                {
                    throw new NotSupportedException($"Couldn't find a codec for type {result.OutputTypedescId}");
                }

                if(inCodec == null)
                {
                    throw new NotSupportedException($"Couldn't find a codec for type {result.InputTypedescId}");
                }

                if (inCodec is not IArgumentCodec argumentCodec)
                    throw new NotSupportedException($"Cannot encode arguments, {inCodec} is not a registered argument codec");

                // convert our arguments if any
                await SendMessageAsync(new Execute() { Capabilities = result.Capabilities, Arguments = argumentCodec.SerializeArguments(arguments) }).ConfigureAwait(false);
                await SendMessageAsync(new Sync()).ConfigureAwait(false);

                List<Data> receivedData = new();

                Task addData(IReceiveable msg)
                {
                    if (msg.Type == ServerMessageTypes.Data)
                        receivedData.Add((Data)msg);

                    return Task.CompletedTask;
                }

                OnMessage += addData;

                // wait for complete or error
                var completionTask = WaitForMessageAsync<CommandComplete>(ServerMessageTypes.CommandComplete);
                var errorTask = WaitForMessageAsync<ErrorResponse>(ServerMessageTypes.ErrorResponse);
                var executionResult = await Task.WhenAny(
                    completionTask,
                    errorTask).ConfigureAwait(false);

                OnMessage -= addData;

                object? queryResult = null;

                if (receivedData.Any()) // TODO: optimize?
                {
                    if(receivedData.Count == 1 && (card == Cardinality.AtMostOne || card == Cardinality.One))
                    {
                        using (var reader = new PacketReader(receivedData[0].PayloadData))
                        {
                            queryResult = outCodec.Deserialize(reader);
                        }
                    }
                    else
                    {
                        object?[] data = new object[receivedData.Count];
                        
                        for(int i = 0; i != data.Length; i++)
                        {
                            using (var reader = new PacketReader(receivedData[i].PayloadData))
                            {
                                data[i] = outCodec.Deserialize(reader);
                            }
                        }

                        queryResult = data;
                    }
                }

                CommandComplete? completeResult = completionTask.IsCompleted ? completionTask.Result : null;
                ErrorResponse? errorResult = errorTask.IsCompleted ? errorTask.Result : null;

                return new ExecuteResult
                {
                    IsSuccess = completeResult != null,
                    Error = errorResult,
                    Result = queryResult,
                    ExecutedQuery = query + $"\nParameters: {(arguments == null ? "none" : $"[{string.Join(", ", arguments.Select(x => $"{x.Key}: {x.Value}"))}]")}"
                };
            }
            catch(Exception x)
            {
                Logger.LogWarning("Failed to execute: {}", x);
                return new ExecuteResult
                {
                    IsSuccess = false,
                    Exception = x,
                    ExecutedQuery = query + $"\nParameters: {(arguments == null ? "none" : $"[{string.Join(", ", arguments.Select(x => $"{x.Key}: {x.Value}"))}]")}"
                };
            }
            finally
            {
                _sephamore.Release();
                IsIdle = true;
            }
        }

        private void ParseServerSettings(ParameterStatus status)
        {
            switch (status.Name)
            {
                case "suggested_pool_concurrency":
                    SuggestedPoolConcurrency = int.Parse(Encoding.UTF8.GetString(status.Value));
                    break;

                case "system_config":
                    using(var reader = new PacketReader(status.Value))
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

                        ServerConfig = (codec.Deserialize(reader) as IDictionary<string, object?>)!;
                    }
                    break;
            }
        }

        private async Task SendMessageAsync<T>(T message) where T : Sendable
        {
            if (_secureStream == null)
                return;

            using var ms = new MemoryStream();
            using (var writer = new PacketWriter(ms))
            {
                message.Write(writer, this);
            }

            await _secureStream.WriteAsync(ms.ToArray()).ConfigureAwait(false);
        }

        internal async Task StartSASLAuthenticationAsync(AuthenticationStatus authStatus)
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
                var timeout = Task.Delay(15000);
                var result = await Task.WhenAny(_authenticationStatusSource.Task, timeout).ConfigureAwait(false);

                if(result == timeout)
                {
                    _readyCancelTokenSource.Cancel();
                    throw new TimeoutException("SASL handshakes timeout out :(");
                }

                authStatus = await (Task<AuthenticationStatus>)result;

                // check the continue
                var final = scram.BuildFinalMessage(authStatus, _connection.Password!);

                await SendMessageAsync(final.FinalMessage);


                timeout = Task.Delay(15000);
                result = await Task.WhenAny(_authenticationStatusSource.Task, timeout).ConfigureAwait(false);

                if (result == timeout)
                {
                    _readyCancelTokenSource.Cancel();
                    throw new TimeoutException("SASL handshakes timeout out :(");
                }

                authStatus = await (Task<AuthenticationStatus>)result;

                var key = scram.ParseServerFinalMessage(authStatus);

                if (!key.SequenceEqual(final.ExpectedSig))
                {
                    Logger.LogWarning("Received signature didn't match expected scram signature");
                    // disconnect
                    await DisconnectAsync();
                }
            }
        }

        internal void SetAuthenticationStatusResult(AuthenticationStatus status)
        {
            if(_authenticationStatusSource != null)
            {
                _authenticationStatusSource?.TrySetResult(status);
                _authenticationStatusSource = new();
            }
        }
        public async Task ConnectAsync()
        {
            _disconnectCancelToken = new();

            await _client.ConnectAsync(_connection.Hostname!, _connection.Port).ConfigureAwait(false);

            _stream = _client.GetStream();

            _secureStream = new SslStream(_stream, false, new RemoteCertificateValidationCallback(ValidateServerCertificate), null);

            var certs = new X509Certificate2Collection();

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

            //  send handshake
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

        private async Task<TMessage?> WaitForMessageAsync<TMessage>(ServerMessageTypes type, CancellationToken? token = null) where TMessage : IReceiveable
        {
            var tcs = new TaskCompletionSource();
            TMessage? returnValue = default;
            Func<IReceiveable, Task> handler = (msg) =>
            {
               if (msg.Type == type)
               {
                   returnValue = (TMessage)msg;
                   tcs.TrySetResult();

               }

               return Task.CompletedTask;
            };

            OnMessage += handler;

            await Task.Run(() => tcs.Task, token ?? _disconnectCancelToken.Token).ConfigureAwait(false);

            OnMessage -= handler;


            return returnValue!;
        }

        public async Task DisconnectAsync()
        {
            await SendMessageAsync(new Terminate()).ConfigureAwait(false);
            _client.Close();
            _disconnectCancelToken.Cancel();
        }

        private static bool ValidateServerCertificate(object sender, X509Certificate? certificate, X509Chain? chain, SslPolicyErrors sslPolicyErrors)
        {
            // TODO: validate with conn params
            return true;
        }

        public async ValueTask DisposeAsync()
        {
            await DisconnectAsync();
            _client.Dispose();
            await (_stream?.DisposeAsync() ?? ValueTask.CompletedTask);
            await (_secureStream?.DisposeAsync() ?? ValueTask.CompletedTask);
        }
    }
}