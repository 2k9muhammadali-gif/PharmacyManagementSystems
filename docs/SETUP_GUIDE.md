# Pharmacy Management System - Setup Guide

## 1. Prerequisites Installation

### .NET 9 SDK

1. Download from https://dotnet.microsoft.com/download
2. Install .NET 9 SDK
3. Verify: `dotnet --version` (should show 9.x.x)

### Node.js

1. Download LTS from https://nodejs.org/
2. Install Node.js 18 or higher
3. Verify: `node --version` and `npm --version`

### SQL Server LocalDB

- Included with Visual Studio or .NET SDK
- Or install SQL Server Express: https://www.microsoft.com/en-us/sql-server/sql-server-downloads
- Verify: `sqllocaldb info` (if using LocalDB)

### Angular CLI (optional)

```bash
npm install -g @angular/cli@latest
```

## 2. Clone/Extract Project

Ensure the project is at: `D:\PharmacyManagementSystem`

## 3. Backend Setup

### Restore Packages

```bash
cd D:\PharmacyManagementSystem
dotnet restore
```

### Configure Connection String

Edit `src/PharmacyManagementSystem.Api/appsettings.json`:

For **LocalDB** (default):
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=PharmacyManagementSystem;Trusted_Connection=True;MultipleActiveResultSets=true"
}
```

For **SQL Server Express**:
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=.\\SQLEXPRESS;Database=PharmacyManagementSystem;Trusted_Connection=True;MultipleActiveResultSets=true"
}
```

For **Named SQL Server**:
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=YOUR_SERVER;Database=PharmacyManagementSystem;User Id=sa;Password=YOUR_PASSWORD;TrustServerCertificate=True;"
}
```

### Run Migrations Manually (if needed)

```bash
dotnet ef database update --project src/PharmacyManagementSystem.Infrastructure --startup-project src/PharmacyManagementSystem.Api
```

### Start API

```bash
dotnet run --project src/PharmacyManagementSystem.Api
```

The API will:
- Create the database if it doesn't exist
- Apply migrations
- Seed initial data (Organization, Branch, Admin user, License)

API URL: **http://localhost:5029**
Swagger: **http://localhost:5029/swagger**

## 4. Frontend Setup

### Install Dependencies

```bash
cd D:\PharmacyManagementSystem\PharmacyManagementSystem.Web
npm install
```

### Configure API URL

Edit `src/environments/environment.ts` (create if not exists) to set the API base URL:

```typescript
export const environment = {
  production: false,
  apiUrl: 'http://localhost:5029/api'
};
```

### Start Development Server

```bash
ng serve
```

Frontend URL: **http://localhost:4200**

## 5. Initial Login

After first run:

- **Email:** admin@pharmacy.com
- **Password:** Admin@123

## 6. Troubleshooting

### Migration fails

- Ensure SQL Server is running
- Check connection string
- Run: `dotnet ef database update` manually

### Port already in use

Edit `src/PharmacyManagementSystem.Api/Properties/launchSettings.json` to change the port.

### CORS errors

The API has `AllowAll` CORS policy for development. Ensure the frontend is calling the correct API URL.

### Angular build fails

```bash
cd PharmacyManagementSystem.Web
rm -rf node_modules package-lock.json
npm install
ng build
```
