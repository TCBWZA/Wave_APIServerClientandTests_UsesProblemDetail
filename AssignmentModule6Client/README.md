# AssignmentModule6Client - API Client Application

A .NET 10 console application that demonstrates RESTful API consumption with enterprise-grade logging and configuration.

> **Version 2.0 - RFC 7807 Problem Details Focused**: This version emphasizes comprehensive Problem Details error handling and requires .NET 10 or later. Not compatible with .NET 8 or earlier versions.

## Features

- HTTP Client for API communication
- Type-safe JSON serialization/deserialization
- **Comprehensive error handling with RFC 7807 Problem Details** (v2.0 feature)
- **Structured logging with Microsoft.Extensions.Logging** (v2.0 feature)
- Configuration management with appsettings.json
- Dependency injection support
- Environment-specific configuration (Development/Production)
- API Key authentication

**What's New in v2.0:**
- RFC 7807 Problem Details parsing and structured logging
- Intelligent error recovery based on error types
- Enterprise-grade logging infrastructure
- Requires .NET 10 or later

## Quick Start

### Prerequisites
- **.NET 10 SDK**: https://dotnet.microsoft.com/download/dotnet/10.0 **(REQUIRED - not compatible with .NET 8)**
- API Server running (AssignmentModule6Svr)

> **Important**: Version 2.0 requires .NET 10 for Problem Details features. If you need .NET 8 support, use Version 1.0

### Run the Application

```powershell
# Start the API server first
cd AssignmentModule6Svr
dotnet run

# In a new terminal, run the client
cd AssignmentModule6Client
dotnet run
```

The application will execute 7 example scenarios demonstrating API operations.

## Configuration

### appsettings.json

Configure the API connection and logging:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "AssignmentModule6Client": "Debug",
      "System.Net.Http": "Warning"
    }
  },
  "ApiSettings": {
    "BaseUrl": "https://localhost:7136",
    "ApiToken": "YOUR_API_TOKEN_HERE",
    "TimeoutSeconds": 30
  }
}
```

### Environment Variables

Override configuration with environment variables:

**PowerShell:**
```powershell
$env:DOTNET_ENVIRONMENT = "Development"
$env:ApiSettings__BaseUrl = "https://api.dev.com"
$env:ApiSettings__ApiToken = "YOUR_API_TOKEN_HERE"
```

**Command Prompt:**
```cmd
set DOTNET_ENVIRONMENT=Development
set ApiSettings__BaseUrl=https://api.dev.com
set ApiSettings__ApiToken=YOUR_API_TOKEN_HERE
```

## Usage Examples

### Initialize the Client

```csharp
var logger = loggerFactory.CreateLogger<ApiClient>();
using var apiClient = new ApiClient(baseUrl, apiToken, logger);
```

### GET Requests

```csharp
// Get all customers
var customers = await apiClient.GetFromJsonAsync<List<Customer>>("/api/Customers");

// Get single customer
var customer = await apiClient.GetFromJsonAsync<Customer>("/api/Customers/1");
```

### POST Requests

```csharp
var newCustomer = new Customer
{
    Name = "Company Name",
    Email = "email@company.com",
    Invoices = new List<Invoice>
    {
        new Invoice
        {
            InvoiceNumber = "INV-123456",
            InvoiceDate = DateTime.Now,
            Amount = 1000.00M
        }
    }
};

