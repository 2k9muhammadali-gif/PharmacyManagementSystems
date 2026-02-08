using PharmacyManagementSystem.Core.Enums;

namespace PharmacyManagementSystem.Core.Entities;

/// <summary>
/// User entity.
/// </summary>
public class User
{
    public Guid Id { get; set; }
    public Guid OrganizationId { get; set; }
    public Guid? BranchId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public bool IsActive { get; set; } = true;

    public Organization Organization { get; set; } = null!;
    public Branch? Branch { get; set; }
}
