using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace EdgeDB.Json.NewtonsoftJson
{
    public abstract class EdgeDBFormatConverter<T> : JsonConverter<T>
    {
        protected bool UseEdgeDBFormat { get; }

        public EdgeDBFormatConverter(bool useEdgeDBFormat)
        {
            UseEdgeDBFormat = useEdgeDBFormat;
        }

        protected void EnsureFormatted(Regex regex, Match match, string value)
        {
            if (!match.Success)
            {
                throw new JsonSerializationException($"Incorrect format for {this.GetType()}: expected to match \"{regex.ToString()}\", input: \"{value}\"");
            }
        }
    }
}
