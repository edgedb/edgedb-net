using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Binary
{
    /// <summary>
    ///     Represents a protocol extension.
    /// </summary>
    internal readonly struct ProtocolExtension
    {
        internal int Size
            => Encoding.UTF8.GetByteCount(Name) + Annotations.Sum(x => x.Size);

        /// <summary>
        ///     The name of the protocol extension.
        /// </summary>
        public readonly string Name;

        /// <summary>
        ///     A collection of headers for this protocol extension.
        /// </summary>
        public readonly Annotation[] Annotations;

        internal ProtocolExtension(ref PacketReader reader)
        {
            Name = reader.ReadString();
            Annotations = reader.ReadAnnotations();
        }

        internal void Write(ref PacketWriter writer)
        {
            writer.Write(Name);
            writer.Write(Annotations);
        }
    }
}
