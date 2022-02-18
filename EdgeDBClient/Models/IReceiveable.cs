using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Models
{
    public interface IReceiveable
    {
        ServerMessageTypes Type { get; }

        void Read(PacketReader reader, uint length, EdgeDBClient client);
    }
}
