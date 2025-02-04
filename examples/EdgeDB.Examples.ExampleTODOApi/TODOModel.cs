using System.Text.Json.Serialization;

namespace Gel.Examples.ExampleTODOApi;

[GelType]
public class TODOModel
{
    [JsonPropertyName("title")]
    [GelProperty("title")]
    public string? Title { get; set; }

    [JsonPropertyName("description")]
    [GelProperty("description")]
    public string? Description { get; set; }

    [JsonPropertyName("date_created")]
    [GelProperty("date_created")]
    public DateTimeOffset DateCreated { get; set; }

    [JsonPropertyName("state")]
    [GelProperty("state")]
    public TODOState State { get; set; }
}

public enum TODOState
{
    NotStarted,
    InProgress,
    Complete
}
