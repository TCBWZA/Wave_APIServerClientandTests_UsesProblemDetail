# Problem Details (RFC 7807) Implementation

All API endpoints now return standardized Problem Details responses for non-200 status codes, including trace information for debugging and diagnostics.

## Overview

The API follows [RFC 7807](https://tools.ietf.org/html/rfc7807) to provide consistent error responses across all endpoints.

### Standard Problem Details Structure

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.4",
  "title": "Resource Not Found",
  "status": 404,
  "detail": "Customer with ID 999 not found",
  "instance": "GET /api/customers/999",
  "traceId": "0HN1QJVJ8KDLM:00000001",
  "customerId": 999
}
```

### Fields Included

- **type**: URI reference identifying the problem type (auto-generated)
- **title**: Short, human-readable summary
- **status**: HTTP status code
- **detail**: Human-readable explanation specific to this occurrence
- **instance**: HTTP method and path that generated the problem
- **traceId**: Unique identifier for tracing the request through logs
- **Additional context**: Entity-specific fields (e.g., customerId, invoiceNumber, phoneNumberId)

## Example Responses by Status Code

### 400 Bad Request

#### Missing Required Data
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "Invalid Customer Data",
  "status": 400,
  "detail": "Customer data is required",
  "instance": "POST /api/customers",
  "traceId": "0HN1QJVJ8KDLM:00000002"
}
```

#### Invalid ID
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "Invalid Customer ID",
  "status": 400,
  "detail": "Customer ID must be greater than 0",
  "instance": "POST /api/customers",
  "traceId": "0HN1QJVJ8KDLM:00000003",
  "customerId": -1
}
```

#### Validation Errors
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "Validation Failed",
  "status": 400,
  "detail": "One or more validation errors occurred",
  "instance": "POST /api/invoices",
  "traceId": "0HN1QJVJ8KDLM:00000004",
  "errors": {
    "InvoiceNumber": [
      "InvoiceNumber must start with 'INV'."
    ],
    "Amount": [
      "Amount must be a positive value."
    ]
  }
}
```

### 401 Unauthorized

#### Missing API Key
```json
{
  "type": "https://tools.ietf.org/html/rfc7235#section-3.1",
  "title": "API Key Missing",
  "status": 401,
  "detail": "API Key is missing. Please provide a valid API key in the X-API-Key header.",
  "instance": "DELETE /api/customers/1",
  "traceId": "0HN1QJVJ8KDLM:00000005",
  "requiredHeader": "X-API-Key"
}
```

#### Invalid API Key
```json
{
  "type": "https://tools.ietf.org/html/rfc7235#section-3.1",
  "title": "Invalid API Key",
  "status": 401,
  "detail": "Invalid API Key. Access denied.",
  "instance": "DELETE /api/customers/1",
  "traceId": "0HN1QJVJ8KDLM:00000006",
  "requiredHeader": "X-API-Key"
}
```

### 404 Not Found

#### Customer Not Found
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.4",
  "title": "Customer Not Found",
  "status": 404,
  "detail": "Customer with ID 999 not found",
  "instance": "GET /api/customers/999",
  "traceId": "0HN1QJVJ8KDLM:00000007",
  "customerId": 999
}
```

#### Invoice Not Found
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.4",
  "title": "Invoice Not Found",
  "status": 404,
  "detail": "Invoice INV-12345 for customer 1 not found",
  "instance": "GET /api/invoices/1/INV-12345",
  "traceId": "0HN1QJVJ8KDLM:00000008",
  "customerId": 1,
  "invoiceNumber": "INV-12345"
}
```

#### Phone Number Not Found
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.4",
  "title": "Phone Number Not Found",
  "status": 404,
  "detail": "Phone number with ID 999 not found",
  "instance": "GET /api/phonenumbers/999",
  "traceId": "0HN1QJVJ8KDLM:00000009",
  "phoneNumberId": 999
}
```

### 409 Conflict

#### Duplicate Customer
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.8",
  "title": "Customer Already Exists",
  "status": 409,
  "detail": "Customer with ID 1 already exists",
  "instance": "POST /api/customers",
  "traceId": "0HN1QJVJ8KDLM:00000010",
  "customerId": 1
}
```

#### Duplicate Invoice
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.8",
  "title": "Invoice Already Exists",
  "status": 409,
  "detail": "Invoice INV-12345 for customer 1 already exists",
  "instance": "POST /api/invoices",
  "traceId": "0HN1QJVJ8KDLM:00000011",
  "invoiceNumber": "INV-12345",
  "customerId": 1
}
```

#### Duplicate Phone Number
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.8",
  "title": "Phone Number Already Exists",
  "status": 409,
  "detail": "Phone number with ID 1 already exists",
  "instance": "POST /api/phonenumbers",
  "traceId": "0HN1QJVJ8KDLM:00000012",
  "phoneNumberId": 1
}
```

## Benefits of Problem Details

