using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB
{
    public class ConnectionFailedTemporarilyException : EdgeDBException
    {
        public ConnectionFailedTemporarilyException()
            : base("The connection could not be established at this time", true, true)
        {

        }
    }
}
