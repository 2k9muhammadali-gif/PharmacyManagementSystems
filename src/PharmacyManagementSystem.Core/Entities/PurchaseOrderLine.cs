namespace PharmacyManagementSystem.Core.Entities;

/// <summary>
/// Purchase order line item. Tracks which company (manufacturer) the product is ordered from under the distribution.
/// </summary>
public class PurchaseOrderLine
{
    public Guid Id { get; set; }
    public Guid PurchaseOrderId { get; set; }
    public Guid ProductId { get; set; }
    public Guid ManufacturerId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }

    public PurchaseOrder PurchaseOrder { get; set; } = null!;
    public Product Product { get; set; } = null!;
    public Manufacturer Manufacturer { get; set; } = null!;
}
