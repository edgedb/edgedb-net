using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB
{
    public static class EdgeDBExtensions
    {
        internal static SubQuery SelectSubQuery(this Guid id, Type queryType)
        {
            return new SubQuery($"(select {queryType.GetEdgeDBTypeName()} filter .id = <uuid>\"{id}\")");
        }
    }
}
