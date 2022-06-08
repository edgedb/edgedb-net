using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB
{
    /// <summary>
    ///     An enum representing the format of a commands result.
    /// </summary>
    public enum IOFormat : byte
    {
        /// <summary>
        ///     The format will be encoded as binary following the 
        ///     <see href="https://www.edgedb.com/docs/reference/protocol/dataformats#data-wire-formats">Data Wire Formats</see> protocol.
        /// </summary>
        Binary = 0x62,
        
        /// <summary>
        ///     The format will be encoded as json.
        /// </summary>
        Json = 0x6a,

        /// <summary>
        ///     The format will be encoded as json elements.
        /// </summary>
        JsonElements = 0x4a
    }
}
