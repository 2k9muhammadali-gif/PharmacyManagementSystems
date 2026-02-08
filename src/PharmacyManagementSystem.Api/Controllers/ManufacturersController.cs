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
public class ManufacturersController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public ManufacturersController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Manufacturer>>> GetManufacturers([FromQuery] string? search)
    {
        var orgId = GetOrganizationId();
        if (orgId == null) return Unauthorized();

        var query = _context.Manufacturers.AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(m => m.Name.Contains(search));

        var list = await query.OrderBy(m => m.Name).ToListAsync();
        return Ok(list);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Manufacturer>> GetManufacturer(Guid id)
    {
        var manufacturer = await _context.Manufacturers.FindAsync(id);
        if (manufacturer == null) return NotFound();
        return Ok(manufacturer);
    }

    [HttpPost]
    public async Task<ActionResult<Manufacturer>> CreateManufacturer([FromBody] Manufacturer manufacturer)
    {
        manufacturer.Id = Guid.NewGuid();
        _context.Manufacturers.Add(manufacturer);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetManufacturer), new { id = manufacturer.Id }, manufacturer);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateManufacturer(Guid id, [FromBody] Manufacturer manufacturer)
    {
        if (id != manufacturer.Id) return BadRequest();
        _context.Entry(manufacturer).State = EntityState.Modified;
        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteManufacturer(Guid id)
    {
        var manufacturer = await _context.Manufacturers.FindAsync(id);
        if (manufacturer == null) return NotFound();
        _context.Manufacturers.Remove(manufacturer);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    private Guid? GetOrganizationId()
    {
        var claim = User.FindFirst("organizationId")?.Value;
        return Guid.TryParse(claim, out var id) ? id : null;
    }
}
