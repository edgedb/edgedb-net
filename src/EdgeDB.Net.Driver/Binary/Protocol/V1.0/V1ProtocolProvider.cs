using EdgeDB.Binary.Codecs;
using EdgeDB.Binary.Protocol.V1._0.Descriptors;
using EdgeDB.Binary.Protocol.V1._0.Packets;
using EdgeDB.Utils;
using Microsoft.Extensions.Logging;
using System.Collections.Immutable;
using System.Dynamic;
using System.Net;
using System.Text;

namespace EdgeDB.Binary.Protocol.V1._0;

internal class V1ProtocolProvider : IProtocolProvider
{
    private readonly EdgeDBBinaryClient _client;
    private Dictionary<string, object?> _rawServerConfig = new();
    private ReadOnlyMemory<byte> _serverKey;

    public V1ProtocolProvider(EdgeDBBinaryClient client)
    {
        _client = client;
    }

    public int SuggestedPoolConcurrency { get; private set; }

    public ref ReadOnlyMemory<byte> ServerKey
        => ref _serverKey;

    private IBinaryDuplexer Duplexer
        => _client.Duplexer;

    private ILogger Logger
        => _client.Logger;

    public IReadOnlyDictionary<string, object?> ServerConfig
        => _rawServerConfig.ToImmutableDictionary();

    public ProtocolPhase Phase { get; private set; }

    public virtual ProtocolVersion Version { get; } = (1, 0);

    public virtual PacketReadFactory? GetPacketFactory(ServerMessageType type) =>
        type switch
        {
            ServerMessageType.Authentication => (ref PacketReader reader, in int length) =>
                new AuthenticationStatus(ref reader),
            ServerMessageType.CommandComplete => (ref PacketReader reader, in int length) =>
                new CommandComplete(ref reader),
            ServerMessageType.CommandDataDescription => (ref PacketReader reader, in int length) =>
                new CommandDataDescription(ref reader),
            ServerMessageType.Data => (ref PacketReader reader, in int length) => new Data(ref reader),
            ServerMessageType.DumpBlock => (ref PacketReader reader, in int length) =>
                new DumpBlock(ref reader, in length),
            ServerMessageType.DumpHeader => (ref PacketReader reader, in int length) =>
                new DumpHeader(ref reader, in length),
            ServerMessageType.ErrorResponse => (ref PacketReader reader, in int length) =>
                new ErrorResponse(ref reader),
            ServerMessageType.LogMessage => (ref PacketReader reader, in int length) => new LogMessage(ref reader),
            ServerMessageType.ParameterStatus => (ref PacketReader reader, in int length) =>
                new ParameterStatus(ref reader),
            ServerMessageType.ReadyForCommand => (ref PacketReader reader, in int length) =>
                new ReadyForCommand(ref reader),
            ServerMessageType.RestoreReady => (ref PacketReader reader, in int length) => new RestoreReady(ref reader),
            ServerMessageType.ServerHandshake => (ref PacketReader reader, in int length) =>
                new ServerHandshake(ref reader),
            ServerMessageType.ServerKeyData => (ref PacketReader reader, in int length) =>
                new ServerKeyData(ref reader),
            ServerMessageType.StateDataDescription => (ref PacketReader reader, in int length) =>
                new StateDataDescription(ref reader),
            _ => null
        };

