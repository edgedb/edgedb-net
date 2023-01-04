using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.DataTypes
{
    /// <summary>
    ///     Represents a standard json value.
    /// </summary>
    public readonly struct Json 
    {
        /// <summary>
        ///     Gets or sets the raw json value.
        /// </summary>
        public readonly string? Value;

        /// <summary>
        ///     Creates a new json type with a provided value.
        /// </summary>
        /// <param name="value">The raw json value of this json object.</param>
        public Json(string? value)
        {
            Value = value;
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
        public T? Deserialize<T>(JsonSerializer? serializer = null)
            => Value is null
                ? default
                : serializer is not null
                    ? serializer.Deserialize<T>(new JsonTextReader(new StringReader(Value)))
                    : EdgeDBConfig.JsonSerializer.DeserializeObject<T>(Value);

        /// <summary>
        ///     Serializes an <see cref="object"/> to <see cref="Json"/> using the default
        ///     <see cref="EdgeDBConfig.JsonSerializer"/> or <paramref name="serializer"/> if specified.
        /// </summary>
        /// <param name="value">The value to serialize.</param>
        /// <param name="serializer">The optional serializer to use when serializing.</param>
        /// <returns>The json representation of <paramref name="value"/>.</returns>
        public static Json Serialize(object? value, JsonSerializer? serializer = null)
            => (serializer ?? EdgeDBConfig.JsonSerializer).SerializeObject(value);

        public static implicit operator string?(Json j) => j.Value;
        public static implicit operator Json(string? value) => new(value);
    }
}
