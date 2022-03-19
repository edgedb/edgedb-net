using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Models
{
    /// <summary>
    ///     Represents a protocol extension.
    /// </summary>
    public struct ProtocolExtension
    {
        /// <summary>
        ///     Gets the name of the protocol extension.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        ///     Gets a collection of headers for this protocol extension.
        /// </summary>
        public IReadOnlyCollection<Header> Headers { get; private set; }

        internal void Write(PacketWriter writer)
        {
            writer.Write(Name);
        }

        internal void Read(PacketReader reader)
        {
            Name = reader.ReadString();
            Headers = reader.ReadHeaders();
        }
    }
}
