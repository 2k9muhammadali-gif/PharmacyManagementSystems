using Microsoft.EntityFrameworkCore;
using PharmacyManagementSystem.Core.Entities;

namespace PharmacyManagementSystem.Infrastructure.Data;

/// <summary>
/// Application database context.
/// </summary>
public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Organization> Organizations => Set<Organization>();
    public DbSet<License> Licenses => Set<License>();
    public DbSet<Branch> Branches => Set<Branch>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Manufacturer> Manufacturers => Set<Manufacturer>();
    public DbSet<ProductForm> ProductForms => Set<ProductForm>();
    public DbSet<SystemSetting> SystemSettings => Set<SystemSetting>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<StockBatch> StockBatches => Set<StockBatch>();
    public DbSet<Distribution> Distributions => Set<Distribution>();
    public DbSet<DistributionCompany> DistributionCompanies => Set<DistributionCompany>();
    public DbSet<PurchaseOrder> PurchaseOrders => Set<PurchaseOrder>();
    public DbSet<PurchaseOrderLine> PurchaseOrderLines => Set<PurchaseOrderLine>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Sale> Sales => Set<Sale>();
    public DbSet<SaleLine> SaleLines => Set<SaleLine>();
    public DbSet<Prescription> Prescriptions => Set<Prescription>();
    public DbSet<ControlledSubstanceLog> ControlledSubstanceLogs => Set<ControlledSubstanceLog>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<SaleReturn> SaleReturns => Set<SaleReturn>();
    public DbSet<TransferRequest> TransferRequests => Set<TransferRequest>();
    public DbSet<TransferRequestLine> TransferRequestLines => Set<TransferRequestLine>();
    public DbSet<StockAdjustment> StockAdjustments => Set<StockAdjustment>();
    public DbSet<Payment> Payments => Set<Payment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Organization
        modelBuilder.Entity<Organization>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).HasMaxLength(200).IsRequired();
        });

        // License
        modelBuilder.Entity<License>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.LicenseKey).HasMaxLength(100).IsRequired();
            e.HasIndex(x => x.LicenseKey).IsUnique();
            e.HasOne(x => x.Organization).WithMany(o => o.Licenses).HasForeignKey(x => x.OrganizationId);
        });

        // Branch
        modelBuilder.Entity<Branch>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).HasMaxLength(200).IsRequired();
            e.HasOne(x => x.Organization).WithMany(o => o.Branches).HasForeignKey(x => x.OrganizationId);
        });

        // User
        modelBuilder.Entity<User>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Email).HasMaxLength(256).IsRequired();
            e.HasIndex(x => x.Email);
            e.HasOne(x => x.Organization).WithMany().HasForeignKey(x => x.OrganizationId);
            e.HasOne(x => x.Branch).WithMany(b => b.Users).HasForeignKey(x => x.BranchId);
        });

        // Manufacturer
        modelBuilder.Entity<Manufacturer>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).HasMaxLength(200).IsRequired();
        });

        // ProductForm
        modelBuilder.Entity<ProductForm>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).HasMaxLength(100).IsRequired();
            e.HasIndex(x => x.Name).IsUnique();
        });

        // SystemSetting
        modelBuilder.Entity<SystemSetting>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Category).HasMaxLength(50).IsRequired();
            e.Property(x => x.Key).HasMaxLength(100).IsRequired();
            e.HasIndex(x => new { x.Category, x.Key }).IsUnique();
        });

        // Product
        modelBuilder.Entity<Product>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).HasMaxLength(300).IsRequired();
            e.HasIndex(x => x.Barcode);
            e.HasOne(x => x.Manufacturer).WithMany(m => m.Products).HasForeignKey(x => x.ManufacturerId);
            e.HasOne(x => x.ProductForm).WithMany().HasForeignKey(x => x.ProductFormId).OnDelete(DeleteBehavior.Restrict);
        });

        // StockBatch
        modelBuilder.Entity<StockBatch>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasOne(x => x.Branch).WithMany().HasForeignKey(x => x.BranchId);
            e.HasOne(x => x.Product).WithMany().HasForeignKey(x => x.ProductId);
            e.HasOne(x => x.Distribution).WithMany().HasForeignKey(x => x.DistributionId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Manufacturer).WithMany().HasForeignKey(x => x.ManufacturerId).OnDelete(DeleteBehavior.Restrict);
        });

        // Distribution
        modelBuilder.Entity<Distribution>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).HasMaxLength(200).IsRequired();
        });

        // DistributionCompany
        modelBuilder.Entity<DistributionCompany>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => new { x.DistributionId, x.ManufacturerId }).IsUnique();
            e.HasOne(x => x.Distribution).WithMany(d => d.Companies).HasForeignKey(x => x.DistributionId);
            e.HasOne(x => x.Manufacturer).WithMany().HasForeignKey(x => x.ManufacturerId);
        });

        // PurchaseOrder
        modelBuilder.Entity<PurchaseOrder>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasOne(x => x.Branch).WithMany().HasForeignKey(x => x.BranchId);
            e.HasOne(x => x.Distribution).WithMany().HasForeignKey(x => x.DistributionId);
        });

        // PurchaseOrderLine
        modelBuilder.Entity<PurchaseOrderLine>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasOne(x => x.PurchaseOrder).WithMany(p => p.Lines).HasForeignKey(x => x.PurchaseOrderId);
            e.HasOne(x => x.Product).WithMany().HasForeignKey(x => x.ProductId);
            e.HasOne(x => x.Manufacturer).WithMany().HasForeignKey(x => x.ManufacturerId).OnDelete(DeleteBehavior.Restrict);
        });

        // Customer
        modelBuilder.Entity<Customer>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).HasMaxLength(200).IsRequired();
        });

        // Sale
        modelBuilder.Entity<Sale>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasOne(x => x.Branch).WithMany().HasForeignKey(x => x.BranchId);
            e.HasOne(x => x.Customer).WithMany().HasForeignKey(x => x.CustomerId);
        });

        // SaleLine
        modelBuilder.Entity<SaleLine>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasOne(x => x.Sale).WithMany(s => s.Lines).HasForeignKey(x => x.SaleId);
            e.HasOne(x => x.Product).WithMany().HasForeignKey(x => x.ProductId);
        });

        // Prescription
        modelBuilder.Entity<Prescription>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasOne(x => x.Sale).WithMany().HasForeignKey(x => x.SaleId);
        });

        // ControlledSubstanceLog
        modelBuilder.Entity<ControlledSubstanceLog>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.CustomerCNIC).HasMaxLength(20).IsRequired();
            e.HasOne(x => x.Product).WithMany().HasForeignKey(x => x.ProductId);
            e.HasOne(x => x.Branch).WithMany().HasForeignKey(x => x.BranchId);
        });

        // AuditLog
        modelBuilder.Entity<AuditLog>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.EntityType).HasMaxLength(100).IsRequired();
        });

        // SaleReturn
        modelBuilder.Entity<SaleReturn>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasOne(x => x.Sale).WithMany().HasForeignKey(x => x.SaleId);
        });

        // TransferRequest
        modelBuilder.Entity<TransferRequest>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasOne(x => x.FromBranch).WithMany().HasForeignKey(x => x.FromBranchId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.ToBranch).WithMany().HasForeignKey(x => x.ToBranchId).OnDelete(DeleteBehavior.Restrict);
        });

        // TransferRequestLine
        modelBuilder.Entity<TransferRequestLine>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasOne(x => x.TransferRequest).WithMany(t => t.Lines).HasForeignKey(x => x.TransferRequestId);
            e.HasOne(x => x.Product).WithMany().HasForeignKey(x => x.ProductId);
        });

        // StockAdjustment - use Restrict to avoid SQL Server cascade path cycles
        modelBuilder.Entity<StockAdjustment>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasOne(x => x.Branch).WithMany().HasForeignKey(x => x.BranchId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.StockBatch).WithMany().HasForeignKey(x => x.StockBatchId).OnDelete(DeleteBehavior.Restrict);
        });

        // Payment
        modelBuilder.Entity<Payment>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasOne(x => x.PurchaseOrder).WithMany().HasForeignKey(x => x.PurchaseOrderId);
        });
    }
}
