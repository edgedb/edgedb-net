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
    public readonly struct ProtocolExtension
    {
        internal int Size
            => Encoding.UTF8.GetByteCount(Name) + _annotations.Sum(x => x.Size);

        /// <summary>
        ///     Gets the name of the protocol extension.
        /// </summary>
        public readonly string Name;

        /// <summary>
        ///     Gets a collection of headers for this protocol extension.
        /// </summary>
        public readonly IReadOnlyCollection<Annotation> Headers
            => _annotations.ToImmutableArray();

        private readonly Annotation[] _annotations;
        internal ProtocolExtension(ref PacketReader reader)
        {
            Name = reader.ReadString();
            _annotations = reader.ReadAnnotations();
        }

        internal void Write(ref PacketWriter writer)
        {
            writer.Write(Name);
            writer.Write(_annotations);
        }
    }
}
