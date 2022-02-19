using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.QueryBuilder.Operators
{
    public interface IEdgeQLOperator
    {
        ExpressionType Operator { get; }
        string EdgeQLOperator { get; }

        string Build(params object[] args);
    }
}
