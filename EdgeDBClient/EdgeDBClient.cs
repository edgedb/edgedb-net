using EdgeDB.Codecs;
using EdgeDB.Models;
using EdgeDB.Utils;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using UtilPack;
using UtilPack.Cryptography.Digest;
using UtilPack.Cryptography.SASL;
using UtilPack.Cryptography.SASL.SCRAM;

namespace EdgeDB 
{
    public class EdgeDBClient
    {
        public event Func<IReceiveable, Task> OnMessage
        {
            add => _onMessage.Add(value);
            remove => _onMessage.Remove(value);
        }

        private AsyncEvent<Func<IReceiveable, Task>> _onMessage = new();

        internal SASLMechanism? SASLClient;
        internal ILogger Logger;

        private readonly TcpClient _client;
        private NetworkStream? _stream;
        private SslStream? _secureStream;
        private readonly EdgeDBConnection _connection;
        private CancellationTokenSource _readCancelToken;
        private TaskCompletionSource<AuthenticationStatus> _authenticationStatusSource;
        private TaskCompletionSource _readySource;
        private CancellationTokenSource _readyCancelTokenSource;
        private SemaphoreSlim _sephamore;

        // server config
        private byte[] _serverKey;
        private int _suggestedPoolConcurrency;
        private IDictionary<string, object?> _serverConfig;

        // TODO: config?
        public EdgeDBClient(EdgeDBConnection connection, ILogger? logger)
        {
            Logger = logger ?? Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
            _sephamore = new(1,1);
            _readCancelToken = new();
            _connection = connection;
            _client = new();
            _authenticationStatusSource = new();
            _readySource = new();
            _readyCancelTokenSource = new();
            _serverKey = new byte[32];
            _serverConfig = new Dictionary<string, object?>();
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

                    if (_readCancelToken.IsCancellationRequested)
                        return;

                    if (_secureStream == null)
                        return;

                    var msg = PacketSerializer.DeserializePacket(_secureStream, this);

                    if (msg != null)
                        await HandlePayloadAsync(msg);

                    await Task.CompletedTask;
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
                                    _ = Task.Run(async () => await StartSASLAuthenticationAsync(authStatus));
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
                        _serverKey = keyData.Key;
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
                await _onMessage.InvokeAsync(payload);
            }
            catch (Exception x)
            {
                Logger.LogCritical("Exception in message handler: {}", x);
            }
        }

        private async Task<IReceiveable> PrepareAsync(Prepare packet)
        {
            await SendMessageAsync(packet);
            await SendMessageAsync(new Sync());
            var timeoutTask = Task.Delay(15000);
            var resultTask = WaitForMessageAsync<PrepareComplete>(ServerMessageTypes.PrepareComplete);
            var errorTask = WaitForMessageAsync<ErrorResponse>(ServerMessageTypes.ErrorResponse);

            var result = await Task.WhenAny(resultTask, errorTask, timeoutTask);

            if (result == timeoutTask)
                throw new TimeoutException("Didn't receive PrepareComplete result after 15 seconds");

            if (resultTask.IsCompleted)
                return await resultTask;
            else return await errorTask;
        }

        public async Task<ExecuteResult<TType>?> QueryAsync<TType>(Expression<Func<TType, bool>> filter)
        {
            var query = QueryBuilder.BuildSelectQuery(filter);

            var result = await ExecuteAsync(query.QueryText!, query.Parameters);

            return ExecuteResult<TType>.Convert(result);
        }

