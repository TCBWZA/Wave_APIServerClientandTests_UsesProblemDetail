# Configuration and Logging Guide

## Overview

The AssignmentModule6Client now uses **Microsoft.Extensions.Configuration** and **Microsoft.Extensions.Logging** for enterprise-grade configuration management and structured logging.

## Configuration Files

### appsettings.json (Required)

Main configuration file for the application:

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
    "ApiToken": "",
    "TimeoutSeconds": 30
  },
  "AppSettings": {
    "EnableDetailedLogging": true,
    "MaxRetryAttempts": 3,
    "ShowFullStackTrace": false
  }
}
```

### appsettings.Development.json (Optional)

Override settings for development environment:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "AssignmentModule6Client": "Trace",
      "System.Net.Http": "Information"
    }
  },
  "AppSettings": {
    "EnableDetailedLogging": true,
    "ShowFullStackTrace": true
  }
}
```

### appsettings.Production.json (Optional)

Override settings for production environment:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      "AssignmentModule6Client": "Information",
      "System.Net.Http": "Error"
    }
  },
  "ApiSettings": {
    "BaseUrl": "https://api.production.com"
  },
  "AppSettings": {
    "EnableDetailedLogging": false,
    "ShowFullStackTrace": false
  }
}
```

## Configuration Sections

### 1. Logging Configuration

Controls log output levels for different components:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",              // Base level for all logs
      "AssignmentModule6Client": "Debug",    // Client-specific logs
      "AssignmentModule6Client.ApiClient": "Trace", // ApiClient logs
      "System.Net.Http": "Warning",          // HTTP client logs
      "Microsoft": "Warning"                  // Framework logs
    }
  }
}
```

**Log Levels (from most to least verbose):**
- **Trace** (6): Very detailed, including request/response bodies
- **Debug** (5): Debugging information, method calls
- **Information** (4): General flow of application
- **Warning** (3): Abnormal or unexpected events
- **Error** (2): Errors and exceptions
- **Critical** (1): Critical failures
- **None** (0): Disable logging

### 2. API Settings

Configuration for API connectivity:

```json
{
  "ApiSettings": {
    "BaseUrl": "https://localhost:7136",    // API server URL
    "ApiToken": "ThisIsANewToken",          // Authentication token
    "TimeoutSeconds": 30                     // Request timeout
  }
}
```

### 3. Application Settings

General application behavior:

```json
{
  "AppSettings": {
    "EnableDetailedLogging": true,          // Extra logging detail
    "MaxRetryAttempts": 3,                   // Future: retry logic
    "ShowFullStackTrace": false              // Console stack traces
  }
}
```

## Environment Variables

Configuration can be overridden via environment variables using double-underscore notation:

### Windows (CMD)
```cmd
set DOTNET_ENVIRONMENT=Development
set ApiSettings__BaseUrl=https://api.dev.com
set ApiSettings__ApiToken=DevToken123
set Logging__LogLevel__Default=Debug
```

### Windows (PowerShell)
```powershell
$env:DOTNET_ENVIRONMENT="Development"
$env:ApiSettings__BaseUrl="https://api.dev.com"
$env:ApiSettings__ApiToken="DevToken123"
$env:Logging__LogLevel__Default="Debug"
```

### Linux/Mac
```bash
export DOTNET_ENVIRONMENT=Development
export ApiSettings__BaseUrl=https://api.dev.com
export ApiSettings__ApiToken=DevToken123
export Logging__LogLevel__Default=Debug
```

### Environment Variable Precedence

Configuration sources are loaded in order (later overrides earlier):
1. appsettings.json
2. appsettings.{Environment}.json
3. Environment variables

## Logging Output

### Console Logging

Structured logs are written to console with this format:

```
[Timestamp] [LogLevel] [Category][EventId]
      Message with {StructuredData}
```

Example:
```
info: Program[0]
      Configuration loaded:
info: Program[0]
      Base URL: https://localhost:7136
dbug: AssignmentModule6Client.ApiClient[0]
      GET Request: https://localhost:7136/api/Customers
info: AssignmentModule6Client.ApiClient[0]
      Response: 200 OK
info: AssignmentModule6Client.ApiClient[0]
      Successfully deserialized response to type List`1
```

### Debug Logging

When debugging in Visual Studio, logs also appear in the Debug Output window.

### Log Categories

The application uses the following log categories:

| Category | Purpose | Typical Level |
|----------|---------|---------------|
| `Program` | Main program flow | Information |
| `AssignmentModule6Client.ApiClient` | API operations | Debug |
| `System.Net.Http.HttpClient` | HTTP requests | Warning |

## Structured Logging Examples

### Information Logging
```csharp
logger.LogInformation("Customer created successfully with ID: {CustomerId}", customerId);
```

### Debug Logging
```csharp
logger.LogDebug("Processing customer ID: {CustomerId}", customerId);
```

### Error Logging
```csharp
logger.LogError(ex, "Failed to add invoice to Customer ID {CustomerId}", customerId);
```

### Trace Logging
```csharp
logger.LogTrace("Request Body: {Json}", json);
```

## Problem Details Logging

When API errors occur, Problem Details are parsed and logged:

### Expected Errors (Information Level)
```csharp
logger.LogInformation(
    "Problem Details - Operation: {Operation}, Type: {Type}, Status: {Status}, Title: {Title}, Detail: {Detail}",
    "Duplicate Customer", "Conflict", 409, "Duplicate Entry", "Invoice number already exists");
