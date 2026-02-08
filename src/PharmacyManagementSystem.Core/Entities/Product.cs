using PharmacyManagementSystem.Core.Enums;

namespace PharmacyManagementSystem.Core.Entities;

/// <summary>
/// Product (drug) entity.
/// </summary>
public class Product
{
    public Guid Id { get; set; }
    public Guid ManufacturerId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? GenericName { get; set; }
    public string? Strength { get; set; }
    public Guid? ProductFormId { get; set; }
    public Schedule Schedule { get; set; }
    public string? Barcode { get; set; }
    public string? DRAPNumber { get; set; }
    public string? TherapeuticCategory { get; set; }
    public string? Contraindications { get; set; }
    public string? DrugInteractions { get; set; }
    public int ReorderPoint { get; set; }
    public decimal SalePrice { get; set; }
    public bool IsActive { get; set; } = true;

    public Manufacturer Manufacturer { get; set; } = null!;
    public ProductForm? ProductForm { get; set; }
}
