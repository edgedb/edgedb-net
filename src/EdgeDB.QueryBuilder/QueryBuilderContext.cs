using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB
{
    internal class QueryBuilderContext
    {
        public bool DontSelectProperties { get; set; }
        public bool UseDetachedSelects { get; set; }
        public bool IntrospectObjectIds { get; set; }

        public QueryBuilderContext? Parent { get; set; }

        public QueryBuilderContext Enter(Action<QueryBuilderContext> modifier)
        {
            var context = new QueryBuilderContext
            {
                Parent = Parent,
                DontSelectProperties = DontSelectProperties
            };

            modifier(context);
            return context;
        }
    }
}
