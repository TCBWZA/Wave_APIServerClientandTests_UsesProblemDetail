# AssignmentModule6Svr.Tests

NUnit test project for the AssignmentModule6Svr API.

## Test Coverage

### AppDBTests
Comprehensive tests for the `AppDB` in-memory database functionality:
- Customer CRUD operations
- Invoice CRUD operations
- Phone Number CRUD operations
- Global ID generation and uniqueness
- Cascade deletion
- Data relationships

### CustomersControllerTests
Tests for the `CustomersController` API endpoints:
- GET all customers
- GET customer by ID
- GET customer invoices
- POST create customer (with auto-generated IDs)
- PUT update customer
- DELETE customer (with cascade deletion)
- Error handling (404, 400 status codes)

### InvoicesControllerTests
Tests for the `InvoicesController` API endpoints:
- GET all invoices
- GET invoice by customer ID and invoice number
- GET invoices by customer ID
- POST create invoice (with auto-generated IDs)
- DELETE invoice by invoice number
- DELETE all invoices by customer ID
- Invoice number validation (must start with "INV")
- Duplicate invoice number detection (409 Conflict)
- Error handling (404, 400, 409 status codes)

### PhoneNumbersControllerTests
Tests for the `PhoneNumbersController` API endpoints:
- GET all phone numbers
- GET phone number by ID
- GET phone numbers by customer ID
- POST create phone number (with auto-generated IDs)
- DELETE phone number by ID
- DELETE all phone numbers by customer ID
- Phone number type validation (Mobile, Work, DirectDial)
- Global ID uniqueness across customers
- Error handling (404, 400 status codes)

## Running the Tests

### Visual Studio
1. Open **Test Explorer** (Test > Test Explorer)
2. Click **Run All** to execute all tests
3. View results in the Test Explorer window

### Command Line (PowerShell or CMD)
```powershell
# Run all tests
dotnet test

# Run tests with detailed output
dotnet test --verbosity normal

# Run tests for a specific test class
dotnet test --filter "FullyQualifiedName~AppDBTests"

# Run tests and generate coverage report
dotnet test --collect:"XPlat Code Coverage"
```

### VS Code
1. Install the **.NET Core Test Explorer** extension
2. Tests will appear in the Test Explorer sidebar
3. Click the play button next to any test to run it

## Test Frameworks and Libraries

- **NUnit 4.2.2** - Test framework
- **FluentAssertions 8.8.0** - Assertion library for readable tests
- **Moq 4.20.72** - Mocking framework for dependencies
- **Microsoft.AspNetCore.Mvc.Testing 10.0.1** - Integration testing support

## Test Patterns

### Arrange-Act-Assert (AAA)
All tests follow the AAA pattern:
```csharp
[Test]
public void TestName_Scenario_ExpectedBehavior()
{
    // Arrange - Set up test data and dependencies
    var customer = new Customer { Name = "Test", Email = "test@test.com" };
    
    // Act - Execute the code being tested
    var result = _controller.CreateCustomer(customer);
    
    // Assert - Verify the outcome
    result.Result.Should().BeOfType<CreatedAtActionResult>();
}
```

### Setup and Teardown
Each test class uses `[SetUp]` and `[TearDown]` to:
- Clear the AppDB before each test
- Initialize controller instances
- Clean up after each test
- Ensure test isolation

### FluentAssertions Syntax
Tests use FluentAssertions for readable assertions:
```csharp
// Instead of: Assert.AreEqual(expected, actual)
actual.Should().Be(expected);

// Instead of: Assert.IsNotNull(value)
value.Should().NotBeNull();

// Instead of: Assert.IsTrue(collection.Count == 2)
collection.Should().HaveCount(2);
```

## Key Test Scenarios

