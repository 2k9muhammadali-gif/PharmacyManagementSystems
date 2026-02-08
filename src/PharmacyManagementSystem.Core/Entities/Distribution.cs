namespace PharmacyManagementSystem.Core.Entities;

/// <summary>
/// Distribution (supplier/wholesaler) entity. Supplies medicines from multiple companies.
/// </summary>
public class Distribution
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Contact { get; set; }
    public string? Address { get; set; }
    public string? Phone { get; set; }

    public ICollection<DistributionCompany> Companies { get; set; } = new List<DistributionCompany>();
}
