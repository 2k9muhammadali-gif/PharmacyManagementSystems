using PharmacyManagementSystem.Core.Enums;

namespace PharmacyManagementSystem.Core.Entities;

public class Payment
{
    public Guid Id { get; set; }
    public Guid PurchaseOrderId { get; set; }
    public decimal Amount { get; set; }
    public DateTime PaymentDate { get; set; }
    public PaymentMode PaymentMode { get; set; }
    public string? Reference { get; set; }

    public PurchaseOrder PurchaseOrder { get; set; } = null!;
}
