namespace EdgeDB
{
    internal class QueryBuilderContext
    {
        public bool DontSelectProperties { get; set; }
        public bool UseDetached { get; set; }
        public bool IntrospectObjectIds { get; set; }
        public QueryBuilderContext? Parent { get; set; }
        public List<string> TrackedVariables { get; set; } = new();
        public bool IncludeEmptySets { get; set; } = true;
        public bool IsVariable { get; set; }
        public string? VariableName { get; set; }
        public bool AllowComputedValues { get; set; } = true;
        public int? MaxAggregationDepth { get; set; } = 10;
        public bool LimitToOne { get; set; }
        public List<QueryBuilder> TrackedSubQueries { get; set; } = new();
        public bool ExplicitShapeDefinition { get; set; }


        // sub query type info
        public Type? ParentQueryType { get; set; }
        public string? ParentQueryTypeName { get; set; }

        public QueryBuilderContext Enter(Action<QueryBuilderContext> modifier)
        {
            var context = new QueryBuilderContext
            {
                Parent = this,
                DontSelectProperties = DontSelectProperties,
                UseDetached = UseDetached,
                IntrospectObjectIds = IntrospectObjectIds,
                TrackedVariables = TrackedVariables,
                TrackedSubQueries = TrackedSubQueries,
                IsVariable = IsVariable,
                VariableName = VariableName,
                IncludeEmptySets = IncludeEmptySets,
                AllowComputedValues = AllowComputedValues,
                MaxAggregationDepth = MaxAggregationDepth,
                ParentQueryType = ParentQueryType,
                ParentQueryTypeName = ParentQueryTypeName,
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

        public void AddTrackedSubQuery(QueryBuilder builder)
        {
            if (Parent != null)
                Parent.AddTrackedSubQuery(builder);
            else
                TrackedSubQueries.Add(builder);
        }
    }
}
