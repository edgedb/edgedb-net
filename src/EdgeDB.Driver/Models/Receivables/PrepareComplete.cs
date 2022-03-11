using EdgeDB.Codecs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Models
{
    public struct PrepareComplete : IReceiveable
    {
        public ServerMessageTypes Type => ServerMessageTypes.PrepareComplete;

        public AllowCapabilities Capabilities { get; set; }

        public Cardinality Cardinality { get; set; }

        public Guid InputTypedescId { get; set; }

        public Guid OutputTypedescId { get; set; }

        void IReceiveable.Read(PacketReader reader, uint length, EdgeDBTcpClient client)
        {
            var headers = reader.ReadHeaders();

            var capabilities = headers.Cast<Header?>().FirstOrDefault(x => x!.Value.Code == 0x1001);

            if(capabilities != null)
            {
                Capabilities = (AllowCapabilities)ICodec.GetScalarCodec<long>()!.Deserialize(capabilities.Value.Value);
            }

            Cardinality = (Cardinality)reader.ReadByte();
            InputTypedescId = reader.ReadGuid();
            OutputTypedescId = reader.ReadGuid();

        }
    }
}
