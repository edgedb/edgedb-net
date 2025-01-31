using BenchmarkDotNet.Running;
using Gel.Tests.Benchmarks;

BenchmarkRunner.Run<FullExecuteBenchmark>();

await Task.Delay(-1);
