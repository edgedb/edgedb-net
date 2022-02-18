using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Codecs
{
    public class Datetime : IScalerCodec<DateTimeOffset> // std::datetime
    {
        public static DateTimeOffset EdgedbEpoc = new DateTimeOffset(2000, 1, 1, 0, 0, 0, TimeSpan.Zero);

        public bool CanConvert(Type t)
        {
            return t == typeof(DateTime) || t == typeof(DateTimeOffset);
        }

        public DateTimeOffset Deserialize(PacketReader reader)
        {
            var val = reader.ReadInt64();

            return EdgedbEpoc.AddTicks(val * 10);

        }

        public void Serialize(PacketWriter writer, DateTimeOffset value)
        {
            var date = (DateTimeOffset?)value;

            var v = (date.Value - EdgedbEpoc).Ticks;

            writer.Write(v / 10);
        }
    }

    public class Duration : IScalerCodec<TimeSpan>
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

    public class RelativeDuration : IScalerCodec<TimeSpan>
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
    public class LocalDatetime : Datetime { } // TODO: implement?
    public class LocalDate : Datetime { } // TODO: implement?

}
