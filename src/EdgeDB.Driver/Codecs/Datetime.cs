﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Codecs
{
    public class Datetime : IScalarCodec<DateTimeOffset> // std::datetime
    {
        public static readonly DateTimeOffset EdgedbEpoc = new DateTimeOffset(2000, 1, 1, 0, 0, 0, TimeSpan.Zero);

        public DateTimeOffset Deserialize(PacketReader reader)
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

    public class LocalDateTime : IScalarCodec<DateTime> // std::local_datetime
    {
        public static readonly DateTime EdgedbEpoc = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Unspecified);

        public DateTime Deserialize(PacketReader reader)
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

    public class LocalDate : IScalarCodec<DateTime>
    {
        public static readonly DateTime EdgedbEpoc = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Unspecified);

        public void Serialize(PacketWriter writer, DateTime value)
        {
            var days = (int)Math.Floor((value - EdgedbEpoc).TotalDays);
            writer.Write(days);
        }

        public DateTime Deserialize(PacketReader reader)
        {
            var val = reader.ReadInt32();
            return EdgedbEpoc.AddDays(val);
        }
    }

    public class Duration : IScalarCodec<TimeSpan>
    {
        public TimeSpan Deserialize(PacketReader reader)
        {
            var microseconds = reader.ReadInt64();

            // skip days and months (depricated)
            reader.ReadInt32();
            reader.ReadInt32();

            return TimeSpan.FromTicks(microseconds * 10);
        }

        public void Serialize(PacketWriter writer, TimeSpan value)
        {
            writer.Write(value.Ticks / 10);
        }
    }

    public class RelativeDuration : IScalarCodec<TimeSpan>
    {
        public TimeSpan Deserialize(PacketReader reader)
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

    public class LocalTime : Duration { }
}