using System.Runtime.CompilerServices;
using PerformanceDemo.Models;

namespace PerformanceDemo.Core;

public class OrderAggregationEngine
{
    private long _totalQty = 0;
    private double _totalCost = 0;
    private int _counter = 0;
    
    public OrderAggregationEngine()
    {
    }

    public (long, double) Totals
    {
        get
        {
            return (_totalQty,  _totalCost);
        }
    }
    
    public void UpdateTotalCost(ref readonly ProductOrder order)
    {
        if (_counter++ < 2)
        {
            Console.Write("    ##### UpdateTotalCost() running on processor ID ");
            Console.WriteLine(Thread.GetCurrentProcessorId());
        }

        while (true)
        {
            double addValue = order.UnitPrice * order.Quantity;
            double current = Volatile.Read(ref _totalCost);
            if (Interlocked.CompareExchange(ref _totalCost, current + addValue, current) == current)
                break;
        }
    }

    public void UpdateTotalQty(ref readonly ProductOrder order)
    {
        if (_counter++ < 2)
        {
            Console.Write("    ##### UpdateTotalQty() running on processor ID ");
            Console.WriteLine(Thread.GetCurrentProcessorId());
        }
        
        long addQty = order.Quantity;
        Interlocked.Add(ref _totalQty, addQty);
    }
}
