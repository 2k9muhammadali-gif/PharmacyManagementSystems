using PharmacyManagementSystem.Core.Enums;

namespace PharmacyManagementSystem.Core.Entities;

/// <summary>
/// Purchase order from Distribution.
/// </summary>
public class PurchaseOrder
{
    public Guid Id { get; set; }
    public Guid BranchId { get; set; }
    public Guid DistributionId { get; set; }
    public DateTime OrderDate { get; set; }
    public PurchaseOrderStatus Status { get; set; }
    public decimal TotalAmount { get; set; }

    public Branch Branch { get; set; } = null!;
    public Distribution Distribution { get; set; } = null!;
    public ICollection<PurchaseOrderLine> Lines { get; set; } = new List<PurchaseOrderLine>();
}
