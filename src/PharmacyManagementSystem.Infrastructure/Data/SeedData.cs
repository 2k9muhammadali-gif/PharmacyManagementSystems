using Microsoft.EntityFrameworkCore;
using PharmacyManagementSystem.Core.Entities;
using PharmacyManagementSystem.Core.Enums;

namespace PharmacyManagementSystem.Infrastructure.Data;

/// <summary>
/// Seeds initial data for development.
/// </summary>
public static class SeedData
{
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        if (await context.Organizations.AnyAsync())
            return;

        var org = new Organization
        {
            Id = Guid.NewGuid(),
            Name = "Default Pharmacy",
            CreatedAt = DateTime.UtcNow
        };

        var branch = new Branch
        {
            Id = Guid.NewGuid(),
            OrganizationId = org.Id,
            Name = "Main Branch",
            Address = "123 Main Street",
            IsActive = true
        };

        var user = new User
        {
            Id = Guid.NewGuid(),
            OrganizationId = org.Id,
            BranchId = branch.Id,
            Email = "admin@pharmacy.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
            FullName = "System Admin",
            Role = UserRole.SuperAdmin,
            IsActive = true
        };

        var license = new License
        {
            Id = Guid.NewGuid(),
            OrganizationId = org.Id,
            LicenseKey = "TRIAL-DEFAULT-001",
            LicenseType = LicenseType.Enterprise,
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddYears(1),
            MaxBranches = 100,
            IsActive = true,
            ActivatedAt = DateTime.UtcNow
        };

        context.Organizations.Add(org);
        context.Branches.Add(branch);
        context.Users.Add(user);
        context.Licenses.Add(license);
        await context.SaveChangesAsync();
    }
}
