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
public class CustomersController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public CustomersController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Customer>>> GetCustomers([FromQuery] string? search)
    {
        var query = _context.Customers.AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(c => c.Name.Contains(search) || (c.Phone != null && c.Phone.Contains(search)) || (c.CNIC != null && c.CNIC.Contains(search)));

        var list = await query.OrderBy(c => c.Name).ToListAsync();
        return Ok(list);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Customer>> GetCustomer(Guid id)
    {
        var customer = await _context.Customers.FindAsync(id);
        if (customer == null) return NotFound();
        return Ok(customer);
    }

    [HttpGet("{id}/sales")]
    public async Task<ActionResult<IEnumerable<object>>> GetCustomerSales(Guid id)
    {
        var orgId = GetOrganizationId();
        if (orgId == null) return Unauthorized();

        var sales = await _context.Sales
            .Where(s => s.CustomerId == id && s.Branch.OrganizationId == orgId)
            .Select(s => new { s.Id, s.SaleDate, s.TotalAmount, s.BranchId })
            .OrderByDescending(s => s.SaleDate)
            .ToListAsync();

        return Ok(sales);
    }

    [HttpGet("{id}/prescriptions")]
    public async Task<ActionResult<IEnumerable<object>>> GetCustomerPrescriptions(Guid id)
    {
        var prescriptions = await _context.Prescriptions
            .Where(p => p.CustomerId == id)
            .Select(p => new { p.Id, p.DoctorName, p.ClinicDetails, p.CreatedAt })
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();

        return Ok(prescriptions);
    }

    [HttpPost]
    public async Task<ActionResult<Customer>> CreateCustomer([FromBody] Customer customer)
    {
        customer.Id = Guid.NewGuid();
        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetCustomer), new { id = customer.Id }, customer);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateCustomer(Guid id, [FromBody] Customer customer)
    {
        if (id != customer.Id) return BadRequest();
        _context.Entry(customer).State = EntityState.Modified;
        await _context.SaveChangesAsync();
        return NoContent();
    }

    private Guid? GetOrganizationId()
    {
        var claim = User.FindFirst("organizationId")?.Value;
        return Guid.TryParse(claim, out var id) ? id : null;
    }
}
