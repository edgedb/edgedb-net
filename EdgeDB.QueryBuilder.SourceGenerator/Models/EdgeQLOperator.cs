using System;
using System.Collections.Generic;
using System.Text;

namespace EdgeDB.QueryBuilder.SourceGenerator
{
    public class EdgeQLOperator
    {
        public string? Expression { get; set; }
        public string? Operator { get; set; }
        public string? Return { get; set; }
        public string? Name { get; set; }
        public List<EdgeQLFunction>? Functions { get; set; } = new();
    }
}