### Auto-Generated IDs
Tests verify that IDs are auto-generated and any provided IDs are ignored:
```csharp
[Test]
public void CreateCustomer_ShouldIgnoreProvidedId()
{
    var customer = new Customer { Id = 9999, Name = "Test", Email = "test@test.com" };
    var result = _controller.CreateCustomer(customer);
    
    var createdCustomer = (result.Result as CreatedAtActionResult)!.Value as Customer;
    createdCustomer!.Id.Should().NotBe(9999); // Auto-generated, not 9999
}
```

### Global ID Uniqueness
Tests verify IDs are globally unique across all customers:
```csharp
[Test]
public void InvoiceIds_ShouldBeGloballyUnique_AcrossAllCustomers()
{
    // Create invoices for customer 1
    var invoice1 = AppDB.AddInvoiceWithAutoId(/*...*/);
    
    // Create invoice for customer 2
    var invoice2 = AppDB.AddInvoiceWithAutoId(/*...*/);
    
    // Assert: IDs are sequential globally
    invoice2!.Id.Should().Be(invoice1!.Id + 1);
}
```

### Invoice Number Validation
Tests verify invoice numbers must start with "INV":
```csharp
[Test]
public void CreateInvoice_InvoiceNumberMustStartWithINV()
{
    var invoice = new Invoice 
    { 
        InvoiceNumber = "INVOICE-001" // Invalid
    };
    
    var validationResults = Validate(invoice);
    validationResults.Should().Contain(r => r.ErrorMessage!.Contains("INV"));
}
```

### Cascade Deletion
Tests verify that deleting a customer also deletes related data:
```csharp
[Test]
public void DeleteCustomer_ShouldCascadeDeleteInvoicesAndPhones()
{
    // Create customer with invoices and phones
    // ...
    
    _controller.DeleteCustomer(customer.Id);
    
    AppDB.GetInvoicesByCustomer(customer.Id).Should().BeEmpty();
    AppDB.GetPhoneNumbersByCustomer(customer.Id).Should().BeEmpty();
}
```

## Test Coverage Statistics

Run coverage analysis:
```powershell
dotnet test --collect:"XPlat Code Coverage"
```

Expected coverage:
- AppDB: ~95%
- Controllers: ~90%
- Classes (Models): ~80%

## Continuous Integration

These tests are designed to run in CI/CD pipelines:
- No external dependencies required
- In-memory database (no SQL Server needed)
- Fast execution (< 5 seconds for all tests)
- Isolated tests (no shared state)

## Adding New Tests

1. Create a new test class in the appropriate file
2. Add `[TestFixture]` attribute to the class
3. Add `[SetUp]` and `[TearDown]` methods
4. Follow the AAA pattern for each test
5. Use descriptive test names: `MethodName_Scenario_ExpectedBehavior`
6. Add `[Test]` attribute to each test method

Example:
```csharp
[TestFixture]
public class NewFeatureTests
{
    [SetUp]
    public void Setup()
    {
        AppDB.ClearDatabase();
    }

    [Test]
    public void NewMethod_ValidInput_ReturnsExpectedResult()
    {
        // Arrange
        var input = "test";
        
        // Act
        var result = NewMethod(input);
        
        // Assert
        result.Should().Be("expected");
    }
}
```

## Troubleshooting

### Tests Fail with "Database not empty"
- Ensure `AppDB.ClearDatabase()` is called in `[SetUp]`
- Check that `[TearDown]` is properly cleaning up

### NullReferenceException in Tests
- Verify mocks are properly initialized
- Check that `AppDB.AddCustomerWithAutoId` returns non-null before using

### Tests Pass Locally but Fail in CI
- Ensure no hardcoded paths or dependencies
- Verify timezone-independent date comparisons
- Check for thread-safety issues

## Contributing

When adding new features:
1. Write tests first (TDD approach)
2. Ensure all existing tests pass
3. Aim for >80% code coverage
4. Add integration tests for new endpoints
5. Update this README with new test coverage

## License

Same as parent project (AssignmentModule6Svr)
