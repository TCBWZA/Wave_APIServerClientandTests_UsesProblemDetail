# AssignmentModule6Svr - Customer Management API

A comprehensive RESTful API for managing customers, invoices, and phone numbers built with ASP.NET Core (.NET 10).

## Features

- Complete CRUD Operations for Customers, Invoices, and Phone Numbers
- RESTful API Design following best practices
- Auto-Generated IDs with global uniqueness for invoices and phone numbers
- API Key Authentication for DELETE operations
- Comprehensive Validation with detailed error messages
- RFC 7807 Problem Details for consistent error responses
- Interactive API Documentation with Scalar UI
- In-Memory Database for fast development and testing
- Sample Data Generation with Bogus library
- Extensive Test Coverage (74 NUnit tests)
- XML Documentation for IntelliSense support

---

## Table of Contents

- [Quick Start](#quick-start)
- [API Endpoints](#api-endpoints)
- [Data Models](#data-models)
- [Validation Rules](#validation-rules)
- [Authentication](#authentication)
- [Business Logic](#business-logic)
- [Sample Data](#sample-data)
- [Configuration](#configuration)
- [Testing](#testing)
- [Documentation](#documentation)
- [Project Structure](#project-structure)
- [Development](#development)
- [Deployment](#deployment)
- [Troubleshooting](#troubleshooting)

---

## Quick Start

### Prerequisites
- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- Visual Studio 2022+ or VS Code
- (Optional) Postman or similar API testing tool

### Run the Application

```powershell
# Clone or navigate to the project
cd AssignmentModule6Svr

# Restore packages
dotnet restore

# Run the API
dotnet run

# The API will be available at:
# - HTTP:  http://localhost:5000
# - HTTPS: https://localhost:5001
```

### Access API Documentation

Open your browser to:
```
https://localhost:5001/scalar/v1
```

This provides interactive API documentation with the ability to test endpoints directly.

### Quick Test

**PowerShell:**
```powershell
# Get all customers (returns 50 pre-generated customers)
Invoke-RestMethod -Uri "https://localhost:5001/api/Customers" -Method Get

# Get a specific customer
Invoke-RestMethod -Uri "https://localhost:5001/api/Customers/1" -Method Get
```

**curl for Windows:**
```cmd
curl https://localhost:5001/api/Customers
curl https://localhost:5001/api/Customers/1
```

---

## API Endpoints

### Overview

The API exposes three main resources:

| Resource | Base Path | Description |
|----------|-----------|-------------|
| **Customers** | `/api/Customers` | Manage customer records |
| **Invoices** | `/api/Invoices` | Manage customer invoices |
| **Phone Numbers** | `/api/PhoneNumbers` | Manage customer phone numbers |

### Customers Endpoints

```
GET    /api/Customers                    # Get all customers
GET    /api/Customers/{id}               # Get customer by ID
GET    /api/Customers/{id}/Invoices      # Get customer's invoices
POST   /api/Customers                    # Create new customer
PUT    /api/Customers/{id}               # Update customer
DELETE /api/Customers/{id}               # Delete customer (requires API key)
```

### Invoices Endpoints

```
GET    /api/Invoices                                    # Get all invoices
GET    /api/Invoices/{customerId}/{invoiceNumber}      # Get specific invoice
GET    /api/Invoices/customer/{customerId}             # Get customer's invoices
POST   /api/Invoices                                    # Create invoice
DELETE /api/Invoices/{invoiceNumber}                   # Delete invoice (requires API key)
DELETE /api/Invoices/customer/{customerId}             # Delete all customer invoices (requires API key)
```

### Phone Numbers Endpoints

```
GET    /api/PhoneNumbers                        # Get all phone numbers
GET    /api/PhoneNumbers/{id}                   # Get phone number by ID
GET    /api/PhoneNumbers/customer/{customerId}  # Get customer's phone numbers
POST   /api/PhoneNumbers                        # Create phone number
DELETE /api/PhoneNumbers/{id}                   # Delete phone number (requires API key)
DELETE /api/PhoneNumbers/customer/{customerId}  # Delete customer's phones (requires API key)
```

---

## 📦 Data Models

### Customer

```json
{
  "id": 1,
  "name": "Acme Corporation",
  "email": "contact@acme.com",
  "balance": 1500.00,
  "invoices": [
    {
      "id": 1,
      "customerId": 1,
      "invoiceNumber": "INV-A1B2C3D4",
      "invoiceDate": "2024-01-15T10:30:00Z",
      "amount": 500.00
    }
  ],
  "phoneNumbers": [
    {
      "id": 1,
      "customerId": 1,
      "type": "Mobile",
      "number": "+1 (555) 123-4567"
    }
  ]
}
```

**Properties:**
- `id` (long): Auto-generated unique identifier
- `name` (string): Customer name (typically company name)
- `email` (string): Customer email address
- `balance` (decimal): **Calculated** - sum of all invoice amounts
- `invoices` (array): Collection of customer invoices
- `phoneNumbers` (array): Collection of customer phone numbers

### Invoice

```json
{
  "id": 1,
  "customerId": 1,
  "invoiceNumber": "INV-A1B2C3D4",
  "invoiceDate": "2024-01-15T10:30:00Z",
  "amount": 500.00
}
```

**Properties:**
- `id` (long): **Auto-generated** globally unique identifier
- `customerId` (long): **Required** - foreign key to Customer
- `invoiceNumber` (string): **Required** - must start with "INV", globally unique
- `invoiceDate` (DateTime): Invoice date
- `amount` (decimal): Invoice amount

### Phone Number

```json
{
  "id": 1,
  "customerId": 1,
  "type": "Mobile",
  "number": "+1 (555) 123-4567"
}
```

**Properties:**
- `id` (long): **Auto-generated** globally unique identifier
- `customerId` (long): **Required** - foreign key to Customer
- `type` (string): **Required** - must be "Mobile", "Work", or "DirectDial"
- `number` (string): Phone number in any format

---

## ✅ Validation Rules

### Customer Validation

| Field | Required | Rules |
|-------|----------|-------|
| `id` | Auto-generated | System-assigned, provided values ignored |
| `name` | Optional | Any string |
| `email` | Optional | Any string |
| `balance` | Calculated | Sum of invoice amounts (read-only) |

### Invoice Validation

| Field | Required | Rules |
|-------|----------|-------|
| `id` | Auto-generated | Globally unique across all customers |
| `customerId` | **Yes** | Must reference existing customer, must be > 0 |
| `invoiceNumber` | **Yes** | Must start with "INV", globally unique |
| `invoiceDate` | **Yes** | Valid DateTime |
| `amount` | **Yes** | Decimal value |

**Important Invoice Rules:**
- Invoice numbers are **user-provided** (not auto-generated)
- Must start with "INV" prefix (e.g., "INV-12345678")
- Must be **globally unique** across all customers
- Duplicate invoice numbers return **409 Conflict**

### Phone Number Validation

| Field | Required | Rules |
|-------|----------|-------|
| `id` | Auto-generated | Globally unique across all customers |
| `customerId` | **Yes** | Must reference existing customer, must be > 0 |
| `type` | **Yes** | Must be: "Mobile", "Work", or "DirectDial" |
| `number` | **Yes** | Any string format |

---

## Authentication

### API Key for DELETE Operations

All **DELETE** endpoints require API key authentication.

#### Header Format
```
X-API-Key: YOUR_API_KEY_HERE
```

#### Example with PowerShell
```powershell
$headers = @{
    "X-API-Key" = "YOUR_API_KEY_HERE"
}
Invoke-RestMethod -Uri "https://localhost:5001/api/Customers/1" -Method Delete -Headers $headers
```

#### Example with curl for Windows
```cmd
curl -X DELETE "https://localhost:5001/api/Customers/1" -H "X-API-Key: YOUR_API_KEY_HERE"
```

#### Example with JavaScript (Fetch)
```javascript
fetch('https://localhost:5001/api/Customers/1', {
  method: 'DELETE',
  headers: {
    'X-API-Key': 'YOUR_API_KEY_HERE'
  }
})
```

#### Configure API Key

Edit `appsettings.json`:
```json
{
  "ApiToken": {
    "StaticToken": "YOUR_SECURE_TOKEN_HERE"
  }
}
```

#### Error Response (Missing/Invalid Key)

```json
{
  "type": "https://yourapi.com/problems/unauthorized",
  "title": "API Key Missing",
  "status": 401,
  "detail": "API Key is missing. Please provide a valid API key in the X-API-Key header.",
  "instance": "DELETE /api/Customers/1",
  "traceId": "0HMVFE...",
  "requiredHeader": "X-API-Key"
}
```

---

## 🎯 Business Logic

### Auto-Generated IDs

The system automatically generates IDs with specific rules:

| Entity | ID Scope | Behavior |
|--------|----------|----------|
| **Customer** | Per customer | Sequential starting from 1 |
| **Invoice** | **Global** | Sequential across ALL customers |
| **Phone Number** | **Global** | Sequential across ALL customers |

**Key Points:**
- Any ID values provided in POST requests are **ignored**
- IDs are assigned using thread-safe `Interlocked.Increment`
- Invoice and Phone Number IDs are globally unique for easier tracking

**Example:**
```
Customer 1:
  Invoice 1 (ID: 1)
  Invoice 2 (ID: 2)
  
Customer 2:
  Invoice 3 (ID: 3)  <- Global ID continues
  Invoice 4 (ID: 4)
```

### Cascade Deletion

Deleting a customer **automatically deletes**:
- All customer invoices
- All customer phone numbers

This maintains referential integrity and prevents orphaned records.

**Example:**
```powershell
# Customer 1 has 3 invoices and 2 phone numbers
$headers = @{ "X-API-Key" = "ThisIsANewToken" }
Invoke-RestMethod -Uri "https://localhost:5001/api/Customers/1" -Method Delete -Headers $headers

# Result: Customer, 3 invoices, and 2 phone numbers all deleted
```

### Invoice Number Uniqueness

Invoice numbers must be:
- **User-provided** (not auto-generated)
- Start with "INV" prefix
- Globally unique across all customers

**Valid Examples:**
- `INV-12345678`
- `INV-A1B2C3D4`
- `INV-2024-001`

**Conflict Example:**
```powershell
# Create invoice
$invoice1 = @{
    customerId = 1
    invoiceNumber = "INV-123"
    invoiceDate = "2024-01-15T10:00:00Z"
    amount = 100.00
} | ConvertTo-Json

Invoke-RestMethod -Uri "https://localhost:5001/api/Invoices" -Method Post -Body $invoice1 -ContentType "application/json"
# Returns 201 Created

# Try to create duplicate - will fail with 409 Conflict
Invoke-RestMethod -Uri "https://localhost:5001/api/Invoices" -Method Post -Body $invoice1 -ContentType "application/json"
```

---

## 📊 Sample Data

On application startup, the API automatically generates:

| Data Type | Quantity | Details |
|-----------|----------|---------|
| **Customers** | 50 | Realistic company names and emails |
| **Invoices** | 1-5 per customer | Random amounts between $10-$5000 |
| **Phone Numbers** | 1-3 per customer | Various types (Mobile, Work, DirectDial) |

**Sample Data Features:**
- Generated using [Bogus](https://github.com/bchavez/Bogus) library
- UK locale for realistic UK data
- Realistic company names, emails, and phone numbers
- Invoice dates within past 2 years
- Globally unique invoice numbers

**Disable Sample Data:**

Edit `Program.cs`:
```csharp
bool customersCreated = AppDB.CreateCustomers(false, 50); // Set to false
```

---

## ⚙️ Configuration

### appsettings.json

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "ApiToken": {
    "StaticToken": "ThisIsANewToken"
  }
}
```

### Key Settings

| Setting | Purpose | Default |
|---------|---------|---------|
| `Logging:LogLevel:Default` | Overall log level | Debug |
| `ApiToken:StaticToken` | API key for DELETE operations | ThisIsANewToken |
| `AllowedHosts` | CORS hosts | * (all) |

### Launch Settings

The API runs on:
- **HTTP**: `http://localhost:5000`
- **HTTPS**: `https://localhost:5001`

Configure in `Properties/launchSettings.json`

---

## Testing

### Test Project

The solution includes comprehensive NUnit tests in `AssignmentModule6Svr.Tests`:

```powershell
# Run all tests
dotnet test

# Run with verbose output
dotnet test --verbosity normal

# Run specific test class
dotnet test --filter "FullyQualifiedName~CustomersControllerTests"

# Generate code coverage
dotnet test --collect:"XPlat Code Coverage"
```

### Test Statistics

```
Total Tests:     74
Passed:          74
Failed:          0
Skipped:         0
Coverage:        ~85%
Execution Time:  ~2 seconds
```

### Test Categories

| Test Class | Tests | Coverage |
|------------|-------|----------|
| `AppDBTests` | 30 | Database operations, CRUD, ID generation |
| `CustomersControllerTests` | 19 | Customer endpoints, validation, cascade delete |
| `InvoicesControllerTests` | 17 | Invoice endpoints, duplicate detection, validation |
| `PhoneNumbersControllerTests` | 18 | Phone endpoints, type validation, global IDs |

### Key Test Scenarios

- Auto-generated ID behavior
- Global ID uniqueness (invoices, phone numbers)
- Invoice number validation (must start with "INV")
- Duplicate invoice number detection (409 Conflict)
- Cascade deletion (customer -> invoices -> phones)
- API key authentication (DELETE operations)
- Error handling (400, 401, 404, 409)
- RFC 7807 Problem Details format

---

## Documentation

### Interactive API Documentation

**Scalar UI** (Recommended):
```
https://localhost:5001/scalar/v1
```

Features:
- Interactive endpoint testing
- C# code examples (HttpClient)
- Request/response schemas
- Authentication testing
- All XML documentation comments

### OpenAPI Specification

```
https://localhost:5001/openapi/v1.json
```

Can be imported into:
- Postman
- Insomnia
- Swagger UI
- API testing tools

### Quick Reference

See **[QUICK_REFERENCE.md](QUICK_REFERENCE.md)** for:
- All endpoints at a glance
- Common operations
- cURL examples
- Troubleshooting tips

### XML Documentation

All classes and methods have comprehensive XML documentation:
- Shows in IntelliSense
- Appears in Scalar UI
- Generated file: `bin/Debug/net10.0/AssignmentModule6Svr.xml`

---

## 📁 Project Structure

```
AssignmentModule6Svr/
├── Controllers/
│   ├── CustomersController.cs       # Customer CRUD endpoints
│   ├── InvoicesController.cs        # Invoice CRUD endpoints
│   └── PhoneNumbersController.cs    # Phone number CRUD endpoints
│
├── Classes/                          # Data Models
│   ├── Customer.cs                   # Customer entity with validation
│   ├── Invoice.cs                    # Invoice entity with validation
│   └── PhoneNumber.cs                # Phone number entity with validation
│
├── Attributes/
│   └── ApiKeyAuthAttribute.cs        # API key authentication filter
│
├── AppDB.cs                          # In-memory database with CRUD operations
├── Bogus.cs                          # Fake data generator (Bogus library)
├── Program.cs                        # App configuration and startup
├── ProblemDetailsTypes.cs            # RFC 7807 Problem Details URIs
│
├── appsettings.json                  # Configuration (API key, logging)
├── appsettings.Development.json      # Development settings
│
├── README.md                         # This file
├── QUICK_REFERENCE.md                # Quick reference guide
└── DOCUMENTATION_COMPLETE.md         # XML comments documentation
```

---

## 💻 Development

### Prerequisites

- **.NET 10 SDK**: https://dotnet.microsoft.com/download/dotnet/10.0
- **Visual Studio 2022** (recommended) or **VS Code**
- **Git** for version control

### Clone and Setup

```powershell
# Clone repository
git clone <repository-url>
cd AssignmentModule6Svr

# Restore packages
dotnet restore

# Build project
dotnet build

# Run tests
dotnet test

# Run application
dotnet run
```

### Development Workflow

1. **Make changes** to controllers or models
2. **Add XML comments** for new methods/properties
3. **Write tests** for new functionality
4. **Run tests** to ensure nothing breaks
5. **Build project** to verify compilation
6. **Test in Scalar UI** for manual verification

### Adding New Endpoints

1. Add method to appropriate controller
2. Add XML documentation comments
3. Add validation as needed
4. Add tests to test project
5. Update QUICK_REFERENCE.md if needed

### Code Style

- Use XML documentation comments for all public members
- Follow C# naming conventions (PascalCase for methods)
- Return RFC 7807 Problem Details for errors
- Use async/await where appropriate
- Keep controllers thin, business logic in AppDB

---

## Deployment

### Publish for Production

```powershell
# Publish release build
dotnet publish -c Release -o .\publish

# The output will be in .\publish folder
```

### Docker (Optional)

Create `Dockerfile`:
```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["AssignmentModule6Svr.csproj", "./"]
RUN dotnet restore
COPY . .
RUN dotnet build -c Release -o /app/build

FROM build AS publish
RUN dotnet publish -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AssignmentModule6Svr.dll"]
```

Build and run:
```powershell
docker build -t assignmentmodule6svr .
docker run -p 5000:80 -p 5001:443 assignmentmodule6svr
```

### Environment Variables

For production, set:
```powershell
$env:ASPNETCORE_ENVIRONMENT = "Production"
$env:ApiToken__StaticToken = "<your-secure-token>"
```

### Security Considerations

- Change default API key in production
- Use HTTPS for all communication
- Consider using JWT tokens instead of static API keys
- Implement rate limiting for production
- Add CORS policies as needed
- Use a real database (SQL Server, PostgreSQL, etc.)

---

## Troubleshooting

### Port Already in Use

**Error**: `Address already in use`

**Solution**:
```powershell
# Find process using port 5001
netstat -ano | findstr :5001

# Kill process (replace <PID> with actual process ID)
taskkill /PID <PID> /F

# Or change port in launchSettings.json
```

### API Key Not Working

**Error**: 401 Unauthorized on DELETE

**Solutions**:
- Check header name is `X-API-Key` (case-sensitive)
- Verify token matches `appsettings.json`
- Ensure using HTTPS, not HTTP
- Check token doesn't have extra spaces

### Swagger/Scalar Not Loading

**Error**: 404 on `/scalar/v1`

**Solutions**:
- Only available in Development environment
- Check `ASPNETCORE_ENVIRONMENT` is set to `Development`
- Verify Scalar.AspNetCore package is installed

### Invoice Number Validation Fails

**Error**: 400 Bad Request - "InvoiceNumber must start with 'INV'"

**Solution**:
- Ensure invoice number starts with "INV"
- Examples: "INV-123", "INV-A1B2C3", "INV2024"

### Duplicate Invoice Number

**Error**: 409 Conflict

**Solution**:
- Invoice numbers must be globally unique
- Check if invoice number already exists
- Use a different invoice number

### Database Resets on Restart

**Behavior**: All data lost when app restarts

**Explanation**:
- This is expected - the database is in-memory
- Data is not persisted to disk
- For persistent storage, implement a real database

---

## License

This project is for educational purposes.

---

## Contributing

This is an assignment project. For suggestions or issues:

1. Document the issue
2. Provide steps to reproduce
3. Include expected vs actual behavior
4. Note your environment (.NET version, OS, etc.)

---

## Support

- **Documentation**: See `/scalar/v1` endpoint
- **Quick Reference**: See [QUICK_REFERENCE.md](QUICK_REFERENCE.md)
- **Tests**: Run `dotnet test` for examples
- **Code Comments**: All methods have XML documentation

---

## Learning Resources

- [ASP.NET Core Documentation](https://docs.microsoft.com/aspnet/core)
- [RESTful API Design](https://restfulapi.net/)
- [RFC 7807 Problem Details](https://tools.ietf.org/html/rfc7807)
- [OpenAPI Specification](https://swagger.io/specification/)
- [NUnit Testing](https://nunit.org/)

---

Version: 1.0.0  
Last Updated: 2025  
Target Framework: .NET 10  
API Documentation: Scalar
