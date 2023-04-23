using EdgeDB.DataTypes;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Runtime.CompilerServices;

namespace EdgeDB.ContractResolvers
{
    internal sealed class TransientTupleConverter : JsonConverter<ITuple>
    {
        public static TransientTupleConverter Instance = new();

        public override ITuple? ReadJson(JsonReader reader, Type objectType, ITuple? existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
                return null;

            var jsonArray = JArray.Load(reader);

            var values = new object?[jsonArray.Count];

            for (int i = 0; i != values.Length; i++)
            {
                values[i] = serializer.Deserialize(jsonArray[i].CreateReader());
            }

            var transient = new TransientTuple(values);

            if (objectType == typeof(TransientTuple))
                return transient;
            else if(objectType.IsGenericType)
            {
                var genericDef = objectType.GetGenericTypeDefinition();

                if (TransientTuple.IsReferenceTupleType(genericDef))
                    return transient.ToReferenceTuple();
                else if (TransientTuple.IsValueTupleType(genericDef))
                    return transient.ToValueTuple();
            }

            throw new InvalidOperationException($"Unable to construct the tuple type {objectType}");
        }

        public override void WriteJson(JsonWriter writer, ITuple? value, JsonSerializer serializer)
        {
            if(value is null)
            {
                writer.WriteNull();
                return;
            }    

            writer.WriteStartArray();

            for (int i = 0; i != value.Length; i++)
            {
                serializer.Serialize(writer, value[i]);
            }

            writer.WriteEndArray();
        }
    }
}

