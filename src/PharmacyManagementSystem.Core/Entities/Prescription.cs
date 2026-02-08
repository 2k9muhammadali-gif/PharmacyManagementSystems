namespace PharmacyManagementSystem.Core.Entities;

/// <summary>
/// Prescription entity (linked to sale).
/// </summary>
public class Prescription
{
    public Guid Id { get; set; }
    public Guid SaleId { get; set; }
    public Guid? CustomerId { get; set; }
    public string? DoctorName { get; set; }
    public string? ClinicDetails { get; set; }
    public string? ImageUrl { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Sale Sale { get; set; } = null!;
}
