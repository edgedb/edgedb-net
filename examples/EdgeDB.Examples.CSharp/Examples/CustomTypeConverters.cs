using EdgeDB.TypeConverters;
using Microsoft.Extensions.Logging;

namespace EdgeDB.ExampleApp.Examples;

internal class CustomTypeConverters : IExample
{
    public ILogger? Logger { get; set; }

    public async Task ExecuteAsync(EdgeDBClient client)
    {
        // add the converter
        TypeBuilder.AddOrUpdateTypeConverter<DiscordSnowflakeIdConverter>();

        var user = await client.QueryAsync<UserWithSnowflakeId>(
            "with u := (insert UserWithSnowflakeId { user_id := \"841451783728529451\", username := \"example\" } unless conflict on .user_id else (select UserWithSnowflakeId)) select u { user_id, username }");

        Logger!.LogInformation("User with snowflake id: {@User}", user);
    }

    public class UserWithSnowflakeId
    {
        public DiscordSnowflakeId UserId { get; set; }

        public string? Username { get; set; }
    }

    public class DiscordSnowflakeIdConverter : EdgeDBTypeConverter<DiscordSnowflakeId, string>
    {
        public override DiscordSnowflakeId ConvertFrom(string? value)
            => value is null ? default : new DiscordSnowflakeId(ulong.Parse(value));

        public override string ConvertTo(DiscordSnowflakeId value)
            => value.Snowflake.ToString();
    }

    public readonly struct DiscordSnowflakeId
    {
        public readonly DateTimeOffset Timestamp;
        public readonly byte WorkerId;
        public readonly byte ProcessId;
        public readonly short Increment;

        public readonly ulong Snowflake;

        public DiscordSnowflakeId(ulong value)
        {
            Increment = (short)(value & 0xFFF); // first 12 bits
            ProcessId = (byte)((value >> 12) & 0x1F); // next 5 bits
            WorkerId = (byte)((value >> 17) & 0x1F); // next 5 bits
            Timestamp =
                DateTimeOffset.FromUnixTimeMilliseconds((long)((value >> 22) +
                                                               1420070400000)); // last 41 bits + Discord Epoch
            Snowflake = value;
        }
    }
}
