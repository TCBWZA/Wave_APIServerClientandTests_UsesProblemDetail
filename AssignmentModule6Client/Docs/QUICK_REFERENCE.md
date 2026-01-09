# Quick Reference Guide

## Running the Application

### 1. Start the API Server (Required First!)
```powershell
cd AssignmentModule6Svr
dotnet run
```
API will be available at: `https://localhost:7136`

### 2. Run the Client Application
```powershell
cd AssignmentModule6Client
dotnet run
```

## Configuration (NEW in v2.0!)

### Quick Edit: appsettings.json
```json
{
  "ApiSettings": {
    "BaseUrl": "https://localhost:7136",
    "ApiToken": "YOUR_API_TOKEN_HERE"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information"
    }
  }
}
```

### Environment Variables

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

## Log Levels

Adjust in `appsettings.json`:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",          // Info and above
      "AssignmentModule6Client": "Debug" // Debug and above for client
    }
  }
}
```

**Levels:** Trace > Debug > Information > Warning > Error > Critical

## Basic Usage

### Initialize the Client (with Logger)
```csharp
var logger = loggerFactory.CreateLogger<ApiClient>();
using var apiClient = new ApiClient(baseUrl, apiToken, logger);
```

### GET Requests

**Get all customers:**
```csharp
var customers = await apiClient.GetFromJsonAsync<List<Customer>>("/api/Customers");
```

**Get single customer:**
```csharp
var customer = await apiClient.GetFromJsonAsync<Customer>("/api/Customers/1");
```

### POST Requests

**Create customer:**
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
    },
    PhoneNumbers = new List<PhoneNumber>
    {
        new PhoneNumber
        {
            Type = "Mobile",
            Number = "+44 1234 567890"
        }
    }
};

var created = await apiClient.PostAsync<Customer, Customer>("/api/Customers", newCustomer);
```

**Create invoice:**
```csharp
var invoice = new Invoice
{
    CustomerId = 5,
    InvoiceNumber = "INV-NEW001",
    InvoiceDate = DateTime.Now,
    Amount = 500.00M
};

var created = await apiClient.PostAsync<Invoice, Invoice>("/api/Invoices", invoice);
```

### DELETE Requests

**Delete customer:**
```csharp
var deleted = await apiClient.DeleteAsync("/api/Customers/10");
```

**Delete invoice:**
```csharp
var deleted = await apiClient.DeleteAsync("/api/Invoices/INV-123456");
```

## Error Handling with Problem Details (NEW!)

```csharp
try
{
    await apiClient.DeleteAsync("/api/Customers/10");
}
catch (HttpRequestException ex)
{
    // Parse Problem Details
    var problemDetail = await ParseProblemDetails(ex.Message);
    
    // Structured logging
    LogProblemDetails(logger, problemDetail, "Delete Customer", "Failed");
    
    // Error recovery based on status
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

## Logging Examples

### Basic Logging
```csharp
logger.LogInformation("Customer created with ID: {CustomerId}", customerId);
logger.LogDebug("Processing customer: {CustomerId}", customerId);
logger.LogWarning("Customer not found: {CustomerId}", customerId);
logger.LogError(ex, "Failed to create customer");
```

### Problem Details Logging
```csharp
logger.LogError(
    "Problem Details - Operation: {Operation}, Status: {Status}, Title: {Title}, Detail: {Detail}",
    "Delete Customer", 404, "Not Found", "Customer does not exist");
```

## Phone Number Types

Valid phone types:
- `"Mobile"`
- `"Work"`
- `"DirectDial"`

## Invoice Number Format

- Must start with "INV"
- Examples: "INV-123", "INV-A1B2C3", "INV-2024-001"

## Common HTTP Status Codes

- **200 OK**: Request successful
- **201 Created**: Resource created successfully
- **400 Bad Request**: Validation error
- **401 Unauthorized**: Missing or invalid API key
- **404 Not Found**: Resource not found
- **409 Conflict**: Duplicate resource (e.g., invoice number)

## Configuration Files

**Main config:**
```
appsettings.json
```

**Environment-specific:**
```
appsettings.Development.json
appsettings.Production.json
```

**Precedence:**
1. appsettings.json
2. appsettings.{Environment}.json
3. Environment variables

## Common Configuration

### Change API URL
```json
{
  "ApiSettings": {
    "BaseUrl": "https://localhost:7136"
  }
}
```

### Change Log Level
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug"
    }
  }
}
```

### Disable Stack Traces in Console
```json
{
  "AppSettings": {
    "ShowFullStackTrace": false
  }
}
```

## Troubleshooting

### Connection Refused
- Start the API server first!

### 401 Unauthorized
- Check API token in appsettings.json

### SSL Certificate Error
- Run: `dotnet dev-certs https --trust`

### Configuration Not Loading
- Check appsettings.json is copied to output:
```xml
<None Update="appsettings.json">
  <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
</None>
```

### Logs Not Appearing
- Check log level in appsettings.json:
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug"  // Not "None"
    }
  }
}
```

### Port Already in Use
- Stop other applications or change port in server's launchSettings.json

## Example Scenarios in Program.cs

1. **Get All Customers** - Loops through all customers and displays details
2. **Create New Customer** - Creates customer with 3 invoices and 3 phone numbers
3. **Duplicate Customer** - Shows error handling for duplicate entries
4. **Delete Customer** - Deletes customer ID 10 with API key
5. **Delete Again** - Shows error handling for non-existing customer
6. **Add Invoice** - Adds invoice to existing customer ID 5
7. **Invalid Customer** - Shows error handling for non-existing customer ID 99

## New Features in v2.0

- **Configuration Management** - appsettings.json
- **Structured Logging** - ILogger<T>
- **Dependency Injection** - IServiceCollection
- **Problem Details** - Parsed and logged
- **Environment Support** - Dev/Prod configs
- **Log Levels** - Trace to Critical

## Log Output Example

```
info: Program[0]
      Configuration loaded:
info: Program[0]
      Base URL: https://localhost:7136
info: AssignmentModule6Client.ApiClient[0]
      ApiClient initialized with base URL: https://localhost:7136
dbug: AssignmentModule6Client.ApiClient[0]
      GET Request: https://localhost:7136/api/Customers
info: AssignmentModule6Client.ApiClient[0]
      Response: 200 OK
info: Program[0]
      Retrieved 50 customers from API
```

## Tips

- Use `ILogger<T>` for structured logging
- Configure log levels per component
- Use environment variables for secrets
- Parse Problem Details for error recovery
- Set DOTNET_ENVIRONMENT for environment-specific config
- Check logs for detailed error information
- Use Trace level for debugging HTTP requests

## Resources

- **Full Documentation**: `Docs/README.md`
- **Configuration Guide**: `Docs/CONFIGURATION_AND_LOGGING.md`
- **Update Summary**: `Docs/UPDATE_SUMMARY_V2.md`
- **API Documentation**: `https://localhost:7136/scalar/v1`
- **Server Documentation**: `AssignmentModule6Svr/Docs/README.md`

---

**Version**: 2.0.0  
**Last Updated**: 2025  
**Status**: Production Ready
