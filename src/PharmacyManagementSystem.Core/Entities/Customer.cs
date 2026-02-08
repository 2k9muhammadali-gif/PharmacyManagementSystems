namespace PharmacyManagementSystem.Core.Entities;

/// <summary>
/// Customer entity.
/// </summary>
public class Customer
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? CNIC { get; set; }
    public string? Email { get; set; }
    public decimal CreditLimit { get; set; }
    public string? Address { get; set; }
}
