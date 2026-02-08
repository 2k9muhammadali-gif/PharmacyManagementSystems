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
public class PurchaseOrdersController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public PurchaseOrdersController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<object>>> GetPurchaseOrders([FromQuery] Guid? branchId)
    {
        var orgId = GetOrganizationId();
        if (orgId == null) return Unauthorized();

        var query = _context.PurchaseOrders
            .Include(p => p.Distribution)
            .Include(p => p.Branch)
            .Where(p => p.Branch.OrganizationId == orgId);

        if (branchId.HasValue) query = query.Where(p => p.BranchId == branchId);

        var list = await query
            .OrderByDescending(p => p.OrderDate)
            .Select(p => new { p.Id, p.BranchId, p.DistributionId, DistributionName = p.Distribution.Name, p.OrderDate, p.Status, p.TotalAmount })
            .ToListAsync();

        return Ok(list);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<object>> GetPurchaseOrder(Guid id)
    {
        var orgId = GetOrganizationId();
        if (orgId == null) return Unauthorized();

        var po = await _context.PurchaseOrders
            .Include(p => p.Distribution)
            .Include(p => p.Lines).ThenInclude(l => l.Product).ThenInclude(pr => pr!.ProductForm)
            .Include(p => p.Lines).ThenInclude(l => l.Manufacturer)
            .Where(p => p.Branch.OrganizationId == orgId && p.Id == id)
            .Select(p => new
            {
                p.Id,
                p.BranchId,
                p.DistributionId,
                DistributionName = p.Distribution.Name,
                p.OrderDate,
                p.Status,
                p.TotalAmount,
                Lines = p.Lines.Select(l => new { l.ProductId, ProductName = l.Product.Name, Formulation = l.Product.ProductForm != null ? l.Product.ProductForm.Name : null, l.ManufacturerId, ManufacturerName = l.Manufacturer.Name, l.Quantity, l.UnitPrice })
            })
            .FirstOrDefaultAsync();

        if (po == null) return NotFound();
        return Ok(po);
    }

    [HttpPost]
    public async Task<ActionResult<object>> CreatePurchaseOrder([FromBody] CreatePurchaseOrderRequest request)
    {
        var orgId = GetOrganizationId();
        if (orgId == null) return Unauthorized();

        var branch = await _context.Branches.FirstOrDefaultAsync(b => b.Id == request.BranchId && b.OrganizationId == orgId);
        if (branch == null) return BadRequest(new { message = "Invalid branch." });

        var dist = await _context.Distributions.FindAsync(request.DistributionId);
        if (dist == null) return BadRequest(new { message = "Invalid distribution." });

        var po = new PurchaseOrder
        {
            Id = Guid.NewGuid(),
            BranchId = request.BranchId,
            DistributionId = request.DistributionId,
            OrderDate = DateTime.UtcNow,
            Status = PurchaseOrderStatus.Draft,
            TotalAmount = 0
        };

        decimal total = 0;
        var distCompanies = await _context.DistributionCompanies
            .Where(dc => dc.DistributionId == request.DistributionId)
            .Select(dc => dc.ManufacturerId)
            .ToListAsync();

        foreach (var line in request.Lines)
        {
            var product = await _context.Products.FindAsync(line.ProductId);
            if (product == null) return BadRequest(new { message = $"Product {line.ProductId} not found." });

            var manufacturerId = line.ManufacturerId ?? product.ManufacturerId;
            if (!distCompanies.Contains(manufacturerId))
                return BadRequest(new { message = $"Manufacturer {manufacturerId} is not supplied by this distribution. Add the company to the distribution first." });

            var lineAmount = line.Quantity * line.UnitPrice;
            total += lineAmount;

            po.Lines.Add(new PurchaseOrderLine
            {
                Id = Guid.NewGuid(),
                PurchaseOrderId = po.Id,
                ProductId = line.ProductId,
                ManufacturerId = manufacturerId,
                Quantity = line.Quantity,
                UnitPrice = line.UnitPrice
            });
        }

        po.TotalAmount = total;
        po.Status = PurchaseOrderStatus.Submitted;

        _context.PurchaseOrders.Add(po);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetPurchaseOrder), new { id = po.Id }, new { po.Id, po.TotalAmount });
    }

    [HttpPost("{id}/receive")]
    public async Task<IActionResult> Receive(Guid id)
    {
        var orgId = GetOrganizationId();
        if (orgId == null) return Unauthorized();

        var po = await _context.PurchaseOrders
            .Include(p => p.Lines)
            .FirstOrDefaultAsync(p => p.Id == id && p.Branch.OrganizationId == orgId);

        if (po == null) return NotFound();

        foreach (var line in po.Lines)
        {
            var batch = new StockBatch
            {
                Id = Guid.NewGuid(),
                BranchId = po.BranchId,
                ProductId = line.ProductId,
                DistributionId = po.DistributionId,
                ManufacturerId = line.ManufacturerId,
                BatchNumber = $"PO-{po.Id:N}-{line.ProductId:N}".Substring(0, 50),
                Quantity = line.Quantity,
                ExpiryDate = DateTime.UtcNow.AddYears(2),
                PurchasePrice = line.UnitPrice
            };
            _context.StockBatches.Add(batch);
        }

        po.Status = PurchaseOrderStatus.Received;
        await _context.SaveChangesAsync();

        return Ok(new { message = "Goods received." });
    }

    private Guid? GetOrganizationId()
    {
        var claim = User.FindFirst("organizationId")?.Value;
        return Guid.TryParse(claim, out var id) ? id : null;
    }
}

public class CreatePurchaseOrderRequest
{
    public Guid BranchId { get; set; }
    public Guid DistributionId { get; set; }
    public List<PurchaseOrderLineRequest> Lines { get; set; } = new();
}

public class PurchaseOrderLineRequest
{
    public Guid ProductId { get; set; }
    public Guid? ManufacturerId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}
