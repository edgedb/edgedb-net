using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB
{
    /// <summary>
    ///     Represents an exception that occurs when a queries cardinality 
    ///     isn't what the client was expecting.
    /// </summary>
    public class ResultCardinalityMismatchException : EdgeDBException
    {
        /// <summary>
        ///     Constructs a new <see cref="ResultCardinalityMismatchException"/>.
        /// </summary>
        /// <param name="expected">The expected cardinality.</param>
        /// <param name="actual">The actual cardinality</param>
        public ResultCardinalityMismatchException(Cardinality expected, Cardinality actual)
            : base($"Got mismatch on cardinality of query. Expected \"{expected}\" but got \"{actual}\"")
        {

        }
    }
}
