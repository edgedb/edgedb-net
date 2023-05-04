using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Tests.Integration.SharedTests
{
    public class ResultNode : IResultNode
    {
        public string? Type { get; set; }

        public object? Value { get; set; }
    }
}
