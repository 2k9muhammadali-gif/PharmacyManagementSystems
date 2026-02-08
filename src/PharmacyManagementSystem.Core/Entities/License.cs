using PharmacyManagementSystem.Core.Enums;

namespace PharmacyManagementSystem.Core.Entities;

/// <summary>
/// License entity for organization activation.
/// </summary>
public class License
{
    public Guid Id { get; set; }
    public Guid OrganizationId { get; set; }
    public string LicenseKey { get; set; } = string.Empty;
    public LicenseType LicenseType { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int MaxBranches { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime? ActivatedAt { get; set; }

    public Organization Organization { get; set; } = null!;
}
