using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB
{
    public class ConnectionFailedException : EdgeDBException
    {
        public ConnectionFailedException(int attempts)
            : base($"The connection failed to be established after {attempts} attempt(s)", false, false)
        {

        }
    }
}
