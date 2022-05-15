namespace EdgeDB.DotnetTool
{
    internal class Property
    {
        public Type? Parent { get; set; }

        public string? Name { get; set; }
        public string? Type { get; set; }
        public bool Required { get; set; }
        public PropertyCardinality Cardinality { get; set; }
        public string? DefaultValue { get; set; }
        public bool ReadOnly { get; set; }
        public List<Constraint> Constraints { get; set; } = new();
        public Annotation? Annotation { get; set; }
        public List<Property> LinkProperties { get; set; } = new();

        public bool IsStrictlyConstraint { get; set; }
        public bool IsAbstract { get; set; }
        public bool IsLink { get; set; }
        public bool IsComputed { get; set; }
        public string? ComputedValue { get; set; }

        public string? Extending { get; set; }

        // used for builder
        public string? BuiltName { get; set; }
    }

    public enum PropertyCardinality
    {
        One,
        Multi
    }
}
