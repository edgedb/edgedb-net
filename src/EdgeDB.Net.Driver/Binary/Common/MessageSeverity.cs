namespace EdgeDB;

/// <summary>
///     Represents the log message severity
/// </summary>
internal enum MessageSeverity : byte
{
    Debug = 0x14,
    Info = 0x28,
    Notice = 0x3c,
    Warning = 0x50
}
