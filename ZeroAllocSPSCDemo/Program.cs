using System.Buffers;
using ZeroAllocSPSCDemo.Managers;
using ZeroAllocSPSCDemo.Models;

namespace ZeroAllocSPSCDemo;

public static class Program
{
    private static readonly int Capacity = 512 * 1024;
    private static OrderGenerator _generator;
    //private static OrderConsumer _consumer;
    
    static void Main(string[] args)
    {
        PreAllocateOrderQueueArray();

        _generator = new OrderGenerator();
        //_consumer = new OrderConsumer();

        // Run the generator 40 times first before starting the consumer
        // Generation and Consumption code must run on separate threads
        // They must gracefully "end" when there is nothing left to process
    }

    static void RandomPause()
    {
        Thread.Sleep(Random.Shared.Next(5, 4000));
    }

    private static void PreAllocateOrderQueueArray()
    {
        var tempArray = ArrayPool<ProductOrder>.Shared.Rent(Capacity);
        ArrayPool<ProductOrder>.Shared.Return(tempArray);
    }
}