using BenchmarkDotNet.Attributes;

namespace ZeroAllocSPSCDemo.Benchmarks;

[MemoryDiagnoser]
public class SPSCBenchmark
{
    [Benchmark]
    public void RunOptimized()
    {
        Program.Run();
    }
}