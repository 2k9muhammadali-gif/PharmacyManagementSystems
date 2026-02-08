using PharmacyManagementSystem.Core.Enums;

namespace PharmacyManagementSystem.Api.DTOs.License;

public class LicenseStatusResponse
{
    public bool IsActive { get; set; }
    public LicenseType LicenseType { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int MaxBranches { get; set; }
    public int CurrentBranchCount { get; set; }
}
