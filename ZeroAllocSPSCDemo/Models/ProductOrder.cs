using System.Runtime.InteropServices;

namespace ZeroAllocSPSCDemo.Models;

[StructLayout(LayoutKind.Sequential, Size = 16)]
public struct Product
{
    public int ProductId;
    public int OrderId;
    public int Quantity;
    public float UnitPrice; 

    public Product() : this(0, 0, 0, 0f)
    {
    }

    public Product(int productId, int orderId, int quantity, float unitPrice)
    {
        ProductId = productId;
        OrderId =  orderId;
        Quantity = quantity;
        UnitPrice = unitPrice;
    }
}
