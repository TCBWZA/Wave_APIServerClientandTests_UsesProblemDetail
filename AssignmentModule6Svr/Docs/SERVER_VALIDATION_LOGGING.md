# Server-Side Validation Warning Logs

## Overview

The API server now outputs **detailed warning logs to the console** whenever validation failures occur, making it easy to debug and monitor validation issues in real-time.

## ?? Enhancement Details

### What Was Added

All validation failures in both **InvoicesController** and **CustomersController** now generate structured warning logs that appear in the console output with clear formatting.

### Log Format

```
========================================
VALIDATION FAILURE
========================================
Endpoint: POST /api/Invoices
TraceId: 00-abc123...
Invoice Number: ORDER-12345
Customer ID: 5
Total Fields with Errors: 1
----------------------------------------
Field: InvoiceNumber
  • InvoiceNumber must start with 'INV'.
========================================
```

## ?? Validation Scenarios Logged

### 1. **ModelState Validation Errors** (InvoicesController)

When model validation fails (e.g., invalid invoice number format):

```
========================================
VALIDATION FAILURE
========================================
Endpoint: POST /api/Invoices
TraceId: 00-a1b2c3d4e5f6...
Invoice Number: ORDER-12345
Customer ID: 5
Total Fields with Errors: 1
----------------------------------------
Field: InvoiceNumber
  • InvoiceNumber must start with 'INV'.
========================================
```

**Multiple Errors Example:**
```
========================================
VALIDATION FAILURE
========================================
Endpoint: POST /api/Invoices
TraceId: 00-a1b2c3d4e5f6...
Invoice Number: (null)
Customer ID: 0
Total Fields with Errors: 2
----------------------------------------
Field: CustomerId
  • Id must be greater than or equal zero.
Field: InvoiceNumber
  • InvoiceNumber is required.
  • InvoiceNumber must start with 'INV'.
========================================
```

### 2. **Invalid Customer ID** (InvoicesController)

When CustomerId is <= 0:

```
========================================
VALIDATION FAILURE - Invalid Customer ID
========================================
Endpoint: POST /api/Invoices
TraceId: 00-a1b2c3d4e5f6...
Customer ID: 0 (must be > 0)
Invoice Number: INV-123
========================================
```

### 3. **Customer Not Found** (InvoicesController)

When attempting to create an invoice for non-existent customer:

```
========================================
VALIDATION FAILURE - Customer Not Found
========================================
Endpoint: POST /api/Invoices
TraceId: 00-a1b2c3d4e5f6...
Customer ID: 99 (does not exist)
Invoice Number: INV-123
========================================
```

### 4. **Duplicate Invoice Number** (InvoicesController)

When invoice number already exists:

```
========================================
VALIDATION FAILURE - Duplicate Invoice Number
========================================
Endpoint: POST /api/Invoices
TraceId: 00-a1b2c3d4e5f6...
Invoice Number: INV-001 (already exists)
Existing Invoice ID: 15
Existing Customer ID: 3
Requested Customer ID: 5
========================================
```

### 5. **Duplicate Customer - Create** (CustomersController)

When creating a customer with duplicate name or email:

```
========================================
VALIDATION FAILURE - Duplicate Customer
========================================
Endpoint: POST /api/Customers
TraceId: 00-a1b2c3d4e5f6...
Duplicate Fields: name, email
Provided Name: TechCorp Solutions Ltd
Provided Email: contact@techcorp.com
Existing Customer ID: 51
----------------------------------------
  • Customer with name 'TechCorp Solutions Ltd' already exists (ID: 51)
  • Customer with email 'contact@techcorp.com' already exists (ID: 51)
========================================
```

### 6. **Duplicate Customer - Update** (CustomersController)

When updating a customer with another customer's name or email:

```
========================================
VALIDATION FAILURE - Duplicate Customer (Update)
========================================
Endpoint: PUT /api/Customers/5
TraceId: 00-a1b2c3d4e5f6...
Customer ID being updated: 5
Duplicate Fields: name
Provided Name: Acme Corporation
Provided Email: new@email.com
Conflicting Customer ID: 1
----------------------------------------
  • Customer with name 'Acme Corporation' already exists (ID: 1)
========================================
```

## ?? Key Features

### 1. **Structured Format**
- Clear section separators (========================================)
- Consistent field labels
- Hierarchical information display

### 2. **Comprehensive Details**
- HTTP Method and Path
- TraceId for correlation
- All relevant field values
- Error counts and messages
- Conflicting resource information

### 3. **Real-Time Console Output**
All warnings appear immediately in the server console when validation failures occur, making debugging trivial.

### 4. **Correlation Support**
Each log includes the **TraceId** which matches the traceId in the API response, allowing you to correlate client errors with server logs.

## ?? Implementation Details

### InvoicesController

