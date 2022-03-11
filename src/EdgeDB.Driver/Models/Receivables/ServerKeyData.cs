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

        void IReceiveable.Read(PacketReader reader, uint length, EdgeDBTcpClient client)
        {
            Key = reader.ReadBytes(32);
        }
    }
}
