namespace PharmacyManagementSystem.Core.Entities;

/// <summary>
/// Pharmaceutical manufacturer (company). Products are supplied through distributions.
/// </summary>
public class Manufacturer
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? ContactInfo { get; set; }
    public string? Address { get; set; }

    public ICollection<Product> Products { get; set; } = new List<Product>();
}
