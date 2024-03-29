namespace EdgeDB;

/// <summary>
///     Represents a protocol version used within EdgeDB.
/// </summary>
public sealed class ProtocolVersion : IComparable<ProtocolVersion>
{
    /// <summary>
    ///     The default protocol version used for dump/restore operations.
    /// </summary>
    public static readonly ProtocolVersion DumpRestoreDefaultVersion = (1, 0);

    /// <summary>
    ///     The default protocol version used for the edgedb binary protocol.
    /// </summary>
    public static readonly ProtocolVersion EdgeDBBinaryDefaultVersion = (2, 0);

    /// <summary>
    ///     Constructs a new <see cref="ProtocolVersion" />.
    /// </summary>
    /// <param name="major">The major component of the protocol.</param>
    /// <param name="minor">The minor component of the protocol.</param>
    public ProtocolVersion(ushort major, ushort minor)
    {
        Major = major;
        Minor = minor;
    }

    /// <summary>
    ///     Gets the major component of the protocol.
    /// </summary>
    public ushort Major { get; init; }

    /// <summary>
    ///     Gets the minor version of the protocol.
    /// </summary>
    public ushort Minor { get; init; }

    private int IntValue
        => Minor + (Major << 16);

    public int CompareTo(ProtocolVersion? other)
    {
        if (other is null)
        {
            return int.MinValue;
        }

        return IntValue - other.IntValue;
    }

    /// <inheritdoc />
    public override string ToString() => $"{Major}.{Minor}";

    /// <inheritdoc />
    public bool Equals(in ushort major, in ushort minor) => Major == major && Minor == minor;

    public override bool Equals(object? obj)
    {
        if (obj is not ProtocolVersion version)
            return base.Equals(obj);

        return version.Major == Major && version.Minor == Minor;
    }

    public static implicit operator ProtocolVersion((int major, int minor) v) => new((ushort)v.major, (ushort)v.minor);

    public override int GetHashCode()
        => HashCode.Combine(Major, Minor);

    public static bool operator ==(ProtocolVersion left, ProtocolVersion right)
    {
        if (left is null)
            return right is null;
        return left.Equals(right);
    }

    public static bool operator !=(ProtocolVersion left, ProtocolVersion right) => !(left == right);

    public static bool operator <(ProtocolVersion left, ProtocolVersion right) =>
        left is null ? right is not null : left.CompareTo(right) < 0;

    public static bool operator <=(ProtocolVersion left, ProtocolVersion right) =>
        left is null || left.CompareTo(right) <= 0;

    public static bool operator >(ProtocolVersion left, ProtocolVersion right) =>
        left is not null && left.CompareTo(right) > 0;

    public static bool operator >=(ProtocolVersion left, ProtocolVersion right) =>
        left is null ? right is null : left.CompareTo(right) >= 0;
}
