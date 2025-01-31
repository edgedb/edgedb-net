using Newtonsoft.Json;

namespace Gel.Models;

internal sealed class CloudProfile
{
    [JsonProperty("secret_key")] public string? SecretKey { get; set; }
}
