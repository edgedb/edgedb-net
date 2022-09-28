using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB
{
    /// <summary>
    ///     Represents an exception that occurs when required data isn't returned.
    /// </summary>
    public class MissingRequiredException : EdgeDBException
    {
        /// <summary>
        ///     Constructs a new <see cref="MissingRequiredException"/>.
        /// </summary>
        public MissingRequiredException()
            : base("Missing required result from query")
        {

        }
    }
}
