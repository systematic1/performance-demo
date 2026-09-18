using BenchmarkDotNet.Attributes;

namespace ZeroAllocSPSCDemo.Benchmarks;

[MemoryDiagnoser]
public class SPSCBenchmark
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