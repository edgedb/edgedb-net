using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Models
{
    /// <summary>
    ///     Represents the <see href="https://www.edgedb.com/docs/reference/protocol/messages#dump-header">Dump Header</see> packet.
    /// </summary>
    public struct DumpHeader : IReceiveable
    {
        /// <inheritdoc/>
        public ServerMessageType Type => ServerMessageType.DumpHeader;

        /// <summary>
        ///     Gets the sha1 hash of this packets data, used when writing a dump file.
        /// </summary>
        public IReadOnlyCollection<byte> Hash { get; private set; }

        /// <summary>
        ///     Gets the length of this packets data, used when writing a dump file.
        /// </summary>
        public uint Length { get; private set; }

        /// <summary>
        ///     Gets a collection of headers sent with this packet.
        /// </summary>
        public IReadOnlyCollection<Header> Headers { get; private set; }

        /// <summary>
        ///     Gets the EdgeDB major version.
        /// </summary>
        public ushort MajorVersion { get; private set; }

        /// <summary>
        ///     Gets the EdgeDB minor version.
        /// </summary>
        public ushort MinorVersion { get; private set; }

        /// <summary>
        ///     Gets the schema currently within the database.
        /// </summary>
        public string? SchemaDDL { get; private set; }

        /// <summary>
        ///     Gets a collection of types within the database.
        /// </summary>
        public IReadOnlyCollection<DumpTypeInfo> Types { get; private set; }

        /// <summary>
        ///     Gets a collection of descriptors used to define the types in <see cref="Types"/>.
        /// </summary>
        public IReadOnlyCollection<DumpObjectDescriptor> Descriptors { get; private set; }

        internal byte[] Raw { get; private set; }
        ulong IReceiveable.Id { get; set; }
        void IReceiveable.Read(PacketReader reader, uint length, EdgeDBBinaryClient client)
        {
            Length = length;
            Raw = reader.ReadBytes((int)length);

            Hash = SHA1.Create().ComputeHash(Raw);

            using(var r = new PacketReader(Raw))
            {
                Headers = r.ReadHeaders();
                MajorVersion = r.ReadUInt16();
                MinorVersion = r.ReadUInt16();
                SchemaDDL = r.ReadString();

                var numTypeInfo = r.ReadUInt32();
                DumpTypeInfo[] typeInfo = new DumpTypeInfo[numTypeInfo];

                for (uint i = 0; i != numTypeInfo; i++)
                    typeInfo[i] = new DumpTypeInfo().Read(r);

                var numDescriptors = r.ReadUInt32();
                DumpObjectDescriptor[] descriptors = new DumpObjectDescriptor[numDescriptors];

                for (uint i = 0; i != numDescriptors; i++)
                    descriptors[i] = new DumpObjectDescriptor().Read(r);

                Types = typeInfo;
                Descriptors = descriptors;
            }
        }

        IReceiveable IReceiveable.Clone()
            => (IReceiveable)MemberwiseClone();
    }

    /// <summary>
    ///     Represents the type info sent within a <see cref="DumpHeader"/> packet.
    /// </summary>
    public struct DumpTypeInfo
    {
        /// <summary>
        ///     Gets the name of this type info.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        ///     Gets the class of this type info.
        /// </summary>
        public string Class { get; private set; }

        /// <summary>
        ///     Gets the Id of the type info.
        /// </summary>
        public Guid Id { get; private set; }

        internal DumpTypeInfo Read(PacketReader reader)
        {
            Name = reader.ReadString();
            Class = reader.ReadString();
            Id = reader.ReadGuid();

            return this;
        }
    }

    /// <summary>
    ///     Represents a object descriptor sent within the <see cref="DumpHeader"/> packet.
    /// </summary>
    public struct DumpObjectDescriptor
    {
        /// <summary>
        ///     Gets the object Id that the descriptor describes.
        /// </summary>
        public Guid ObjectId { get; private set; }

        /// <summary>
        ///     Gets the description of the object.
        /// </summary>
        public IReadOnlyCollection<byte> Description { get; private set; }

        /// <summary>
        ///     Gets a collection of dependencies that this descriptor relies on.
        /// </summary>
        public IReadOnlyCollection<Guid> Dependencies { get; private set; }

        internal DumpObjectDescriptor Read(PacketReader reader)
        {
            ObjectId = reader.ReadGuid();
            Description = reader.ReadByteArray();

            var numDep = reader.ReadUInt16();
            Guid[] dep = new Guid[numDep];

            for(ushort i = 0; i != numDep; i++)
            {
                dep[i] = reader.ReadGuid();
            }

            Dependencies = dep;

            return this;
        }
    }
}
