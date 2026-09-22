using System.Buffers;
using PerformanceDemo.Models;

namespace PerformanceDemo.Core;

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
            int quantity = Random.Shared.Next(1, 8);
            float unitPrice = Random.Shared.Next(100, 10000) / 100f;
            int discountPercentage = 5 * Random.Shared.Next(0, 3);
            float taxRate = Random.Shared.Next(0, 20) > 12 ? 7.0f : 8.5f;
            float shippingCost = Random.Shared.Next(0, 20) > 12 ? 9.99f : 0f;
            int alternateProductId = Random.Shared.Next();
            int shipPriority = Random.Shared.Next(0, 20) > 14 ? 1 : 2;
            
            ProductOrder next = _availableItems[_index];
            next.Init(orderId, productId, quantity, unitPrice, discountPercentage, taxRate, 
                shippingCost, alternateProductId, shipPriority);

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