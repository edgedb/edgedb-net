using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Binary.Codecs
{
    internal sealed class Duration : BaseTemporalCodec<DataTypes.Duration>
    {
        protected override Dictionary<Type, (FromTransient From, ToTransient To)>? SystemConverters { get; }

        public Duration()
        {
            SystemConverters = new()
            {
                { typeof(TimeSpan), (From, To) }
            };
        }

        public override DataTypes.Duration Deserialize(ref PacketReader reader)
        {
            var microseconds = reader.ReadInt64();

            // skip days and months
            reader.Skip(sizeof(int) + sizeof(int));

            return new(microseconds);
        }

        public override void Serialize(ref PacketWriter writer, DataTypes.Duration value)
        {
            writer.Write(value.Microseconds);
            writer.Write(0); // days
            writer.Write(0); // months
        }

        private DataTypes.Duration From(ref TransientTemporal transient)
        {
            // transient here is timespan, since our only supported sys type is timespan
            return new DataTypes.Duration(transient.TimeSpan);
        }

        private TransientTemporal To(ref DataTypes.Duration dateDuration)
        {
            var timespan = dateDuration.TimeSpan;

            return TransientTemporal.From(ref timespan);
        }
    }
}
