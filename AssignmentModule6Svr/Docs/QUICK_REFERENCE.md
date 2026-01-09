# AssignmentModule6Svr - Quick Reference

## Quick Start

```powershell
# Run the API
dotnet run --project AssignmentModule6Svr

# Access API Documentation
https://localhost:5001/scalar/v1

# Run Tests
dotnet test
```

---

## API Endpoints

### **Customers** (`/api/Customers`)

| Method | Endpoint | Description | Auth Required |
|--------|----------|-------------|---------------|
| `GET` | `/api/Customers` | Get all customers | No |
| `GET` | `/api/Customers/{id}` | Get customer by ID | No |
| `GET` | `/api/Customers/{id}/Invoices` | Get customer's invoices | No |
| `POST` | `/api/Customers` | Create new customer | No |
| `PUT` | `/api/Customers/{id}` | Update customer | No |
| `DELETE` | `/api/Customers/{id}` | Delete customer (cascade) | **Yes** [KEY] |

### **Invoices** (`/api/Invoices`)

| Method | Endpoint | Description | Auth Required |
|--------|----------|-------------|---------------|
| `GET` | `/api/Invoices` | Get all invoices | No |
| `GET` | `/api/Invoices/{customerId}/{invoiceNumber}` | Get specific invoice | No |
| `GET` | `/api/Invoices/customer/{customerId}` | Get customer's invoices | No |
| `POST` | `/api/Invoices` | Create new invoice | No |
| `DELETE` | `/api/Invoices/{invoiceNumber}` | Delete invoice by number | **Yes** [KEY] |
| `DELETE` | `/api/Invoices/customer/{customerId}` | Delete all customer invoices | **Yes** [KEY] |

### **Phone Numbers** (`/api/PhoneNumbers`)

| Method | Endpoint | Description | Auth Required |
|--------|----------|-------------|---------------|
| `GET` | `/api/PhoneNumbers` | Get all phone numbers | No |
| `GET` | `/api/PhoneNumbers/{id}` | Get phone number by ID | No |
| `GET` | `/api/PhoneNumbers/customer/{customerId}` | Get customer's phones | No |
| `POST` | `/api/PhoneNumbers` | Create new phone number | No |
| `DELETE` | `/api/PhoneNumbers/{id}` | Delete phone number | **Yes** [KEY] |
| `DELETE` | `/api/PhoneNumbers/customer/{customerId}` | Delete customer's phones | **Yes** [KEY] |

---

## Authentication

**DELETE endpoints require API Key authentication:**

### Header
```
X-API-Key: YOUR_API_KEY_HERE
```

### Example with cURL
```bash
curl -X DELETE "https://localhost:5001/api/Customers/1" \
     -H "X-API-Key: YOUR_API_KEY_HERE"
```

### Configure API Key
Edit `appsettings.json`:
```json
{
  "ApiToken": {
    "StaticToken": "YOUR_SECURE_TOKEN_HERE"
  }
}
```

---

## Data Models

### Customer
```json
{
  "id": 1,
  "name": "Acme Corporation",
  "email": "contact@acme.com",
  "balance": 1500.00,
  "invoices": [],
  "phoneNumbers": []
}
```

### Invoice
```json
{
  "id": 1,
  "customerId": 1,
  "invoiceNumber": "INV-A1B2C3D4",
  "invoiceDate": "2024-01-15T10:30:00Z",
  "amount": 500.00
}
```

### Phone Number
```json
{
  "id": 1,
  "customerId": 1,
  "type": "Mobile",
  "number": "+1 (555) 123-4567"
}
```

---

## Validation Rules

### Customer
- **ID**: Auto-generated (ignore provided values)
- **Name**: Optional
- **Email**: Optional
- **Balance**: Calculated (sum of invoice amounts)

### Invoice
- **ID**: Auto-generated globally (unique across all customers)
- **InvoiceNumber**: **Required**, must start with "INV", globally unique
- **CustomerId**: Required, must exist
- **InvoiceDate**: Required
- **Amount**: Required

### Phone Number
- **ID**: Auto-generated globally (unique across all customers)
- **CustomerId**: Required, must exist
- **Type**: Must be one of: `Mobile`, `Work`, `DirectDial`
- **Number**: Required

---

## ID Generation Rules

| Entity | ID Generation | Uniqueness |
|--------|---------------|------------|
| **Customer** | Auto-generated | Per customer |
| **Invoice** | Auto-generated | **Global** (across all customers) |
| **Phone Number** | Auto-generated | **Global** (across all customers) |
| **Invoice Number** | User-provided | **Global** (must start with "INV") |

