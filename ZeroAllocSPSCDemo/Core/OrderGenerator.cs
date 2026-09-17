using System.Buffers;
using ZeroAllocSPSCDemo.Models;

namespace ZeroAllocSPSCDemo.Managers;

public class OrderGenerator : IDisposable
{
    private readonly OrderQueue _orderQueue;
    private readonly ProductOrder[] _availableItems;
    private readonly int _maxItemsToGenerate = 400 * 1024;
    private int _index;
    
    public OrderGenerator()
    {
        _orderQueue = new OrderQueue();
        _availableItems = ArrayPool<ProductOrder>.Shared.Rent(_maxItemsToGenerate);
        _index = 0;
    }

    public int ActiveOrderCount() => _orderQueue.Count();

    public bool ProduceNewOrder()
    {
        if (_index < _availableItems.Length)
        {
            int orderId = Random.Shared.Next();
            int productId = Random.Shared.Next();
            int quantity = Random.Shared.Next(1, 25);
            float unitPrice = Random.Shared.NextSingle() * 100f;
            
            ProductOrder next = _availableItems[_index];
            next.Init(orderId, productId, quantity, unitPrice);

            _index++;
            
            _orderQueue.Enqueue(next);
            return true;
        }

        return false;
    }

    public void Dispose()
    {
        _orderQueue?.Dispose();
        if (_availableItems?.Length > 0)
            ArrayPool<ProductOrder>.Shared.Return(_availableItems);
    }
}