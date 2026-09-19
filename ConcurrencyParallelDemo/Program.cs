using System.Buffers;

namespace ConcurrencyParallelDemo;

public static class Program
{
    // static members
    private static readonly int Capacity = 512 * 1024;
    
    static void Main(string[] args)
    {
        // Multithreaded simultaneous operations/calculations
        // Parse and transform generated (incoming) data and send to multiple
        //  processing engines (threads) to perform different operations on each
        // Minimize/eliminate memory allocation
        // Use Parallel.For/Each() to perform parallel CPU tasking
    }

    public static void PreAllocateWorkQueueArray()
    {
        // This allocates a fixed size array up front to rent to both the OrderQueue and the generator.
        // The generator and consumer classes will rent from this ArrayPool and must return them.
        var tempArray = ArrayPool<ProductOrder>.Shared.Rent(Capacity);
        ArrayPool<ProductOrder>.Shared.Return(tempArray);
    }
}