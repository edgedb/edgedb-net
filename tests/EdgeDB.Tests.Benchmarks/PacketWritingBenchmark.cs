using BenchmarkDotNet.Attributes;
using EdgeDB.Binary;
using EdgeDB.Binary.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Tests.Benchmarks
{
    // commented out because of internalization of sendables.

    //[MemoryDiagnoser]
    //public class PacketWritingBenchmark
    //{
    //    [Benchmark]
    //    [ArgumentsSource(nameof(Packets))]
    //    public Span<byte> WritePacket(Sendable packet)
    //    {
    //        var p = new PacketWriter();

    //        packet!.Write(ref p, null!);

    //        return p.GetBytes();
    //    }

    //    public IEnumerable<Sendable> Packets => new Sendable[]
    //    {
    //        new ClientHandshake()
    //        {
    //            MajorVersion = 1,
    //            MinorVersion = 0,
    //            Extensions = Array.Empty<ProtocolExtension>(),
    //            ConnectionParameters = Array.Empty<ConnectionParam>(),
    //        }
    //    };
    //}
}
