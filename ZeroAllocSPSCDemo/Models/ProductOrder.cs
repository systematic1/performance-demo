using System.Runtime.InteropServices;

namespace ZeroAllocSPSCDemo.Models;

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
