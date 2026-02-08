# Pharmacy Management System - Project Structure

## Solution Projects

### PharmacyManagementSystem.Api

ASP.NET Core 9 Web API. Entry point for the backend.

**Key files:**
- `Program.cs` - App configuration, DI, middleware
- `appsettings.json` - Connection string, JWT config
- `Controllers/` - API controllers
- `DTOs/` - Data transfer objects

### PharmacyManagementSystem.Core

Class library. Domain layer - no dependencies on infrastructure.

**Contents:**
- `Entities/` - Domain entities (Organization, Branch, User, Product, etc.)
- `Enums/` - LicenseType, UserRole, Schedule, PaymentMode, etc.

### PharmacyManagementSystem.Infrastructure

Class library. Data access and external services.

**Contents:**
- `Data/ApplicationDbContext.cs` - EF Core DbContext
- `Data/Migrations/` - EF migrations
- `Data/SeedData.cs` - Initial data seeding

### PharmacyManagementSystem.Web

Angular 19 application. Admin dashboard frontend.

**Contents:**
- `src/app/` - Components, services, routing
- `src/styles.css` - Global styles (Bootstrap imported)
- `angular.json` - Angular CLI config

## Documentation

- `README.md` - Main readme
- `docs/SETUP_GUIDE.md` - Setup instructions
- `docs/API_DOCUMENTATION.md` - API reference
- `docs/DATABASE_SCHEMA.md` - Database schema
- `docs/PROJECT_STRUCTURE.md` - This file

## Postman

- `postman/Pharmacy_Management_System_API.postman_collection.json` - Import into Postman