        public async Task<ExecuteResult?> ExecuteAsync(string query, IDictionary<string, object?>? arguments = null)
        {
            await _sephamore.WaitAsync(_readCancelToken.Token);

            try
            {
                var prepareResult = await PrepareAsync(new Prepare
                {
                    Capabilities = AllowCapabilities.ReadOnly, // TODO: change this
                    Command = query,
                    Format = IOFormat.Binary,
                    ExpectedCardinality = Cardinality.Many
                });

                if(prepareResult is ErrorResponse error)
                {
                    return new ExecuteResult
                    {
                        Error = error,
                        IsSuccess = false,
                    };
                }

                if(prepareResult is not PrepareComplete result)
                {
                    throw new InvalidDataException($"Got unexpected result from prepare: {prepareResult.Type}");
                }

                // get the codec for the return type
                var outCodec = PacketSerializer.GetCodec(result.OutputTypedescId);
                var inCodec = (IArgumentCodec?)PacketSerializer.GetCodec(result.InputTypedescId);

                // if its not cached or we dont have a default one for it, ask the server to describe it for us
                if (outCodec == null || inCodec == null)
                {
                    await SendMessageAsync(new DescribeStatement());
                    await SendMessageAsync(new Sync());
                    var describer = await WaitForMessageAsync<CommandDataDescription>(ServerMessageTypes.CommandDataDescription);

                    using (var innerReader = new PacketReader(describer.OutputTypeDescriptor))
                    {
                        outCodec ??= PacketSerializer.BuildCodec(describer.OutputTypeDescriptorId, innerReader);
                    }

                    using(var innerReader = new PacketReader(describer.InputTypeDescriptor))
                    {
                        inCodec ??= (IArgumentCodec?)PacketSerializer.BuildCodec(describer.InputTypeDescriptorId, innerReader);
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

                // convert our arguments if any
                await SendMessageAsync(new Execute() { Capabilities = result.Capabilities, Arguments = inCodec.SerializeArguments(arguments) });
                await SendMessageAsync(new Sync());

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
                    errorTask);

                OnMessage -= addData;

                object? queryResult = null;

                if (receivedData.Any()) // TODO: optimize?
                {
                    if(receivedData.Count == 1)
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
                };
            }
            catch(Exception x)
            {
                Logger.LogWarning("Failed to execute: {}", x);
                return new ExecuteResult
                {
                    IsSuccess = false,
                    Exception = x
                };
            }
            finally
            {
                _sephamore.Release();
            }
        }

        private void ParseServerSettings(ParameterStatus status)
        {
            switch (status.Name)
            {
                case "suggested_pool_concurrency":
                    _suggestedPoolConcurrency = int.Parse(Encoding.UTF8.GetString(status.Value));
                    break;

                case "system_config":
                    using(var reader = new PacketReader(status.Value))
                    {
                        var length = reader.ReadInt32() - 16;
                        var descriptorId = reader.ReadGuid();
                        var typeDesc = reader.ReadBytes(length);

                        Codecs.ICodec? codec;

                        using(var innerReader = new PacketReader(typeDesc))
                        {
                            codec = PacketSerializer.BuildCodec(descriptorId, innerReader);

                            if (codec == null)
                                throw new Exception("Failed to build codec for system config");
                        }

                        // disard length
                        reader.ReadUInt32();

                        _serverConfig = (codec.Deserialize(reader) as IDictionary<string, object?>)!;
                    }
                    break;
            }
        }

        private async Task SendMessageAsync<T>(T message) where T : Sendable
        {
            if (_secureStream == null)
                return;

            using(var ms = new MemoryStream())
            {
                await using (var writer = new PacketWriter(ms))
                {
                    message.Write(writer, this);
                }

                await _secureStream.WriteAsync(ms.ToArray());
            }
        }

        internal async Task StartSASLAuthenticationAsync(AuthenticationStatus authStatus)
        {
            using (SASLClient = new SHA256().CreateSASLClientSCRAM())
            {
                var method = authStatus.AuthenticationMethods[0];

                if (method != "SCRAM-SHA-256")
                {
                    // TODO: add converter for string methods?
                    throw new NotSupportedException("The only supported method is SCRAM-SHA-256");
                }

                var credentials = new SASLCredentialsSCRAMForClient(_connection.Username, _connection.Password);

                var writeArray = new ResizableArray<Byte>();

                var encoding = new UTF8Encoding(false, false).CreateDefaultEncodingInfo();

                var saslResult = await SASLClient.ChallengeOrThrowOnErrorAsync(
                    credentials.CreateChallengeArguments(null, -1, -1, writeArray, 0, encoding)
                    );

                await SendMessageAsync(new AuthenticationSASLInitialResponse(writeArray.Array, method));

                // wait for continue or timeout
                var timeout = Task.Delay(15000);
                var result = await Task.WhenAny(_authenticationStatusSource.Task, timeout);

                if(result == timeout)
                {
                    _readyCancelTokenSource.Cancel();
                    throw new TimeoutException("SASL handshakes timeout out :(");
                }

                authStatus = await (Task<AuthenticationStatus>)result;

                // check the continue
                saslResult = await SASLClient.ChallengeOrThrowOnErrorAsync(credentials.CreateChallengeArguments(
                    authStatus.SASLData,
                    0,
                    authStatus.SASLData.Length,
                    writeArray,
                    0,
                    encoding));

                Logger.LogDebug("Got {}:{} for continue", authStatus.AuthStatus, saslResult.Item2);

                await SendMessageAsync(new AuthenticationSASLResponse(writeArray.Array));

                // wait for final
                timeout = Task.Delay(15000);
                result = await Task.WhenAny(_authenticationStatusSource.Task, timeout);

                if (result == timeout)
                {
                    _readyCancelTokenSource.Cancel();
                    throw new TimeoutException("SASL handshakes timeout out :(");
                }

                authStatus = await (Task<AuthenticationStatus>)result;

                saslResult = await SASLClient.ChallengeOrThrowOnErrorAsync(credentials.CreateChallengeArguments(
                    authStatus.SASLData,
                    0,
                    authStatus.SASLData.Length,
                    writeArray,
                    0,
                    encoding));

                Logger.LogDebug("Got {}:{} for final", authStatus.AuthStatus, saslResult.Item2);
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
            _readCancelToken = new();

            //_client.BeginConnect(_connection.Hostname, _connection.Port)

            await _client.ConnectAsync(_connection.Hostname, _connection.Port).ConfigureAwait(false);

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

            await _secureStream.AuthenticateAsClientAsync(options);

            _ = Task.Run(async () => await ReceiveAsync());

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
                        Value = _connection.Username
                    },
                    new ConnectionParam
                    {
                        Name = "database",
                        Value = _connection.Database
                    }
                }
            });

            // wait for auth promise
            await Task.Run(() => _readySource.Task, _readyCancelTokenSource.Token);
        }

        private async Task<TMessage?> WaitForMessageAsync<TMessage>(ServerMessageTypes type, CancellationToken token = default) where TMessage : IReceiveable
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

            await Task.Run(() => tcs.Task, token);

            OnMessage -= handler;


            return returnValue!;
        }

        public Task DisconnectAsync()
        {
            return Task.CompletedTask;
        }

        private static bool ValidateServerCertificate(object sender, X509Certificate? certificate, X509Chain? chain, SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }
    }
}