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
    }
}
