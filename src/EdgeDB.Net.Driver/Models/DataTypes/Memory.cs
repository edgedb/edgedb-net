namespace EdgeDB.DataTypes;

/// <summary>
///     Represents the memory type in EdgeDB.
/// </summary>
public readonly struct Memory
{
    /// <summary>
    ///     Gets the total amount of bytes for this memory object.
    /// </summary>
    public long TotalBytes { get; }

    /// <summary>
    ///     Gets the total amount of megabytes for this memory object.
    /// </summary>
    public long TotalMegabytes
        => TotalBytes / 1024 / 1024;

    internal Memory(long bytes)
    {
        TotalBytes = bytes;
    }
}
