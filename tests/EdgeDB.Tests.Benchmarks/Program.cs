using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using EdgeDB.Tests.Benchmarks;

BenchmarkRunner.Run<QueryBenchmarks>();

await Task.Delay(-1);
