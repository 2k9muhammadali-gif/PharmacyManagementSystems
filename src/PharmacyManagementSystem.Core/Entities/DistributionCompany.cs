namespace PharmacyManagementSystem.Core.Entities;

/// <summary>
/// Company (manufacturer) under a distribution. A distribution supplies medicines from multiple companies.
/// </summary>
public class DistributionCompany
{
    public Guid Id { get; set; }
    public Guid DistributionId { get; set; }
    public Guid ManufacturerId { get; set; }

    public Distribution Distribution { get; set; } = null!;
    public Manufacturer Manufacturer { get; set; } = null!;
}
