using BenchmarkDotNet.Running;
using PerformanceTests;

_ = BenchmarkRunner.Run<MemoryFlowBenchmark>();
_ = BenchmarkRunner.Run<DistributedFlowBenchmark>();
