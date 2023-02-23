using EdgeDB.Binary;
using EdgeDB.Binary.Codecs;
using Microsoft.Extensions.Logging;

namespace EdgeDB
{
    internal static partial class Log
    {
        [LoggerMessage(
            1,
            LogLevel.Error,
            "Failed to execute query"
        )]
        public static partial void InternalExecuteFailed(this ILogger logger, Exception x);

        [LoggerMessage(
            2,
            LogLevel.Error,
            "Got error level: {ErrorSeverity} Message: {Message}"
        )]
        public static partial void ErrorResponseReceived(this ILogger logger, ErrorSeverity errorSeverity, string message);

        [LoggerMessage(
            3,
            LogLevel.Debug,
            "Client {ClientId}: {MessageType}"
        )]
        public static partial void MessageReceived(this ILogger logger, ulong clientId, ServerMessageType messageType);

        [LoggerMessage(
            4,
            LogLevel.Error,
            "Error in event handler"
        )]
        public static partial void EventHandlerError(this ILogger logger, Exception x);

        [LoggerMessage(
            5,
            LogLevel.Warning,
            "Attempting to reconnect {Current}/{Max}"
        )]
        public static partial void AttemptToReconnect(this ILogger logger, uint current, uint max);

        [LoggerMessage(
            6,
            LogLevel.Error,
            "Max number of connection retries reached ({Max})"
        )]
        public static partial void MaxConnectionRetries(this ILogger logger, uint max);

        [LoggerMessage(
            7,
            LogLevel.Error,
            "Failed to complete authentication"
        )]
        public static partial void AuthenticationFailed(this ILogger logger, Exception? x = null);

        [LoggerMessage(
            8,
            LogLevel.Error,
            "Failed to parse server settings"
        )]
        public static partial void ServerSettingsParseFailed(this ILogger logger, Exception x);

        [LoggerMessage(
            9,
            LogLevel.Critical,
            "No reader found for packet {PacketId}. Please file a bug report"
        )]
        public static partial void UnknownPacket(this ILogger logger, string packetId);

        [LoggerMessage(
            10,
            LogLevel.Critical,
            "Error occured while reading binary stream"
        )]
        public static partial void ReadException(this ILogger logger, Exception x);

        [LoggerMessage(
            11,
            LogLevel.Warning,
            "Packet reader was left with data remaining while deserializing {Type} of length {Length}"
        )]
        public static partial void DidntReadTillEnd(this ILogger logger, ServerMessageType type, int length);

        [LoggerMessage(
            12,
            LogLevel.Critical,
            "The server requested protocol version {ServerVersion} but the currently installed client only supports {ClientVersion}. Please switch to a different client version that supports the requested protocol."
        )]
        public static partial void ProtocolMajorMismatch(this ILogger logger, string serverVersion, string clientVersion);

        [LoggerMessage(
            13,
            LogLevel.Warning,
            "The server requested protocol version {ServerVersion} but the currently installed client only supports {ClientVersion}. Functionality may be limited and bugs may arise, please switch to a different client version that supports the requested protocol."
        )]
        public static partial void ProtocolMinorMismatch(this ILogger logger, string serverVersion, string clientVersion);

        [LoggerMessage(
            14,
            LogLevel.Trace,
            "Client reconnecting, read disconnect request.")]
        public static partial void IdleDisconnect(this ILogger logger);

        [LoggerMessage(
            15,
            LogLevel.Trace,
            "The codec {Codec}:{ID} couln't be cached likely due to a race condition")]
        public static partial void CodecCouldntBeCached(this ILogger logger, ICodec codec, Guid id);

        [LoggerMessage(
            16,
            LogLevel.Trace,
            "{pos}: Read type descriptor {Descriptor} with UUID {ID}. Size {Size}")]
        public static partial void TraceTypeDescriptor(this ILogger logger, ITypeDescriptor descriptor, Guid id, int size, string pos);

        [LoggerMessage(
            17,
            LogLevel.Trace,
            "Codec built: {Final} with tree size of {TreeSize}. Final cache size: {CacheSize}")]
        public static partial void TraceCodecBuilderResult(this ILogger logger, ICodec final, int treeSize, int cacheSize);

        [LoggerMessage(
            18,
            LogLevel.Debug,
            "{Depth}Transitioning codec visitor stack from {OldType} to {NewType}")]
        public static partial void CodecVisitorStackTransition(this ILogger logger, string depth, Type oldType, Type newType);

        [LoggerMessage(
            19,
            LogLevel.Debug,
            "{Depth}Visiting codec {Codec}")]
        public static partial void CodecVisitorNewCodec(this ILogger logger, string depth, ICodec codec);

        [LoggerMessage(
            20,
            LogLevel.Debug,
            "{Depth}Codec mutated from {OldCodec} to {NewCodec}")]
        public static partial void CodecVisitorMutatedCodec(this ILogger logger, string depth, ICodec oldCodec, ICodec newCodec);

        [LoggerMessage(
            21,
            LogLevel.Debug,
            "{Depth}Compilable codec {CompilableCodec} produced {ProducedCodec} from context type {ContextType}")]
        public static partial void CodecVisitorCompiledCodec(this ILogger logger, string depth, ICodec compilableCodec, ICodec producedCodec, Type contextType);

        [LoggerMessage(
            22,
            LogLevel.Debug,
            "{Depth}Complex codec {ComplexCodec} flattened into {FlattenedCodec} based off of contextual type {ContextType}")]
        public static partial void CodecVisitorComplexCodecFlattened(this ILogger logger, string depth, ICodec complexCodec, ICodec flattenedCodec, Type contextType);

        [LoggerMessage(
            23,
            LogLevel.Debug,
            "{Depth}Runtime codec {RuntimeCodec} was incompatable with type {ContextType}; Called codec broker {Broker} with contextual type and produced {ProducedCodec}"
            )]
        public static partial void CodecVisitorRuntimeCodecBroker(this ILogger logger, string depth, ICodec runtimeCodec, ICodec broker, ICodec producedCodec, Type contextType);

        [LoggerMessage(
            24,
            LogLevel.Debug,
            "{Depth}Frame for {Type} exited")]
        public static partial void CodecVisitorFramePopped(this ILogger logger, string depth, Type type);

        [LoggerMessage(
            25,
            LogLevel.Debug,
            "Skipping codec visitor for {ExternalType} on codec {Codec}: it's already been visited for that type")]
        public static partial void SkippingCodecVisiting(this ILogger logger, Type externalType, ICodec codec);
    }
}
