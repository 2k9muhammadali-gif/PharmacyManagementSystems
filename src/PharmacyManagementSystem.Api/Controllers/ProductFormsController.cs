using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PharmacyManagementSystem.Core.Entities;
using PharmacyManagementSystem.Infrastructure.Data;

namespace PharmacyManagementSystem.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProductFormsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public ProductFormsController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<object>>> GetProductForms([FromQuery] bool? activeOnly)
    {
        var query = _context.ProductForms.AsQueryable();
        if (activeOnly == true)
            query = query.Where(f => f.IsActive);
        var list = await query
            .OrderBy(f => f.DisplayOrder)
            .ThenBy(f => f.Name)
            .Select(f => new { f.Id, f.Name, f.DisplayOrder, f.IsActive })
            .ToListAsync();
        return Ok(list);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<object>> GetProductForm(Guid id)
    {
        var form = await _context.ProductForms.FindAsync(id);
        if (form == null) return NotFound();
        return Ok(new { form.Id, form.Name, form.DisplayOrder, form.IsActive });
    }

    [HttpPost]
    public async Task<ActionResult<ProductForm>> CreateProductForm([FromBody] ProductForm productForm)
    {
        if (string.IsNullOrWhiteSpace(productForm.Name))
            return BadRequest(new { message = "Name is required." });
        var exists = await _context.ProductForms.AnyAsync(f => f.Name == productForm.Name);
        if (exists)
            return BadRequest(new { message = "A product form with this name already exists." });
        productForm.Id = Guid.NewGuid();
        productForm.IsActive = true;
        _context.ProductForms.Add(productForm);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetProductForm), new { id = productForm.Id }, productForm);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateProductForm(Guid id, [FromBody] ProductForm productForm)
    {
        if (id != productForm.Id) return BadRequest();
        if (string.IsNullOrWhiteSpace(productForm.Name))
            return BadRequest(new { message = "Name is required." });
        var exists = await _context.ProductForms.AnyAsync(f => f.Name == productForm.Name && f.Id != id);
        if (exists)
            return BadRequest(new { message = "A product form with this name already exists." });
        var existing = await _context.ProductForms.FindAsync(id);
        if (existing == null) return NotFound();
        existing.Name = productForm.Name;
        existing.DisplayOrder = productForm.DisplayOrder;
        existing.IsActive = productForm.IsActive;
        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProductForm(Guid id)
    {
        var form = await _context.ProductForms.FindAsync(id);
        if (form == null) return NotFound();
        var inUse = await _context.Products.AnyAsync(p => p.ProductFormId == id);
        if (inUse)
            return BadRequest(new { message = "Cannot delete: this form is used by one or more products. Deactivate it instead." });
        _context.ProductForms.Remove(form);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
