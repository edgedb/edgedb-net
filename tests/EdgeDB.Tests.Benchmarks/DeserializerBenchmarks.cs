using BenchmarkDotNet.Attributes;
using EdgeDB.Models;
using EdgeDB.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Tests.Benchmarks
{
    public class DeserializerBenchmarks
    {
        public static readonly byte[] ServerHandshake;
        public static readonly byte[] Authentication1;
        public static readonly byte[] Authentication2;
        public static readonly byte[] Authentication3;
        public static readonly byte[] Authentication4;
        public static readonly byte[] ServerKeyData;
        public static readonly byte[] ParameterStatus1;
        public static readonly byte[] ParameterStatus2;
        public static readonly byte[] ReadyForCommand;

        public IEnumerable<(ServerMessageType Type, byte[] Data)> ValuesForPacket
            => new (ServerMessageType Type, byte[] Data)[]
            {
                (ServerMessageType.ServerHandshake, ServerHandshake),
                (ServerMessageType.Authentication, Authentication1),
                (ServerMessageType.Authentication, Authentication2),
                (ServerMessageType.Authentication, Authentication3),
                (ServerMessageType.Authentication, Authentication4),
                (ServerMessageType.ServerKeyData, ServerKeyData),
                (ServerMessageType.ParameterStatus, ParameterStatus1),
                (ServerMessageType.ParameterStatus, ParameterStatus2),
                (ServerMessageType.ReadyForCommand, ReadyForCommand)
            };

        static DeserializerBenchmarks()
        {
            ServerHandshake = HexConverter.FromHex("0000000A0000000D0000");
            Authentication1 = HexConverter.FromHex("0000001D0000000A000000010000000D534352414D2D5348412D323536");
            Authentication2 = HexConverter.FromHex("000000600000000B00000054723D435948636F2F333056784F4E6F6E6F7830564262524A2B504642706C55714B3475467358656465443274676A506C7A4E2C733D7A5244694F545130534971734A6877774C71536278673D3D2C693D34303936");
            Authentication3 = HexConverter.FromHex("0000003A0000000C0000002E763D7938654F576867615A5874463477357362336C78713430462B4D7A44744E617061533145712B7875552F453D");
            Authentication4 = HexConverter.FromHex("0000000800000000");
            ServerKeyData = HexConverter.FromHex("000000240000000000000000000000000000000000000000000000000000000000000000");
            ParameterStatus1 = HexConverter.FromHex("000000290000001A7375676765737465645F706F6F6C5F636F6E63757272656E637900000003313030");
            ParameterStatus2 = HexConverter.FromHex("000000C60000000D73797374656D5F636F6E666967000000AD00000071F2F5594609176568E2521789EE64F97F0200000000000000000000000000000100020000000000000000000000000000010E01F2F5594609176568E2521789EE64F97F00020000000141000000026964000000000000410000001473657373696F6E5F69646C655F74696D656F75740001000000340000000200000B8600000010172097A439F411E9B1899321EB2F4B97000040490000001000000000039387000000000000000000");
            ReadyForCommand = HexConverter.FromHex("00000007000049");
        }

        [ParamsSource(nameof(ValuesForPacket))]
        public (ServerMessageType Type, byte[] Data) Packet { get; set; }

        [Benchmark]
        public IReceiveable? Deserialize()
        {
            return PacketSerializer.DeserializePacket(Packet.Type, Packet.Data, null!); // client as null is OK as its only used for logging unknown packet
        }
    }
}
