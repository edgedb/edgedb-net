using EdgeDB.Codecs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Models
{
    public struct CommandComplete : IReceiveable
    {
        public ServerMessageTypes Type => ServerMessageTypes.CommandComplete;

        public AllowCapabilities UsedCapabilities { get; set; }

        public string Status { get; set; }

        public void Read(PacketReader reader, uint length, EdgeDBClient client)
        {
            var headers = reader.ReadHeaders();

            var usedCap = headers.Cast<Header?>().FirstOrDefault(x => x!.Value.Code == 0x1001);

            if (usedCap.HasValue)
            {
                UsedCapabilities = (AllowCapabilities)ICodec.GetScalarCodec<long>()!.Deserialize(usedCap.Value.Value);
            }

            Status = reader.ReadString();
        }
    }
}
