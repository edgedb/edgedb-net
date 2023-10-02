using Newtonsoft.Json;

namespace EdgeDB;

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
