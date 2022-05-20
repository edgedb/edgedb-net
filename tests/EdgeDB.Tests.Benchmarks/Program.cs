using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using EdgeDB;
using EdgeDB.Tests.Benchmarks;
using System.Reflection;

//var asm = typeof(ClientPoolBenchmarks).Assembly;

//BenchmarkRunner.Run(asm);

// verify no exceptions
try
{
    var inst = new DeserializerBenchmarks();

    foreach (var item in inst.ValuesForPacket)
    {
        inst.Packet = item;

        if (inst.Deserialize() == null)
        {
            throw new NullReferenceException("Deserialize didn't return packet");
        }
    }
}
catch(Exception x)
{
    Console.WriteLine(x);
}
BenchmarkRunner.Run<DeserializerBenchmarks>();



await Task.Delay(-1);
