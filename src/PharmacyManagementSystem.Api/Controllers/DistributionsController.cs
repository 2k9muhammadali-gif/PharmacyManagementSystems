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
    public async Task<ActionResult<IEnumerable<Distribution>>> GetDistributions([FromQuery] string? search)
    {
        var query = _context.Distributions.AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(d => d.Name.Contains(search));

        var list = await query.OrderBy(d => d.Name).ToListAsync();
        return Ok(list);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Distribution>> GetDistribution(Guid id)
    {
        var dist = await _context.Distributions.FindAsync(id);
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
}
