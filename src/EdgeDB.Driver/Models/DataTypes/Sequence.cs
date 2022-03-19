using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB
{
    /// <summary>
    ///     Represents a <see href="https://www.edgedb.com/docs/datamodel/primitives#sequences">Sequence</see> base type in EdgeDB.
    /// </summary>
    public abstract class Sequence 
    {
        /// <summary>
        ///     Gets the value of the sequence.
        /// </summary>
        public long Value { get; protected set; }
    }
}
