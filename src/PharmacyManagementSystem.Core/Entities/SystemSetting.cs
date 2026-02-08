namespace PharmacyManagementSystem.Core.Entities;

/// <summary>
/// Generic system-level configuration for future settings.
/// </summary>
public class SystemSetting
{
    public Guid Id { get; set; }
    public string Category { get; set; } = string.Empty;  // e.g. "General", "Pharmacy"
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string? Description { get; set; }
}
