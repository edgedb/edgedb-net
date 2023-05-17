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

        public int Length
            => _length;

        private readonly int _length;
        private readonly int _packetPosition;

        private readonly BufferContract* _contract;

        public ReservedBuffer(
            BufferContract* contract,
            int start,
            int length)
        {
            _contract = contract;
            _length = length;
            _packetPosition = start;
        }

        public PacketReader GetReader()
            => _contract->CreateReader();

        public void Dispose()
        {
            _contract->Dispose();
        }
    }
}
