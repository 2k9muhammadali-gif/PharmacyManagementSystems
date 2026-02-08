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
public class DistributionsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public DistributionsController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<object>>> GetDistributions([FromQuery] string? search)
    {
        var query = _context.Distributions.AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(d => d.Name.Contains(search));

        var list = await query
            .Select(d => new
            {
                d.Id,
                d.Name,
                d.Contact,
                d.Address,
                d.Phone,
                CompanyCount = d.Companies.Count
            })
            .OrderBy(d => d.Name)
            .ToListAsync();
        return Ok(list);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<object>> GetDistribution(Guid id)
    {
        var dist = await _context.Distributions
            .Include(d => d.Companies).ThenInclude(c => c.Manufacturer)
            .Where(d => d.Id == id)
            .Select(d => new
            {
                d.Id,
                d.Name,
                d.Contact,
                d.Address,
                d.Phone,
                Companies = d.Companies.Select(c => new { c.Id, c.ManufacturerId, ManufacturerName = c.Manufacturer.Name })
            })
            .FirstOrDefaultAsync();
        if (dist == null) return NotFound();
        return Ok(dist);
    }

    [HttpPost]
    public async Task<ActionResult<Distribution>> CreateDistribution([FromBody] Distribution distribution)
    {
        distribution.Id = Guid.NewGuid();
        _context.Distributions.Add(distribution);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetDistribution), new { id = distribution.Id }, distribution);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateDistribution(Guid id, [FromBody] Distribution distribution)
    {
        if (id != distribution.Id) return BadRequest();
        _context.Entry(distribution).State = EntityState.Modified;
        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteDistribution(Guid id)
    {
        var dist = await _context.Distributions.FindAsync(id);
        if (dist == null) return NotFound();
        _context.Distributions.Remove(dist);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
