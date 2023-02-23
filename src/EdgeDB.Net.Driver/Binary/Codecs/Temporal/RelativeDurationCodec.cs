using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Binary.Codecs
{
    internal sealed class RelativeDurationCodec : BaseTemporalCodec<DataTypes.RelativeDuration>
    {
        protected override Dictionary<Type, (FromTransient From, ToTransient To)>? Converters { get; }

        public RelativeDurationCodec()
        {
            Converters = new()
            {
                { typeof(TimeSpan), (From, To) }
            };
        }

        public override DataTypes.RelativeDuration Deserialize(ref PacketReader reader)
        {
            var microseconds = reader.ReadInt64();
            var days = reader.ReadInt32();
            var months = reader.ReadInt32();

            return new(microseconds, days, months);
        }

        public override void Serialize(ref PacketWriter writer, DataTypes.RelativeDuration value)
        {
            writer.Write(value.Microseconds);
            writer.Write(value.Days);
            writer.Write(value.Months);
        }

        private DataTypes.RelativeDuration From(ref TransientTemporal transient)
        {
            // transient here is timespan, since our only supported sys type is timespan
            return new DataTypes.RelativeDuration(transient.TimeSpan);
        }

        private TransientTemporal To(ref DataTypes.RelativeDuration dateDuration)
        {
            var timespan = dateDuration.TimeSpan;

            return TransientTemporal.From(ref timespan);
        }
    }
}
