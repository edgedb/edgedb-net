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
        /// <summary>
        ///     Gets the name of the protocol extension.
        /// </summary>
        public readonly string Name;

        /// <summary>
        ///     Gets a collection of headers for this protocol extension.
        /// </summary>
        public readonly IReadOnlyCollection<Annotation> Headers
            => _headers.ToImmutableArray();

        private readonly Annotation[] _headers;
        internal ProtocolExtension(ref PacketReader reader)
        {
            Name = reader.ReadString();
            _headers = reader.ReadAnnotaions();
        }

        internal void Write(PacketWriter writer)
        {
            writer.Write(Name);
        }
    }
}
