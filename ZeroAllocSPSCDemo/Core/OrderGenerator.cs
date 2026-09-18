using System.Buffers;
using ZeroAllocSPSCDemo.Models;

namespace ZeroAllocSPSCDemo.Managers;

public class OrderGenerator : IDisposable
{
    private readonly OrderQueue _orderQueue;
    private readonly ProductOrder[] _availableItems;
    private readonly int _maxItemsToGenerate;
    private int _index;
    
    public OrderGenerator(ref OrderQueue orderQueue, int maxItems)
    {
        // Can produce a maximum number of items
        _maxItemsToGenerate = maxItems;
        _orderQueue = orderQueue;
        _availableItems = ArrayPool<ProductOrder>.Shared.Rent(_maxItemsToGenerate);
        _index = 0;
    }

    public long ActiveOrderCount() => _orderQueue.Count();

    // Call this to generate a new ProductOrder and put it in the OrderQueue
    public bool ProduceNewOrder()
    {
        if (_index < _maxItemsToGenerate)
        {
            int orderId = Random.Shared.Next();
            int productId = Random.Shared.Next();
            int quantity = Random.Shared.Next(1, 25);
            float unitPrice = Random.Shared.NextSingle() * 100f;
            
            ProductOrder next = _availableItems[_index];
            next.Init(orderId, productId, quantity, unitPrice);

            _index++;
            
            return _orderQueue.Enqueue(ref next);
        }

        return false;
    }

    public void Dispose()
    {
        if (_availableItems?.Length > 0)
            ArrayPool<ProductOrder>.Shared.Return(_availableItems);
    }
}