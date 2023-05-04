using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Tests.Integration.SharedTests
{
    public interface IResultNode
    {
        string? Type { get; }
        object? Value { get; }
    }
}
