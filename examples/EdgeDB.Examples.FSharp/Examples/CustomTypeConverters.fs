module CustomTypeConverters

open Examples
open EdgeDB
open Microsoft.Extensions.Logging
open System
open System.Runtime.CompilerServices
open EdgeDB.TypeConverters



[<IsReadOnly; Struct>]
type DiscordSnowflakeId =
    struct
        val Timestamp: DateTimeOffset
        val WorkerId: byte
        val ProcessId: byte
        val Increment: int16
        val Snowflake: uint64

        new(value: uint64) = {
            Increment = int16 (value &&& 0xFFFUL);
            ProcessId = byte ((value >>> 12) &&& 0x1FUL);
            WorkerId = byte ((value >>> 17) &&& 0x1FUL);
            Timestamp = DateTimeOffset.FromUnixTimeMilliseconds(int64 ((value >>> 22) + 1420070400000UL));
            Snowflake = value
        }
    end

type DiscordSnowflakeConverter() =
    inherit EdgeDBTypeConverter<DiscordSnowflakeId, string>()
    override this.ConvertFrom(value: string) =
        DiscordSnowflakeId(uint64 value)
    override this.ConvertTo(value: DiscordSnowflakeId) =
        value.Snowflake.ToString()

type UserWithSnowflakeId = {
    UserId: DiscordSnowflakeId
    Username: string
}

type CustomTypeConverters() =
    interface IExample with
        member this.ExecuteAsync(client: EdgeDBClient, logger: ILogger) =
            task {
                TypeBuilder.AddOrUpdateTypeConverter<DiscordSnowflakeConverter>()

                let! user = client.QueryAsync<UserWithSnowflakeId>("with u := (insert UserWithSnowflakeId { user_id := \"841451783728529451\", username := \"example\" } unless conflict on .user_id else (select UserWithSnowflakeId)) select u { user_id, username }")

                logger.LogInformation("User with snowflake id: {@User}", user)
            }
