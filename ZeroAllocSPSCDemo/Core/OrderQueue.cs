using System.Buffers;
using ZeroAllocSPSCDemo.Models;

namespace ZeroAllocSPSCDemo.Managers;

public class OrderQueue : IDisposable
{
    private static readonly int Capacity = 16 * 1024;
    
    private readonly ProductOrder[] _orders;
    private int _head;
    private int _tail;
    
    public OrderQueue()
    {
        _orders = ArrayPool<ProductOrder>.Shared.Rent(Capacity);
        _head = 0; // dequeue from
        _tail = 0; // enqueue at
    }

    public bool IsEmpty()
    {
        return _head == _tail;
    }

    public bool IsFull()
    {
        return (_tail + 1) % Capacity == _head;
    }

    public int Count()
    {
        return (
            _head < _tail ? 
            _tail - _head : 
            Capacity - _head - _tail
        );
    }

    public bool Enqueue(ProductOrder order)
    {
        if (!IsFull())
        {
            _orders[_tail] = order;
            _tail = (_tail + 1) % Capacity;
            return true;
        }

        return false;
    }

    public ProductOrder? Dequeue()
    {
        if (!IsEmpty())
        {
            var order = _orders[_head];
            _head++;
            return order;
        }

        return null;
    }
    
    public void Dispose()
    {
        if (_orders?.Length > 0)
            ArrayPool<ProductOrder>.Shared.Return(_orders);
    }
}