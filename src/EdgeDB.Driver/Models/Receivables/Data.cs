using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Models
{
    public struct Data : IReceiveable
    {
        public ServerMessageTypes Type => ServerMessageTypes.Data;

        public byte[] PayloadData { get; set; }

        void IReceiveable.Read(PacketReader reader, uint length, EdgeDBTcpClient client)
        {
            // skip arary since its always one, errr should be one
            var numElements = reader.ReadUInt16();
            if (numElements != 1)
            {
                throw new Exception($"Expected one element array for data, got {numElements}");
            }

            var payloadLength = reader.ReadUInt32();

            PayloadData = reader.ReadBytes((int)payloadLength); 
        }
    }
}
