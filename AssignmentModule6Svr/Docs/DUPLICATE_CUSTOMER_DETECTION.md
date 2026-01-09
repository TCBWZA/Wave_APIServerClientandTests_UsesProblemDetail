# Duplicate Customer Detection - Feature Documentation

## Overview

The API server now includes **duplicate customer detection** functionality that prevents the creation of customers with duplicate names or email addresses. This ensures data integrity and prevents accidental duplicate entries.

## ?? Features

### 1. Create Customer with Duplicate Detection

When creating a new customer via `POST /api/Customers`, the system now checks:
- ? **Customer Name** (case-insensitive)
- ? **Customer Email** (case-insensitive)

### 2. Update Customer with Duplicate Detection

When updating a customer via `PUT /api/Customers/{id}`, the system checks for duplicates while **excluding the customer being updated**.

## ?? New AppDB Methods

### FindCustomerByName
```csharp
public static Customer? FindCustomerByName(string name)
```
Finds a customer by name (case-insensitive).

**Parameters:**
- `name` - The customer name to search for

**Returns:**
- `Customer` object if found, `null` otherwise

**Example:**
```csharp
var customer = AppDB.FindCustomerByName("TechCorp Solutions Ltd");
```

### FindCustomerByEmail
```csharp
public static Customer? FindCustomerByEmail(string email)
```
Finds a customer by email (case-insensitive).

**Parameters:**
- `email` - The customer email to search for

**Returns:**
- `Customer` object if found, `null` otherwise

**Example:**
```csharp
var customer = AppDB.FindCustomerByEmail("contact@techcorp.com");
```

### IsDuplicateCustomer
```csharp
public static bool IsDuplicateCustomer(string? name, string? email, long excludeId = 0)
```
Checks if a customer with the given name or email already exists.

**Parameters:**
- `name` - The customer name to check
- `email` - The customer email to check
- `excludeId` - Optional customer ID to exclude from the check (for updates)

**Returns:**
- `true` if a duplicate exists, `false` otherwise

**Example:**
```csharp
// Check for duplicate during creation
bool isDuplicate = AppDB.IsDuplicateCustomer("TechCorp", "tech@corp.com");

// Check for duplicate during update (exclude current customer)
bool isDuplicate = AppDB.IsDuplicateCustomer("TechCorp", "tech@corp.com", excludeId: 5);
```

## ?? API Responses

### 409 Conflict - Duplicate Customer

When a duplicate is detected, the API returns a `409 Conflict` status with detailed Problem Details:

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.8",
  "title": "Duplicate Customer",
  "status": 409,
  "detail": "A customer with the provided name and/or email already exists in the system. Customer with name 'TechCorp Solutions Ltd' already exists (ID: 51)",
  "instance": "POST /api/Customers",
  "traceId": "00-abc123...",
  "duplicateFields": ["name"],
  "existingCustomerId": 51,
  "providedName": "TechCorp Solutions Ltd",
  "providedEmail": "contact@techcorp.com",
  "suggestion": "Use PUT /api/customers/{id} to update the existing customer or provide a different name/email"
}
```

**Response Properties:**
- `duplicateFields` - Array of fields that caused the conflict ("name", "email", or both)
- `existingCustomerId` - The ID of the existing customer with duplicate data
- `providedName` - The name that was provided in the request
- `providedEmail` - The email that was provided in the request
- `suggestion` - Helpful guidance on how to resolve the conflict

## ?? Detection Rules

### Case-Insensitive Matching

All comparisons are case-insensitive:
- "TechCorp" = "techcorp" = "TECHCORP"
- "email@example.com" = "Email@Example.COM"

### Null/Empty Handling

- If both name and email are null/empty, no duplicate check is performed
- If only name is provided, only name is checked
- If only email is provided, only email is checked
- If both are provided, both are checked (either can trigger duplicate detection)

### Whitespace Handling

Leading and trailing whitespace is ignored:
- " TechCorp " = "TechCorp"

## ?? Examples

### Example 1: Create Customer with Duplicate Name

**Request:**
```http
POST /api/Customers
Content-Type: application/json

