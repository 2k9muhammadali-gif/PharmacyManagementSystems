using PharmacyManagementSystem.Core.Enums;

namespace PharmacyManagementSystem.Core.Entities;

/// <summary>
/// Sale entity.
/// </summary>
public class Sale
{
    public Guid Id { get; set; }
    public Guid BranchId { get; set; }
    public Guid? CustomerId { get; set; }
    public DateTime SaleDate { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal DiscountPercent { get; set; }
    public PaymentMode PaymentMode { get; set; }
    public string? FBRFiscalNumber { get; set; }
    public string? FBRQRCode { get; set; }
    public string? FBRStatus { get; set; }
    public Guid CreatedBy { get; set; }

    public Branch Branch { get; set; } = null!;
    public Customer? Customer { get; set; }
    public ICollection<SaleLine> Lines { get; set; } = new List<SaleLine>();
}
