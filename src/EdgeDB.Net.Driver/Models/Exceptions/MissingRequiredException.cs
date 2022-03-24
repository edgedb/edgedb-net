using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB
{
    public class MissingRequiredException : EdgeDBException
    {
        public MissingRequiredException()
            : base("Missing required result from query")
        {

        }
    }
}
