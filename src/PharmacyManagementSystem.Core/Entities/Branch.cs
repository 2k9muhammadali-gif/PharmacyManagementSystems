namespace PharmacyManagementSystem.Core.Entities;

/// <summary>
/// Branch (store) entity.
/// </summary>
public class Branch
{
    public Guid Id { get; set; }
    public Guid OrganizationId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? FBRStoreId { get; set; }
    public string? FBRPosDeviceId { get; set; }
    public bool IsActive { get; set; } = true;

    public Organization Organization { get; set; } = null!;
    public ICollection<User> Users { get; set; } = new List<User>();
}
