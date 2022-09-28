using EdgeDB.TypeConverters;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.ExampleApp.Examples
{
    internal class CustomTypeConverters : IExample
    {
        public ILogger? Logger { get; set; }

        public class UserWithSnowflakeId
        {
            [EdgeDBTypeConverter(typeof(UlongTypeConverter))]
            public ulong UserId { get; set; }

            public string? Username { get; set; }
        }

        public class UlongTypeConverter : EdgeDBTypeConverter<ulong, string>
        {
            public override ulong ConvertFrom(string? value)
            {
                if (value is null)
                    return 0;

                return ulong.Parse(value);
            }
            
            public override string? ConvertTo(ulong value)
            {
                return value.ToString();
            }
        }

        public async Task ExecuteAsync(EdgeDBClient client)
        {
            var user = await client.QueryAsync<UserWithSnowflakeId>("with u := (insert UserWithSnowflakeId { user_id := \"841451783728529451\", username := \"example\" } unless conflict on .user_id else (select UserWithSnowflakeId)) select u { user_id, username }");

            Logger!.LogInformation("User with snowflake id: {@User}", user);
        }
    }
}
