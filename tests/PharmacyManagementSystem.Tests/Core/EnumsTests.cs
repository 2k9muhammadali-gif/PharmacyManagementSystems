using PharmacyManagementSystem.Core.Enums;
using Xunit;

namespace PharmacyManagementSystem.Tests.Core;

/// <summary>
/// Unit tests for Core enums.
/// </summary>
public class EnumsTests
{
    [Fact]
    public void LicenseType_HasExpectedValues()
    {
        Assert.Equal(0, (int)LicenseType.Trial);
        Assert.Equal(1, (int)LicenseType.SingleBranch);
        Assert.Equal(2, (int)LicenseType.MultiBranch);
        Assert.Equal(3, (int)LicenseType.Enterprise);
    }

    [Fact]
    public void UserRole_HasExpectedValues()
    {
        Assert.Equal(0, (int)UserRole.SuperAdmin);
        Assert.Equal(1, (int)UserRole.OrganizationOwner);
        Assert.Equal(2, (int)UserRole.BranchManager);
        Assert.Equal(3, (int)UserRole.LicensedPharmacist);
        Assert.Equal(4, (int)UserRole.SalesStaff);
        Assert.Equal(5, (int)UserRole.Accountant);
        Assert.Equal(6, (int)UserRole.StockKeeper);
    }

    [Fact]
    public void Schedule_HasExpectedValues()
    {
        Assert.Equal(0, (int)Schedule.OTC);
        Assert.Equal(1, (int)Schedule.Prescription);
        Assert.Equal(2, (int)Schedule.ScheduleH);
    }

    [Fact]
    public void PaymentMode_HasExpectedValues()
    {
        Assert.Equal(0, (int)PaymentMode.Cash);
        Assert.Equal(1, (int)PaymentMode.Card);
        Assert.Equal(2, (int)PaymentMode.JazzCash);
        Assert.Equal(3, (int)PaymentMode.Easypaisa);
        Assert.Equal(4, (int)PaymentMode.BankTransfer);
    }
}
