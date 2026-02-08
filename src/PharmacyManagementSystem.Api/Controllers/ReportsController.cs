using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PharmacyManagementSystem.Infrastructure.Data;

namespace PharmacyManagementSystem.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ReportsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public ReportsController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet("sales")]
    public async Task<ActionResult<object>> GetSalesReport([FromQuery] Guid? branchId, [FromQuery] DateTime from, [FromQuery] DateTime to)
    {
        var orgId = GetOrganizationId();
        if (orgId == null) return Unauthorized();

        var query = _context.Sales.Where(s => s.Branch.OrganizationId == orgId && s.SaleDate >= from && s.SaleDate <= to.AddDays(1));
        if (branchId.HasValue) query = query.Where(s => s.BranchId == branchId);

        var total = await query.SumAsync(s => s.TotalAmount);
        var count = await query.CountAsync();

        var byBranch = await query
            .GroupBy(s => s.BranchId)
            .Select(g => new { BranchId = g.Key, Total = g.Sum(s => s.TotalAmount), Count = g.Count() })
            .ToListAsync();

        return Ok(new { total, count, byBranch });
    }

    [HttpGet("top-products")]
    public async Task<ActionResult<IEnumerable<object>>> GetTopProducts([FromQuery] Guid? branchId, [FromQuery] DateTime? from, [FromQuery] DateTime? to, [FromQuery] int top = 10)
    {
        var orgId = GetOrganizationId();
        if (orgId == null) return Unauthorized();

        var query = _context.SaleLines
            .Include(l => l.Sale)
            .Include(l => l.Product)
            .Where(l => l.Sale.Branch.OrganizationId == orgId);

        if (branchId.HasValue) query = query.Where(l => l.Sale.BranchId == branchId);
        if (from.HasValue) query = query.Where(l => l.Sale.SaleDate >= from);
        if (to.HasValue) query = query.Where(l => l.Sale.SaleDate <= to.Value.AddDays(1));

        var topProducts = await query
            .GroupBy(l => new { l.ProductId, l.Product.Name })
            .Select(g => new { ProductId = g.Key.ProductId, ProductName = g.Key.Name, Quantity = g.Sum(l => l.Quantity), Revenue = g.Sum(l => l.Quantity * l.UnitPrice) })
            .OrderByDescending(x => x.Quantity)
            .Take(top)
            .ToListAsync();

        return Ok(topProducts);
    }

    [HttpGet("expiry")]
    public async Task<ActionResult<IEnumerable<object>>> GetExpiryReport([FromQuery] Guid branchId, [FromQuery] int days = 30)
    {
        var orgId = GetOrganizationId();
        if (orgId == null) return Unauthorized();

        var expiryDate = DateTime.UtcNow.AddDays(days);
        var items = await _context.StockBatches
            .Include(s => s.Product)
            .Where(s => s.BranchId == branchId && s.Branch.OrganizationId == orgId && s.Quantity > 0 && s.ExpiryDate <= expiryDate)
            .Select(s => new { s.ProductId, ProductName = s.Product.Name, s.BatchNumber, s.Quantity, s.ExpiryDate })
            .OrderBy(s => s.ExpiryDate)
            .ToListAsync();

        return Ok(items);
    }

    [HttpGet("stock-valuation")]
    public async Task<ActionResult<object>> GetStockValuation([FromQuery] Guid? branchId)
    {
        var orgId = GetOrganizationId();
        if (orgId == null) return Unauthorized();

        var query = _context.StockBatches
            .Where(s => s.Branch.OrganizationId == orgId && s.Quantity > 0);

        if (branchId.HasValue) query = query.Where(s => s.BranchId == branchId);

        var total = await query.SumAsync(s => s.Quantity * s.PurchasePrice);
        var byBranch = await query
            .GroupBy(s => s.BranchId)
            .Select(g => new { BranchId = g.Key, Value = g.Sum(s => s.Quantity * s.PurchasePrice) })
            .ToListAsync();

        return Ok(new { total, byBranch });
    }

    [HttpGet("profit-loss")]
    public async Task<ActionResult<object>> GetProfitLoss([FromQuery] Guid? branchId, [FromQuery] DateTime from, [FromQuery] DateTime to)
    {
        var orgId = GetOrganizationId();
        if (orgId == null) return Unauthorized();

        var salesQuery = _context.Sales.Where(s => s.Branch.OrganizationId == orgId && s.SaleDate >= from && s.SaleDate <= to.AddDays(1));
        if (branchId.HasValue) salesQuery = salesQuery.Where(s => s.BranchId == branchId);

        var revenue = await salesQuery.SumAsync(s => s.TotalAmount);

        var saleIds = await salesQuery.Select(s => s.Id).ToListAsync();
        var cogs = await _context.SaleLines
            .Where(l => saleIds.Contains(l.SaleId) && l.StockBatchId != null)
            .Join(_context.StockBatches, l => l.StockBatchId, b => b.Id, (l, b) => new { l.Quantity, b.PurchasePrice })
            .SumAsync(x => x.Quantity * x.PurchasePrice);

        return Ok(new { revenue, costOfGoodsSold = cogs, profit = revenue - cogs });
    }

    [HttpGet("tax")]
    public async Task<ActionResult<object>> GetTaxReport([FromQuery] Guid? branchId, [FromQuery] DateTime from, [FromQuery] DateTime to)
    {
        var orgId = GetOrganizationId();
        if (orgId == null) return Unauthorized();

        var query = _context.Sales.Where(s => s.Branch.OrganizationId == orgId && s.SaleDate >= from && s.SaleDate <= to.AddDays(1));
        if (branchId.HasValue) query = query.Where(s => s.BranchId == branchId);

        var totalSales = await query.SumAsync(s => s.TotalAmount);
        var count = await query.CountAsync();

        return Ok(new { totalSales, invoiceCount = count, period = new { from, to } });
    }

    private Guid? GetOrganizationId()
    {
        var claim = User.FindFirst("organizationId")?.Value;
        return Guid.TryParse(claim, out var id) ? id : null;
    }
}
