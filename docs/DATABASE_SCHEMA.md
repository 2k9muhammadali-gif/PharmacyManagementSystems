# Pharmacy Management System - Database Schema

## Overview

SQL Server database with Entity Framework Core 9. LocalDB used for development.

## Entity Relationship

```
Organization (1) ----< (*) License
Organization (1) ----< (*) Branch
Branch (1) ----< (*) User
Branch (1) ----< (*) StockBatch
Manufacturer (1) ----< (*) Product
Product (1) ----< (*) StockBatch
Distribution (1) ----< (*) PurchaseOrder
PurchaseOrder (1) ----< (*) PurchaseOrderLine
Branch (1) ----< (*) Sale
Sale (1) ----< (*) SaleLine
Sale (1) ----< (*) Prescription
Customer (1) ----< (*) Sale
```

## Tables

### Organization
| Column | Type | Description |
|--------|------|-------------|
| Id | uniqueidentifier | PK |
| Name | nvarchar(200) | Organization name |
| CreatedAt | datetime2 | Created timestamp |

### License
| Column | Type | Description |
|--------|------|-------------|
| Id | uniqueidentifier | PK |
| OrganizationId | uniqueidentifier | FK |
| LicenseKey | nvarchar(100) | Unique key |
| LicenseType | int | Trial, SingleBranch, MultiBranch, Enterprise |
| StartDate | datetime2 | |
| EndDate | datetime2 | |
| MaxBranches | int | |
| IsActive | bit | |
| ActivatedAt | datetime2? | |

### Branch
| Column | Type | Description |
|--------|------|-------------|
| Id | uniqueidentifier | PK |
| OrganizationId | uniqueidentifier | FK |
| Name | nvarchar(200) | |
| Address | nvarchar(max) | |
| Phone | nvarchar(max)? | |
| FBRStoreId | nvarchar(max)? | FBR registration |
| FBRPosDeviceId | nvarchar(max)? | FBR device ID |
| IsActive | bit | |

### User
| Column | Type | Description |
|--------|------|-------------|
| Id | uniqueidentifier | PK |
| OrganizationId | uniqueidentifier | FK |
| BranchId | uniqueidentifier? | FK |
| Email | nvarchar(256) | |
| PasswordHash | nvarchar(max) | BCrypt hash |
| FullName | nvarchar(max) | |
| Role | int | UserRole enum |
| IsActive | bit | |

### Manufacturer
| Column | Type | Description |
|--------|------|-------------|
| Id | uniqueidentifier | PK |
| Name | nvarchar(200) | |
| ContactInfo | nvarchar(max)? | |
| Address | nvarchar(max)? | |

### Product
| Column | Type | Description |
|--------|------|-------------|
| Id | uniqueidentifier | PK |
| ManufacturerId | uniqueidentifier | FK |
| Name | nvarchar(300) | |
| GenericName | nvarchar(max)? | |
| Strength | nvarchar(max)? | |
| Formulation | nvarchar(max)? | |
| Schedule | int | OTC, Prescription, ScheduleH |
| Barcode | nvarchar(max)? | |
| DRAPNumber | nvarchar(max)? | |
| TherapeuticCategory | nvarchar(max)? | |
| Contraindications | nvarchar(max)? | |
| DrugInteractions | nvarchar(max)? | |
| ReorderPoint | int | |
| SalePrice | decimal | |
| IsActive | bit | |

### StockBatch
| Column | Type | Description |
|--------|------|-------------|
| Id | uniqueidentifier | PK |
| BranchId | uniqueidentifier | FK |
| ProductId | uniqueidentifier | FK |
| BatchNumber | nvarchar(max) | |
| Quantity | int | |
| ExpiryDate | datetime2 | |
| PurchasePrice | decimal | |
| ReceivedAt | datetime2 | |

### Distribution, PurchaseOrder, PurchaseOrderLine, Customer, Sale, SaleLine, Prescription, ControlledSubstanceLog, AuditLog

See plan document for full schema.

## Migrations

Located in: `src/PharmacyManagementSystem.Infrastructure/Data/Migrations/`

Apply: `dotnet ef database update`
Add new: `dotnet ef migrations add MigrationName`
