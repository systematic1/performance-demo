using BenchmarkDotNet.Attributes;

namespace PerformanceDemo.Benchmarks;

[MemoryDiagnoser]
public class PerformanceBenchmark
{
    [GlobalSetup]
    public void Setup()
    {
        Program.PreAllocateOrderQueueArray();
        Program.InitializeObjects(16 * 1024, 1);
    }
    
    [Benchmark]
    public void RunOptimized()
    {
        Program.Run();
    }
}