**Important**: Provided IDs are always ignored and replaced with auto-generated values.

---

## HTTP Status Codes

| Code | Description | When |
|------|-------------|------|
| **200** OK | Success | GET, PUT operations |
| **201** Created | Resource created | POST operations |
| **204** No Content | Success, no body | DELETE operations |
| **400** Bad Request | Invalid data | Validation failures |
| **401** Unauthorized | Missing/invalid API key | DELETE without auth |
| **404** Not Found | Resource doesn't exist | Invalid ID/invoice number |
| **409** Conflict | Duplicate resource | Duplicate invoice number |

---

## Common Operations

### Create Customer

**PowerShell:**
```powershell
$body = @{
    name = "New Company"
    email = "info@newcompany.com"
} | ConvertTo-Json

Invoke-RestMethod -Uri "https://localhost:5001/api/Customers" -Method Post -Body $body -ContentType "application/json"
```

**curl for Windows:**
```cmd
curl -X POST "https://localhost:5001/api/Customers" -H "Content-Type: application/json" -d "{\"name\":\"New Company\",\"email\":\"info@newcompany.com\"}"
```

### Create Invoice

**PowerShell:**
```powershell
$body = @{
    customerId = 1
    invoiceNumber = "INV-12345678"
    invoiceDate = "2024-01-15T10:00:00Z"
    amount = 250.00
} | ConvertTo-Json

Invoke-RestMethod -Uri "https://localhost:5001/api/Invoices" -Method Post -Body $body -ContentType "application/json"
```

**curl for Windows:**
```cmd
curl -X POST "https://localhost:5001/api/Invoices" -H "Content-Type: application/json" -d "{\"customerId\":1,\"invoiceNumber\":\"INV-12345678\",\"invoiceDate\":\"2024-01-15T10:00:00Z\",\"amount\":250.00}"
```

### Create Phone Number

**PowerShell:**
```powershell
$body = @{
    customerId = 1
    type = "Mobile"
    number = "555-1234"
} | ConvertTo-Json

Invoke-RestMethod -Uri "https://localhost:5001/api/PhoneNumbers" -Method Post -Body $body -ContentType "application/json"
```

**curl for Windows:**
```cmd
curl -X POST "https://localhost:5001/api/PhoneNumbers" -H "Content-Type: application/json" -d "{\"customerId\":1,\"type\":\"Mobile\",\"number\":\"555-1234\"}"
```

### Get Customer with Relations

**PowerShell:**
```powershell
Invoke-RestMethod -Uri "https://localhost:5001/api/Customers/1" -Method Get
```

**curl for Windows:**
```cmd
curl "https://localhost:5001/api/Customers/1"
```

### Delete Customer (Cascade)

**PowerShell:**
```powershell
$headers = @{
    "X-API-Key" = "YOUR_API_KEY_HERE"
}
Invoke-RestMethod -Uri "https://localhost:5001/api/Customers/1" -Method Delete -Headers $headers
```

**curl for Windows:**
```cmd
curl -X DELETE "https://localhost:5001/api/Customers/1" -H "X-API-Key: YOUR_API_KEY_HERE"
```

---

## Sample Data

On startup, the API automatically generates:
- **50 customers** with realistic names and emails
- **1-5 invoices** per customer (random amounts)
- **1-3 phone numbers** per customer (various types)

---

## Problem Details Format

All errors return RFC 7807 Problem Details:

```json
{
  "type": "https://yourapi.com/problems/resource-not-found",
  "title": "Customer Not Found",
  "status": 404,
  "detail": "Customer with ID 999 does not exist.",
  "instance": "GET /api/Customers/999",
  "traceId": "0HMVFE...",
  "customerId": 999,
  "suggestion": "Verify the customer ID is correct..."
}
```

---

## Business Rules

### Cascade Deletion
Deleting a customer **automatically deletes**:
- All customer invoices
- All customer phone numbers

### Global ID Uniqueness
- **Invoice IDs** are globally unique (sequential across all customers)
- **Phone Number IDs** are globally unique (sequential across all customers)
- **Customer IDs** are unique per customer

### Invoice Number Requirements
- Must be user-provided (not auto-generated)
- Must start with "INV"
- Must be globally unique
- Case-sensitive
- Returns **409 Conflict** if duplicate

---

## Health Check

**PowerShell:**
```powershell
Invoke-RestMethod -Uri "https://localhost:5001/health" -Method Get
```

**curl for Windows:**
```cmd
curl "https://localhost:5001/health"
```

Returns: `Healthy`

