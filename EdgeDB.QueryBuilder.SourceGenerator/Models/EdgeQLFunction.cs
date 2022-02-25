using System;
using System.Collections.Generic;
using System.Text;

namespace EdgeDB.QueryBuilder.SourceGenerator
{
    public class EdgeQLFunction
    {
        public string? Name { get; set; }
        public List<string>? Parameters { get; set; }
        public string? Return { get; set; }
    }
}
