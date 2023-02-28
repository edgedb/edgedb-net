using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EdgeDB;
using EdgeDB.Json.NewtonsoftJson;

namespace EdgeDB
{
    public static class JsonExtensions
    {
        private static readonly JsonSerializer _serializer = new JsonSerializer()
        {
            ContractResolver = new EdgeDBContractResolver(),
            NullValueHandling = NullValueHandling.Ignore,
        };

        /// <summary>
        ///     Gets the <see cref="JsonSerializer"/> capable of serializing/deserializing edgedb types.
        /// </summary>
        public static JsonSerializer GetJsonSerializer(this DataTypes.Json _)
        {
            return _serializer;
        }

        /// <summary>
        ///     Deserializes <see cref="Json.Value"/> into a dotnet type using Newtonsoft.Json.
        /// </summary>
        /// <remarks>
        ///     If <see cref="Value"/> is null, the <see langword="default"/> value 
        ///     of <typeparamref name="T"/> will be returned.
        /// </remarks>
        /// <typeparam name="T">The type to deserialize as.</typeparam>
        /// <param name="serializer">
        ///     The optional custom serializer to use to deserialize <see cref="Value"/>.
        /// </param>
        /// <returns>
        ///     The deserialized form of <see cref="Value"/>; or <see langword="default"/>.
        /// </returns>
        public static T? Deserialize<T>(this DataTypes.Json json, JsonSerializer? serializer = null)
            => json.Value is null
                ? default
                : serializer is not null
                    ? serializer.DeserializeObject<T>(json.Value)
                    : _serializer.DeserializeObject<T>(json.Value);
    }
}
