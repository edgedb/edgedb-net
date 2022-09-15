using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using EdgeDB.Tests.Benchmarks;

BenchmarkRunner.Run<PacketWritingBenchmark>();

await Task.Delay(-1);
