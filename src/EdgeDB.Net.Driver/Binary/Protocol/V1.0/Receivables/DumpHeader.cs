using System.Security.Cryptography;

namespace EdgeDB.Binary.Protocol.V1._0.Packets;

/// <summary>
///     Represents the <see href="https://www.edgedb.com/docs/reference/protocol/messages#dump-header">Dump Header</see>
///     packet.
/// </summary>
internal readonly struct DumpHeader : IReceiveable
{
    /// <inheritdoc />
    public ServerMessageType Type
        => ServerMessageType.DumpHeader;

    /// <summary>
    ///     The length of this packets data, used when writing a dump file.
    /// </summary>
    public readonly int Length;

    /// <summary>
    ///     The EdgeDB major version.
    /// </summary>
    public readonly ushort MajorVersion;

    /// <summary>
    ///     The EdgeDB minor version.
    /// </summary>
    public readonly ushort MinorVersion;

    /// <summary>
    ///     The schema currently within the database.
    /// </summary>
    public readonly string? SchemaDDL;

    /// <summary>
    ///     A collection of types within the database.
    /// </summary>
    public readonly DumpTypeInfo[] Types;

    /// <summary>
    ///     A collection of descriptors used to define the types in <see cref="Types" />.
    /// </summary>
    public readonly DumpObjectDescriptor[] Descriptors;

    public readonly byte[] Raw;

    /// <summary>
    ///     The sha1 hash of this packets data, used when writing a dump file.
    /// </summary>
    public readonly byte[] Hash;

    /// <summary>
    ///     A collection of attributes sent with this packet.
    /// </summary>
    public readonly KeyValue[] Attributes;

    internal DumpHeader(ref PacketReader reader, in int length)
    {
        Length = length;
        reader.ReadBytes(length, out var rawBuffer);

        Raw = rawBuffer.ToArray();

        Hash = SHA1.Create().ComputeHash(Raw);

        var r = new PacketReader(rawBuffer);

        Attributes = r.ReadKeyValues();
        MajorVersion = r.ReadUInt16();
        MinorVersion = r.ReadUInt16();
        SchemaDDL = r.ReadString();

        var numTypeInfo = r.ReadUInt32();
        var typeInfo = new DumpTypeInfo[numTypeInfo];

        for (uint i = 0; i != numTypeInfo; i++)
            typeInfo[i] = new DumpTypeInfo(ref r);

        var numDescriptors = r.ReadUInt32();
        var descriptors = new DumpObjectDescriptor[numDescriptors];

        for (uint i = 0; i != numDescriptors; i++)
            descriptors[i] = new DumpObjectDescriptor(ref r);

        Types = typeInfo;
        Descriptors = descriptors;
    }
}

/// <summary>
///     Represents the type info sent within a <see cref="DumpHeader" /> packet.
/// </summary>
internal readonly struct DumpTypeInfo
{
    /// <summary>
    ///     The name of this type info.
    /// </summary>
    public readonly string Name;

    /// <summary>
    ///     The class of this type info.
    /// </summary>
    public readonly string Class;

    /// <summary>
    ///     The Id of the type info.
    /// </summary>
    public readonly Guid Id;

    internal DumpTypeInfo(ref PacketReader reader)
    {
        Name = reader.ReadString();
        Class = reader.ReadString();
        Id = reader.ReadGuid();
    }
}

/// <summary>
///     Represents a object descriptor sent within the <see cref="DumpHeader" /> packet.
/// </summary>
internal readonly struct DumpObjectDescriptor
{
    /// <summary>
    ///     The object Id that the descriptor describes.
    /// </summary>
    public readonly Guid ObjectId;

    /// <summary>
    ///     The description of the object.
    /// </summary>
    public readonly byte[] Description;

    /// <summary>
    ///     Gets a collection of dependencies that this descriptor relies on.
    /// </summary>
    public readonly Guid[] Dependencies;

    internal DumpObjectDescriptor(ref PacketReader reader)
    {
        ObjectId = reader.ReadGuid();
        Description = reader.ReadByteArray();

        var numDep = reader.ReadUInt16();
        var dep = new Guid[numDep];

        for (ushort i = 0; i != numDep; i++)
        {
            dep[i] = reader.ReadGuid();
        }

        Dependencies = dep;
    }
}
