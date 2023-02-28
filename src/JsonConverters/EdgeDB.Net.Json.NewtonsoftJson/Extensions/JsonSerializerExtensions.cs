using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Json.NewtonsoftJson
{
    internal static class JsonSerializerExtensions
    {
        public static T? DeserializeObject<T>(this JsonSerializer serializer, string value)
        {
            using var textReader = new StringReader(value);
            using var jsonReader = new JsonTextReader(textReader);
            return serializer.Deserialize<T>(jsonReader);
        }

        public static string SerializeObject(this JsonSerializer serialier, object? value)
        {
            using var textWriter = new StringWriter();
            using var jsonWriter = new JsonTextWriter(textWriter);
            serialier.Serialize(jsonWriter, value);
            return textWriter.ToString();
        }
    }
}
