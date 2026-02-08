namespace PharmacyManagementSystem.Core.Entities;

/// <summary>
/// Sale line item.
/// </summary>
public class SaleLine
{
    public Guid Id { get; set; }
    public Guid SaleId { get; set; }
    public Guid ProductId { get; set; }
    public Guid? StockBatchId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public bool IsPrescription { get; set; }

    public Sale Sale { get; set; } = null!;
    public Product Product { get; set; } = null!;
}
