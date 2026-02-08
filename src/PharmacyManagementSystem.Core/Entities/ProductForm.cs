namespace PharmacyManagementSystem.Core.Entities;

/// <summary>
/// Configurable product form (e.g. Tablet, Capsule, Injection).
/// Managed in System Configuration.
/// </summary>
public class ProductForm
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;
}
