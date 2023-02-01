using EdgeDB.Binary;
using EdgeDB.Binary.Codecs;
using Microsoft.Extensions.Logging;

namespace EdgeDB
{
    internal static class LoggerExtensions
    {
        #region Delegates
        private static readonly Action<ILogger, Exception> _internalExecuteFailed;
        private static readonly Action<ILogger, ErrorSeverity, string, Exception?> _errorResponseReceived;
        private static readonly Action<ILogger, ulong, ServerMessageType, Exception?> _messageReceived;
        private static readonly Action<ILogger, Exception> _eventHandlerException;
        private static readonly Action<ILogger, uint, uint, Exception?> _connectionRetry;
        private static readonly Action<ILogger, uint, Exception?> _maxConnectionRetryReached;
        private static readonly Action<ILogger, Exception?> _authenticationFailed;
        private static readonly Action<ILogger, Exception> _serverSettingsParseFailed;
        private static readonly Action<ILogger, string, Exception?> _unknownPacket;
        private static readonly Action<ILogger, Exception> _readException;
        private static readonly Action<ILogger, ServerMessageType, int, Exception?> _didntReadTillEnd;
        private static readonly Action<ILogger, string, string,Exception?> _protocolMajorMismatch;
        private static readonly Action<ILogger, string, string,Exception?> _protocolMinorMismatch;
        private static readonly Action<ILogger, ICodec, Guid, Exception?> _codecCouldntBeCached;
        #endregion

        static LoggerExtensions()
        {
            _internalExecuteFailed = LoggerMessage.Define(
                LogLevel.Error,
                new EventId(1, nameof(InternalExecuteFailed)),
                "Failed to execute query");

            _errorResponseReceived = LoggerMessage.Define<ErrorSeverity, string>(
                LogLevel.Error,
                new EventId(2, nameof(ErrorResponseReceived)),
                "Got error level: {ErrorLevel} Message: {Message}");

            _messageReceived = LoggerMessage.Define<ulong, ServerMessageType>(
                LogLevel.Debug,
                new EventId(3, nameof(MessageReceived)),
                "Client {ClientId}: {MessageType}");

            _eventHandlerException = LoggerMessage.Define(
                LogLevel.Error,
                new EventId(4, nameof(EventHandlerError)),
                "Error in event handler");

            _connectionRetry = LoggerMessage.Define<uint, uint>(
                LogLevel.Warning,
                new EventId(5, nameof(AttemptToReconnect)),
                "Attempting to reconnect {CurrentAttempts}/{MaxAttempts}");

            _maxConnectionRetryReached = LoggerMessage.Define<uint>(
                LogLevel.Error,
                new EventId(6, nameof(MaxConnectionRetries)),
                "Max number of connection retries reached ({MaxAttempts})");

            _authenticationFailed = LoggerMessage.Define(
                LogLevel.Error,
                new EventId(7, nameof(AuthenticationFailed)),
                "Failed to complete authentication");

            _serverSettingsParseFailed = LoggerMessage.Define(
                LogLevel.Error,
                new EventId(8, nameof(ServerSettingsParseFailed)),
                "Failed to parse server settings");

            _unknownPacket = LoggerMessage.Define<string>(
                LogLevel.Critical,
                new EventId(9, nameof(UnknownPacket)),
                "No reader found for packet {PacketId}. Please file a bug report");

            _readException = LoggerMessage.Define(
                LogLevel.Critical,
                new EventId(10, nameof(ReadException)),
                "Error occured while reading binary stream");

            _didntReadTillEnd = LoggerMessage.Define<ServerMessageType, int>(
                LogLevel.Warning,
                new EventId(11, nameof(DidntReadTillEnd)),
                "Packet reader was left with data remaining while deserializing {PacketType} of length {Length}");

            _protocolMajorMismatch = LoggerMessage.Define<string, string>(
                LogLevel.Critical,
                new EventId(12, nameof(ProtocolMajorMismatch)),
                "The server requested protocol version {ServerVersion} but the currently installed client only supports {ClientVersion}. Please switch to a different client version that supports the requested protocol.");
            
            _protocolMinorMismatch = LoggerMessage.Define<string, string>(
                LogLevel.Warning,
                new EventId(13, nameof(ProtocolMinorMismatch)),
                "The server requested protocol version {ServerVersion} but the currently installed client only supports {ClientVersion}. Functionality may be limited and bugs may arise, please switch to a different client version that supports the requested protocol.");

            _codecCouldntBeCached = LoggerMessage.Define<ICodec, Guid>(
                LogLevel.Trace,
                new EventId(14, nameof(CodecCouldntBeCached)),
                "The codec {@Codec}:{ID} couln't be cached likely due to a race condition");
        }

        public static void CodecCouldntBeCached(this ILogger logger, ICodec codec, Guid id)
            => _codecCouldntBeCached(logger, codec, id, null);

        public static void ProtocolMinorMismatch(this ILogger logger, string serverVersion, string clientVersion)
            => _protocolMinorMismatch(logger, serverVersion, clientVersion, null);

        public static void ProtocolMajorMismatch(this ILogger logger, string serverVersion, string clientVersion)
            => _protocolMajorMismatch(logger, serverVersion, clientVersion, null);

        public static void DidntReadTillEnd(this ILogger logger, ServerMessageType type, int length)
            => _didntReadTillEnd(logger, type, length, null);

        public static void InternalExecuteFailed(this ILogger logger, Exception x)
            => _internalExecuteFailed(logger, x);

        public static void ErrorResponseReceived(this ILogger logger, ErrorSeverity errorSeverity, string message)
            => _errorResponseReceived(logger, errorSeverity, message, null);

        public static void MessageReceived(this ILogger logger, ulong clientId, ServerMessageType messageType)
            => _messageReceived(logger, clientId, messageType, null);

        public static void EventHandlerError(this ILogger logger, Exception x)
            => _eventHandlerException(logger, x);

        public static void AttemptToReconnect(this ILogger logger, uint current, uint max)
            => _connectionRetry(logger, current, max, null);

        public static void MaxConnectionRetries(this ILogger logger, uint max)
            => _maxConnectionRetryReached(logger, max, null);

        public static void AuthenticationFailed(this ILogger logger, Exception? x = null)
            => _authenticationFailed(logger, x);

        public static void ServerSettingsParseFailed(this ILogger logger, Exception x)
            => _serverSettingsParseFailed(logger, x);

        public static void UnknownPacket(this ILogger logger, string packetType)
            => _unknownPacket(logger, packetType, null);

        public static void ReadException(this ILogger logger, Exception x)
            => _readException(logger, x);
    }
}
