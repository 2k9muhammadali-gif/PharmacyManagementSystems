namespace PharmacyManagementSystem.Core.Entities;

public class SaleReturn
{
    public Guid Id { get; set; }
    public Guid SaleId { get; set; }
    public DateTime ReturnDate { get; set; }
    public string? Reason { get; set; }
    public string? FBRCreditNoteNumber { get; set; }
    public decimal TotalRefund { get; set; }

    public Sale Sale { get; set; } = null!;
}
