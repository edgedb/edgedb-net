using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB
{
    internal readonly struct BufferedSource
    {
#if LEGACY_BUFFERS
        public readonly byte[] Buffer;
        public readonly int Length;
#else
        public readonly Memory<byte> Buffer;
        public int Length
            => Buffer.Length;
#endif

        public Span<byte> Span
            =>
#if LEGACY_BUFFERS
            new(Buffer, 0, Length);
#else
            Buffer.Span;
#endif

        public BufferedSource(
#if LEGACY_BUFFERS
            byte[] buffer,
            int length
#else
            Memory<byte> buffer
#endif
            )
        {
            Buffer = buffer;

#if LEGACY_BUFFERS
            Length = length;
#endif
        }

#if LEGACY_BUFFERS
        public static implicit operator byte[](BufferedSource src) => src.Buffer;
#else
        public static implicit operator Memory<byte>(BufferedSource src) => src.Buffer;
        public static implicit operator BufferedSource(Memory<byte> mem) => new BufferedSource(mem);
#endif
    }
}
