using EdgeDB.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB
{
    public class ResultCardinalityMismatchException : EdgeDBException
    {
        public ResultCardinalityMismatchException(Cardinality expected, Cardinality actual)
            : base($"Got mismatch on cardinality of query. Expected \"{expected}\" but got \"{actual}\"")
        {

        }
    }
}
