using System.Runtime.InteropServices;

namespace PerformanceDemo.Models;

// This is the main data being produced and consumed
// It is struct type that is block sized to 64 bytes
//  to prevent false sharing in the CPU cache
[StructLayout(LayoutKind.Sequential, Size = 64)]
public struct ProductOrder
{
    public int ProductId;
    public int OrderId;
    public int Quantity;
    public float UnitPrice;
    public int DiscountPercentage;
    public float TaxRate;
    public float ShippingCost;
    public int AlternateProductId;
    public int ShipPriority;

    public ProductOrder()
    {
    }

    public void Init(int productId, int orderId, int quantity, float unitPrice,
        int discountPercentage, float taxRate, float shippingCost, int alternateProductId, 
        int shipPriority)
    {
        ProductId = productId;
        OrderId = orderId;
        Quantity = quantity;
        UnitPrice = unitPrice;
        DiscountPercentage = discountPercentage;
        TaxRate = taxRate;
        ShippingCost = shippingCost;
        AlternateProductId = alternateProductId;
        ShipPriority = shipPriority;
    }
}
