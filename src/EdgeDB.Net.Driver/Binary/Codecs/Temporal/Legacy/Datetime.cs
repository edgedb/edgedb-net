namespace EdgeDB.Binary.Codecs
{
    internal sealed class LegacyDatetime : IScalarCodec<DateTimeOffset> // std::datetime
    {
        public static readonly DateTimeOffset EdgedbEpoc = new(2000, 1, 1, 0, 0, 0, TimeSpan.Zero);

        public DateTimeOffset Deserialize(ref PacketReader reader)
        {
            var val = reader.ReadInt64();

            return EdgedbEpoc.AddTicks(val * 10);
        }

        public void Serialize(ref PacketWriter writer, DateTimeOffset value)
        {
            var v = (value - EdgedbEpoc).Ticks;

            writer.Write(v / 10);
        }
    }

    internal sealed class LegacyLocalDateTime : IScalarCodec<System.DateTime> // std::local_datetime
    {
        public static readonly System.DateTime EdgedbEpoc = new(2000, 1, 1, 0, 0, 0, DateTimeKind.Unspecified);

        public System.DateTime Deserialize(ref PacketReader reader)
        {
            var val = reader.ReadInt64();

            return EdgedbEpoc.AddTicks(val * 10);
        }

        public void Serialize(ref PacketWriter writer, System.DateTime value)
        {
            var v = (value - EdgedbEpoc).Ticks;

            writer.Write(v / 10);
        }
    }

    internal sealed class LegacyLocalDate : IScalarCodec<DateOnly>
    {
        public static readonly DateOnly EdgedbEpoc = new(2000, 1, 1);

        public void Serialize(ref PacketWriter writer, DateOnly value)
        {
            var days = (int)Math.Floor((value.ToDateTime(default) - EdgedbEpoc.ToDateTime(default)).TotalDays);
            writer.Write(days);
        }

        public DateOnly Deserialize(ref PacketReader reader)
        {
            var val = reader.ReadInt32();
            return EdgedbEpoc.AddDays(val);
        }
    }

    internal class LegacyDuration : IScalarCodec<TimeSpan>
    {
        public TimeSpan Deserialize(ref PacketReader reader)
        {
            var microseconds = reader.ReadInt64();

            if (!reader.Empty)
            {
                // skip days and months (depricated)
                reader.ReadInt32();
                reader.ReadInt32();
            }

            return TimeSpan.FromTicks(microseconds * 10);
        }

        public void Serialize(ref PacketWriter writer, TimeSpan value)
        {
            writer.Write(value.Ticks / 10);
        }
    }

    internal sealed class LegacyRelativeDuration : IScalarCodec<TimeSpan>
    {
        public TimeSpan Deserialize(ref PacketReader reader)
        {
            var microseconds = reader.ReadInt64();
            var days = reader.ReadInt32();
            var months = reader.ReadInt32();

            return new TimeSpan(microseconds * 10).Add(TimeSpan.FromDays(days + months * 31));
        }

        public void Serialize(ref PacketWriter writer, TimeSpan value)
        {
            var (microseconds, days, months) = DataTypes.TemporalCommon.ToComponents(value);

            writer.Write(microseconds);
            writer.Write(days);
            writer.Write(months);
        }
    }

    internal sealed class LegacyLocalTime : LegacyDuration { }
}
