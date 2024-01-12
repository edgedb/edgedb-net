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
            return new SubQuery(writer => writer
                .Wrapped(writer => writer
                    .Append("select ")
                    .Append(queryType.GetEdgeDBTypeName())
                    .Append(" filter .id = <uuid>")
                    .SingleQuoted(id.ToString())
                )
            );
        }
    }
}
