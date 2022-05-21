using EdgeDB.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB
{
    [System.Runtime.InteropServices.StructLayoutAttribute(System.Runtime.InteropServices.LayoutKind.Sequential)]
    public unsafe readonly ref struct DummyPacket
    {
        public readonly ServerMessageType Type = ServerMessageType.ServerKeyData;

        public readonly ulong SomeNumber;

        public readonly byte* _content;
        private readonly int _contentLength;

        public DummyPacket()
        {
            byte[] arr = new byte[] { 0x48, 0x65, 0x6C, 0x6C, 0x6F };

            fixed (byte* c = arr)
            {
                _content = c;
            }

            _contentLength = arr.Length;

            SomeNumber = 1234;
        }

        public string GetContent()
            => Encoding.UTF8.GetString(_content, _contentLength);
    }
}
