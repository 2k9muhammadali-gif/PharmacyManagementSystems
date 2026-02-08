namespace PharmacyManagementSystem.Core.Enums;

/// <summary>
/// Purchase order status.
/// </summary>
public enum PurchaseOrderStatus
{
    Draft = 0,
    Submitted = 1,
    Received = 2,
    PartiallyReceived = 3,
    Cancelled = 4
}
