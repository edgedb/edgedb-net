using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Models
{
    public struct ParameterStatus : IReceiveable
    {
        public ServerMessageTypes Type => ServerMessageTypes.ParameterStatus;

        public string Name { get; set; }
        public byte[] Value { get; set; }

        public void Read(PacketReader reader, uint length, EdgeDBTcpClient client)
        {
            Name = reader.ReadString();
            Value = reader.ReadByteArray();
        }
    }
}
