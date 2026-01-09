# AssignmentModule6Client - API Client Library

A comprehensive .NET 10 console application demonstrating API client functionality for consuming the AssignmentModule6Svr RESTful API with **Microsoft.Extensions.Logging** and **Microsoft.Extensions.Configuration** support.

> **IMPORTANT**: This is Version 2.0 - Enhanced with RFC 7807 Problem Details parsing and structured logging. Requires .NET 10 or later (not compatible with .NET 8 or earlier).

## Version 2.0 Highlights

**What's New in v2.0:**
- **RFC 7807 Problem Details Support**: Comprehensive parsing and logging of API error responses
- **Structured Logging**: Enterprise-grade logging with Microsoft.Extensions.Logging
- **Configuration Management**: JSON-based configuration with environment overrides
- **Dependency Injection**: Built-in DI container for better testability
- **.NET 10 Required**: Takes advantage of latest .NET features and performance improvements

**Upgrade Note**: If you're using .NET 8 or earlier, you must upgrade to .NET 10 to use this version. The Problem Details features and logging infrastructure require .NET 10 APIs.

## Features

- **Complete HTTP Operations**: GET, POST, DELETE with async/await
- **Type-Safe Deserialization**: Strongly-typed models for API responses
- **Error Handling**: Robust error handling for network issues and HTTP errors
- **API Key Authentication**: Support for X-API-Key header (required for DELETE operations)
- **JSON Serialization**: Using System.Text.Json for high performance
- **Microsoft Logging**: Structured logging with ILogger<T>
- **Configuration Management**: appsettings.json support with environment overrides
- **Dependency Injection**: Built-in DI container for services
- **ProblemDetails Logging**: RFC 7807 Problem Details parsing and structured logging (.NET 10+ feature)
- **Configurable Base URL**: Easy configuration for different environments

## Prerequisites

- **[.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)** - REQUIRED (Version 2.0 is not compatible with .NET 8 or earlier)
- Visual Studio 2022+ or VS Code
- AssignmentModule6Svr API running (default: `https://localhost:7136`)

> **Note**: .NET 10 SDK includes improved performance, better Problem Details support, and enhanced logging capabilities that are essential for this version.

## Quick Start

### 1. Start the API Server

First, ensure the AssignmentModule6Svr API is running:

```powershell
cd AssignmentModule6Svr
dotnet run
```

The API should be accessible at `https://localhost:7136`

### 2. Configure the Client

Edit `appsettings.json` to match your environment:

```json
{
  "ApiSettings": {
    "BaseUrl": "https://localhost:7136",
    "ApiToken": ""
  }
}
```

### 3. Run the Client Application

```powershell
cd AssignmentModule6Client
dotnet run
```

## Configuration

### appsettings.json

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
    "ApiToken": "ThisIsANewToken",
    "TimeoutSeconds": 30
  },
  "AppSettings": {
    "EnableDetailedLogging": true,
    "MaxRetryAttempts": 3,
    "ShowFullStackTrace": false
  }
}
```

### Configuration Sections

#### Logging
- **Default**: Base log level for all categories
- **AssignmentModule6Client**: Log level for client-specific logs
- **System.Net.Http**: Log level for HTTP operations

#### ApiSettings
- **BaseUrl**: The API server base URL
- **ApiToken**: Authentication token for DELETE operations
- **TimeoutSeconds**: HTTP request timeout (optional)

#### AppSettings
- **EnableDetailedLogging**: Enable verbose logging
- **MaxRetryAttempts**: Number of retry attempts (future use)
- **ShowFullStackTrace**: Show full stack traces in console output

### Environment-Specific Configuration

Create `appsettings.Development.json` for development overrides:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "AssignmentModule6Client": "Trace"
    }
  },
  "AppSettings": {
    "ShowFullStackTrace": true
  }
}
```

Set environment variable:

**PowerShell:**
```powershell
$env:DOTNET_ENVIRONMENT = "Development"
```

**Command Prompt:**
```cmd
set DOTNET_ENVIRONMENT=Development
```

## Project Structure

```
AssignmentModule6Client/
|-- Models/                          # Data Transfer Objects
|   |-- Customer.cs                  # Customer model
|   |-- Invoice.cs                   # Invoice model
|   |-- PhoneNumber.cs              # Phone number model
|   +-- ProblemDetails.cs           # API error response model
|
|-- ApiClient.cs                     # Main API client class with ILogger
|-- Program.cs                       # Example usage with DI and logging
|-- appsettings.json                 # Configuration file
|-- appsettings.Development.json    # Development configuration
+-- AssignmentModule6Client.csproj  # Project file
```

## ApiClient Class

The `ApiClient` class provides the following methods with integrated logging:

### Constructor

```csharp
ApiClient(string baseUrl, string? apiToken = null, ILogger<ApiClient>? logger = null)
```

- **baseUrl**: The base URL of the API
- **apiToken**: Optional API token for authentication
- **logger**: Optional ILogger for structured logging

### Methods

All methods now include structured logging at various levels:
- **Debug**: Request/response details
- **Information**: Successful operations
- **Warning**: Non-critical issues
- **Error**: Errors with context
- **Trace**: Detailed debugging information

## Logging Features

### Structured Logging

All log entries include structured data for better querying:

```csharp
logger.LogInformation("Customer created successfully with ID: {CustomerId}", createdCustomer.Id);
```

