using ZeroAllocSPSCDemo.Models;

namespace ZeroAllocSPSCDemo.Managers;

public class OrderConsumer
{
    private readonly OrderQueue _orderQueue;
    private ProductOrder? _order;

    public OrderConsumer(ref OrderQueue orderQueue)
    {
        _orderQueue = orderQueue;
        _order = null;
    }

    public bool ConsumeNextOrder()
    {
        return _orderQueue.Dequeue(ref _order);
    }
}