using Newtonsoft.Json;

namespace EdgeDB.Models;

internal sealed class CloudProfile
{
    [JsonProperty("secret_key")] public string? SecretKey { get; set; }
}
