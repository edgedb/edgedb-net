using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Models.DataTypes
{
    public class Set<T> : List<T> 
    {
        public Set() : base()
        {
        }

        public Set(IEnumerable<T> collection) : base(collection)
        {
        }

        public Set(int capacity) : base(capacity)
        {
        }
    }
}
