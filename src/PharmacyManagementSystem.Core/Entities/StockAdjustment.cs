using PharmacyManagementSystem.Core.Enums;

namespace PharmacyManagementSystem.Core.Entities;

public class StockAdjustment
{
    public Guid Id { get; set; }
    public Guid BranchId { get; set; }
    public Guid StockBatchId { get; set; }
    public int Quantity { get; set; }
    public AdjustmentType AdjustmentType { get; set; }
    public string? Reason { get; set; }
    public DateTime AdjustedAt { get; set; } = DateTime.UtcNow;
    public Guid AdjustedBy { get; set; }

    public Branch Branch { get; set; } = null!;
    public StockBatch StockBatch { get; set; } = null!;
}