var created = await apiClient.PostAsync<Customer, Customer>("/api/Customers", newCustomer);
```

### DELETE Requests

```csharp
// Requires API key authentication
var deleted = await apiClient.DeleteAsync("/api/Customers/10");
```

## Error Handling

The client includes comprehensive error handling with Problem Details parsing:

```csharp
try
{
    await apiClient.DeleteAsync("/api/Customers/10");
}
catch (HttpRequestException ex)
{
    var problemDetail = await ParseProblemDetails(ex.Message);
    
    if (problemDetail?.Status == 404)
    {
        logger.LogInformation("Customer not found - already deleted");
    }
    else if (problemDetail?.Status == 401)
    {
        logger.LogWarning("Unauthorized - check API token");
    }
}
```

## Example Scenarios

The Program.cs demonstrates 7 key scenarios:

1. **Get All Customers** - Retrieve and display all customers with their invoices and phone numbers
2. **Create New Customer** - Create a customer with 3 invoices and 3 phone numbers
3. **Duplicate Customer Error** - Handle 409 Conflict when creating duplicate entries
4. **Delete Customer** - Delete a customer using API key authentication
5. **Delete Non-Existing Customer** - Handle 404 Not Found errors
6. **Add Invoice** - Add an invoice to an existing customer
7. **Invalid Customer** - Handle errors when adding invoice to non-existing customer

## Logging

### Log Levels

Adjust in `appsettings.json`:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "AssignmentModule6Client": "Debug"
    }
  }
}
```

Available levels: Trace, Debug, Information, Warning, Error, Critical

### Structured Logging

```csharp
logger.LogInformation("Customer created with ID: {CustomerId}", customerId);
logger.LogDebug("Processing customer: {CustomerId}", customerId);
logger.LogWarning("Customer not found: {CustomerId}", customerId);
logger.LogError(ex, "Failed to create customer");
```

## Project Structure

```
AssignmentModule6Client/
|-- Models/
|   |-- Customer.cs
|   |-- Invoice.cs
|   |-- PhoneNumber.cs
|   +-- ProblemDetails.cs
|-- ApiClient.cs
|-- Program.cs
|-- appsettings.json
|-- appsettings.Development.json
+-- Docs/
    |-- README.md
    |-- QUICK_REFERENCE.md
    |-- CONFIGURATION_AND_LOGGING.md
    +-- IMPLEMENTATION_SUMMARY.md
```

## Dependencies

```xml
<PackageReference Include="Microsoft.Extensions.Configuration" Version="9.0.0" />
<PackageReference Include="Microsoft.Extensions.Configuration.Json" Version="9.0.0" />
<PackageReference Include="Microsoft.Extensions.Configuration.EnvironmentVariables" Version="9.0.0" />
<PackageReference Include="Microsoft.Extensions.DependencyInjection" Version="9.0.0" />
<PackageReference Include="Microsoft.Extensions.Logging" Version="9.0.0" />
<PackageReference Include="Microsoft.Extensions.Logging.Console" Version="9.0.0" />
<PackageReference Include="Microsoft.Extensions.Logging.Debug" Version="9.0.0" />
```

## Documentation

- Full Documentation: `Docs/README.md` (v2.0 features and Problem Details examples)
- Quick Reference: `Docs/QUICK_REFERENCE.md`
- Configuration Guide: `Docs/CONFIGURATION_AND_LOGGING.md`
- Implementation Summary: `Docs/IMPLEMENTATION_SUMMARY.md`
- **Update Summary: `Docs/UPDATE_SUMMARY_V2.md`** (What's new in v2.0, Problem Details focus)

## Version Information

**Current Version**: 2.0.0
**Target Framework**: .NET 10 (Required)
**Key Feature**: RFC 7807 Problem Details Support
**Compatibility**: Not backward compatible with .NET 8 or earlier

**Version History:**
- v2.0.0: Problem Details support, structured logging, configuration management (.NET 10+)
- v1.0.0: Basic API client functionality (.NET 6/7/8 compatible)

## Troubleshooting

### Connection Refused
- Ensure the API server is running first
- Check the BaseUrl in appsettings.json

### 401 Unauthorized
- Verify the API token matches the server configuration
- Check that X-API-Key header is being sent

### SSL Certificate Error
- Run: `dotnet dev-certs https --trust`

### Configuration Not Loading
- Ensure appsettings.json is copied to output directory
- Check .csproj file for CopyToOutputDirectory setting

### Logs Not Appearing
- Check log level in appsettings.json
- Ensure Default log level is not "None"

## License

This project is for educational purposes.

## Support

For questions or issues:
1. Review the README.md in the Docs folder
2. Check the QUICK_REFERENCE.md for common scenarios
3. Examine the Program.cs for working examples
4. Review console logs for detailed error messages
