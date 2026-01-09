# ValidationProblemDetails Enhancement

## Overview

The API now returns detailed **ValidationProblemDetails** responses that include all ModelState validation errors in a structured format, making it easier for clients to identify and handle specific validation issues.

## ?? Changes Implemented

### 1. **Server-Side (InvoicesController.cs)**

Enhanced the ModelState validation error response:

```csharp
if (!ModelState.IsValid)
{
    var errors = ModelState
        .Where(x => x.Value?.Errors.Count > 0)
        .ToDictionary(
            kvp => kvp.Key,
            kvp => kvp.Value?.Errors.Select(e => e.ErrorMessage).ToArray() ?? Array.Empty<string>()
        );
    
    var problemDetails = new ValidationProblemDetails(errors)
    {
        Status = StatusCodes.Status400BadRequest,
        Title = "Validation Failed",
        Detail = "One or more validation errors occurred. Please review the 'errors' property for details.",
        Instance = $"{HttpContext.Request.Method} {HttpContext.Request.Path}"
    };
    problemDetails.Extensions["traceId"] = HttpContext.TraceIdentifier;
    problemDetails.Extensions["errorCount"] = errors.Count;
    problemDetails.Extensions["invoiceNumber"] = invoice.InvoiceNumber;
    problemDetails.Extensions["customerId"] = invoice.CustomerId;
    
    return BadRequest(problemDetails);
}
```

**Key Changes:**
- ? All ModelState errors are collected into a dictionary
- ? Passed to `ValidationProblemDetails(errors)` constructor
- ? Additional context included in extensions (invoiceNumber, customerId, errorCount)
- ? Logged with structured data for monitoring

### 2. **Client-Side (ProblemDetails.cs)**

Added `Errors` property to the ProblemDetails model:

```csharp
public class ProblemDetails
{
    public string? Type { get; set; }
    public string? Title { get; set; }
    public int Status { get; set; }
    public string? Detail { get; set; }
    public string? Instance { get; set; }
    public string? TraceId { get; set; }
    
    /// <summary>
    /// Dictionary of validation errors (field name -> error messages)
    /// </summary>
    public Dictionary<string, string[]>? Errors { get; set; }
}
```

### 3. **Client-Side (Program.cs)**

#### Enhanced LogProblemDetails Helper

```csharp
static void LogProblemDetails(ILogger logger, ProblemDetails? problemDetails, string operation, string severity)
{
    // ... existing logging ...
    
    // Log validation errors if present
    if (problemDetails.Errors != null && problemDetails.Errors.Any())
    {
        logger.LogWarning("Validation Errors:");
        foreach (var error in problemDetails.Errors)
        {
            logger.LogWarning("  Field: {Field}, Errors: {Errors}", 
                error.Key, string.Join(", ", error.Value));
        }
    }
}
```

#### Enhanced Examples 7a and 7b

Both examples now display validation errors:

```csharp
// Display validation errors if present
if (problemDetail?.Errors != null && problemDetail.Errors.Any())
{
    Console.WriteLine($"\n  Validation Errors:");
    foreach (var error in problemDetail.Errors)
    {
        Console.WriteLine($"    • {error.Key}: {string.Join(", ", error.Value)}");
    }
}
```

## ?? Example Responses

### Example 1: Invalid Invoice Number Format

**Request:**
```http
POST /api/Invoices
Content-Type: application/json

{
  "customerId": 5,
  "invoiceNumber": "ORDER-12345",
  "invoiceDate": "2025-01-15T10:00:00Z",
  "amount": 750.00
}
```

**Response (400 Bad Request):**
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "Validation Failed",
  "status": 400,
  "detail": "One or more validation errors occurred. Please review the 'errors' property for details.",
  "instance": "POST /api/Invoices",
  "traceId": "00-abc123...",
  "errorCount": 1,
  "invoiceNumber": "ORDER-12345",
  "customerId": 5,
  "errors": {
    "InvoiceNumber": [
      "InvoiceNumber must start with 'INV'."
    ]
  }
}
```

### Example 2: Empty Invoice Number

**Request:**
```http
POST /api/Invoices
Content-Type: application/json

{
  "customerId": 5,
  "invoiceNumber": "",
  "invoiceDate": "2025-01-15T10:00:00Z",
  "amount": 500.00
}
```

**Response (400 Bad Request):**
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "Validation Failed",
  "status": 400,
  "detail": "One or more validation errors occurred. Please review the 'errors' property for details.",
  "instance": "POST /api/Invoices",
  "traceId": "00-def456...",
  "errorCount": 2,
  "invoiceNumber": "",
  "customerId": 5,
  "errors": {
    "InvoiceNumber": [
      "InvoiceNumber is required.",
      "InvoiceNumber must start with 'INV'."
    ]
  }
}
```

### Example 3: Multiple Validation Errors

**Request:**
```http
POST /api/Invoices
Content-Type: application/json

{
  "customerId": 0,
  "invoiceNumber": "",
  "invoiceDate": "2025-01-15T10:00:00Z",
  "amount": 500.00
}
```

