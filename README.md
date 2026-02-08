# Pharmacy Management System - Pakistan

A multi-branch pharmacy management system for Pakistan with license-based access, FBR integration, and digitized Distribution order forms.

## Technology Stack

| Layer | Technology |
|-------|------------|
| Backend | ASP.NET Core 9 Web API (C#) |
| Database | SQL Server (LocalDB for development) |
| ORM | Entity Framework Core 9 |
| Auth | JWT (Bearer tokens) |
| Frontend | Angular 19 + Bootstrap 5 |

## Project Structure

```
D:\PharmacyManagementSystem\
├── src/
│   ├── PharmacyManagementSystem.Api/       # Web API
│   ├── PharmacyManagementSystem.Core/      # Domain entities, enums
│   └── PharmacyManagementSystem.Infrastructure/  # EF Core, data access
├── PharmacyManagementSystem.Web/           # Angular admin dashboard
├── docs/                                   # Documentation
├── postman/                                # Postman collections
└── PharmacyManagementSystem.sln
```

## Prerequisites

- .NET 9 SDK
- Node.js 18+
- SQL Server LocalDB (or SQL Server Express)
- Angular CLI: `npm install -g @angular/cli`

## Quick Start

### 1. Backend (API)

```bash
cd D:\PharmacyManagementSystem

# Restore packages
dotnet restore

# Apply migrations and run (creates DB automatically)
dotnet run --project src/PharmacyManagementSystem.Api
```

API runs at: **http://localhost:5029**

- Swagger UI: http://localhost:5029/swagger

### 2. Default Login

After first run, the database is seeded with:

| Field | Value |
|-------|-------|
| Email | admin@pharmacy.com |
| Password | Admin@123 |

### 3. Frontend (Angular)

```bash
cd D:\PharmacyManagementSystem\PharmacyManagementSystem.Web

# Install dependencies (if not done)
npm install

# Run development server
ng serve
```

Frontend runs at: **http://localhost:4200**

The Angular dev server uses a proxy (`proxy.conf.json`) to forward `/api` requests to the API. Ensure the API is running at http://localhost:5029 before using the frontend.

## Configuration

### Connection String

Edit `src/PharmacyManagementSystem.Api/appsettings.json`:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=PharmacyManagementSystem;Trusted_Connection=True;MultipleActiveResultSets=true"
}
```

For SQL Server Express, use:
```
Server=.\SQLEXPRESS;Database=PharmacyManagementSystem;Trusted_Connection=True;
```

### JWT Settings

In `appsettings.json`:

```json
"Jwt": {
  "Key": "YourSuperSecretKeyThatIsAtLeast32CharactersLong!",
  "Issuer": "PharmacyManagementSystem",
  "Audience": "PharmacyManagementSystem",
  "ExpiryMinutes": 60
}
```

## API Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | /api/health | Health check |
| POST | /api/auth/login | Login (returns JWT) |
| GET | /api/* | Other endpoints (require Authorization header) |

## Postman Collection

Import `postman/Pharmacy_Management_System_API.postman_collection.json` into Postman.

## Testing

```bash
dotnet test
```

See [Testing Guide](docs/TESTING.md).

## Documentation

- [Setup Guide](docs/SETUP_GUIDE.md)
- [API Documentation](docs/API_DOCUMENTATION.md)
- [Database Schema](docs/DATABASE_SCHEMA.md)
- [Project Structure](docs/PROJECT_STRUCTURE.md)
- [Testing Guide](docs/TESTING.md)

## License

Proprietary - Licensed product. Only users with valid license can use.
