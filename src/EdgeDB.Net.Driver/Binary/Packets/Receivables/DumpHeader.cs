using EdgeDB.Utils;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Binary.Packets
{
    /// <summary>
    ///     Represents the <see href="https://www.edgedb.com/docs/reference/protocol/messages#dump-header">Dump Header</see> packet.
    /// </summary>
    internal readonly struct DumpHeader : IReceiveable
    {
        /// <inheritdoc/>
        public ServerMessageType Type 
            => ServerMessageType.DumpHeader;

        /// <summary>
        ///     Gets the sha1 hash of this packets data, used when writing a dump file.
        /// </summary>
        public IReadOnlyCollection<byte> Hash
            => RawHash.ToImmutableArray();

        /// <summary>
        ///     Gets the length of this packets data, used when writing a dump file.
        /// </summary>
        public int Length { get; }

        /// <summary>
        ///     Gets a collection of attributes sent with this packet.
        /// </summary>
        public IReadOnlyCollection<KeyValue> Attributes
            => _attributes.ToImmutableArray();

        /// <summary>
        ///     Gets the EdgeDB major version.
        /// </summary>
        public ushort MajorVersion { get; }

        /// <summary>
        ///     Gets the EdgeDB minor version.
        /// </summary>
        public ushort MinorVersion { get; }

        /// <summary>
        ///     Gets the schema currently within the database.
        /// </summary>
        public string? SchemaDDL { get; }

        /// <summary>
        ///     Gets a collection of types within the database.
        /// </summary>
        public IReadOnlyCollection<DumpTypeInfo> Types { get; }

        /// <summary>
        ///     Gets a collection of descriptors used to define the types in <see cref="Types"/>.
        /// </summary>
        public IReadOnlyCollection<DumpObjectDescriptor> Descriptors { get; }

        internal byte[] Raw { get; }
        internal byte[] RawHash { get; }

        private readonly KeyValue[] _attributes;

        internal DumpHeader(ref PacketReader reader, int length)
        {
            Length = length;
            var rawBuffer = reader.ReadBytes(length);

            Raw = rawBuffer.ToArray();

            RawHash = SHA1.Create().ComputeHash(Raw);

            var innerReader = PacketReader.CreateFrom(rawBuffer);

            _attributes = innerReader.ReadKeyValues();
            MajorVersion = innerReader.ReadUInt16();
            MinorVersion = innerReader.ReadUInt16();
            SchemaDDL = innerReader.ReadString();

            var numTypeInfo = innerReader.ReadUInt32();
            DumpTypeInfo[] typeInfo = new DumpTypeInfo[numTypeInfo];

            for (uint i = 0; i != numTypeInfo; i++)
                typeInfo[i] = new DumpTypeInfo(ref innerReader);

            var numDescriptors = innerReader.ReadUInt32();
            DumpObjectDescriptor[] descriptors = new DumpObjectDescriptor[numDescriptors];

            for (uint i = 0; i != numDescriptors; i++)
                descriptors[i] = new DumpObjectDescriptor(ref innerReader);

            Types = typeInfo;
            Descriptors = descriptors;
        }
    }

    /// <summary>
    ///     Represents the type info sent within a <see cref="DumpHeader"/> packet.
    /// </summary>
    internal readonly struct DumpTypeInfo
    {
        /// <summary>
        ///     Gets the name of this type info.
        /// </summary>
        public string Name { get; }

        /// <summary>
        ///     Gets the class of this type info.
        /// </summary>
        public string Class { get; }

        /// <summary>
        ///     Gets the Id of the type info.
        /// </summary>
        public Guid Id { get; }

        internal DumpTypeInfo(ref PacketReader reader)
        {
            Name = reader.ReadString();
            Class = reader.ReadString();
            Id = reader.ReadGuid();
        }
    }

    /// <summary>
    ///     Represents a object descriptor sent within the <see cref="DumpHeader"/> packet.
    /// </summary>
    internal readonly struct DumpObjectDescriptor
    {
        /// <summary>
        ///     Gets the object Id that the descriptor describes.
        /// </summary>
        public Guid ObjectId { get; }

        /// <summary>
        ///     Gets the description of the object.
        /// </summary>
        public IReadOnlyCollection<byte> Description { get; }

        /// <summary>
        ///     Gets a collection of dependencies that this descriptor relies on.
        /// </summary>
        public IReadOnlyCollection<Guid> Dependencies { get; }

        internal DumpObjectDescriptor(ref PacketReader reader)
        {
            ObjectId = reader.ReadGuid();
            Description = reader.ReadByteArray();

            var numDep = reader.ReadUInt16();
            Guid[] dep = new Guid[numDep];

            for (ushort i = 0; i != numDep; i++)
            {
                dep[i] = reader.ReadGuid();
            }

            Dependencies = dep;
        }
    }
}
