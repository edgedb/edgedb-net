using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB
{
    /// <summary>
    ///     Represents a generic exception that occured with the edgedb library.
    /// </summary>
    public class EdgeDBException : Exception
    {
        public EdgeDBException() { }

        public EdgeDBException(string? message) : base(message) { }

        public EdgeDBException(string? message, Exception? innerException) : base(message, innerException) { }
    }
}
