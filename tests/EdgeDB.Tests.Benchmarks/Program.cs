using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using EdgeDB.Tests.Benchmarks;

BenchmarkRunner.Run<DeserializerBenchmarks>();

await Task.Delay(-1);
