using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB
{
    public class CustomClientException : EdgeDBException
    {
        public CustomClientException(string message)
            : base(message, false, false)
        {

        }
    }
}