{
  "name": "Acme Corporation",
  "email": "new-email@acme.com"
}
```

**Response (if "Acme Corporation" already exists):**
```http
HTTP/1.1 409 Conflict
Content-Type: application/problem+json

{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.8",
  "title": "Duplicate Customer",
  "status": 409,
  "detail": "A customer with the provided name already exists in the system. Customer with name 'Acme Corporation' already exists (ID: 1)",
  "duplicateFields": ["name"],
  "existingCustomerId": 1,
  "suggestion": "Use PUT /api/customers/1 to update the existing customer or provide a different name"
}
```

### Example 2: Create Customer with Duplicate Email

**Request:**
```http
POST /api/Customers
Content-Type: application/json

{
  "name": "New Company",
  "email": "existing@example.com"
}
```

**Response (if "existing@example.com" already exists):**
```http
HTTP/1.1 409 Conflict
Content-Type: application/problem+json

{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.8",
  "title": "Duplicate Customer",
  "status": 409,
  "detail": "A customer with the provided email already exists in the system. Customer with email 'existing@example.com' already exists (ID: 5)",
  "duplicateFields": ["email"],
  "existingCustomerId": 5
}
```

### Example 3: Create Customer with Both Name and Email Duplicates

**Request:**
```http
POST /api/Customers
Content-Type: application/json

{
  "name": "Existing Company",
  "email": "existing@company.com"
}
```

**Response (if both exist):**
```http
HTTP/1.1 409 Conflict
Content-Type: application/problem+json

{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.8",
  "title": "Duplicate Customer",
  "status": 409,
  "detail": "A customer with the provided name and/or email already exists in the system. Customer with name 'Existing Company' already exists (ID: 10)",
  "duplicateFields": ["name", "email"],
  "existingCustomerId": 10
}
```

### Example 4: Update Customer (Allowed - Same Customer)

**Request:**
```http
PUT /api/Customers/5
Content-Type: application/json

{
  "name": "TechCorp Solutions Ltd",
  "email": "contact@techcorp.com"
}
```

**Response:**
```http
HTTP/1.1 200 OK

{
  "id": 5,
  "name": "TechCorp Solutions Ltd",
  "email": "contact@techcorp.com",
  "balance": 5150.49,
  "invoices": [...],
  "phoneNumbers": [...]
}
```

**Note:** This succeeds because customer 5 is allowed to keep its own name/email.

### Example 5: Update Customer (Conflict - Different Customer)

**Request:**
```http
PUT /api/Customers/5
Content-Type: application/json

{
  "name": "Acme Corporation",
  "email": "new@techcorp.com"
}
```

**Response (if "Acme Corporation" belongs to customer 1):**
```http
HTTP/1.1 409 Conflict
Content-Type: application/problem+json

