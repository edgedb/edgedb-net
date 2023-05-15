using EdgeDB.Binary.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using static EdgeDB.Binary.Packets.PacketContract;

namespace EdgeDB.Binary
{
    internal unsafe readonly struct ReservedBuffer
    {
        public int PacketPosition
            => _packetPosition;

        public int Size
            => _size;

        private readonly int _size;
        private readonly int _packetPosition;

        private readonly BufferContract* _contract;

        public ReservedBuffer(
            BufferContract* contract,
            int size)
        {
            _contract = contract;
            _size = size;
        }

        public PacketReader GetReader()
            => _contract->CreateReader();

        public void Dispose()
        {
            _contract->Dispose();
        }
    }
}
