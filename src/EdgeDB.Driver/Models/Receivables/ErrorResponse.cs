using EdgeDB.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Models
{
    public struct ErrorResponse : IReceiveable
    {
        public ServerMessageTypes Type => ServerMessageTypes.ErrorResponse;

        public ErrorSeverity Severity { get; private set; }

        public uint ErrorCode { get; private set; }

        public string Message { get; private set; }

        public Header[] Headers { get; private set; }

        void IReceiveable.Read(PacketReader reader, uint length, EdgeDBTcpClient client)
        {
            Severity = (ErrorSeverity)reader.ReadByte();
            ErrorCode = reader.ReadUInt32();
            Message = reader.ReadString();
            Headers = reader.ReadHeaders();
        }
    }
}
