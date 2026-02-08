using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PharmacyManagementSystem.Core.Enums;
using PharmacyManagementSystem.Core.Entities;
using PharmacyManagementSystem.Infrastructure.Data;

namespace PharmacyManagementSystem.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class InventoryController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public InventoryController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<object>>> GetInventory([FromQuery] Guid branchId, [FromQuery] Guid? productId)
    {
        var orgId = GetOrganizationId();
        if (orgId == null) return Unauthorized();

        var query = _context.StockBatches
            .Include(s => s.Product).ThenInclude(p => p.ProductForm)
            .Include(s => s.Distribution)
            .Include(s => s.Manufacturer)
            .Where(s => s.BranchId == branchId && s.Branch.OrganizationId == orgId && s.Quantity > 0);

        if (productId.HasValue)
            query = query.Where(s => s.ProductId == productId);

        var items = await query
            .Select(s => new
            {
                s.Id,
                s.BranchId,
                s.ProductId,
                ProductName = s.Product.Name,
                Formulation = s.Product.ProductForm != null ? s.Product.ProductForm.Name : null,
                s.DistributionId,
                DistributionName = s.Distribution != null ? s.Distribution.Name : null,
                s.ManufacturerId,
                ManufacturerName = s.Manufacturer != null ? s.Manufacturer.Name : null,
                s.BatchNumber,
                s.Quantity,
                s.ExpiryDate,
                s.PurchasePrice
            })
            .ToListAsync();

        return Ok(items);
    }

    [HttpGet("alerts")]
    public async Task<ActionResult<object>> GetAlerts([FromQuery] Guid branchId, [FromQuery] int expiryDays = 30)
    {
        var orgId = GetOrganizationId();
        if (orgId == null) return Unauthorized();

        var expiryDate = DateTime.UtcNow.AddDays(expiryDays);
        var expiringSoon = await _context.StockBatches
            .Include(s => s.Product)
            .Where(s => s.BranchId == branchId && s.Branch.OrganizationId == orgId && s.Quantity > 0 && s.ExpiryDate <= expiryDate)
            .Select(s => new { s.ProductId, ProductName = s.Product.Name, s.BatchNumber, s.Quantity, s.ExpiryDate })
            .ToListAsync();

        var lowStock = await _context.StockBatches
            .Where(s => s.BranchId == branchId && s.Branch.OrganizationId == orgId && s.Quantity > 0)
            .GroupBy(s => s.ProductId)
            .Select(g => new { ProductId = g.Key, TotalQty = g.Sum(x => x.Quantity) })
            .ToListAsync();

        var productIds = lowStock.Select(x => x.ProductId).ToList();
        var products = await _context.Products
            .Where(p => productIds.Contains(p.Id) && p.ReorderPoint > 0)
            .Select(p => new { p.Id, p.Name, p.ReorderPoint })
            .ToListAsync();

        var lowStockAlerts = lowStock
            .Join(products, l => l.ProductId, p => p.Id, (l, p) => new { l.ProductId, p.Name, l.TotalQty, p.ReorderPoint })
            .Where(x => x.TotalQty <= x.ReorderPoint)
            .ToList();

        return Ok(new { expiringSoon, lowStockAlerts });
    }

    [HttpPost("adjust")]
    public async Task<IActionResult> Adjust([FromBody] StockAdjustmentRequest request)
    {
        var userId = GetUserId();
        var orgId = GetOrganizationId();
        if (userId == null || orgId == null) return Unauthorized();

        var batch = await _context.StockBatches
            .Include(s => s.Branch)
            .FirstOrDefaultAsync(s => s.Id == request.StockBatchId && s.Branch.OrganizationId == orgId);

        if (batch == null) return NotFound();

        var adjustment = new StockAdjustment
        {
            Id = Guid.NewGuid(),
            BranchId = batch.BranchId,
            StockBatchId = batch.Id,
            Quantity = request.Quantity,
            AdjustmentType = request.AdjustmentType,
            Reason = request.Reason,
            AdjustedBy = userId.Value
        };

        batch.Quantity += request.Quantity;
        if (batch.Quantity < 0) return BadRequest(new { message = "Insufficient quantity." });

        _context.StockAdjustments.Add(adjustment);
        await _context.SaveChangesAsync();
        return Ok(new { message = "Adjustment recorded." });
    }

    [HttpGet("stock-batches")]
    public async Task<ActionResult<IEnumerable<object>>> GetStockBatches([FromQuery] Guid branchId)
    {
        var orgId = GetOrganizationId();
        if (orgId == null) return Unauthorized();

        var batches = await _context.StockBatches
            .Include(s => s.Product)
            .Where(s => s.BranchId == branchId && s.Branch.OrganizationId == orgId)
            .Select(s => new { s.Id, s.ProductId, ProductName = s.Product.Name, s.BatchNumber, s.Quantity, s.ExpiryDate, s.PurchasePrice })
            .ToListAsync();

        return Ok(batches);
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
}

public class StockAdjustmentRequest
{
    public Guid StockBatchId { get; set; }
    public int Quantity { get; set; }
    public AdjustmentType AdjustmentType { get; set; }
    public string? Reason { get; set; }
}
