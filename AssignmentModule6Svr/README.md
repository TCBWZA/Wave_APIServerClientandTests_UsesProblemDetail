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

## Quick Start

### Prerequisites
- .NET 10 SDK: https://dotnet.microsoft.com/download/dotnet/10.0
- Visual Studio 2022+ or VS Code

### Run the Application

```powershell
# Navigate to the project
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

Use PowerShell with Invoke-RestMethod:
```powershell
# Get all customers (returns 50 pre-generated customers)
Invoke-RestMethod -Uri "https://localhost:5001/api/Customers" -Method Get

# Get a specific customer
Invoke-RestMethod -Uri "https://localhost:5001/api/Customers/1" -Method Get
```

Or use curl for Windows:
```cmd
curl https://localhost:5001/api/Customers
curl https://localhost:5001/api/Customers/1
```

## API Endpoints

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

## Authentication

### API Key for DELETE Operations

All DELETE endpoints require API key authentication.

Header Format:
```
X-API-Key: YOUR_API_KEY_HERE
```

Example with PowerShell:
```powershell
$headers = @{
    "X-API-Key" = "YOUR_API_KEY_HERE"
}
Invoke-RestMethod -Uri "https://localhost:5001/api/Customers/1" -Method Delete -Headers $headers
```

Example with curl for Windows:
```cmd
curl -X DELETE "https://localhost:5001/api/Customers/1" -H "X-API-Key: YOUR_API_KEY_HERE"
```

Example with JavaScript (Fetch):
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

## Testing

### Run Tests

```powershell
# Run all tests
dotnet test

# Run with verbose output
dotnet test --verbosity normal

# Run specific test class
dotnet test --filter "FullyQualifiedName~CustomersControllerTests"
```

### Test Statistics

```
Total Tests:     74
Passed:          74
Failed:          0
Coverage:        ~85%
```

## Project Structure

```
AssignmentModule6Svr/
|-- Controllers/
|   |-- CustomersController.cs
|   |-- InvoicesController.cs
|   +-- PhoneNumbersController.cs
|-- Classes/
|   |-- Customer.cs
|   |-- Invoice.cs
|   +-- PhoneNumber.cs
|-- Attributes/
|   +-- ApiKeyAuthAttribute.cs
|-- AppDB.cs
|-- Bogus.cs
|-- Program.cs
|-- ProblemDetailsTypes.cs
+-- appsettings.json
```

## Documentation

- Full Documentation: `Docs/README.md`
- Quick Reference: `Docs/QUICK_REFERENCE.md`
- API Documentation: `https://localhost:5001/scalar/v1`

## Configuration

Edit `appsettings.json`:
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

## Development

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

### Build the Project
```powershell
dotnet build
```

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

### Database Resets on Restart

**Behavior**: All data lost when app restarts

**Explanation**:
- This is expected - the database is in-memory
- Data is not persisted to disk

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
- **Quick Reference**: See [QUICK_REFERENCE.md](Docs/QUICK_REFERENCE.md)
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
