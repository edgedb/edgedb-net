using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using EdgeDB.Tests.Benchmarks;

BenchmarkRunner.Run<TypeBuilderBenchmarks>();

await Task.Delay(-1);
