
namespace EdgeDB.Binary
{
    /// <summary>
    ///		Represents all supported message types sent by the server.
    /// </summary>
    public enum ServerMessageType : sbyte
    {
        /// <summary>
        ///		A <see cref="Packets.AuthenticationStatus"/> message.
        /// </summary>
        Authentication = 0x52,

        /// <summary>
        ///		A <see cref="Packets.CommandComplete"/> message.
        /// </summary>
        CommandComplete = 0x43,

        /// <summary>
        ///		A <see cref="Packets.CommandDataDescription"/> message.
        /// </summary>
        CommandDataDescription = 0x54,

        /// <summary>
        ///		A <see cref="Packets.Data"/> message.
        /// </summary>
        Data = 0x44,

        /// <summary>
        ///		A <see cref="Packets.DumpBlock"/> message.
        /// </summary>
        DumpBlock = 0x3d,

        /// <summary>
        ///		A <see cref="Packets.DumpHeader"/> message.
        /// </summary>
        DumpHeader = 0x40,

        /// <summary>
        ///		A <see cref="Packets.ErrorResponse"/> message.
        /// </summary>
        ErrorResponse = 0x45,

        /// <summary>
        ///		A <see cref="LogMessage"/> message.
        /// </summary>
        LogMessage = 0x4c,

        /// <summary>
        ///		A <see cref="Packets.ParameterStatus"/> message.
        /// </summary>
        ParameterStatus = 0x53,

        /// <summary>
        ///		A <see cref="Packets.ParseComplete"/> message.
        /// </summary>
        ParseComplete = 0x31,

        /// <summary>
        ///		A <see cref="Packets.ReadyForCommand"/> message.
        /// </summary>
        ReadyForCommand = 0x5a,

        /// <summary>
        ///		A <see cref="Packets.RestoreReady"/> message.
        /// </summary>
        RestoreReady = 0x2b,

        /// <summary>
        ///		A <see cref="Packets.ServerHandshake"/> message.
        /// </summary>
        ServerHandshake = 0x76,

        /// <summary>
        ///		A <see cref="Packets.ServerKeyData"/> message.
        /// </summary>
        ServerKeyData = 0x4b
    }
}
