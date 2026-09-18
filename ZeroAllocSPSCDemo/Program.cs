using System.Buffers;
using BenchmarkDotNet.Running;
using ZeroAllocSPSCDemo.Managers;
using ZeroAllocSPSCDemo.Models;

namespace ZeroAllocSPSCDemo;

public static class Program
{
    private static readonly bool IsBenchmark = false;            // <--- Whether to run benchmark or normal
    
    private static readonly int Capacity = 512 * 1024;
    private static int MaxGenerations = 128 * 1024;
    private static int MaxSleepMs = 3;
    private static readonly int MaxGenerateHeadstartMs = 5000;
    private static char[] _messageBuffer = new char[16];

    private static long _startTimestamp = 0;

    private static OrderQueue _orderQueue;
    private static OrderGenerator _generator;
    private static OrderConsumer _consumer;
    
    private static Thread _generatorThread;
    private static Thread _consumerThread;

    private static int generatedCount = 0;
    private static int consumedCount = 0;
    private static long queueCount = 0;

    static void Main(string[] args)
    {
        if (IsBenchmark)
        {
            BenchmarkRunner.Run<Benchmarks.SPSCBenchmark>();
        }
        else
        { 
            PreAllocateOrderQueueArray();
            InitializeObjects(128 * 1024, 4);
            Run();
        }
    }

    public static void InitializeObjects(int maxItems, int maxSleep)
    {
        MaxGenerations = maxItems;
        MaxSleepMs = maxSleep;
        
        _orderQueue = new OrderQueue();
        _generator = new OrderGenerator(ref _orderQueue, MaxGenerations);
        _consumer = new OrderConsumer(ref _orderQueue);
    }
    
    public static void Run()
    {
        Console.WriteLine("Starting Generator/Consumer App");
        Console.WriteLine("Queue Capacity is 16K items");
        Console.WriteLine();
        
        int nextGeneratedCount = 1000;
        int nextConsumedCount = 1000;
        int byteCount = 0;
        
        _startTimestamp = DateTime.Now.Ticks;
        
        _generatorThread = new Thread(new ThreadStart(GenerateNewOrders));      // Unavoidable allocation for benchmark code
        _consumerThread = new Thread(new ThreadStart(ConsumeGeneratedOrders));

        // Run the generator for 2 seconds first before starting the consumer
        // Generation and Consumption code must run on separate threads
        // They must gracefully "end" when there is nothing left to process

        //_generatorThread.Priority = ThreadPriority.AboveNormal;
        _generatorThread.Start(); 
        
        int generateHeadstartMs = Random.Shared.Next(2000, MaxGenerateHeadstartMs);
        Thread.Sleep(generateHeadstartMs);

        //_consumerThread.Priority = ThreadPriority.BelowNormal;
        _consumerThread.Start();

        while (_generatorThread.IsAlive || _consumerThread.IsAlive)
        {
            // Display a status message after so many items are generated (zero-allocation)
            if (generatedCount >= nextGeneratedCount && !IsBenchmark)
            {
                queueCount.TryFormat(_messageBuffer.AsSpan(), out byteCount);
                Console.Write("------------------- Active Queue Count: ");
                Console.WriteLine(_messageBuffer.AsSpan().Slice(0, byteCount));

                generatedCount.TryFormat(_messageBuffer.AsSpan(), out byteCount);
                Console.Write(">> Produced: ");
                Console.WriteLine(_messageBuffer.AsSpan().Slice(0, byteCount));
                nextGeneratedCount += 1000;
            }

            // Display a status message after so many items are consumed (zero-allocation)
            if (consumedCount >= nextConsumedCount && !IsBenchmark)
            {
                consumedCount.TryFormat(_messageBuffer.AsSpan(), out byteCount);
                Console.Write("<< Consumed: ");
                Console.WriteLine(_messageBuffer.AsSpan().Slice(0, byteCount));
                nextConsumedCount += 1000;
            }
        }

        long runtimeTicks = DateTime.Now.Ticks - _startTimestamp;
        TimeSpan timeSpan = TimeSpan.FromTicks(runtimeTicks);
        double opsPerSec = MaxGenerations / timeSpan.TotalSeconds;
        
        Console.WriteLine();
        Console.WriteLine("Application has finished processing.");
        
        opsPerSec = Math.Round(opsPerSec, 4);
        opsPerSec.TryFormat(_messageBuffer.AsSpan(), out byteCount);
        Console.Write("Average of ");
        Console.Write(_messageBuffer.AsSpan());
        Console.WriteLine(" items processed/second");
        
        // The application will not actually end until all the remaining items are consumed
        while (_generatorThread.IsAlive || _consumerThread.IsAlive) 
        {}
    }

    static void DoRandomPause()
    {
        int waitTime = Random.Shared.Next(0, MaxSleepMs);
        if (waitTime > 0)
            Thread.Sleep(waitTime);
    }

    static void GenerateNewOrders()
    {
        bool succeeded = false;
        do
        {
            succeeded = _generator.ProduceNewOrder();
            queueCount = _generator.ActiveOrderCount();
            DoRandomPause();
            generatedCount++;
        }
        while (succeeded);
    }

    static void ConsumeGeneratedOrders()
    {
        bool succeeded = false;
        do
        {
            succeeded = _consumer.ConsumeNextOrder();
            DoRandomPause();
            consumedCount++;
        }
        while (succeeded);
    }

    public static void PreAllocateOrderQueueArray()
    {
        // This allocates a fixed size array up front to rent to both the OrderQueue and the generator.
        // The generator and consumer classes will rent from this ArrayPool and must return them.
        var tempArray = ArrayPool<ProductOrder>.Shared.Rent(Capacity);
        ArrayPool<ProductOrder>.Shared.Return(tempArray);
    }
}