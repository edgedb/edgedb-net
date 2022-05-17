using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB
{
    /// <summary>
    ///     Represents an exception that occurs when the server signature is incorrect.
    /// </summary>
    public class InvalidSignatureException : EdgeDBException
    {
        public InvalidSignatureException()
            : base("The received signature didn't match the expected one")
        {

        }
    }
}
