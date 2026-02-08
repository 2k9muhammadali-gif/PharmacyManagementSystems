using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PharmacyManagementSystem.Core.Entities;
using PharmacyManagementSystem.Infrastructure.Data;

namespace PharmacyManagementSystem.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class BranchesController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public BranchesController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<object>>> GetBranches()
    {
        var orgId = GetOrganizationId();
        if (orgId == null) return Unauthorized();

        var branches = await _context.Branches
            .Where(b => b.OrganizationId == orgId)
            .Select(b => new { b.Id, b.Name, b.Address, b.Phone, b.IsActive, b.FBRStoreId })
            .ToListAsync();

        return Ok(branches);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<object>> GetBranch(Guid id)
    {
        var orgId = GetOrganizationId();
        if (orgId == null) return Unauthorized();

        var branch = await _context.Branches
            .Where(b => b.OrganizationId == orgId && b.Id == id)
            .Select(b => new { b.Id, b.Name, b.Address, b.Phone, b.IsActive, b.FBRStoreId, b.FBRPosDeviceId })
            .FirstOrDefaultAsync();

        if (branch == null) return NotFound();
        return Ok(branch);
    }

    [HttpPost]
    public async Task<ActionResult<Branch>> CreateBranch([FromBody] CreateBranchRequest request)
    {
        var orgId = GetOrganizationId();
        if (orgId == null) return Unauthorized();

        var license = await _context.Licenses
            .Where(l => l.OrganizationId == orgId && l.IsActive)
            .OrderByDescending(l => l.EndDate)
            .FirstOrDefaultAsync();

        if (license != null)
        {
            var branchCount = await _context.Branches.CountAsync(b => b.OrganizationId == orgId);
            if (branchCount >= license.MaxBranches)
                return BadRequest(new { message = $"License allows maximum {license.MaxBranches} branches." });
        }

        var branch = new Branch
        {
            Id = Guid.NewGuid(),
            OrganizationId = orgId.Value,
            Name = request.Name,
            Address = request.Address,
            Phone = request.Phone,
            FBRStoreId = request.FBRStoreId,
            FBRPosDeviceId = request.FBRPosDeviceId,
            IsActive = true
        };

        _context.Branches.Add(branch);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetBranch), new { id = branch.Id }, new { branch.Id, branch.Name });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateBranch(Guid id, [FromBody] UpdateBranchRequest request)
    {
        var orgId = GetOrganizationId();
        if (orgId == null) return Unauthorized();

        var branch = await _context.Branches.FirstOrDefaultAsync(b => b.OrganizationId == orgId && b.Id == id);
        if (branch == null) return NotFound();

        branch.Name = request.Name ?? branch.Name;
        branch.Address = request.Address ?? branch.Address;
        branch.Phone = request.Phone ?? branch.Phone;
        branch.FBRStoreId = request.FBRStoreId ?? branch.FBRStoreId;
        branch.FBRPosDeviceId = request.FBRPosDeviceId ?? branch.FBRPosDeviceId;
        branch.IsActive = request.IsActive ?? branch.IsActive;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    private Guid? GetOrganizationId()
    {
        var claim = User.FindFirst("organizationId")?.Value;
        return Guid.TryParse(claim, out var id) ? id : null;
    }
}

public class CreateBranchRequest
{
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? FBRStoreId { get; set; }
    public string? FBRPosDeviceId { get; set; }
}

public class UpdateBranchRequest
{
    public string? Name { get; set; }
    public string? Address { get; set; }
    public string? Phone { get; set; }
    public string? FBRStoreId { get; set; }
    public string? FBRPosDeviceId { get; set; }
    public bool? IsActive { get; set; }
}
