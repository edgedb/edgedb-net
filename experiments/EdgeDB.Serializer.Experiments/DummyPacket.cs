using EdgeDB.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB
{
    [System.Runtime.InteropServices.StructLayoutAttribute(System.Runtime.InteropServices.LayoutKind.Sequential, Pack = 0)]
    public readonly unsafe struct DummyPacket
    {
        [MarshalAs(UnmanagedType.I1)]
        public readonly ServerMessageType Type;

        [MarshalAs(UnmanagedType.U4)]
        public readonly uint Length;

        [MarshalAs(UnmanagedType.U4)]
        public readonly uint NameLength;
        [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)]
        public readonly byte* NameValue;

        public readonly uint ValueLength;
        [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 4)]
        public readonly byte* ValueValue;

        //public readonly Bytes Name;
        //public readonly Bytes Value;

        
    }

    [System.Runtime.InteropServices.StructLayoutAttribute(System.Runtime.InteropServices.LayoutKind.Sequential)]
    public unsafe readonly ref struct Bytes
    {
        public readonly uint Length;
        [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)]
        public readonly byte* Data;
    }
}
