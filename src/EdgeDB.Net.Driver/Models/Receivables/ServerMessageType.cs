namespace EdgeDB.Models
{
    /// <summary>
    ///		Represents all supported message types sent by the server.
    /// </summary>
    public enum ServerMessageType : sbyte
    {
        /// <summary>
        ///		A <see cref="AuthenticationStatus"/> message.
        /// </summary>
        Authentication = 0x52,

        /// <summary>
        ///		A <see cref="Models.CommandComplete"/> message.
        /// </summary>
        CommandComplete = 0x43,

        /// <summary>
        ///		A <see cref="Models.CommandDataDescription"/> message.
        /// </summary>
        CommandDataDescription = 0x54,

        /// <summary>
        ///		A <see cref="Models.Data"/> message.
        /// </summary>
        Data = 0x44,

        /// <summary>
        ///		A <see cref="DumpBlock"/> message.
        /// </summary>
        DumpBlock = 0x3d,

        /// <summary>
        ///		A <see cref="DumpHeader"/> message.
        /// </summary>
        DumpHeader = 0x40,

        /// <summary>
        ///		A <see cref="ErrorResponse"/> message.
        /// </summary>
        ErrorResponse = 0x45,

        /// <summary>
        ///		A <see cref="LogMessage"/> message.
        /// </summary>
        LogMessage = 0x4c,

        /// <summary>
        ///		A <see cref="ParameterStatus"/> message.
        /// </summary>
        ParameterStatus = 0x53,

        /// <summary>
        ///		A <see cref="PrepareComplete"/> message.
        /// </summary>
        PrepareComplete = 0x31,

        /// <summary>
        ///		A <see cref="ReadyForCommand"/> message.
        /// </summary>
        ReadyForCommand = 0x5a,

        /// <summary>
        ///		A <see cref="RestoreReady"/> message.
        /// </summary>
        RestoreReady = 0x2b,

        /// <summary>
        ///		A <see cref="ServerHandshake"/> message.
        /// </summary>
        ServerHandshake = 0x76,

        /// <summary>
        ///		A <see cref="ServerKeyData"/> message.
        /// </summary>
        ServerKeyData = 0x4b
    }
}
