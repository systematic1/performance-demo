using System.Buffers;
using ZeroAllocSPSCDemo.Models;

namespace ZeroAllocSPSCDemo.Managers;

public class OrderQueue : IDisposable
{
    private static readonly int Capacity = 16 * 1024;
    
    private readonly ProductOrder[] _orders;
    private long _head;
    private long _tail;
    
    public OrderQueue()
    {
        _orders = ArrayPool<ProductOrder>.Shared.Rent(Capacity);
        _head = 0; // dequeue from
        _tail = 0; // enqueue at
    }

    public bool IsEmpty()
    {
        return _head % Capacity == _tail % Capacity;
    }

    public bool IsFull()
    {
        return (_tail - _head) >= Capacity;
    }

    public long Count()
    {
        return (_tail - _head) % Capacity;
    }

    public bool Enqueue(ref ProductOrder order)
    {
        if (!IsFull())
        {
            _orders[_tail % Capacity] = order;
            _tail++;
            return true;
        }

        return false;
    }

    public bool Dequeue(ref ProductOrder? order)
    {
        if (!IsEmpty())
        {
            order = _orders[_head % Capacity];
            _head++;
            return true;
        }

        order = default(ProductOrder);
        return false;
    }
    
    public void Dispose()
    {
        if (_orders?.Length > 0)
            ArrayPool<ProductOrder>.Shared.Return(_orders);
    }
}