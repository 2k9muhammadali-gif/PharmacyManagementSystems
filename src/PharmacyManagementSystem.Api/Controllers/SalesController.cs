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
public class SalesController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public SalesController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<object>>> GetSales([FromQuery] Guid? branchId, [FromQuery] DateTime? from, [FromQuery] DateTime? to)
    {
        var orgId = GetOrganizationId();
        if (orgId == null) return Unauthorized();

        var query = _context.Sales.Include(s => s.Branch).Where(s => s.Branch.OrganizationId == orgId);

        if (branchId.HasValue) query = query.Where(s => s.BranchId == branchId);
        if (from.HasValue) query = query.Where(s => s.SaleDate >= from);
        if (to.HasValue) query = query.Where(s => s.SaleDate <= to.Value.AddDays(1));

        var sales = await query
            .OrderByDescending(s => s.SaleDate)
            .Select(s => new { s.Id, s.BranchId, s.SaleDate, s.TotalAmount, s.PaymentMode })
            .Take(100)
            .ToListAsync();

        return Ok(sales);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<object>> GetSale(Guid id)
    {
        var orgId = GetOrganizationId();
        if (orgId == null) return Unauthorized();

        var sale = await _context.Sales
            .Include(s => s.Lines).ThenInclude(l => l.Product)
            .Include(s => s.Customer)
            .Where(s => s.Branch.OrganizationId == orgId && s.Id == id)
            .Select(s => new
            {
                s.Id,
                s.BranchId,
                s.CustomerId,
                s.SaleDate,
                s.TotalAmount,
                s.DiscountAmount,
                s.PaymentMode,
                s.FBRFiscalNumber,
                s.FBRQRCode,
                CustomerName = s.Customer != null ? s.Customer.Name : null,
                Lines = s.Lines.Select(l => new { l.ProductId, ProductName = l.Product.Name, l.Quantity, l.UnitPrice, l.IsPrescription })
            })
            .FirstOrDefaultAsync();

        if (sale == null) return NotFound();
        return Ok(sale);
    }

    [HttpPost]
    public async Task<ActionResult<object>> CreateSale([FromBody] CreateSaleRequest request)
    {
        var userId = GetUserId();
        var branchId = GetBranchId() ?? request.BranchId;
        var orgId = GetOrganizationId();
        if (userId == null || orgId == null) return Unauthorized();

        if (!branchId.HasValue) return BadRequest(new { message = "Branch is required." });

        var branch = await _context.Branches.FirstOrDefaultAsync(b => b.Id == branchId && b.OrganizationId == orgId);
        if (branch == null) return BadRequest(new { message = "Invalid branch." });

        var sale = new Sale
        {
            Id = Guid.NewGuid(),
            BranchId = branchId.Value,
            CustomerId = request.CustomerId,
            SaleDate = DateTime.UtcNow,
            TotalAmount = 0,
            DiscountAmount = request.DiscountAmount ?? 0,
            DiscountPercent = request.DiscountPercent ?? 0,
            PaymentMode = request.PaymentMode,
            CreatedBy = userId.Value,
            FBRStatus = "Pending"
        };

        decimal total = 0;
        foreach (var line in request.Lines)
        {
            var product = await _context.Products.FindAsync(line.ProductId);
            if (product == null) return BadRequest(new { message = $"Product {line.ProductId} not found." });

            if (product.Schedule == Schedule.ScheduleH && string.IsNullOrEmpty(request.CustomerCNIC))
                return BadRequest(new { message = "Customer CNIC required for Schedule H products." });

            var batch = await _context.StockBatches
                .Where(s => s.BranchId == branchId && s.ProductId == line.ProductId && s.Quantity >= line.Quantity)
                .OrderBy(s => s.ExpiryDate)
                .FirstOrDefaultAsync();

            if (batch == null) return BadRequest(new { message = $"Insufficient stock for {product.Name}." });

            var lineTotal = line.Quantity * (line.UnitPrice > 0 ? line.UnitPrice : product.SalePrice);
            total += lineTotal;

            sale.Lines.Add(new SaleLine
            {
                Id = Guid.NewGuid(),
                SaleId = sale.Id,
                ProductId = line.ProductId,
                StockBatchId = batch.Id,
                Quantity = line.Quantity,
                UnitPrice = line.UnitPrice > 0 ? line.UnitPrice : product.SalePrice,
                IsPrescription = product.Schedule == Schedule.Prescription || product.Schedule == Schedule.ScheduleH
            });

            batch.Quantity -= line.Quantity;

            if (product.Schedule == Schedule.ScheduleH)
            {
                _context.ControlledSubstanceLogs.Add(new ControlledSubstanceLog
                {
                    Id = Guid.NewGuid(),
                    BranchId = branchId.Value,
                    ProductId = line.ProductId,
                    SaleId = sale.Id,
                    CustomerCNIC = request.CustomerCNIC ?? "",
                    Quantity = line.Quantity
                });
            }
        }

        sale.TotalAmount = total - sale.DiscountAmount - (total * sale.DiscountPercent / 100);

        if (request.PrescriptionId.HasValue)
        {
            _context.Prescriptions.Add(new Prescription
            {
                Id = Guid.NewGuid(),
                SaleId = sale.Id,
                CustomerId = request.CustomerId,
                DoctorName = request.DoctorName,
                ClinicDetails = request.ClinicDetails
            });
        }

        _context.Sales.Add(sale);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetSale), new { id = sale.Id }, new { sale.Id, sale.TotalAmount });
    }

    [HttpPost("return")]
    public async Task<IActionResult> ReturnSale([FromBody] SaleReturnRequest request)
    {
        var orgId = GetOrganizationId();
        if (orgId == null) return Unauthorized();

        var sale = await _context.Sales.Include(s => s.Lines).FirstOrDefaultAsync(s => s.Id == request.SaleId && s.Branch.OrganizationId == orgId);
        if (sale == null) return NotFound();

        var saleReturn = new SaleReturn
        {
            Id = Guid.NewGuid(),
            SaleId = sale.Id,
            ReturnDate = DateTime.UtcNow,
            Reason = request.Reason,
            TotalRefund = request.TotalRefund
        };

        _context.SaleReturns.Add(saleReturn);
        await _context.SaveChangesAsync();
        return Ok(new { message = "Return recorded." });
    }

    private Guid? GetOrganizationId()
    {
        var claim = User.FindFirst("organizationId")?.Value;
        return Guid.TryParse(claim, out var id) ? id : null;
    }

    private Guid? GetUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(claim, out var id) ? id : null;
    }

    private Guid? GetBranchId()
    {
        var claim = User.FindFirst("branchId")?.Value;
        return Guid.TryParse(claim, out var id) ? id : null;
    }
}

public class CreateSaleRequest
{
    public Guid? BranchId { get; set; }
    public Guid? CustomerId { get; set; }
    public decimal? DiscountAmount { get; set; }
    public decimal? DiscountPercent { get; set; }
    public PaymentMode PaymentMode { get; set; }
    public string? CustomerCNIC { get; set; }
    public Guid? PrescriptionId { get; set; }
    public string? DoctorName { get; set; }
    public string? ClinicDetails { get; set; }
    public List<SaleLineRequest> Lines { get; set; } = new();
}

public class SaleLineRequest
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}

public class SaleReturnRequest
{
    public Guid SaleId { get; set; }
    public string? Reason { get; set; }
    public decimal TotalRefund { get; set; }
}
