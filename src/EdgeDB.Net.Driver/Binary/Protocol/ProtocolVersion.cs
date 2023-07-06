using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Binary.Protocol
{
    internal sealed class ProtocolVersion
    {
        public ushort Major { get; }
        public ushort Minor { get; }

        public ProtocolVersion(ushort major, ushort minor)
        {
            Major = major;
            Minor = minor;
        }

        public static implicit operator ProtocolVersion((int major, int minor) v) => new((ushort)v.major, (ushort)v.minor);

        public override string ToString()
        {
            return $"{Major}.{Minor}";
        }

        public bool Equals(in ushort major, in ushort minor)
        {
            return Major == major && Minor == minor;
        }
    }
}
