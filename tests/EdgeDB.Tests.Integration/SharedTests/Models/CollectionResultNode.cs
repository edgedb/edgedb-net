using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Tests.Integration.SharedTests
{
    public class CollectionResultNode : IResultNode
    {
        public string? Type { get; set; }

        public object? Value { get; set; }

        public string? ElementType { get; set; }
    }
}
