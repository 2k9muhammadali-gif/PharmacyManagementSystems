using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using PharmacyManagementSystem.Infrastructure.Data;

namespace PharmacyManagementSystem.Api.Middleware;

/// <summary>
/// Validates that the organization has an active license for protected endpoints.
/// Skip for: health, auth/login, license/activate.
/// </summary>
public class LicenseValidationMiddleware
{
    private readonly RequestDelegate _next;
    private static readonly string[] SkipPaths = { "/api/health", "/api/auth/login", "/api/auth/login/", "/api/license/activate", "/api/license/status", "/swagger", "/api/" };

    public LicenseValidationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ApplicationDbContext db)
    {
        if (ShouldSkip(context.Request.Path))
        {
            await _next(context);
            return;
        }

        var organizationIdClaim = context.User.FindFirst("organizationId")?.Value;
        if (string.IsNullOrEmpty(organizationIdClaim))
        {
            await _next(context);
            return;
        }

        if (!Guid.TryParse(organizationIdClaim, out var orgId))
        {
            await _next(context);
            return;
        }

        var hasValidLicense = await db.Licenses.AnyAsync(l =>
            l.OrganizationId == orgId
            && l.IsActive
            && l.ActivatedAt != null
            && l.StartDate <= DateTime.UtcNow
            && l.EndDate >= DateTime.UtcNow);

        if (!hasValidLicense)
        {
            context.Response.StatusCode = 403;
            await context.Response.WriteAsJsonAsync(new { message = "License required or expired. Please activate your license." });
            return;
        }

        await _next(context);
    }

    private static bool ShouldSkip(PathString path)
    {
        var pathStr = path.Value?.ToLowerInvariant() ?? "";
        if (pathStr.StartsWith("/swagger") || pathStr == "/" || pathStr == "")
            return true;
        if (pathStr.StartsWith("/api/health") || pathStr.StartsWith("/api/auth/login"))
            return true;
        if (pathStr.StartsWith("/api/license/activate") || pathStr.StartsWith("/api/license/status"))
            return true;
        return false;
    }
}
