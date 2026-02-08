namespace PharmacyManagementSystem.Core.Entities;

/// <summary>
/// Pharmaceutical manufacturer (company on Distribution order form).
/// </summary>
public class Manufacturer
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? ContactInfo { get; set; }
    public string? Address { get; set; }

    public ICollection<Product> Products { get; set; } = new List<Product>();
}
