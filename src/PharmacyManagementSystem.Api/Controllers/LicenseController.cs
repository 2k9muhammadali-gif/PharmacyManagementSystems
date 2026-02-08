using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PharmacyManagementSystem.Api.DTOs.License;
using PharmacyManagementSystem.Core.Enums;
using PharmacyManagementSystem.Infrastructure.Data;

namespace PharmacyManagementSystem.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LicenseController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public LicenseController(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Activate organization with license key. No auth required.
    /// </summary>
    [HttpPost("activate")]
    [AllowAnonymous]
    public async Task<ActionResult> Activate([FromBody] ActivateLicenseRequest request)
    {
        var license = await _context.Licenses
            .FirstOrDefaultAsync(l => l.LicenseKey == request.LicenseKey);

        if (license == null)
            return BadRequest(new { message = "Invalid license key." });

        if (license.ActivatedAt != null)
            return Ok(new { message = "License already activated." });

        license.ActivatedAt = DateTime.UtcNow;
        license.IsActive = true;
        await _context.SaveChangesAsync();

        return Ok(new { message = "License activated successfully." });
    }

    /// <summary>
    /// Get license status for current organization. Requires auth.
    /// </summary>
    [HttpGet("status")]
    [Authorize]
    public async Task<ActionResult<LicenseStatusResponse>> GetStatus()
    {
        var orgIdClaim = User.FindFirst("organizationId")?.Value;
        if (string.IsNullOrEmpty(orgIdClaim) || !Guid.TryParse(orgIdClaim, out var orgId))
            return Unauthorized();

        var license = await _context.Licenses
            .Where(l => l.OrganizationId == orgId)
            .OrderByDescending(l => l.ActivatedAt)
            .FirstOrDefaultAsync();

        if (license == null)
            return Ok(new LicenseStatusResponse { IsActive = false });

        var branchCount = await _context.Branches.CountAsync(b => b.OrganizationId == orgId);

        return Ok(new LicenseStatusResponse
        {
            IsActive = license.IsActive && license.ActivatedAt != null
                && license.StartDate <= DateTime.UtcNow && license.EndDate >= DateTime.UtcNow,
            LicenseType = license.LicenseType,
            StartDate = license.StartDate,
            EndDate = license.EndDate,
            MaxBranches = license.MaxBranches,
            CurrentBranchCount = branchCount
        });
    }
}
