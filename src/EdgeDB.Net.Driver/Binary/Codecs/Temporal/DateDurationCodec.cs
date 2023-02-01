using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Binary.Codecs
{
    internal sealed class DateDurationCodec : BaseTemporalCodec<DataTypes.DateDuration>
    {
        protected override Dictionary<Type, (FromTransient From, ToTransient To)>? Converters { get; }

        public DateDurationCodec()
        {
            Converters = new()
            {
                { typeof(TimeSpan), (From, To) }
            };
        }

        public override DataTypes.DateDuration Deserialize(ref PacketReader reader)
        {
            reader.Skip(sizeof(long));
            var days = reader.ReadInt32();
            var months = reader.ReadInt32();

            return new(days, months);
        }
        
        public override void Serialize(ref PacketWriter writer, DataTypes.DateDuration value)
        {
            writer.Write(0L);
            writer.Write(value.Days);
            writer.Write(value.Months);
        }

        private DataTypes.DateDuration From(ref TransientTemporal transient)
        {
            // transient here is timespan, since our only supported sys type is timespan
            return new DataTypes.DateDuration(transient.TimeSpan);
        }

        private TransientTemporal To(ref DataTypes.DateDuration dateDuration)
        {
            var timespan = dateDuration.TimeSpan;

            return TransientTemporal.From(ref timespan);
        }
    }
}
