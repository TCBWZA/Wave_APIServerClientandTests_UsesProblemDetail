# Duplicate Customer Detection - Quick Summary

## ? What Was Implemented

The API server now **prevents duplicate customers** based on name or email addresses.

## ?? Key Features

### 1. New AppDB Methods

```csharp
// Find customer by name (case-insensitive)
Customer? FindCustomerByName(string name)

// Find customer by email (case-insensitive)
Customer? FindCustomerByEmail(string email)

// Check if duplicate exists (optionally exclude a customer ID)
bool IsDuplicateCustomer(string? name, string? email, long excludeId = 0)
```

### 2. Updated API Endpoints

**POST /api/Customers**
- ? Checks for duplicate name **OR** duplicate email
- ? Returns `409 Conflict` if duplicate found
- ? Provides detailed Problem Details with existing customer ID

**PUT /api/Customers/{id}**
- ? Checks for duplicates excluding the customer being updated
- ? Allows customer to keep their own name/email
- ? Returns `409 Conflict` if trying to use another customer's name/email

### 3. Updated Client Example

**Example 3** now demonstrates duplicate customer detection by name/email instead of invoice numbers.

## ?? Response Example

When duplicate detected:

```json
{
  "status": 409,
  "title": "Duplicate Customer",
  "detail": "A customer with the provided name already exists...",
  "duplicateFields": ["name"],
  "existingCustomerId": 51,
  "providedName": "TechCorp Solutions Ltd",
  "providedEmail": "contact@techcorp.com",
  "suggestion": "Use PUT /api/customers/51 to update or provide different name/email"
}
```

## ?? Detection Rules

- ? **Case-insensitive** matching
- ? **Whitespace** trimmed automatically
- ? Checks **name OR email** (either triggers duplicate)
- ? **Update operations** exclude the customer being updated
- ? **Null/empty** values are ignored

## ?? Usage Examples

### Create Customer (will fail if duplicate)
```csharp
var customer = new Customer
{
    Name = "TechCorp Solutions Ltd",  // Must be unique
    Email = "contact@techcorp.com"     // Must be unique
};

try
{
    var created = await apiClient.PostAsync<Customer, Customer>("/api/Customers", customer);
}
catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.Conflict)
{
    // Duplicate detected!
    var problemDetails = await ParseProblemDetails(ex.Message);
    Console.WriteLine($"Duplicate fields: {string.Join(", ", problemDetails.DuplicateFields)}");
    Console.WriteLine($"Existing customer ID: {problemDetails.ExistingCustomerId}");
}
```

### Update Customer (excludes self)
```csharp
// This is OK - customer 5 can keep its own name/email
await apiClient.PutAsync("/api/Customers/5", existingCustomer);

// This fails - trying to use customer 1's name
existingCustomer.Name = "Acme Corporation"; // Belongs to customer 1
await apiClient.PutAsync("/api/Customers/5", existingCustomer); // 409 Conflict
```

## ?? Client Program.cs Changes

**Example 3** updated:
```csharp
// Old: Demonstrated duplicate invoice numbers (internal validation)
var newCustomer = new Customer { ... same invoice numbers ... };

// New: Demonstrates duplicate customer name/email (business rule)
var duplicateCustomer = new Customer
{
    Name = "TechCorp Solutions Ltd",  // Same as Example 2
    Email = "contact@techcorp.com",    // Same as Example 2
    Invoices = new List<Invoice>
    {
        new Invoice
        {
            InvoiceNumber = $"INV-DUP{DateTime.Now.Ticks}",  // Different!
            // ...
        }
    }
};
```

## ?? Benefits

| Benefit | Description |
|---------|-------------|
| **Data Integrity** | No duplicate customers in the system |
| **Clear Errors** | Detailed 409 Conflict responses |
| **User Guidance** | Suggestions on how to resolve conflicts |
| **Case Insensitive** | Works regardless of capitalization |
| **Update Safe** | Customers can keep their own data |

## ?? Files Modified

### Server (AssignmentModule6Svr)
1. **AppDB.cs**
   - Added `FindCustomerByName()`
   - Added `FindCustomerByEmail()`
   - Added `IsDuplicateCustomer()`

2. **Controllers/CustomersController.cs**
   - Updated `CreateCustomer()` - duplicate check before creation
   - Updated `UpdateCustomer()` - duplicate check excluding current customer

### Client (AssignmentModule6Client)
3. **Program.cs**
   - Updated Example 3 to demonstrate name/email duplication

### Documentation
4. **Docs/DUPLICATE_CUSTOMER_DETECTION.md** - Complete feature documentation
5. **Docs/DUPLICATE_DETECTION_SUMMARY.md** - This summary

## ? Testing Checklist

- [x] Create customer with duplicate name ? 409 Conflict ?
- [x] Create customer with duplicate email ? 409 Conflict ?
- [x] Create customer with both duplicate ? 409 Conflict ?
- [x] Update customer with own name/email ? 200 OK ?
- [x] Update customer with another's name ? 409 Conflict ?
- [x] Case-insensitive matching ? Works ?
- [x] Build successful ? ?
- [x] Client example updated ? ?

## ?? How to Test

### 1. Start the Server
```bash
cd AssignmentModule6Svr
dotnet run
```

### 2. Run the Client
```bash
cd AssignmentModule6Client
dotnet run
```

**Expected Result:**
- Example 2 creates "TechCorp Solutions Ltd"
- Example 3 attempts to create duplicate ? **409 Conflict** with detailed message

### 3. Manual Testing with Scalar UI

Visit: `https://localhost:7136/scalar/v1`

**Test Scenario:**
1. POST /api/Customers - Create customer "Test Company"
2. POST /api/Customers - Try to create another "Test Company" ? 409
3. GET /api/Customers - Verify only one "Test Company" exists

## ?? Additional Resources

- **Full Documentation**: `AssignmentModule6Svr/Docs/DUPLICATE_CUSTOMER_DETECTION.md`
- **API Documentation**: `https://localhost:7136/scalar/v1`
- **Problem Details Spec**: RFC 7807

---

**Version**: 2.1.0  
**Status**: ? Complete  
**Breaking Changes**: None  
**Backward Compatible**: Yes
