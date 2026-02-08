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
public class UsersController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public UsersController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<object>>> GetUsers([FromQuery] Guid? branchId)
    {
        var orgId = GetOrganizationId();
        if (orgId == null) return Unauthorized();

        var query = _context.Users
            .Include(u => u.Branch)
            .Where(u => u.OrganizationId == orgId);

        if (branchId.HasValue)
            query = query.Where(u => u.BranchId == branchId);

        var users = await query
            .Select(u => new { u.Id, u.Email, u.FullName, u.Role, u.IsActive, u.BranchId, BranchName = u.Branch != null ? u.Branch.Name : null })
            .ToListAsync();

        return Ok(users);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<object>> GetUser(Guid id)
    {
        var orgId = GetOrganizationId();
        if (orgId == null) return Unauthorized();

        var user = await _context.Users
            .Include(u => u.Branch)
            .Where(u => u.OrganizationId == orgId && u.Id == id)
            .Select(u => new { u.Id, u.Email, u.FullName, u.Role, u.IsActive, u.BranchId, BranchName = u.Branch != null ? u.Branch.Name : null })
            .FirstOrDefaultAsync();

        if (user == null) return NotFound();
        return Ok(user);
    }

    [HttpPost]
    public async Task<ActionResult<User>> CreateUser([FromBody] CreateUserRequest request)
    {
        var orgId = GetOrganizationId();
        if (orgId == null) return Unauthorized();

        if (await _context.Users.AnyAsync(u => u.Email == request.Email && u.OrganizationId == orgId))
            return BadRequest(new { message = "Email already exists." });

        var user = new User
        {
            Id = Guid.NewGuid(),
            OrganizationId = orgId.Value,
            BranchId = request.BranchId,
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            FullName = request.FullName,
            Role = request.Role,
            IsActive = true
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetUser), new { id = user.Id }, new { user.Id, user.Email, user.FullName, user.Role });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateUser(Guid id, [FromBody] UpdateUserRequest request)
    {
        var orgId = GetOrganizationId();
        if (orgId == null) return Unauthorized();

        var user = await _context.Users.FirstOrDefaultAsync(u => u.OrganizationId == orgId && u.Id == id);
        if (user == null) return NotFound();

        user.FullName = request.FullName ?? user.FullName;
        user.BranchId = request.BranchId ?? user.BranchId;
        user.Role = request.Role ?? user.Role;
        user.IsActive = request.IsActive ?? user.IsActive;

        if (!string.IsNullOrEmpty(request.Password))
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

        await _context.SaveChangesAsync();
        return NoContent();
    }

    private Guid? GetOrganizationId()
    {
        var claim = User.FindFirst("organizationId")?.Value;
        return Guid.TryParse(claim, out var id) ? id : null;
    }
}

public class CreateUserRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public Guid? BranchId { get; set; }
}

public class UpdateUserRequest
{
    public string? FullName { get; set; }
    public string? Password { get; set; }
    public UserRole? Role { get; set; }
    public Guid? BranchId { get; set; }
    public bool? IsActive { get; set; }
}
