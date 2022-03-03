using System;
using System.Collections.Generic;
using System.Text;

namespace EdgeDB.QueryBuilder.OperatorGenerator
{
    public class EdgeQLFunction
    {
        public string? Name { get; set; }
        public List<string> Parameters { get; set; } = new();
        public string? Return { get; set; }
    }
}
