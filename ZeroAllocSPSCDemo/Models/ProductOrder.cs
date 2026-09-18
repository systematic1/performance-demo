using System.Runtime.InteropServices;

namespace ZeroAllocSPSCDemo.Models;

// This is the main data being produced and consumed
// It is struct type that is block sized to 64 bytes
//  to prevent false sharing in the CPU cache
[StructLayout(LayoutKind.Sequential, Size = 16)]
public struct ProductOrder
{
    public int ProductId;
    public int OrderId;
    public int Quantity;
    public float UnitPrice; 

    public ProductOrder()
    {
    }

    public void Init(int productId, int orderId, int quantity, float unitPrice)
    {
        ProductId = productId;
        OrderId = orderId;
        Quantity = quantity;
        UnitPrice = unitPrice;
    }
}
