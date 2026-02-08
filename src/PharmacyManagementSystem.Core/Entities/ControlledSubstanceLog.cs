namespace PharmacyManagementSystem.Core.Entities;

/// <summary>
/// Log for Schedule H (controlled substance) sales.
/// </summary>
public class ControlledSubstanceLog
{
    public Guid Id { get; set; }
    public Guid BranchId { get; set; }
    public Guid ProductId { get; set; }
    public Guid SaleId { get; set; }
    public string CustomerCNIC { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public DateTime LoggedAt { get; set; } = DateTime.UtcNow;

    public Product Product { get; set; } = null!;
    public Branch Branch { get; set; } = null!;
}
