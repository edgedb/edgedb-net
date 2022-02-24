using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB
{
    internal class QueryContext<TInner, TReturn>
    {
        public Type? ParameterType { get; set; }
        public string? ParameterName { get; set; }
        public Expression? Body { get; set; }

        public bool IsCharContext { get; set; } = false;

        public QueryContext() { }

        public QueryContext(Expression<Func<TInner, TReturn>> func)
        {
            Body = func.Body;
            ParameterType = func.Parameters[0].Type;
            ParameterName = func.Parameters[0].Name;
        }
    }
}
