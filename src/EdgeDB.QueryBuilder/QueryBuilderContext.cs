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
        public List<string> TrackedVariables { get; set; } = new();
        public bool IncludeEmptySets { get; set; } = true;
        public bool IsVariable { get; set; }
        public string? VariableName { get; set; }

        public QueryBuilderContext Enter(Action<QueryBuilderContext> modifier)
        {
            var context = new QueryBuilderContext
            {
                Parent = this,
                DontSelectProperties = DontSelectProperties,
                UseDetachedSelects = UseDetachedSelects,
                IntrospectObjectIds = IntrospectObjectIds,
                TrackedVariables = TrackedVariables,
                IsVariable = IsVariable,
                VariableName = VariableName,
                IncludeEmptySets = IncludeEmptySets
            };

            modifier(context);
            return context;
        }

        public void AddTrackedVariable(string var)
        {
            if (Parent != null)
                Parent.AddTrackedVariable(var);
            else
                TrackedVariables.Add(var);
        }
    }
}