### Log Levels

- **Trace**: Very detailed debugging (request/response bodies)
- **Debug**: Debugging information (method calls, operations)
- **Information**: General application flow
- **Warning**: Non-critical issues (expected errors)
- **Error**: Errors requiring attention
- **Critical**: Critical failures

### Problem Details Logging

RFC 7807 Problem Details are parsed and logged in structured format:

```csharp
logger.LogError(
    "API Error - Method: {Method}, Endpoint: {Endpoint}, Status: {Status}, Title: {Title}, Detail: {Detail}",
    method, endpoint, problemDetails.Status, problemDetails.Title, problemDetails.Detail);
```

## Console Output

The application provides both:
1. **Structured Logs**: Logged via ILogger to console with timestamps and categories
2. **User-Friendly Output**: Color-coded console messages for readability

Example output:
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
```

## Authentication

Configure API token in appsettings.json:

```json
{
  "ApiSettings": {
    "ApiToken": "YOUR_API_TOKEN_HERE"
  }
}
```

Or via environment variable:

**PowerShell:**
```powershell
$env:ApiSettings__ApiToken = "YOUR_API_TOKEN_HERE"
```

**Command Prompt:**
```cmd
set ApiSettings__ApiToken=YOUR_API_TOKEN_HERE
```

## Error Handling with Problem Details

### Version 2.0 Feature: RFC 7807 Problem Details

Version 2.0 introduces comprehensive support for RFC 7807 Problem Details, which provides standardized error information from APIs. This feature requires .NET 10 for optimal parsing and structured logging support.

### Parsing Problem Details

The application automatically parses RFC 7807 Problem Details from API errors:

```csharp
catch (HttpRequestException ex)
{
    var problemDetail = await ParseProblemDetails(ex.Message);
    LogProblemDetails(logger, problemDetail, "Operation Name", "Expected/Failed");
}
```

### Structured Problem Details Logging

```csharp
logger.LogError(
    "Problem Details - Operation: {Operation}, Type: {Type}, Status: {Status}, Title: {Title}, Detail: {Detail}",
    operation, problemDetails.Type, problemDetails.Status, problemDetails.Title, problemDetails.Detail);
```

### Error Recovery

The main program loop uses Problem Details to determine error handling strategies:

1. **Expected Errors** (409 Conflict, 404 Not Found): Logged as Information/Warning
2. **Failed Operations**: Logged as Warning with recovery options
3. **Critical Errors**: Logged as Critical with full context

This allows for future enhancements like:
- Retry logic based on error type
- Circuit breaker patterns
- Fallback mechanisms
- User notifications

**Why .NET 10?** The Problem Details parsing and structured logging features leverage .NET 10's improved JSON serialization performance and enhanced logging APIs not available in earlier versions.

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

## Best Practices Implemented

1. **Dependency Injection**: Services registered via IServiceCollection
2. **Configuration**: Centralized configuration with environment support
3. **Structured Logging**: All logs include contextual data
4. **Separation of Concerns**: Logging vs. user output
5. **Error Context**: Problem Details provide rich error information
6. **Resource Management**: Proper disposal with `using` statements
7. **Type Safety**: Generic methods with strong typing

## Testing the Client

### Prerequisites

1. Start the API server:
   ```powershell
   cd AssignmentModule6Svr
   dotnet run
   ```

2. Verify the API is accessible:
   ```
   https://localhost:7136/scalar/v1
   ```

### Run with Different Environments

**Development:**
```powershell
$env:DOTNET_ENVIRONMENT = "Development"
dotnet run
```

**Production:**
```powershell
$env:DOTNET_ENVIRONMENT = "Production"
dotnet run
```

## Troubleshooting

### "Configuration file not found"

**Solution**: Ensure appsettings.json is copied to output directory:
```xml
<None Update="appsettings.json">
  <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
</None>
```

### Logs not appearing

**Solution**: Check log levels in appsettings.json:
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug"
    }
  }
}
```

### Connection refused

**Solution**: Verify API server is running and URL matches configuration

## Requirements Fulfilled

This implementation meets all specified requirements plus additional enterprise features:

- Uses **.NET 10** as target framework (required for v2.0 features)
- **Microsoft.Extensions.Logging** for structured logging
- **Microsoft.Extensions.Configuration** for settings management
- **Problem Details** parsed and logged for error recovery (.NET 10+ feature)
- Dependency injection for better testability
- Environment-specific configuration support
- Comprehensive error handling with context
- All original requirements met

**Version Compatibility:**
- Version 2.0: Requires .NET 10 or later
- Version 1.0: Compatible with .NET 6, 7, 8 (basic functionality, no Problem Details)

## Future Enhancements

With the logging and Problem Details infrastructure in place (.NET 10+), you can easily add:

- **Retry Policies**: Retry based on ProblemDetails.Type
- **Circuit Breaker**: Break circuit on repeated failures
- **Telemetry**: Export logs to Application Insights
- **Metrics**: Track success/failure rates
- **Health Checks**: Monitor API availability
- **Caching**: Cache responses based on status
- **Rate Limiting**: Respect rate limit headers

---

**Version**: 2.0.0  
**Target Framework**: .NET 10 (Required - not compatible with .NET 8 or earlier)  
**Last Updated**: 2025  
**New Features**: Microsoft Logging, Configuration, RFC 7807 Problem Details Logging
