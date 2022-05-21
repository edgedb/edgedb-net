using EdgeDB.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB
{
    internal unsafe class Serializer
    {
        public static ServerMessageType* Deserialize()
        {
            var packet = new DummyPacket();

            var addr = &packet;

            var p = (ServerMessageType*)addr;

            return p;
        }
    }
}
