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

    /// <summary>
    /// Returns the list of active product forms (for dropdowns).
    /// </summary>
    [HttpGet("forms")]
    public async Task<ActionResult<IEnumerable<object>>> GetProductForms()
    {
        var forms = await _context.ProductForms
            .Where(f => f.IsActive)
            .OrderBy(f => f.DisplayOrder)
            .ThenBy(f => f.Name)
            .Select(f => new { value = f.Id.ToString(), label = f.Name })
            .ToListAsync();
        return Ok(forms);
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<object>>> GetProducts([FromQuery] string? search, [FromQuery] Guid? manufacturerId, [FromQuery] Schedule? schedule, [FromQuery] Guid? productFormId)
    {
        var query = _context.Products.Include(p => p.Manufacturer).Include(p => p.ProductForm).AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(p => p.Name.Contains(search) || (p.Barcode != null && p.Barcode.Contains(search)) || (p.GenericName != null && p.GenericName.Contains(search)));
        if (manufacturerId.HasValue)
            query = query.Where(p => p.ManufacturerId == manufacturerId);
        if (schedule.HasValue)
            query = query.Where(p => p.Schedule == schedule);
        if (productFormId.HasValue)
            query = query.Where(p => p.ProductFormId == productFormId);

        var list = await query
            .OrderBy(p => p.Name)
            .Select(p => new
            {
                p.Id,
                p.Name,
                p.GenericName,
                p.Barcode,
                p.Strength,
                Formulation = p.ProductForm != null ? p.ProductForm.Name : null,
                p.ProductFormId,
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
            .Include(p => p.ProductForm)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (product == null) return NotFound();
        return Ok(new
        {
            product.Id,
            product.Name,
            product.GenericName,
            product.Strength,
            product.ProductFormId,
            Formulation = product.ProductForm != null ? product.ProductForm.Name : null,
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
        if (product.ProductFormId.HasValue)
        {
            var formExists = await _context.ProductForms.AnyAsync(f => f.Id == product.ProductFormId && f.IsActive);
            if (!formExists)
                return BadRequest(new { message = "Invalid product form selected." });
        }
        product.Id = Guid.NewGuid();
        _context.Products.Add(product);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, product);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateProduct(Guid id, [FromBody] Product product)
    {
        if (id != product.Id) return BadRequest();
        var existing = await _context.Products.FindAsync(id);
        if (existing == null) return NotFound();
        if (product.ProductFormId.HasValue)
        {
            var formExists = await _context.ProductForms.AnyAsync(f => f.Id == product.ProductFormId && f.IsActive);
            if (!formExists)
                return BadRequest(new { message = "Invalid product form selected." });
        }
        existing.ManufacturerId = product.ManufacturerId;
        existing.Name = product.Name;
        existing.GenericName = product.GenericName;
        existing.Strength = product.Strength;
        existing.ProductFormId = product.ProductFormId;
        existing.Schedule = product.Schedule;
        existing.Barcode = product.Barcode;
        existing.ReorderPoint = product.ReorderPoint;
        existing.SalePrice = product.SalePrice;
        existing.IsActive = product.IsActive;
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
