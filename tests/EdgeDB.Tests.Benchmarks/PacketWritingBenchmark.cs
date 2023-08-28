using BenchmarkDotNet.Attributes;
using EdgeDB.Binary;
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
    //    public Memory<byte> WritePacket(Sendable packet)
    //    {
    //        var p = new PacketWriter(packet.Size + 5);
    //        packet!.Write(ref p, null!);
    //        var data = p.GetBytes();
    //        p.Dispose();
    //        return data;
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
