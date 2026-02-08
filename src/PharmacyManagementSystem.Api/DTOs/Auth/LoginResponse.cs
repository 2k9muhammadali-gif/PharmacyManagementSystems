namespace PharmacyManagementSystem.Api.DTOs.Auth;

/// <summary>
/// Login response DTO with JWT token.
/// </summary>
public class LoginResponse
{
    public string Token { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public Guid? BranchId { get; set; }
    public Guid OrganizationId { get; set; }
    public DateTime ExpiresAt { get; set; }
}
