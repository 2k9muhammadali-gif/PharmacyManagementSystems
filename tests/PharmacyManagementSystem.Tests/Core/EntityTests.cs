using PharmacyManagementSystem.Core.Entities;
using PharmacyManagementSystem.Core.Enums;
using Xunit;

namespace PharmacyManagementSystem.Tests.Core;

/// <summary>
/// Unit tests for Core entities.
/// </summary>
public class EntityTests
{
    [Fact]
    public void Organization_CanBeCreated()
    {
        var org = new Organization
        {
            Id = Guid.NewGuid(),
            Name = "Test Pharmacy",
            CreatedAt = DateTime.UtcNow
        };

        Assert.NotEqual(Guid.Empty, org.Id);
        Assert.Equal("Test Pharmacy", org.Name);
        Assert.NotNull(org.Licenses);
    }

    [Fact]
    public void Product_RequiresManufacturer()
    {
        var manufacturer = new Manufacturer { Id = Guid.NewGuid(), Name = "Getz Pharma" };
        var product = new Product
        {
            Id = Guid.NewGuid(),
            ManufacturerId = manufacturer.Id,
            Name = "Panadol",
            Schedule = Schedule.OTC,
            SalePrice = 50
        };

        Assert.Equal(manufacturer.Id, product.ManufacturerId);
        Assert.Equal(Schedule.OTC, product.Schedule);
        Assert.Equal(50, product.SalePrice);
    }
}
