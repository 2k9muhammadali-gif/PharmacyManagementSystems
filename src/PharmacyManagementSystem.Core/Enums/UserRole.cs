namespace PharmacyManagementSystem.Core.Enums;

/// <summary>
/// User role in the system.
/// </summary>
public enum UserRole
{
    SuperAdmin = 0,
    OrganizationOwner = 1,
    BranchManager = 2,
    LicensedPharmacist = 3,
    SalesStaff = 4,
    Accountant = 5,
    StockKeeper = 6
}
