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
public class TransfersController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public TransfersController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<object>>> GetTransfers([FromQuery] Guid? branchId)
    {
        var orgId = GetOrganizationId();
        if (orgId == null) return Unauthorized();

        var query = _context.TransferRequests
            .Include(t => t.FromBranch)
            .Include(t => t.ToBranch)
            .Where(t => t.FromBranch.OrganizationId == orgId || t.ToBranch.OrganizationId == orgId);

        if (branchId.HasValue)
            query = query.Where(t => t.FromBranchId == branchId || t.ToBranchId == branchId);

        var list = await query
            .OrderByDescending(t => t.RequestedAt)
            .Select(t => new { t.Id, t.FromBranchId, t.ToBranchId, FromBranchName = t.FromBranch.Name, ToBranchName = t.ToBranch.Name, t.Status, t.RequestedAt })
            .ToListAsync();

        return Ok(list);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<object>> GetTransfer(Guid id)
    {
        var orgId = GetOrganizationId();
        if (orgId == null) return Unauthorized();

        var transfer = await _context.TransferRequests
            .Include(t => t.FromBranch)
            .Include(t => t.ToBranch)
            .Include(t => t.Lines).ThenInclude(l => l.Product)
            .Where(t => (t.FromBranch.OrganizationId == orgId || t.ToBranch.OrganizationId == orgId) && t.Id == id)
            .Select(t => new
            {
                t.Id,
                t.FromBranchId,
                t.ToBranchId,
                FromBranchName = t.FromBranch.Name,
                ToBranchName = t.ToBranch.Name,
                t.Status,
                t.RequestedAt,
                t.ApprovedAt,
                Lines = t.Lines.Select(l => new { l.ProductId, ProductName = l.Product.Name, l.Quantity })
            })
            .FirstOrDefaultAsync();

        if (transfer == null) return NotFound();
        return Ok(transfer);
    }

    [HttpPost]
    public async Task<ActionResult<object>> CreateTransfer([FromBody] CreateTransferRequest request)
    {
        var userId = GetUserId();
        var orgId = GetOrganizationId();
        if (userId == null || orgId == null) return Unauthorized();

        var fromBranch = await _context.Branches.FirstOrDefaultAsync(b => b.Id == request.FromBranchId && b.OrganizationId == orgId);
        var toBranch = await _context.Branches.FirstOrDefaultAsync(b => b.Id == request.ToBranchId && b.OrganizationId == orgId);
        if (fromBranch == null || toBranch == null) return BadRequest(new { message = "Invalid branch." });

        var transfer = new TransferRequest
        {
            Id = Guid.NewGuid(),
            FromBranchId = request.FromBranchId,
            ToBranchId = request.ToBranchId,
            Status = TransferRequestStatus.Pending,
            RequestedBy = userId.Value
        };

        foreach (var line in request.Lines)
        {
            var product = await _context.Products.FindAsync(line.ProductId);
            if (product == null) return BadRequest(new { message = $"Product {line.ProductId} not found." });

            var batch = await _context.StockBatches
                .Where(s => s.BranchId == request.FromBranchId && s.ProductId == line.ProductId && s.Quantity >= line.Quantity)
                .OrderBy(s => s.ExpiryDate)
                .FirstOrDefaultAsync();

            if (batch == null) return BadRequest(new { message = $"Insufficient stock for {product.Name} in source branch." });

            transfer.Lines.Add(new TransferRequestLine
            {
                Id = Guid.NewGuid(),
                TransferRequestId = transfer.Id,
                ProductId = line.ProductId,
                StockBatchId = batch.Id,
                Quantity = line.Quantity
            });
        }

        _context.TransferRequests.Add(transfer);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetTransfer), new { id = transfer.Id }, new { transfer.Id });
    }

    [HttpPost("{id}/approve")]
    public async Task<IActionResult> Approve(Guid id)
    {
        var userId = GetUserId();
        var orgId = GetOrganizationId();
        if (userId == null || orgId == null) return Unauthorized();

        var transfer = await _context.TransferRequests
            .Include(t => t.ToBranch)
            .Include(t => t.Lines)
            .FirstOrDefaultAsync(t => t.Id == id && t.ToBranch.OrganizationId == orgId);

        if (transfer == null) return NotFound();
        if (transfer.Status != TransferRequestStatus.Pending) return BadRequest(new { message = "Transfer already processed." });

        foreach (var line in transfer.Lines)
        {
            var sourceBatch = await _context.StockBatches.FindAsync(line.StockBatchId);
            if (sourceBatch != null)
            {
                sourceBatch.Quantity -= line.Quantity;

                var destBatch = new StockBatch
                {
                    Id = Guid.NewGuid(),
                    BranchId = transfer.ToBranchId,
                    ProductId = line.ProductId,
                    BatchNumber = sourceBatch.BatchNumber,
                    Quantity = line.Quantity,
                    ExpiryDate = sourceBatch.ExpiryDate,
                    PurchasePrice = sourceBatch.PurchasePrice
                };
                _context.StockBatches.Add(destBatch);
            }
        }

        transfer.Status = TransferRequestStatus.Completed;
        transfer.ApprovedBy = userId.Value;
        transfer.ApprovedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return Ok(new { message = "Transfer approved." });
    }

    [HttpPost("{id}/reject")]
    public async Task<IActionResult> Reject(Guid id)
    {
        var orgId = GetOrganizationId();
        if (orgId == null) return Unauthorized();

        var transfer = await _context.TransferRequests
            .FirstOrDefaultAsync(t => t.Id == id && (t.FromBranch.OrganizationId == orgId || t.ToBranch.OrganizationId == orgId));

        if (transfer == null) return NotFound();
        if (transfer.Status != TransferRequestStatus.Pending) return BadRequest(new { message = "Transfer already processed." });

        transfer.Status = TransferRequestStatus.Rejected;
        await _context.SaveChangesAsync();

        return Ok(new { message = "Transfer rejected." });
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

public class CreateTransferRequest
{
    public Guid FromBranchId { get; set; }
    public Guid ToBranchId { get; set; }
    public List<TransferLineRequest> Lines { get; set; } = new();
}

public class TransferLineRequest
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
}
