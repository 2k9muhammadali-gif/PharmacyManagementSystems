namespace PharmacyManagementSystem.Core.Entities;

/// <summary>
/// Stock batch entity (inventory per branch). Tracks Distribution and Company (manufacturer) source.
/// </summary>
public class StockBatch
{
    public Guid Id { get; set; }
    public Guid BranchId { get; set; }
    public Guid ProductId { get; set; }
    public Guid? DistributionId { get; set; }
    public Guid? ManufacturerId { get; set; }
    public string BatchNumber { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public DateTime ExpiryDate { get; set; }
    public decimal PurchasePrice { get; set; }
    public DateTime ReceivedAt { get; set; } = DateTime.UtcNow;

    public Branch Branch { get; set; } = null!;
    public Product Product { get; set; } = null!;
    public Distribution? Distribution { get; set; }
    public Manufacturer? Manufacturer { get; set; }
}
