using System.Buffers;
using ZeroAllocSPSCDemo.Managers;
using ZeroAllocSPSCDemo.Models;

namespace ZeroAllocSPSCDemo;

public static class Program
{
    private static readonly int Capacity = 512 * 1024;
    private static readonly int MaxGenerations = 256 * 1024;
    private static readonly int MaxSleepMs = 4;
    private static readonly int GenerateHeadstartMs = 2000;

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
        
        _generatorThread.Start(); 
        
        Thread.Sleep(GenerateHeadstartMs);

        _consumerThread.Start();

        while (_generatorThread.IsAlive && _consumerThread.IsAlive)
        {
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
    }

    static void RandomPause()
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
            RandomPause();
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
            RandomPause();
            // Not really doing anything with it
            consumedCount++;
        }
        while (succeeded);
    }

    private static void PreAllocateOrderQueueArray()
    {
        var tempArray = ArrayPool<ProductOrder>.Shared.Rent(Capacity);
        ArrayPool<ProductOrder>.Shared.Return(tempArray);
    }
}