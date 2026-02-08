# Migration Notes

## Migration History

- **InitialCreate** – Core tables (Organizations, Branches, Users, Products, Manufacturers, Sales, etc.)
- **AddNewEntities** – SaleReturn, TransferRequest, TransferRequestLine, StockAdjustment, Payment

## Run Migrations

Migrations are applied automatically when the API starts. To apply manually:

```bash
cd D:\PharmacyManagementSystem
dotnet ef database update --project src/PharmacyManagementSystem.Infrastructure --startup-project src/PharmacyManagementSystem.Api
```

To add a new migration (stop the API first):

```bash
dotnet ef migrations add MigrationName --project src/PharmacyManagementSystem.Infrastructure --startup-project src/PharmacyManagementSystem.Api --context ApplicationDbContext --output-dir Data/Migrations
```
