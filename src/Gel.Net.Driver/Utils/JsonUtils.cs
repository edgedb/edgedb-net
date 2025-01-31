using Gel.DataTypes;
using Newtonsoft.Json;

namespace Gel.Utils;

internal class AsStringConverter : JsonConverter<string>
{
    // Always reads numbers as strings

    public override string? ReadJson(
        JsonReader reader,
        Type objectType,
        string? existingValue,
        bool hasExistingValue,
        JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.Integer)
        {
            return reader.Value!.ToString();
        }
        else if (reader.TokenType == JsonToken.Float)
        {
            return reader.Value!.ToString();
        }
        else if (reader.TokenType == JsonToken.String)
        {
            return (string)reader.Value!;
        }
        throw new JsonException("Expected Number or String.");
    }

    public override void WriteJson(
        JsonWriter writer, string? value, JsonSerializer serializer)
    {
        throw new NotImplementedException();
    }
}
