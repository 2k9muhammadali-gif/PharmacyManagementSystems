# Pharmacy Management System - API Documentation

## Base URL

- Development: `http://localhost:5029/api`
- Use `Authorization: Bearer <token>` header for protected endpoints

## Authentication

### POST /api/auth/login

Login and receive JWT token.

**Request:**
```json
{
  "email": "admin@pharmacy.com",
  "password": "Admin@123"
}
```

**Response (200):**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "email": "admin@pharmacy.com",
  "fullName": "System Admin",
  "role": "SuperAdmin",
  "branchId": "guid-or-null",
  "organizationId": "guid",
  "expiresAt": "2025-02-09T12:00:00Z"
}
```

**Errors:**
- 401: Invalid email or password
- 403: License required or expired

### Using the Token

For all protected endpoints, add header:
```
Authorization: Bearer <your-jwt-token>
```

## Health

### GET /api/health

Health check endpoint (no auth required).

**Response (200):**
```json
{
  "status": "Healthy",
  "timestamp": "2025-02-08T12:00:00Z",
  "version": "1.0.0"
}
```

## Planned Endpoints (To Be Implemented)

| Module | Method | Endpoint | Description |
|--------|--------|----------|-------------|
| License | POST | /api/license/activate | Activate with license key |
| License | GET | /api/license/status | Get license status |
| Users | GET | /api/users | List users |
| Users | POST | /api/users | Create user |
| Branches | GET | /api/branches | List branches |
| Manufacturers | GET/POST/PUT/DELETE | /api/manufacturers | CRUD manufacturers |
| Products | GET/POST/PUT/DELETE | /api/products | CRUD products |
| Inventory | GET | /api/inventory | Get stock |
| Inventory | POST | /api/inventory/adjust | Stock adjustment |
| Sales | POST | /api/sales | Create sale |
| ... | ... | ... | See full plan |

## Swagger

Interactive API documentation: **http://localhost:5029/swagger**

## Enums Reference

### UserRole
- SuperAdmin, OrganizationOwner, BranchManager, LicensedPharmacist, SalesStaff, Accountant, StockKeeper

### Schedule
- OTC, Prescription, ScheduleH

### PaymentMode
- Cash, Card, JazzCash, Easypaisa, BankTransfer

### LicenseType
- Trial, SingleBranch, MultiBranch, Enterprise
