using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace EdgeDB.Binary.Protocol.DumpRestore.V1._0;

// 0x00 -> 0x02 Type
// 0x02 -> 0x16 Hash
// 0x16 -> 0x20 Length
[StructLayout(LayoutKind.Explicit, Size = 26)]
internal struct DumpSectionHeader
{
    [FieldOffset(0)] public char Type;

    [FieldOffset(22)] public int Length;

    public readonly unsafe ReadOnlySpan<byte> Hash
        => new((byte*)Unsafe.AsPointer(ref Unsafe.AsRef(in this)) + 1, 20);
}
