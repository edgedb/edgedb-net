using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB
{
    public class InvalidSignatureException : EdgeDBException
    {
        public InvalidSignatureException()
            : base("The received signature didn't match the expected one")
        {

        }
    }
}
