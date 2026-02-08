namespace PharmacyManagementSystem.Core.Entities;

public class TransferRequestLine
{
    public Guid Id { get; set; }
    public Guid TransferRequestId { get; set; }
    public Guid ProductId { get; set; }
    public Guid? StockBatchId { get; set; }
    public int Quantity { get; set; }

    public TransferRequest TransferRequest { get; set; } = null!;
    public Product Product { get; set; } = null!;
}
