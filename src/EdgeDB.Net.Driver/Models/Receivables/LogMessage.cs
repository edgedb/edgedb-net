using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Models
{
    public readonly struct LogMessage : IReceiveable
    {
        public ServerMessageType Type => ServerMessageType.LogMessage;

        public MessageSeverity Severity { get; }

        public ServerErrorCodes Code { get; }

        public string Text { get; }

        public IReadOnlyCollection<Header> Attributes
            => _attributes.ToImmutableArray();

        private readonly Header[] _attributes;

        internal LogMessage(ref PacketReader reader)
        {
            Severity = (MessageSeverity)reader.ReadByte();
            Code = (ServerErrorCodes)reader.ReadUInt32();
            Text = reader.ReadString();
            _attributes = reader.ReadHeaders();
        }

    }
}
