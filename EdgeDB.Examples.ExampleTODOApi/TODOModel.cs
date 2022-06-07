using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations;

namespace EdgeDB.Examples.ExampleTODOApi
{
    [EdgeDBType]
    public class TODOModel
    {
        [JsonPropertyName("title")]
        [EdgeDBProperty("title")]
        public string? Title { get; set; }

        [JsonPropertyName("description")]
        [EdgeDBProperty("description")]
        public string? Description { get; set; }

        [JsonPropertyName("date_created")]
        [EdgeDBProperty("date_created")]
        public DateTimeOffset DateCreated { get; set; }

        [JsonPropertyName("state")]
        [EdgeDBProperty("state")]
        public TODOState State { get; set; }
    }

    public enum TODOState
    {
        NotStarted,
        InProgress,
        Complete
    }
}
