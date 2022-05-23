using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB
{
    [AttributeUsage(AttributeTargets.Field)]
    internal class ShouldReconnectAttribute : Attribute
    {
    }
}