### 1. **Consistency**
All error responses follow the same structure, making it easier for clients to handle errors uniformly.

### 2. **Traceability**
The `traceId` field allows you to correlate API responses with server logs for debugging.

### 3. **Context**
Entity-specific fields provide immediate context about what entity caused the error.

### 4. **Machine-Readable**
The structured format allows clients to programmatically parse and handle specific error types.

### 5. **Human-Friendly**
Clear titles and detailed descriptions help developers understand what went wrong.

## Using Trace IDs for Debugging

When an error occurs:

1. **Client receives** the error response with a `traceId`
2. **Client logs or displays** the traceId to the user
3. **Support/Developers search** server logs using the traceId
4. **Find exact request** that caused the error

### Example Log Correlation

**API Response:**
```json
{
  "title": "Customer Not Found",
  "status": 404,
  "traceId": "0HN1QJVJ8KDLM:00000007",
  "customerId": 999
}
```

**Server Log:**
```
[14:23:45 DBG] [0HN1QJVJ8KDLM:00000007] GetCustomer: Retrieving customer with ID 999
[14:23:45 DBG] [0HN1QJVJ8KDLM:00000007] GetCustomer: Customer with ID 999 not found
```

## Client-Side Error Handling Example

### C# HttpClient
```csharp
try
{
    var response = await httpClient.GetAsync($"/api/customers/{id}");
    
    if (!response.IsSuccessStatusCode)
    {
        var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        
        Console.WriteLine($"Error: {problemDetails.Title}");
        Console.WriteLine($"Detail: {problemDetails.Detail}");
        Console.WriteLine($"Trace ID: {problemDetails.Extensions["traceId"]}");
        
        if (problemDetails.Extensions.ContainsKey("customerId"))
        {
            Console.WriteLine($"Customer ID: {problemDetails.Extensions["customerId"]}");
        }
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Request failed: {ex.Message}");
}
```

### JavaScript Fetch
```javascript
try {
    const response = await fetch(`/api/customers/${id}`);
    
    if (!response.ok) {
        const problemDetails = await response.json();
        
        console.error('Error:', problemDetails.title);
        console.error('Detail:', problemDetails.detail);
        console.error('Trace ID:', problemDetails.traceId);
        
        if (problemDetails.customerId) {
            console.error('Customer ID:', problemDetails.customerId);
        }
    }
} catch (error) {
    console.error('Request failed:', error.message);
}
```

## Endpoints Affected

All endpoints that return non-200 status codes now use Problem Details:

### Customers Controller
- `GET /api/customers/{id}` - 404
- `POST /api/customers` - 400, 409
- `PUT /api/customers/{id}` - 400, 404
- `DELETE /api/customers/{id}` - 401, 404
- `GET /api/customers/{id}/invoices` - 404

### Invoices Controller
- `GET /api/invoices/{customerId}/{invoiceNumber}` - 404
- `GET /api/invoices/customer/{customerId}` - 404
- `POST /api/invoices` - 400 (with validation), 404, 409
- `DELETE /api/invoices/{invoiceNumber}` - 400, 401, 404
- `DELETE /api/invoices/customer/{customerId}` - 400, 401, 404

### Phone Numbers Controller
- `GET /api/phonenumbers/{id}` - 404
- `GET /api/phonenumbers/customer/{customerId}` - 404
- `POST /api/phonenumbers` - 400 (with validation), 404, 409
- `DELETE /api/phonenumbers/{id}` - 400, 401, 404
- `DELETE /api/phonenumbers/customer/{customerId}` - 400, 401, 404

## Configuration

Problem Details is configured in `Program.cs`:

```csharp
builder.Services.AddProblemDetails(options =>
{
    options.CustomizeProblemDetails = context =>
    {
        context.ProblemDetails.Instance = $"{context.HttpContext.Request.Method} {context.HttpContext.Request.Path}";
        context.ProblemDetails.Extensions["traceId"] = context.HttpContext.TraceIdentifier;
        context.ProblemDetails.Extensions["timestamp"] = DateTime.UtcNow.ToString("o");
    };
});
```

This automatically adds:
- `instance`: The HTTP method and path
- `traceId`: The unique request trace identifier
- `timestamp`: When the error occurred (UTC)

## Testing Problem Details

### Using Swagger UI
1. Navigate to `http://localhost:5000/swagger`
2. Try endpoints with invalid data
3. Observe the structured error responses

### Using cURL
```bash
# Get non-existent customer
curl -i http://localhost:5000/api/customers/999

# Create duplicate customer
curl -i -X POST http://localhost:5000/api/customers \
  -H "Content-Type: application/json" \
  -d '{"id":1,"name":"Test"}'

# Delete without API key
curl -i -X DELETE http://localhost:5000/api/customers/1
```

### Using Postman
1. Import the API from Swagger
2. Create test cases for error scenarios
3. Verify Problem Details structure in responses

---

**Standard**: RFC 7807  
**Implementation Date**: January 2025  
**Version**: 1.0
