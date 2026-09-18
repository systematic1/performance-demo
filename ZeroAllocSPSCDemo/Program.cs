using System.Buffers;
using BenchmarkDotNet.Running;
using ZeroAllocSPSCDemo.Managers;
using ZeroAllocSPSCDemo.Models;

namespace ZeroAllocSPSCDemo;

public static class Program
{
    private static readonly int Capacity = 512 * 1024;
    private static readonly int MaxGenerations = 128 * 1024;
    private static readonly int MaxSleepMs = 3;
    private static readonly int GenerateHeadstartMs = 3500;

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
        Run();
        //BenchmarkRunner.Run<Benchmarks.SPSCBenchmark>();
    }
    
    public static void Run()
    {
        Console.WriteLine("Starting Generator/Consumer App");
        Console.WriteLine("Queue Capacity is 16K items");
        Console.WriteLine();
        
        PreAllocateOrderQueueArray();

        char[] messageBuffer = new char[16];
        int nextGeneratedCount = 1000;
        int nextConsumedCount = 1000;
        int byteCount = 0;
        
        _orderQueue = new OrderQueue();
        _generator = new OrderGenerator(ref _orderQueue, MaxGenerations);
        _consumer = new OrderConsumer(ref _orderQueue);

        // Run the generator for 2 seconds first before starting the consumer
        // Generation and Consumption code must run on separate threads
        // They must gracefully "end" when there is nothing left to process

        _generatorThread = new Thread(new ThreadStart(GenerateNewOrders));
        _consumerThread = new Thread(new ThreadStart(ConsumeGeneratedOrders));
        
        // Note: There is no new heap memory allocations past this point
        
        _generatorThread.Start(); 
        
        Thread.Sleep(GenerateHeadstartMs);

        _consumerThread.Start();

        while (_generatorThread.IsAlive && _consumerThread.IsAlive)
        {
            // Display a status message after so many items are generated (zero-allocation)
            if (generatedCount >= nextGeneratedCount)
            {
                generatedCount.TryFormat(messageBuffer.AsSpan(), out byteCount);
                Console.Write(">> Produced: ");
                Console.WriteLine(messageBuffer.AsSpan().Slice(0, byteCount));
                nextGeneratedCount += 1000;
                
                queueCount.TryFormat(messageBuffer.AsSpan(), out byteCount); 
                Console.Write("                         ** Active Queue Count: ");
                Console.WriteLine(messageBuffer.AsSpan().Slice(0, byteCount));
            }
            
            // Display a status message after so many items are consumed (zero-allocation)
            if (consumedCount >= nextConsumedCount)
            {
                consumedCount.TryFormat(messageBuffer.AsSpan(), out byteCount);
                Console.Write("<< Consumed: ");
                Console.WriteLine(messageBuffer.AsSpan().Slice(0, byteCount));
                nextConsumedCount += 1000;
            }
        }
        
        Console.WriteLine();
        Console.WriteLine("Application has finished processing.");
        
        // The application will not actually end until all of the remaining items are consumed
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

    private static void PreAllocateOrderQueueArray()
    {
        // This allocates a fixed size array up front to rent to both the OrderQueue and the generator.
        // The generator and consumer classes will rent from this ArrayPool and must return them.
        var tempArray = ArrayPool<ProductOrder>.Shared.Rent(Capacity);
        ArrayPool<ProductOrder>.Shared.Return(tempArray);
    }
}