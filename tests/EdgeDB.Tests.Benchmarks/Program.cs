using BenchmarkDotNet.Running;
using EdgeDB.Tests.Benchmarks;

BenchmarkRunner.Run<FullExecuteBenchmark>();

await Task.Delay(-1);
