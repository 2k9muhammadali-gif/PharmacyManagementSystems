using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PharmacyManagementSystem.Infrastructure.Data;

namespace PharmacyManagementSystem.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ComplianceController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public ComplianceController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet("controlled-substances")]
    public async Task<ActionResult<IEnumerable<object>>> GetControlledSubstances([FromQuery] Guid? branchId, [FromQuery] DateTime? from, [FromQuery] DateTime? to)
    {
        var orgId = GetOrganizationId();
        if (orgId == null) return Unauthorized();

        var query = _context.ControlledSubstanceLogs
            .Include(c => c.Product)
            .Include(c => c.Branch)
            .Where(c => c.Branch.OrganizationId == orgId);

        if (branchId.HasValue) query = query.Where(c => c.BranchId == branchId);
        if (from.HasValue) query = query.Where(c => c.LoggedAt >= from);
        if (to.HasValue) query = query.Where(c => c.LoggedAt <= to.Value.AddDays(1));

        var list = await query
            .OrderByDescending(c => c.LoggedAt)
            .Select(c => new { c.Id, c.BranchId, c.ProductId, ProductName = c.Product.Name, c.CustomerCNIC, c.Quantity, c.LoggedAt })
            .ToListAsync();

        return Ok(list);
    }

    [HttpGet("audit-log")]
    public async Task<ActionResult<IEnumerable<object>>> GetAuditLog([FromQuery] string? entityType, [FromQuery] Guid? entityId, [FromQuery] int take = 100)
    {
        var query = _context.AuditLogs.AsQueryable();
        if (!string.IsNullOrWhiteSpace(entityType)) query = query.Where(a => a.EntityType == entityType);
        if (entityId.HasValue) query = query.Where(a => a.EntityId == entityId);

        var list = await query
            .OrderByDescending(a => a.Timestamp)
            .Take(take)
            .Select(a => new { a.Id, a.UserId, a.Action, a.EntityType, a.EntityId, a.Timestamp })
            .ToListAsync();

        return Ok(list);
    }

    private Guid? GetOrganizationId()
    {
        var claim = User.FindFirst("organizationId")?.Value;
        return Guid.TryParse(claim, out var id) ? id : null;
    }
}
