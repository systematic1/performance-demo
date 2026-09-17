using System.Buffers;
using ZeroAllocSPSCDemo.Models;

namespace ZeroAllocSPSCDemo.Managers;

public class OrderCollector : IDisposable
{
    private static readonly int Capacity = 256 * 1024;
    
    static OrderCollector()
    {
        // Pre-allocate rentable memory array buffer for lock-free ring array
        var tempArray = ArrayPool<ProductOrder>.Shared.Rent(Capacity);
        ArrayPool<ProductOrder>.Shared.Return(tempArray);
    }

    private ProductOrder[] _orders;
    private int _head;
    private int _tail;
    
    public OrderCollector()
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