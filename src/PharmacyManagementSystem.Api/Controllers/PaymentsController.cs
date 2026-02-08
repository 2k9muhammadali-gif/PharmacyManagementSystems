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
public class PaymentsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public PaymentsController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<object>>> GetPayments([FromQuery] Guid? purchaseOrderId)
    {
        var orgId = GetOrganizationId();
        if (orgId == null) return Unauthorized();

        var query = _context.Payments
            .Include(p => p.PurchaseOrder)
            .Where(p => p.PurchaseOrder.Branch.OrganizationId == orgId);

        if (purchaseOrderId.HasValue)
            query = query.Where(p => p.PurchaseOrderId == purchaseOrderId);

        var list = await query
            .OrderByDescending(p => p.PaymentDate)
            .Select(p => new { p.Id, p.PurchaseOrderId, p.Amount, p.PaymentDate, p.PaymentMode, p.Reference })
            .ToListAsync();

        return Ok(list);
    }

    [HttpPost]
    public async Task<ActionResult<Payment>> CreatePayment([FromBody] CreatePaymentRequest request)
    {
        var orgId = GetOrganizationId();
        if (orgId == null) return Unauthorized();

        var po = await _context.PurchaseOrders
            .FirstOrDefaultAsync(p => p.Id == request.PurchaseOrderId && p.Branch.OrganizationId == orgId);
        if (po == null) return NotFound();

        var payment = new Payment
        {
            Id = Guid.NewGuid(),
            PurchaseOrderId = request.PurchaseOrderId,
            Amount = request.Amount,
            PaymentDate = request.PaymentDate ?? DateTime.UtcNow,
            PaymentMode = request.PaymentMode,
            Reference = request.Reference
        };

        _context.Payments.Add(payment);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetPayments), new { purchaseOrderId = payment.PurchaseOrderId }, payment);
    }

    private Guid? GetOrganizationId()
    {
        var claim = User.FindFirst("organizationId")?.Value;
        return Guid.TryParse(claim, out var id) ? id : null;
    }
}

public class CreatePaymentRequest
{
    public Guid PurchaseOrderId { get; set; }
    public decimal Amount { get; set; }
    public DateTime? PaymentDate { get; set; }
    public PaymentMode PaymentMode { get; set; }
    public string? Reference { get; set; }
}
