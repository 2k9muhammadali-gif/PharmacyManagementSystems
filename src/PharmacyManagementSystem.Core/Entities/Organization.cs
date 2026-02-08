namespace PharmacyManagementSystem.Core.Entities;

/// <summary>
/// Organization (pharmacy chain) entity.
/// </summary>
public class Organization
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<License> Licenses { get; set; } = new List<License>();
    public ICollection<Branch> Branches { get; set; } = new List<Branch>();
}
