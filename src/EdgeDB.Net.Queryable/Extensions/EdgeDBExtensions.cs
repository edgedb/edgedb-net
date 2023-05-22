using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB
{
    public static class EdgeDBExtensions
    {
        public static EdgeDBQueryable<T> GetCollection<T>(this IEdgeDBQueryable client)
        {
            var provider = new EdgeDBQueryProvider();

            return new EdgeDBQueryable<T>(provider);
        }
    }
}
