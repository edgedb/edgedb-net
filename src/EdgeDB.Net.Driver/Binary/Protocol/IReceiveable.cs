using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Binary.Protocol
{
    /// <summary>
    ///     Represents a generic packet received from the server.
    /// </summary>
    internal interface IReceiveable
    {
        /// <summary>
        ///     Gets the type of the message.
        /// </summary>
        ServerMessageType Type { get; }
    }
}
