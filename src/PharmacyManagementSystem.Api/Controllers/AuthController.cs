using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PharmacyManagementSystem.Api.DTOs.Auth;
using PharmacyManagementSystem.Core.Entities;
using PharmacyManagementSystem.Infrastructure.Data;

namespace PharmacyManagementSystem.Api.Controllers;

/// <summary>
/// Authentication endpoints.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _configuration;

    public AuthController(ApplicationDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    /// <summary>
    /// Login with email and password. Returns JWT token.
    /// </summary>
    /// <param name="request">Login credentials</param>
    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
    {
        var user = await _context.Users
            .Include(u => u.Organization)
            .FirstOrDefaultAsync(u => u.Email == request.Email && u.IsActive);

        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            return Unauthorized(new { message = "Invalid email or password" });

        // Check license
        var hasValidLicense = await _context.Licenses
            .AnyAsync(l => l.OrganizationId == user.OrganizationId
                && l.IsActive
                && l.ActivatedAt != null
                && l.StartDate <= DateTime.UtcNow
                && l.EndDate >= DateTime.UtcNow);

        if (!hasValidLicense)
            return StatusCode(403, new { message = "License required or expired. Please activate your license." });

        var token = GenerateJwtToken(user);
        var expiresAt = DateTime.UtcNow.AddMinutes(GetJwtExpiryMinutes());

        return Ok(new LoginResponse
        {
            Token = token,
            Email = user.Email,
            FullName = user.FullName,
            Role = user.Role.ToString(),
            BranchId = user.BranchId,
            OrganizationId = user.OrganizationId,
            ExpiresAt = expiresAt
        });
    }

    private string GenerateJwtToken(User user)
    {
        var key = _configuration["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key not configured");
        var issuer = _configuration["Jwt:Issuer"] ?? "PharmacyManagementSystem";
        var audience = _configuration["Jwt:Audience"] ?? "PharmacyManagementSystem";

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Name, user.FullName),
            new("role", user.Role.ToString()),
            new("organizationId", user.OrganizationId.ToString())
        };

        if (user.BranchId.HasValue)
            claims.Add(new Claim("branchId", user.BranchId.Value.ToString()));

        var keyBytes = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        var creds = new SigningCredentials(keyBytes, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(GetJwtExpiryMinutes()),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private int GetJwtExpiryMinutes()
    {
        var expiry = _configuration["Jwt:ExpiryMinutes"];
        return int.TryParse(expiry, out var minutes) ? minutes : 60;
    }
}
