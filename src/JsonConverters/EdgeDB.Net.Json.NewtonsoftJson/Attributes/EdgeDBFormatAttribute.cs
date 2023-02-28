using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Json.NewtonsoftJson
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public sealed class EdgeDBFormatAttribute : Attribute
    { }
}
