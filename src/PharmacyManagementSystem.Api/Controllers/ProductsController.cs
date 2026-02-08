using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PharmacyManagementSystem.Core.Entities;
using PharmacyManagementSystem.Core.Enums;
using PharmacyManagementSystem.Infrastructure.Data;

namespace PharmacyManagementSystem.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProductsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public ProductsController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<object>>> GetProducts([FromQuery] string? search, [FromQuery] Guid? manufacturerId, [FromQuery] Schedule? schedule)
    {
        var query = _context.Products.Include(p => p.Manufacturer).AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(p => p.Name.Contains(search) || (p.Barcode != null && p.Barcode.Contains(search)) || (p.GenericName != null && p.GenericName.Contains(search)));
        if (manufacturerId.HasValue)
            query = query.Where(p => p.ManufacturerId == manufacturerId);
        if (schedule.HasValue)
            query = query.Where(p => p.Schedule == schedule);

        var list = await query
            .OrderBy(p => p.Name)
            .Select(p => new
            {
                p.Id,
                p.Name,
                p.GenericName,
                p.Barcode,
                p.Strength,
                p.Formulation,
                p.Schedule,
                p.SalePrice,
                p.ReorderPoint,
                p.IsActive,
                ManufacturerId = p.ManufacturerId,
                ManufacturerName = p.Manufacturer.Name
            })
            .ToListAsync();

        return Ok(list);
    }

    [HttpGet("search")]
    public async Task<ActionResult<object>> SearchByBarcode([FromQuery] string barcode)
    {
        if (string.IsNullOrWhiteSpace(barcode)) return BadRequest();

        var product = await _context.Products
            .Include(p => p.Manufacturer)
            .FirstOrDefaultAsync(p => p.Barcode == barcode);

        if (product == null) return NotFound();
        return Ok(new
        {
            product.Id,
            product.Name,
            product.GenericName,
            product.Barcode,
            product.SalePrice,
            product.Schedule,
            ManufacturerName = product.Manufacturer.Name
        });
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<object>> GetProduct(Guid id)
    {
        var product = await _context.Products
            .Include(p => p.Manufacturer)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (product == null) return NotFound();
        return Ok(new
        {
            product.Id,
            product.Name,
            product.GenericName,
            product.Strength,
            product.Formulation,
            product.Schedule,
            product.Barcode,
            product.DRAPNumber,
            product.TherapeuticCategory,
            product.Contraindications,
            product.DrugInteractions,
            product.ReorderPoint,
            product.SalePrice,
            product.IsActive,
            product.ManufacturerId,
            ManufacturerName = product.Manufacturer.Name
        });
    }

    [HttpPost]
    public async Task<ActionResult<Product>> CreateProduct([FromBody] Product product)
    {
        product.Id = Guid.NewGuid();
        _context.Products.Add(product);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, product);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateProduct(Guid id, [FromBody] Product product)
    {
        if (id != product.Id) return BadRequest();
        _context.Entry(product).State = EntityState.Modified;
        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProduct(Guid id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null) return NotFound();
        product.IsActive = false;
        await _context.SaveChangesAsync();
        return NoContent();
    }

    private Guid? GetOrganizationId()
    {
        var claim = User.FindFirst("organizationId")?.Value;
        return Guid.TryParse(claim, out var id) ? id : null;
    }
}
