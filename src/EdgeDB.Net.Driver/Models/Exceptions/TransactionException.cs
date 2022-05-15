using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB
{
    public class TransactionException : EdgeDBException
    {
        public TransactionException(string message, Exception? innerException = null)
            : base(message, innerException, false, false)
        {

        }
    }
}
