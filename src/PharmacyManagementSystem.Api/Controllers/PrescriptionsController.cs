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
public class PrescriptionsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public PrescriptionsController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<object>>> GetPrescriptions([FromQuery] Guid? saleId, [FromQuery] Guid? customerId)
    {
        var orgId = GetOrganizationId();
        if (orgId == null) return Unauthorized();

        var query = _context.Prescriptions
            .Include(p => p.Sale)
            .Where(p => p.Sale.Branch.OrganizationId == orgId);

        if (saleId.HasValue) query = query.Where(p => p.SaleId == saleId);
        if (customerId.HasValue) query = query.Where(p => p.CustomerId == customerId);

        var list = await query
            .OrderByDescending(p => p.CreatedAt)
            .Select(p => new { p.Id, p.SaleId, p.CustomerId, p.DoctorName, p.ClinicDetails, p.ImageUrl, p.CreatedAt })
            .ToListAsync();

        return Ok(list);
    }

    [HttpPost("upload")]
    public async Task<ActionResult<object>> UploadPrescription([FromBody] PrescriptionUploadRequest request)
    {
        var orgId = GetOrganizationId();
        if (orgId == null) return Unauthorized();

        var sale = await _context.Sales.FirstOrDefaultAsync(s => s.Id == request.SaleId && s.Branch.OrganizationId == orgId);
        if (sale == null) return NotFound();

        var prescription = new Prescription
        {
            Id = Guid.NewGuid(),
            SaleId = request.SaleId,
            CustomerId = request.CustomerId,
            DoctorName = request.DoctorName,
            ClinicDetails = request.ClinicDetails,
            ImageUrl = request.ImageUrl
        };

        _context.Prescriptions.Add(prescription);
        await _context.SaveChangesAsync();

        return Ok(new { prescription.Id });
    }

    private Guid? GetOrganizationId()
    {
        var claim = User.FindFirst("organizationId")?.Value;
        return Guid.TryParse(claim, out var id) ? id : null;
    }
}

public class PrescriptionUploadRequest
{
    public Guid SaleId { get; set; }
    public Guid? CustomerId { get; set; }
    public string? DoctorName { get; set; }
    public string? ClinicDetails { get; set; }
    public string? ImageUrl { get; set; }
}
