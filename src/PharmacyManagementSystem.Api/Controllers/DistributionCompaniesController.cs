using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PharmacyManagementSystem.Core.Entities;
using PharmacyManagementSystem.Infrastructure.Data;

namespace PharmacyManagementSystem.Api.Controllers;

[ApiController]
[Route("api/distributions/{distributionId:guid}/companies")]
[Authorize]
public class DistributionCompaniesController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public DistributionCompaniesController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<object>>> GetCompanies(Guid distributionId)
    {
        var list = await _context.DistributionCompanies
            .Where(dc => dc.DistributionId == distributionId)
            .Include(dc => dc.Manufacturer)
            .Select(dc => new { dc.Id, dc.ManufacturerId, ManufacturerName = dc.Manufacturer.Name })
            .ToListAsync();
        return Ok(list);
    }

    [HttpPost]
    public async Task<ActionResult<object>> AddCompany(Guid distributionId, [FromBody] AddCompanyRequest request)
    {
        var dist = await _context.Distributions.FindAsync(distributionId);
        if (dist == null) return NotFound();

        var manufacturer = await _context.Manufacturers.FindAsync(request.ManufacturerId);
        if (manufacturer == null) return BadRequest(new { message = "Manufacturer not found." });

        var exists = await _context.DistributionCompanies
            .AnyAsync(dc => dc.DistributionId == distributionId && dc.ManufacturerId == request.ManufacturerId);
        if (exists) return BadRequest(new { message = "Company already added to this distribution." });

        var dc = new DistributionCompany
        {
            Id = Guid.NewGuid(),
            DistributionId = distributionId,
            ManufacturerId = request.ManufacturerId
        };
        _context.DistributionCompanies.Add(dc);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetCompanies), new { distributionId }, new { dc.Id, dc.ManufacturerId, ManufacturerName = manufacturer.Name });
    }

    [HttpDelete("{manufacturerId:guid}")]
    public async Task<IActionResult> RemoveCompany(Guid distributionId, Guid manufacturerId)
    {
        var dc = await _context.DistributionCompanies
            .FirstOrDefaultAsync(dc => dc.DistributionId == distributionId && dc.ManufacturerId == manufacturerId);
        if (dc == null) return NotFound();

        _context.DistributionCompanies.Remove(dc);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}

public class AddCompanyRequest
{
    public Guid ManufacturerId { get; set; }
}