    public virtual async Task<ExecuteResult> ExecuteQueryAsync(QueryParameters queryParameters, ParseResult parseResult,
        CancellationToken token)
    {
        if (parseResult.InCodecInfo.Codec is not IArgumentCodec argumentCodec)
            throw new MissingCodecException(
                $"Cannot encode arguments, {parseResult.InCodecInfo.Codec} is not a registered argument codec");

        ErrorResponse? error = null;
        var executeSuccess = false;
        var gotStateDescriptor = false;
        var successfullyParsed = false;
        var receivedData = new List<ReadOnlyMemory<byte>>();

        var stateBuf = parseResult.StateData;

        do
        {
            await foreach (var result in Duplexer.DuplexAndSyncAsync(new Execute
                           {
                               Capabilities = queryParameters.Capabilities,
                               Query = queryParameters.Query,
                               Format = queryParameters.Format,
                               ExpectedCardinality = queryParameters.Cardinality,
                               ExplicitObjectIds = _client.ClientConfig.ExplicitObjectIds,
                               StateTypeDescriptorId = _client.StateDescriptorId,
                               StateData = stateBuf,
                               ImplicitTypeNames = queryParameters.ImplicitTypeNames, // used for type builder
                               ImplicitTypeIds = _client.ClientConfig.ImplicitTypeIds,
                               Arguments = argumentCodec.SerializeArguments(_client, queryParameters.Arguments),
                               ImplicitLimit = _client.ClientConfig.ImplicitLimit,
                               InputTypeDescriptorId = parseResult.InCodecInfo.Id,
                               OutputTypeDescriptorId = parseResult.OutCodecInfo.Id
                           }, token))
            {
                switch (result.Packet)
                {
                    case Data data:
                        receivedData.Add(data.PayloadBuffer);
                        break;
                    case StateDataDescription stateDescriptor:
                    {
                        var stateCodec = CodecBuilder.BuildCodec(_client, in stateDescriptor.TypeDescriptorId,
                            stateDescriptor.TypeDescriptorBuffer);
                        _client.UpdateStateCodec(stateCodec, in stateDescriptor.TypeDescriptorId);
                        gotStateDescriptor = true;
                        stateBuf = _client.SerializeState();
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
                        _client.UpdateTransactionState(ready.TransactionState);
                        result.Finish();
                        executeSuccess = true;
                        break;
                }
            }
        } while (!successfullyParsed && !executeSuccess);

        if (error.HasValue)
            throw new EdgeDBErrorException(error.Value, queryParameters.Query);

        return new ExecuteResult(receivedData.ToArray(), parseResult.OutCodecInfo);
    }

    public virtual async Task<ParseResult> ParseQueryAsync(QueryParameters queryParameters, CancellationToken token)
    {
        ErrorResponse? error = null;
        var parseAttempts = 0;
        var successfullyParsed = false;
        var gotStateDescriptor = false;

        var cacheKey = queryParameters.GetCacheKey();

        var stateBuf = _client.SerializeState();

        if (!CodecBuilder.TryGetCodecs(this, cacheKey, out var inCodecInfo, out var outCodecInfo))
        {
            while (!successfullyParsed)
            {
                if (parseAttempts > 2)
                {
                    throw error.HasValue
                        ? new EdgeDBException($"Failed to parse query after {parseAttempts} attempts",
                            new EdgeDBErrorException(error.Value, queryParameters.Query))
                        : new EdgeDBException($"Failed to parse query after {parseAttempts} attempts");
                }

                await foreach (var result in Duplexer.DuplexAndSyncAsync(new Parse
                               {
                                   Capabilities = queryParameters.Capabilities,
                                   Query = queryParameters.Query,
                                   Format = queryParameters.Format,
                                   ExpectedCardinality = queryParameters.Cardinality,
                                   ExplicitObjectIds = _client.ClientConfig.ExplicitObjectIds,
                                   StateTypeDescriptorId = _client.StateDescriptorId,
                                   StateData = stateBuf,
                                   ImplicitLimit = _client.ClientConfig.ImplicitLimit,
                                   ImplicitTypeNames = queryParameters.ImplicitTypeNames, // used for type builder
                                   ImplicitTypeIds = _client.ClientConfig.ImplicitTypeIds
                               }, token))
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
                            outCodecInfo = new CodecInfo(descriptor.OutputTypeDescriptorId,
                                CodecBuilder.BuildCodec(_client, in descriptor.OutputTypeDescriptorId,
                                    descriptor.OutputTypeDescriptorBuffer));

                            inCodecInfo = new CodecInfo(descriptor.InputTypeDescriptorId,
                                CodecBuilder.BuildCodec(_client, in descriptor.InputTypeDescriptorId,
                                    descriptor.InputTypeDescriptorBuffer));

                            CodecBuilder.UpdateKeyMap(cacheKey, in descriptor.InputTypeDescriptorId,
                                in descriptor.OutputTypeDescriptorId);
                        }
                            break;
                        case StateDataDescription stateDescriptor:
                        {
                            var stateCodec = CodecBuilder.BuildCodec(_client, in stateDescriptor.TypeDescriptorId,
                                stateDescriptor.TypeDescriptorBuffer);
                            _client.UpdateStateCodec(stateCodec, in stateDescriptor.TypeDescriptorId);
                            gotStateDescriptor = true;
                            stateBuf = _client.SerializeState();
                        }
                            break;
                        case ReadyForCommand ready:
                            _client.UpdateTransactionState(ready.TransactionState);
                            result.Finish();
                            successfullyParsed = true;
                            break;
                    }
                }

                parseAttempts++;
            }
        }

        if (error.HasValue)
            throw new EdgeDBErrorException(error.Value, queryParameters.Query);

        if (outCodecInfo is null)
            throw new MissingCodecException("Couldn't find a valid output codec");

        if (inCodecInfo is null)
            throw new MissingCodecException("Couldn't find a valid input codec");

        if (inCodecInfo.Codec is not IArgumentCodec)
            throw new MissingCodecException(
                $"Cannot encode arguments, {inCodecInfo.Codec} is not a registered argument codec");

        return new ParseResult(inCodecInfo, outCodecInfo, in stateBuf);
    }

    public virtual ICodec? BuildCodec<T>(in T descriptor, RelativeCodecDelegate getRelativeCodec,
        RelativeDescriptorDelegate getRelativeDescriptor)
        where T : ITypeDescriptor
    {
        switch (descriptor)
        {
            case EnumerationTypeDescriptor:
                return CodecBuilder.GetOrCreateCodec<TextCodec>(this, _ => new TextCodec());
            case NamedTupleTypeDescriptor namedTuple:
            {
                var names = new string[namedTuple.Elements.Length];
                var innerCodecs = new ICodec[namedTuple.Elements.Length];

                for (var i = 0; i != namedTuple.Elements.Length; i++)
                {
                    var element = namedTuple.Elements[i];

                    names[i] = element.Name;
                    innerCodecs[i] = getRelativeCodec(element.TypePos)!;
                }

                return new ObjectCodec(in namedTuple.Id, innerCodecs, names);
            }
            case ObjectShapeDescriptor objectShape:
            {
                var names = new string[objectShape.Shapes.Length];
                var innerCodecs = new ICodec[objectShape.Shapes.Length];

                for (var i = 0; i != objectShape.Shapes.Length; i++)
                {
                    var element = objectShape.Shapes[i];

                    names[i] = element.Name;
                    innerCodecs[i] = getRelativeCodec(element.TypePos)!;
                }

                return new ObjectCodec(in objectShape.Id, innerCodecs, names);
            }
            case InputShapeDescriptor input:
            {
                var names = new string[input.Shapes.Length];
                var innerCodecs = new ICodec[input.Shapes.Length];

                for (var i = 0; i != input.Shapes.Length; i++)
                {
                    var element = input.Shapes[i];

                    names[i] = element.Name;
                    innerCodecs[i] = getRelativeCodec(element.TypePos)!;
                }

                return new SparceObjectCodec(in input.Id, innerCodecs, names);
            }
            case TupleTypeDescriptor tuple:
            {
                var innerCodecs = new ICodec[tuple.ElementTypeDescriptorsIndex.Length];
                for (var i = 0; i != tuple.ElementTypeDescriptorsIndex.Length; i++)
                    innerCodecs[i] = getRelativeCodec(tuple.ElementTypeDescriptorsIndex[i])!;

                return new TupleCodec(in tuple.Id, innerCodecs);
            }
            case RangeTypeDescriptor range:
            {
                ref var innerCodec = ref getRelativeCodec(range.TypePos)!;

                return new CompilableWrappingCodec(in range.Id, innerCodec, typeof(RangeCodec<>));
            }
            case ArrayTypeDescriptor array:
            {
                ref var innerCodec = ref getRelativeCodec(array.TypePos)!;

                return new CompilableWrappingCodec(in array.Id, innerCodec, typeof(ArrayCodec<>));
            }
            case SetTypeDescriptor set:
            {
                ref var innerCodec = ref getRelativeCodec(set.TypePos)!;

                return new CompilableWrappingCodec(in set.Id, innerCodec, typeof(SetCodec<>));
            }
            case BaseScalarTypeDescriptor scalar:
                throw new MissingCodecException(
                    $"Could not find the scalar type {scalar.Id}. Please file a bug report with your query that caused this error.");
            default:
                throw new MissingCodecException(
                    $"Could not find a type descriptor with type {descriptor.Id}. Please file a bug report with your query that caused this error.");
        }
    }

    public virtual ITypeDescriptor GetDescriptor(ref PacketReader reader)
    {
        var type = (DescriptorType)reader.ReadByte();
        var id = reader.ReadGuid();

        ITypeDescriptor? descriptor = type switch
        {
            DescriptorType.ArrayTypeDescriptor => new ArrayTypeDescriptor(in id, ref reader),
            DescriptorType.BaseScalarTypeDescriptor => new BaseScalarTypeDescriptor(id),
            DescriptorType.EnumerationTypeDescriptor => new EnumerationTypeDescriptor(id, ref reader),
            DescriptorType.NamedTupleDescriptor => new NamedTupleTypeDescriptor(id, ref reader),
            DescriptorType.ObjectShapeDescriptor => new ObjectShapeDescriptor(id, ref reader),
            DescriptorType.ScalarTypeDescriptor => new ScalarTypeDescriptor(id, ref reader),
            DescriptorType.ScalarTypeNameAnnotation => new ScalarTypeNameAnnotation(id, ref reader),
            DescriptorType.SetDescriptor => new SetTypeDescriptor(id, ref reader),
            DescriptorType.TupleTypeDescriptor => new TupleTypeDescriptor(id, ref reader),
            DescriptorType.InputShapeDescriptor => new InputShapeDescriptor(id, ref reader),
            DescriptorType.RangeTypeDescriptor => new RangeTypeDescriptor(id, ref reader),
            _ => null
        };

        if (descriptor is null)
        {
            var rawType = (byte)type;

            if (rawType >= 0x80 && rawType <= 0xfe)
            {
                descriptor = new TypeAnnotationDescriptor(in type, in id, ref reader);
            }
            else
                throw new InvalidDataException($"No descriptor found for type {type}");
        }

        return descriptor;
    }

    public virtual Sendable Handshake() =>
        new ClientHandshake
        {
            MajorVersion = Version.Major,
            MinorVersion = Version.Minor,
            ConnectionParameters = _client.Connection.SecretKey is not null
                ? new ConnectionParam[]
                {
                    new() {Name = "user", Value = _client.Connection.Username!},
                    new() {Name = "database", Value = _client.Connection.Database!},
                    new() {Name = "secret_key", Value = _client.Connection.SecretKey}
                }
                : new ConnectionParam[]
                {
                    new() {Name = "user", Value = _client.Connection.Username!},
                    new() {Name = "database", Value = _client.Connection.Database!}
                }
        };

    public virtual ValueTask ProcessAsync<T>(in T message) where T : IReceiveable
    {
        switch (message)
        {
            case ReadyForCommand ready:
                _client.UpdateTransactionState(ready.TransactionState);
                Phase = ProtocolPhase.Command;
                break;
            case ServerHandshake handshake:
                if (!Version.Equals(in handshake.MajorVersion, in handshake.MinorVersion))
                {
                    var negotiated = _client.TryNegotiateProtocol(in handshake.MajorVersion, in handshake.MinorVersion);

                    if (!negotiated && Version.Major != handshake.MajorVersion)
                    {
                        // major difference results in a disconnect
                        Logger.ProtocolMajorMismatch($"{handshake.MajorVersion}.{handshake.MinorVersion}",
                            Version.ToString());
                        return _client.DisconnectAsync();
                    }

                    if (!negotiated)
                    {
                        // minor mismatch
                        Logger.ProtocolMinorMismatch($"{handshake.MajorVersion}.{handshake.MinorVersion}",
                            Version.ToString());
                    }

                    if (negotiated)
                    {
                        // this provider is basically dead now, do nothing.
                        return ValueTask.CompletedTask;
                    }
                }

                break;
            case ErrorResponse err:
                Logger.ErrorResponseReceived(err.Severity, err.Message);
                _client.CancelReadyState();
                Phase = ProtocolPhase.Errored;
                break;
            case AuthenticationStatus authStatus:
                if (authStatus.AuthStatus == AuthStatus.AuthenticationRequiredSASLMessage)
                {
                    if (authStatus.AuthenticationMethods is null || authStatus.AuthenticationMethods.Length == 0)
                        throw new EdgeDBException(
                            "Expected an authentication method for AuthenticationStatus message. but got null");

                    return new ValueTask(StartSASLAuthenticationAsync(authStatus.AuthenticationMethods[0]));
                }

                if (authStatus.AuthStatus != AuthStatus.AuthenticationOK)
                    throw new UnexpectedMessageException("Expected AuthenticationRequiredSASLMessage, got " +
                                                         authStatus.AuthStatus);
                break;
            case ServerKeyData keyData:
                _serverKey = keyData.KeyBuffer;
                break;
            case StateDataDescription stateDescriptor:
                var stateCodec = CodecBuilder.BuildCodec(_client, stateDescriptor.TypeDescriptorId,
                    stateDescriptor.TypeDescriptorBuffer);
                var stateCodecId = stateDescriptor.TypeDescriptorId;
                _client.UpdateStateCodec(stateCodec, stateCodecId);
                break;
            case ParameterStatus parameterStatus:
                ParseServerSettings(parameterStatus);
                break;
            case LogMessage log:
                return _client.OnLogAsync(log.Severity, log.Code, log.Content);
        }

        return ValueTask.CompletedTask;
    }

    public virtual async Task SendSyncMessageAsync(CancellationToken token)
    {
        // if the current client is not connected, reconnect it
        if (!Duplexer.IsConnected)
            await _client.ReconnectAsync(token);

        var result = await Duplexer.DuplexSingleAsync(new Sync(), token).ConfigureAwait(false);
        var rfc = result?.ThrowIfErrorOrNot<ReadyForCommand>();

        if (rfc.HasValue)
        {
            _client.UpdateTransactionState(rfc.Value.TransactionState);
        }
    }

    public virtual Sendable Terminate()
        => new Terminate();

    public virtual Sendable Sync()
        => new Sync();

    protected virtual void ParseServerSettings(ParameterStatus status)
    {
        try
        {
            switch (status.Name)
            {
                case "suggested_pool_concurrency":
                    var str = Encoding.UTF8.GetString(status.ValueBuffer);
                    if (!int.TryParse(str, out var suggestedPoolConcurrency))
                    {
                        throw new FormatException(
                            "suggested_pool_concurrency type didn't match the expected type of int");
                    }

                    SuggestedPoolConcurrency = suggestedPoolConcurrency;
                    break;

                case "system_config":
                    var reader = new PacketReader(status.ValueBuffer);
                    var length = reader.ReadInt32() - 16;
                    var descriptorId = reader.ReadGuid();
                    reader.ReadBytes(length, out var typeDesc);

                    var codec = CodecBuilder.GetCodec(this, descriptorId);

                    if (codec is null)
                    {
                        var innerReader = new PacketReader(in typeDesc);
                        codec = CodecBuilder.BuildCodec(_client, descriptorId, ref innerReader);

                        if (codec is null)
                            throw new MissingCodecException("Failed to build codec for system_config");
                    }

                    // disard length
                    reader.Skip(4);

                    var obj = codec.Deserialize(ref reader, _client.CodecContext)!;

                    _rawServerConfig = ((ExpandoObject)obj).ToDictionary(x => x.Key, x => x.Value);
                    break;
            }
        }
        catch (Exception x)
        {
            Logger.ServerSettingsParseFailed(x);
        }
    }

    protected virtual async Task StartSASLAuthenticationAsync(string method)
    {
        Phase = ProtocolPhase.Auth;

        using var scram = new Scram();

        if (method is not "SCRAM-SHA-256")
        {
            throw new ProtocolViolationException("The only supported method is SCRAM-SHA-256");
        }

        var initialMsg = scram.BuildInitialMessage(_client.Connection.Username!);

        var expectedSig = Array.Empty<byte>();

        await foreach (var result in Duplexer.DuplexAsync(
                           new AuthenticationSASLInitialResponse(
                               Encoding.UTF8.GetBytes(initialMsg), method
                           )))
        {
            switch (result.Packet)
            {
                case AuthenticationStatus authResult:
                {
                    switch (authResult.AuthStatus)
                    {
                        case AuthStatus.AuthenticationSASLContinue:
                        {
                            var (final, sig) = scram.BuildFinalMessage(
                                Encoding.UTF8.GetString(authResult.SASLDataBuffer), _client.Connection.Password!);

                            expectedSig = sig;

                            await Duplexer.SendAsync(packets: new AuthenticationSASLResponse(
                                Encoding.UTF8.GetBytes(final)
                            )).ConfigureAwait(false);
                        }
                            break;
                        case AuthStatus.AuthenticationSASLFinal:
                        {
                            var key = Scram.ParseServerSig(
                                Encoding.UTF8.GetString(authResult.SASLDataBuffer)
                            );

                            if (!key.SequenceEqual(expectedSig))
                            {
                                throw new InvalidSignatureException();
                            }
                        }
                            break;
                        case AuthStatus.AuthenticationOK:
                        {
                            result.Finish();
                        }
                            break;
                        default:
                            throw new UnexpectedMessageException(
                                $"Expected coninue or final but got {authResult.AuthStatus}");
                    }
                }
                    break;
                case ErrorResponse err:
                    throw new EdgeDBErrorException(err);
            }
        }
    }
}