**Response (400 Bad Request):**
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "Validation Failed",
  "status": 400,
  "detail": "One or more validation errors occurred. Please review the 'errors' property for details.",
  "instance": "POST /api/Invoices",
  "traceId": "00-ghi789...",
  "errorCount": 3,
  "invoiceNumber": "",
  "customerId": 0,
  "errors": {
    "CustomerId": [
      "Id must be greater than or equal zero."
    ],
    "InvoiceNumber": [
      "InvoiceNumber is required.",
      "InvoiceNumber must start with 'INV'."
    ]
  }
}
```

## ??? Console Output

### Example 7a Output (Invalid Format)

```
--- Example 7a: Attempt to Add Invoice with Invalid Invoice Number to Customer ID 5 ---

? Expected error caught!
  Status: BadRequest
  Message: POST /api/Invoices failed: Validation Failed - One or more validation...
  Reason: Invoice number must start with 'INV' (400 Bad Request - Validation Error)

  Validation Errors:
    • InvoiceNumber: InvoiceNumber must start with 'INV'.
```

### Example 7b Output (Empty Number)

```
--- Example 7b: Attempt to Add Invoice with Empty Invoice Number to Customer ID 5 ---

? Expected error caught!
  Status: BadRequest
  Message: POST /api/Invoices failed: Validation Failed - One or more validation...
  Reason: Invoice number is required (400 Bad Request - Validation Error)

  Validation Errors:
    • InvoiceNumber: InvoiceNumber is required., InvoiceNumber must start with 'INV'.
```

## ?? Structured Logging Output

```
info: Program[0]
      Starting Example 7a: Attempt to Add Invoice with Invalid Invoice Number
warn: AssignmentModule6Svr.Controllers.InvoicesController[0]
      CreateInvoice: Model state is invalid. Errors: {"InvoiceNumber":["InvoiceNumber must start with 'INV'."]}
info: Program[0]
      Expected error caught: POST /api/Invoices failed: Validation Failed - ..., StatusCode: BadRequest
info: Program[0]
      Problem Details - Operation: Add Invoice with Invalid Format, Status: 400, Title: Validation Failed
warn: Program[0]
      Validation Errors:
warn: Program[0]
      Field: InvoiceNumber, Errors: InvoiceNumber must start with 'INV'.
```

## ?? Benefits

1. **Precise Error Identification**: Clients can identify exactly which fields failed validation
2. **Multiple Errors**: All validation errors returned in a single response
3. **Structured Format**: Errors are in a consistent dictionary format
4. **Better UX**: Users see specific field-level error messages
5. **Easier Debugging**: Structured logs make troubleshooting simple
6. **Programmatic Handling**: Clients can loop through errors and handle each field

## ?? Client Usage

### C# Example

```csharp
try
{
    var invoice = new Invoice
    {
        CustomerId = 5,
        InvoiceNumber = "ORDER-123",  // Invalid: doesn't start with "INV"
        InvoiceDate = DateTime.Now,
        Amount = 100.00M
    };
    
    await apiClient.PostAsync<Invoice, Invoice>("/api/Invoices", invoice);
}
catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.BadRequest)
{
    var problemDetail = await ParseProblemDetails(ex.Message);
    
    if (problemDetail?.Errors != null)
    {
        foreach (var error in problemDetail.Errors)
        {
            string fieldName = error.Key;
            string[] errorMessages = error.Value;
            
            Console.WriteLine($"Field '{fieldName}' has errors:");
            foreach (var message in errorMessages)
            {
                Console.WriteLine($"  - {message}");
            }
        }
    }
}
```

### JavaScript Example

```javascript
try {
    const response = await fetch('https://api.example.com/api/Invoices', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
            customerId: 5,
            invoiceNumber: 'ORDER-123',  // Invalid: doesn't start with "INV"
            invoiceDate: '2025-01-15T10:00:00Z',
            amount: 100.00
        })
    });
    
    if (!response.ok) {
        const problemDetails = await response.json();
        
        if (problemDetails.errors) {
            Object.keys(problemDetails.errors).forEach(field => {
                const errors = problemDetails.errors[field];
                console.log(`Field '${field}' has errors:`, errors);
            });
        }
    }
} catch (error) {
    console.error('Network error:', error);
}
```

## ?? Related Documentation

- **RFC 7807**: Problem Details for HTTP APIs
- **ASP.NET Core Validation**: Model State Validation
- **ValidationProblemDetails**: Microsoft.AspNetCore.Mvc.ValidationProblemDetails

## ? Validation Rules

| Field | Rule | Error Message |
|-------|------|---------------|
| **InvoiceNumber** | Required | "InvoiceNumber is required." |
| **InvoiceNumber** | Starts with "INV" | "InvoiceNumber must start with 'INV'." |
| **CustomerId** | > 0 | "Id must be greater than or equal zero." |
| **Amount** | Decimal | (Type validation) |
| **InvoiceDate** | DateTime | (Type validation) |

## ?? Summary

The enhancement provides:
- ? **Server**: Returns all validation errors in structured format
- ? **Client**: Parses and displays validation errors clearly
- ? **Logging**: Structured logging of validation failures
- ? **User Experience**: Clear, actionable error messages
- ? **Developer Experience**: Easy to debug and handle programmatically

---

**Version**: 2.2.0  
**Release Date**: 2025  
**Breaking Changes**: None (additive change)  
**Backward Compatible**: Yes
