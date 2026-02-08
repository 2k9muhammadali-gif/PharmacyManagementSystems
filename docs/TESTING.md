# Pharmacy Management System - Testing Guide

## Unit Tests

### Run All Tests

```bash
cd D:\PharmacyManagementSystem
dotnet test
```

### Run with Verbose Output

```bash
dotnet test --logger "console;verbosity=detailed"
```

### Test Project Structure

```
tests/PharmacyManagementSystem.Tests/
├── Core/
│   ├── EnumsTests.cs    # Tests for enums
│   └── EntityTests.cs   # Tests for entities
└── PharmacyManagementSystem.Tests.csproj
```

### Adding New Tests

1. Create test class in appropriate folder under `tests/PharmacyManagementSystem.Tests/`
2. Use xUnit: `[Fact]` for single test, `[Theory]` for parameterized
3. Follow naming: `MethodName_Scenario_ExpectedResult`

### Example

```csharp
[Fact]
public void Product_WithScheduleH_RequiresPrescription()
{
    var product = new Product { Schedule = Schedule.ScheduleH };
    Assert.Equal(Schedule.ScheduleH, product.Schedule);
}
```

## Integration Tests (Future)

For API integration tests, add:
- `PharmacyManagementSystem.IntegrationTests` project
- Use `WebApplicationFactory<Program>` for in-memory API testing
- Test endpoints with real HTTP requests

## Postman Testing

1. Import `postman/Pharmacy_Management_System_API.postman_collection.json`
2. Run "Login" request first (saves token to collection variable)
3. Run other requests - they use `{{token}}` automatically