{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.8",
  "title": "Duplicate Customer",
  "status": 409,
  "detail": "Cannot update customer 5. A different customer with the provided name already exists. Customer with name 'Acme Corporation' already exists (ID: 1)",
  "customerId": 5,
  "duplicateFields": ["name"],
  "conflictingCustomerId": 1,
  "suggestion": "Provide a different name that doesn't conflict with existing customers"
}
```

## ?? Client Usage Example

### C# Client with Error Handling

```csharp
try
{
    var newCustomer = new Customer
    {
        Name = "TechCorp Solutions Ltd",
        Email = "contact@techcorp.com",
        Invoices = new List<Invoice>
        {
            new Invoice
            {
                InvoiceNumber = "INV-001",
                InvoiceDate = DateTime.Now,
                Amount = 1000.00M
            }
        }
    };

    var created = await apiClient.PostAsync<Customer, Customer>("/api/Customers", newCustomer);
    Console.WriteLine($"Customer created: {created.Id}");
}
catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Conflict)
{
    // Parse Problem Details
    var problemDetail = await ParseProblemDetails(ex.Message);
    
    if (problemDetail != null)
    {
        Console.WriteLine($"Duplicate customer detected!");
        Console.WriteLine($"Duplicate fields: {string.Join(", ", problemDetail.DuplicateFields)}");
        Console.WriteLine($"Existing customer ID: {problemDetail.ExistingCustomerId}");
        Console.WriteLine($"Suggestion: {problemDetail.Suggestion}");
        
        // Recovery options:
        // 1. Use a different name/email
        // 2. Update the existing customer instead
        // 3. Prompt user to confirm merge
    }
}
```

## ?? Configuration

No configuration is required. Duplicate detection is always enabled.

## ?? Benefits

1. **Data Integrity**: Prevents duplicate customer records
2. **Better UX**: Clear error messages help users understand what went wrong
3. **Automatic Recovery**: Suggests alternative actions (use PUT instead)
4. **Detailed Information**: Provides existing customer ID for reference
5. **Case-Insensitive**: Works regardless of capitalization

## ?? Use Cases

### Use Case 1: Prevent Accidental Duplicates
A user tries to create a customer that already exists.

**Before:** Two customers with the same name exist  
**After:** 409 Conflict with helpful message

### Use Case 2: Import from External System
Importing customers from another system where duplicates exist.

**Solution:** Check for 409 Conflict and skip or merge duplicates

### Use Case 3: Update Customer Information
Updating a customer's name but accidentally using another customer's name.

**Before:** Silent success, two customers with same name  
**After:** 409 Conflict prevents the update

### Use Case 4: Case-Insensitive Search
Find a customer regardless of capitalization.

**Solution:** Use `FindCustomerByName` or `FindCustomerByEmail` methods

## ?? Future Enhancements

Potential improvements:
- **Fuzzy Matching**: Detect similar names (e.g., "TechCorp" vs "Tech Corp")
- **Merge Functionality**: API endpoint to merge duplicate customers
- **Configuration**: Optional flag to allow duplicates
- **Audit Log**: Track duplicate detection events
- **Batch Import**: Bulk import with duplicate handling options

## ?? Testing

### Unit Tests
```csharp
[Test]
public void IsDuplicateCustomer_SameName_ReturnsTrue()
{
    // Arrange
    var customer1 = new Customer { Id = 1, Name = "TechCorp", Email = "a@tech.com" };
    AppDB.AddCustomer(customer1);
    
    // Act
    var isDuplicate = AppDB.IsDuplicateCustomer("TechCorp", "b@tech.com");
    
    // Assert
    Assert.IsTrue(isDuplicate);
}

[Test]
public void IsDuplicateCustomer_SameEmail_ReturnsTrue()
{
    // Arrange
    var customer1 = new Customer { Id = 1, Name = "Company A", Email = "same@example.com" };
    AppDB.AddCustomer(customer1);
    
    // Act
    var isDuplicate = AppDB.IsDuplicateCustomer("Company B", "same@example.com");
    
    // Assert
    Assert.IsTrue(isDuplicate);
}

[Test]
public void IsDuplicateCustomer_ExcludeId_ReturnsFalse()
{
    // Arrange
    var customer1 = new Customer { Id = 1, Name = "TechCorp", Email = "tech@corp.com" };
    AppDB.AddCustomer(customer1);
    
    // Act
    var isDuplicate = AppDB.IsDuplicateCustomer("TechCorp", "tech@corp.com", excludeId: 1);
    
    // Assert
    Assert.IsFalse(isDuplicate); // Same customer, not a duplicate
}
```

## ?? Related Documentation

- **API Documentation**: `/scalar/v1`
- **Problem Details**: RFC 7807
- **HTTP Status Codes**: RFC 7231

---

**Version**: 2.1.0  
**Release Date**: 2025  
**Breaking Changes**: None  
**Backward Compatible**: Yes
