using PerformanceDemo.Models;

namespace PerformanceDemo.Core;

public class OrderConsumer
{
    private readonly OrderQueue _orderQueue;
    private readonly OrderAggregationEngine _aggregationEngine;
    private ProductOrder _order;

    public OrderConsumer(ref OrderQueue orderQueue)
    {
        _orderQueue = orderQueue;
        _order = new ProductOrder();
        _aggregationEngine = new OrderAggregationEngine();
    }

    public bool ConsumeNextOrder()
    {
        if (_orderQueue.Dequeue(ref _order))
        {
            Parallel.Invoke(
                () => _aggregationEngine.UpdateTotalQty(ref _order),
                () => _aggregationEngine.UpdateTotalCost(ref _order)
            );
            return true;
        }
        
        return false;
    }

    public (long, double) GetTotals()
    {
        return _aggregationEngine.Totals;
    }
}