---

## Documentation

| Resource | URL |
|----------|-----|
| **Scalar API Docs** | `https://localhost:5001/scalar/v1` |
| **OpenAPI Spec** | `https://localhost:5001/openapi/v1.json` |
| **Health Check** | `https://localhost:5001/health` |

---

## Configuration

### `appsettings.json`
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug"
    }
  },
  "ApiToken": {
    "StaticToken": "ThisIsANewToken"
  }
}
```

### Launch URLs
- **HTTP**: `http://localhost:5000`
- **HTTPS**: `https://localhost:5001`

---

## Testing

```powershell
# Run all tests
dotnet test

# Run specific test class
dotnet test --filter "FullyQualifiedName~CustomersControllerTests"

# Run with verbose output
dotnet test --verbosity normal

# Generate coverage
dotnet test --collect:"XPlat Code Coverage"
```

**Test Statistics:**
- **74 tests** total
- **100% passing**
- **~85% code coverage**

---

## Troubleshooting

### Port Already in Use
```powershell
# Find process using port 5001
netstat -ano | findstr :5001

# Kill process (replace <PID> with actual process ID)
taskkill /PID <PID> /F
```

### API Key Not Working
- Check header name: `X-API-Key` (case-sensitive)
- Verify token in `appsettings.json`
- Ensure using HTTPS for DELETE operations

### Database Reset
The in-memory database resets on each application restart. No persistence.

---

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
|-- AppDB.cs               # In-memory database
|-- Bogus.cs               # Fake data generator
|-- Program.cs             # App configuration
+-- appsettings.json       # Configuration
```

---

## Tips

- **API Keys**: Only required for DELETE operations
- **IDs**: Always auto-generated, provided IDs are ignored
- **Invoice Numbers**: Must be user-provided and start with "INV"
- **Global IDs**: Invoice and Phone Number IDs are unique across ALL customers
- **Cascade Delete**: Deleting customer removes all related data
- **Validation**: Invoice numbers must be unique globally
- **Documentation**: Use Scalar UI for interactive API exploration

---

## Examples

### Complete Workflow

**PowerShell:**
```powershell
# 1. Create customer
$customer = @{
    name = "Test Co"
    email = "test@test.com"
} | ConvertTo-Json

$newCustomer = Invoke-RestMethod -Uri "https://localhost:5001/api/Customers" -Method Post -Body $customer -ContentType "application/json"
# Returns: { "id": 51, "name": "Test Co", ... }

# 2. Add invoice
$invoice = @{
    customerId = 51
    invoiceNumber = "INV-TEST001"
    amount = 100
} | ConvertTo-Json

Invoke-RestMethod -Uri "https://localhost:5001/api/Invoices" -Method Post -Body $invoice -ContentType "application/json"

# 3. Add phone
$phone = @{
    customerId = 51
    type = "Mobile"
    number = "555-0000"
} | ConvertTo-Json

Invoke-RestMethod -Uri "https://localhost:5001/api/PhoneNumbers" -Method Post -Body $phone -ContentType "application/json"

# 4. Get customer with all data
Invoke-RestMethod -Uri "https://localhost:5001/api/Customers/51" -Method Get

# 5. Delete customer (removes invoices and phones)
$headers = @{ "X-API-Key" = "YOUR_API_KEY_HERE" }
Invoke-RestMethod -Uri "https://localhost:5001/api/Customers/51" -Method Delete -Headers $headers
```

**curl for Windows:**
```cmd
REM 1. Create customer
curl -X POST "https://localhost:5001/api/Customers" -H "Content-Type: application/json" -d "{\"name\":\"Test Co\",\"email\":\"test@test.com\"}"

REM 2. Add invoice
curl -X POST "https://localhost:5001/api/Invoices" -H "Content-Type: application/json" -d "{\"customerId\":51,\"invoiceNumber\":\"INV-TEST001\",\"amount\":100}"

REM 3. Add phone
curl -X POST "https://localhost:5001/api/PhoneNumbers" -H "Content-Type: application/json" -d "{\"customerId\":51,\"type\":\"Mobile\",\"number\":\"555-0000\"}"

REM 4. Get customer with all data
curl "https://localhost:5001/api/Customers/51"

REM 5. Delete customer
curl -X DELETE "https://localhost:5001/api/Customers/51" -H "X-API-Key: YOUR_API_KEY_HERE"
```

---

**Version**: .NET 10  
**Framework**: ASP.NET Core Web API  
**Documentation**: Scalar  
**Testing**: NUnit  
**Data**: In-Memory (Non-persistent)