**ModelState Validation:**
```csharp
_logger.LogWarning("========================================");
_logger.LogWarning("VALIDATION FAILURE");
_logger.LogWarning("========================================");
_logger.LogWarning("Endpoint: {Method} {Path}", HttpContext.Request.Method, HttpContext.Request.Path);
_logger.LogWarning("TraceId: {TraceId}", HttpContext.TraceIdentifier);
_logger.LogWarning("Invoice Number: {InvoiceNumber}", invoice.InvoiceNumber ?? "(null)");
_logger.LogWarning("Customer ID: {CustomerId}", invoice.CustomerId);
_logger.LogWarning("Total Fields with Errors: {ErrorCount}", errors.Count);
_logger.LogWarning("----------------------------------------");

foreach (var error in errors)
{
    _logger.LogWarning("Field: {FieldName}", error.Key);
    foreach (var message in error.Value)
    {
        _logger.LogWarning("  • {ErrorMessage}", message);
    }
}

_logger.LogWarning("========================================");
```

### CustomersController

**Duplicate Detection:**
```csharp
_logger.LogWarning("========================================");
_logger.LogWarning("VALIDATION FAILURE - Duplicate Customer");
_logger.LogWarning("========================================");
_logger.LogWarning("Endpoint: {Method} {Path}", HttpContext.Request.Method, HttpContext.Request.Path);
_logger.LogWarning("TraceId: {TraceId}", HttpContext.TraceIdentifier);
_logger.LogWarning("Duplicate Fields: {Fields}", string.Join(", ", duplicateFields));
_logger.LogWarning("Provided Name: {Name}", customer.Name ?? "(null)");
_logger.LogWarning("Provided Email: {Email}", customer.Email ?? "(null)");
_logger.LogWarning("Existing Customer ID: {ExistingId}", existingByName?.Id ?? existingByEmail?.Id ?? 0);
_logger.LogWarning("----------------------------------------");
foreach (var detail in conflictDetails)
{
    _logger.LogWarning("  • {Detail}", detail);
}
_logger.LogWarning("========================================");
```

## ?? Example Console Output

### Server Console (Running API)

```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: https://localhost:7136
info: Microsoft.Hosting.Lifetime[0]
      Application started. Press Ctrl+C to shut down.
warn: AssignmentModule6Svr.Controllers.InvoicesController[0]
      ========================================
warn: AssignmentModule6Svr.Controllers.InvoicesController[0]
      VALIDATION FAILURE
warn: AssignmentModule6Svr.Controllers.InvoicesController[0]
      ========================================
warn: AssignmentModule6Svr.Controllers.InvoicesController[0]
      Endpoint: POST /api/Invoices
warn: AssignmentModule6Svr.Controllers.InvoicesController[0]
      TraceId: 00-a1b2c3d4e5f6789...
warn: AssignmentModule6Svr.Controllers.InvoicesController[0]
      Invoice Number: ORDER-12345
warn: AssignmentModule6Svr.Controllers.InvoicesController[0]
      Customer ID: 5
warn: AssignmentModule6Svr.Controllers.InvoicesController[0]
      Total Fields with Errors: 1
warn: AssignmentModule6Svr.Controllers.InvoicesController[0]
      ----------------------------------------
warn: AssignmentModule6Svr.Controllers.InvoicesController[0]
      Field: InvoiceNumber
warn: AssignmentModule6Svr.Controllers.InvoicesController[0]
        • InvoiceNumber must start with 'INV'.
warn: AssignmentModule6Svr.Controllers.InvoicesController[0]
      ========================================
```

## ?? Benefits

1. **Immediate Visibility**: See validation failures as they happen
2. **Easy Debugging**: All relevant context in one place
3. **Correlation**: TraceId links server logs to client errors
4. **Monitoring**: Can be ingested by log aggregation systems
5. **Structured Data**: Queryable fields for analysis
6. **Production Ready**: Safe for production with appropriate log levels

## ?? Finding Logs

### Filter by Log Level
Since all validation failures use `LogWarning`, you can filter:

```bash
# View only warnings
dotnet run | grep "warn:"

# View warnings and errors
dotnet run | grep -E "(warn:|error:)"
```

### Search by TraceId
```bash
# Find all logs for a specific request
dotnet run | grep "00-a1b2c3d4e5f6"
```

### Search by Validation Type
```bash
# Find all validation failures
dotnet run | grep "VALIDATION FAILURE"

# Find specific types
dotnet run | grep "Duplicate Customer"
dotnet run | grep "Customer Not Found"
```

## ?? Log Level Configuration

The logs use **LogWarning** which is appropriate for:
- ? Production environments (informational, not errors)
- ? Development debugging
- ? Monitoring and alerting
- ? Audit trails

You can configure log levels in `appsettings.json`:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "AssignmentModule6Svr.Controllers": "Warning"  // Show all validation logs
    }
  }
}
```

To hide validation warnings in production:
```json
{
  "Logging": {
    "LogLevel": {
      "AssignmentModule6Svr.Controllers": "Error"  // Only show errors, not warnings
    }
  }
}
```

## ?? Summary

The server now provides:
- ? **Detailed validation logs** to console
- ? **Structured format** for easy reading
- ? **Multiple validation scenarios** covered
- ? **TraceId correlation** with client errors
- ? **Production-safe** warning level logging
- ? **Easy debugging** with all context included

All validation failures are now highly visible and easy to troubleshoot! ??

---

**Version**: 2.4.0  
**Release Date**: 2025  
**Breaking Changes**: None  
**Impact**: Server console output only