```

### Failed Operations (Warning Level)
```csharp
logger.LogWarning(
    "Problem Details - Operation: {Operation}, Status: {Status}, Title: {Title}",
    "Delete Customer", 404, "Not Found");
```

### Critical Errors (Error Level)
```csharp
logger.LogError(
    "Problem Details - Operation: {Operation}, Status: {Status}, Detail: {Detail}",
    "Unexpected Error", 500, "Internal server error");
```

## Configuration in Code

### Reading Configuration

```csharp
// Get simple value
var baseUrl = configuration["ApiSettings:BaseUrl"];

// Get with default
var timeout = configuration.GetValue<int>("ApiSettings:TimeoutSeconds", 30);

// Get section
var apiSettings = configuration.GetSection("ApiSettings");
var baseUrl = apiSettings["BaseUrl"];
```

### Using IOptions Pattern (Advanced)

Create a settings class:
```csharp
public class ApiSettings
{
    public string BaseUrl { get; set; } = string.Empty;
    public string ApiToken { get; set; } = string.Empty;
    public int TimeoutSeconds { get; set; } = 30;
}
```

Register in DI:
```csharp
services.Configure<ApiSettings>(configuration.GetSection("ApiSettings"));
```

Inject in constructor:
```csharp
public MyService(IOptions<ApiSettings> settings)
{
    var apiSettings = settings.Value;
}
```

## Best Practices

### 1. Use Structured Logging
? **Good:**
```csharp
logger.LogInformation("Customer {CustomerId} created with balance {Balance}", id, balance);
```

? **Avoid:**
```csharp
logger.LogInformation($"Customer {id} created with balance {balance}");
```

### 2. Choose Appropriate Log Levels

| Scenario | Level | Example |
|----------|-------|---------|
| Normal operation | Information | "Customer created" |
| Debugging details | Debug | "Processing customer ID 5" |
| Expected errors | Warning | "Customer not found" |
| Unexpected errors | Error | "Database connection failed" |
| System failure | Critical | "Unable to start application" |

### 3. Don't Log Sensitive Data

? **Never log:**
- Passwords
- API tokens (full value)
- Credit card numbers
- Personal data (in production)

? **Safe logging:**
```csharp
logger.LogInformation("API Token loaded: {TokenPreview}", 
    string.IsNullOrEmpty(token) ? "NOT SET" : $"{token[..4]}***");
```

### 4. Use Log Scopes for Context

```csharp
using (logger.BeginScope("CustomerId: {CustomerId}", customerId))
{
    logger.LogInformation("Processing customer");
    logger.LogDebug("Loading invoices");
}
```

Output:
```
info: Program[0]
      => CustomerId: 5
      Processing customer
dbug: Program[0]
      => CustomerId: 5
      Loading invoices
```

## Troubleshooting

### Logs Not Appearing

**Check:** Log level configuration
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug"  // Make sure this is not "None"
    }
  }
}
```

### Too Many Logs

**Reduce:** Increase minimum log level
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      "System": "Error"
    }
  }
}
```

### Configuration Not Loading

**Check:** File properties in .csproj
```xml
<None Update="appsettings.json">
  <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
</None>
```

### Environment Not Recognized

**Set:** DOTNET_ENVIRONMENT variable

**PowerShell:**
```powershell
$env:DOTNET_ENVIRONMENT = "Development"
```

**Command Prompt:**
```cmd
set DOTNET_ENVIRONMENT=Development
```

## Production Recommendations

### 1. Use Separate Configuration Files

- `appsettings.json` - Defaults
- `appsettings.Production.json` - Production overrides
- Environment variables - Secrets

### 2. Minimize Production Logging

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      "AssignmentModule6Client": "Information"
    }
  }
}
```

### 3. Store Secrets Securely

Don't commit sensitive values to source control:

```json
{
  "ApiSettings": {
    "ApiToken": ""  // Leave empty, use environment variable
  }
}
```

Set at runtime:

**PowerShell:**
```powershell
$env:ApiSettings__ApiToken = "YOUR_PRODUCTION_TOKEN_FROM_SECRET_MANAGER"
```

**Command Prompt:**
```cmd
set ApiSettings__ApiToken=YOUR_PRODUCTION_TOKEN_FROM_SECRET_MANAGER
```

### 4. Enable Correlation IDs

For distributed systems:
```csharp
logger.LogInformation("Request {CorrelationId} completed", correlationId);
```

## Advanced: Custom Log Providers

Add additional log outputs:

```csharp
services.AddLogging(builder =>
{
    builder.AddConfiguration(configuration.GetSection("Logging"));
    builder.AddConsole();
    builder.AddDebug();
    builder.AddEventLog();  // Windows Event Log
    builder.AddApplicationInsights();  // Azure Application Insights
});
```

## Summary

- **Configuration**: Centralized in appsettings.json
- **Environment Support**: Dev/Prod overrides
- **Structured Logging**: Rich contextual data
- **Problem Details**: Parsed and logged for recovery
- **Best Practices**: Security, performance, maintainability

---

**Version**: 2.0.0  
**Last Updated**: 2025  
**Related**: README.md, QUICK_REFERENCE.md
