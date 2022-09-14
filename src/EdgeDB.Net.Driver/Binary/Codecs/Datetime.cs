namespace EdgeDB.Binary.Codecs
{
    internal class Datetime : IScalarCodec<DateTimeOffset> // std::datetime
    {
        public static readonly DateTimeOffset EdgedbEpoc = new(2000, 1, 1, 0, 0, 0, TimeSpan.Zero);

        public DateTimeOffset Deserialize(ref PacketReader reader)
        {
            var val = reader.ReadInt64();

            return EdgedbEpoc.AddTicks(val * 10);
        }

        public void Serialize(PacketWriter writer, DateTimeOffset value)
        {
            var v = (value - EdgedbEpoc).Ticks;

            writer.Write(v / 10);
        }
    }

    internal class LocalDateTime : IScalarCodec<DateTime> // std::local_datetime
    {
        public static readonly DateTime EdgedbEpoc = new(2000, 1, 1, 0, 0, 0, DateTimeKind.Unspecified);

        public DateTime Deserialize(ref PacketReader reader)
        {
            var val = reader.ReadInt64();

            return EdgedbEpoc.AddTicks(val * 10);
        }

        public void Serialize(PacketWriter writer, DateTime value)
        {
            var v = (value - EdgedbEpoc).Ticks;

            writer.Write(v / 10);
        }
    }

    internal class LocalDate : IScalarCodec<DateOnly>
    {
        public static readonly DateOnly EdgedbEpoc = new(2000, 1, 1);

        public void Serialize(PacketWriter writer, DateOnly value)
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

    internal class Duration : IScalarCodec<TimeSpan>
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

        public void Serialize(PacketWriter writer, TimeSpan value)
        {
            writer.Write(value.Ticks / 10);
        }
    }

    internal class RelativeDuration : IScalarCodec<TimeSpan>
    {
        public TimeSpan Deserialize(ref PacketReader reader)
        {
            var microseconds = reader.ReadInt64();
            var days = reader.ReadInt32();
            var months = reader.ReadInt32();

            return new TimeSpan(microseconds * 10).Add(TimeSpan.FromDays(days + months * 31));
        }

        public void Serialize(PacketWriter writer, TimeSpan value)
        {

        }
    }

    internal class LocalTime : Duration { }
}
