using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Models
{
    public struct ServerKeyData : IReceiveable
    {
        public ServerMessageTypes Type => ServerMessageTypes.ServerKeyData;

        public byte[] Key { get; set; }

        public void Read(PacketReader reader, uint length, EdgeDBClient client)
        {
            Key = reader.ReadBytes(32);
        }
    }
}
