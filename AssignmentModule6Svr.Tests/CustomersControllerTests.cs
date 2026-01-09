using AssignmentModule6Svr;
using AssignmentModule6Svr.Classes;
using AssignmentModule6Svr.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace AssignmentModule6Svr.Tests;

[TestFixture]
public class CustomersControllerTests
{
    private CustomersController _controller;
    private Mock<ILogger<CustomersController>> _mockLogger;

    [SetUp]
    public void Setup()
    {
        AppDB.ClearDatabase();
        _mockLogger = new Mock<ILogger<CustomersController>>();
        _controller = new CustomersController(_mockLogger.Object);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
    }

    [TearDown]
    public void TearDown()
    {
        AppDB.ClearDatabase();
    }

    #region GET Tests

    [Test]
    public void GetAllCustomers_ShouldReturnOkWithCustomerList()
    {
        // Arrange
        AppDB.AddCustomerWithAutoId(new Customer { Name = "Customer 1", Email = "c1@test.com" });
        AppDB.AddCustomerWithAutoId(new Customer { Name = "Customer 2", Email = "c2@test.com" });

        // Act
        var result = _controller.GetAllCustomers();

        // Assert
        Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
        var okResult = result.Result as OkObjectResult;
        var customers = okResult!.Value as List<Customer>;
        Assert.That(customers, Has.Count.EqualTo(2));
    }

    [Test]
    public void GetCustomer_WithValidId_ShouldReturnOkWithCustomer()
    {
        // Arrange
        var customer = AppDB.AddCustomerWithAutoId(new Customer 
        { 
            Name = "Test Customer", 
            Email = "test@test.com" 
        });

        // Act
        var result = _controller.GetCustomer(customer!.Id);

        // Assert
        Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
        var okResult = result.Result as OkObjectResult;
        var returnedCustomer = okResult!.Value as Customer;
        Assert.That(returnedCustomer!.Id, Is.EqualTo(customer.Id));
        Assert.That(returnedCustomer.Name, Is.EqualTo("Test Customer"));
    }

    [Test]
    public void GetCustomer_WithInvalidId_ShouldReturn404()
    {
        // Act
        var result = _controller.GetCustomer(99999);

        // Assert
        Assert.That(result.Result, Is.InstanceOf<NotFoundObjectResult>());
        var notFoundResult = result.Result as NotFoundObjectResult;
        var problemDetails = notFoundResult!.Value as ProblemDetails;
        Assert.That(problemDetails!.Status, Is.EqualTo(404));
        Assert.That(problemDetails.Title, Is.EqualTo("Customer Not Found"));
    }

    [Test]
    public void GetCustomerInvoices_WithValidId_ShouldReturnOkWithInvoices()
    {
        // Arrange
        var customer = AppDB.AddCustomerWithAutoId(new Customer 
        { 
            Name = "Test Customer", 
            Email = "test@test.com" 
        });
        AppDB.AddInvoiceWithAutoId(new Invoice 
        { 
            CustomerId = customer!.Id, 
            InvoiceNumber = "INV-TEST-001", 
            Amount = 100, 
            InvoiceDate = DateTime.Now 
        });

        // Act
        var result = _controller.GetCustomerInvoices(customer.Id);

        // Assert
        Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
        var okResult = result.Result as OkObjectResult;
        var invoices = okResult!.Value as List<Invoice>;
        Assert.That(invoices, Has.Count.EqualTo(1));
    }

    [Test]
    public void GetCustomerInvoices_WithInvalidCustomerId_ShouldReturn404()
    {
        // Act
        var result = _controller.GetCustomerInvoices(99999);

        // Assert
        Assert.That(result.Result, Is.InstanceOf<NotFoundObjectResult>());
    }

    #endregion

    #region POST Tests

    [Test]
    public void CreateCustomer_WithValidData_ShouldReturn201Created()
    {
        // Arrange
        var customer = new Customer { Name = "New Customer", Email = "new@test.com" };

        // Act
        var result = _controller.CreateCustomer(customer);

        // Assert
        Assert.That(result.Result, Is.InstanceOf<CreatedAtActionResult>());
        var createdResult = result.Result as CreatedAtActionResult;
        var createdCustomer = createdResult!.Value as Customer;
        Assert.That(createdCustomer!.Id, Is.GreaterThan(0));
        Assert.That(createdCustomer.Name, Is.EqualTo("New Customer"));
    }

    [Test]
    public void CreateCustomer_WithNullCustomer_ShouldReturn400()
    {
        // Act
        var result = _controller.CreateCustomer(null!);

        // Assert
        Assert.That(result.Result, Is.InstanceOf<BadRequestObjectResult>());
        var badRequestResult = result.Result as BadRequestObjectResult;
        var problemDetails = badRequestResult!.Value as ValidationProblemDetails;
        Assert.That(problemDetails!.Status, Is.EqualTo(400));
    }

    [Test]
    public void CreateCustomer_ShouldIgnoreProvidedId()
    {
        // Arrange
        var customer = new Customer { Id = 9999, Name = "Test Customer", Email = "test@test.com" };

        // Act
        var result = _controller.CreateCustomer(customer);

        // Assert
        Assert.That(result.Result, Is.InstanceOf<CreatedAtActionResult>());
        var createdResult = result.Result as CreatedAtActionResult;
        var createdCustomer = createdResult!.Value as Customer;
        Assert.That(createdCustomer!.Id, Is.Not.EqualTo(9999)); // Should be auto-generated
    }

    #endregion

    #region PUT Tests

    [Test]
    public void UpdateCustomer_WithValidData_ShouldReturn200Ok()
    {
        // Arrange
        var customer = AppDB.AddCustomerWithAutoId(new Customer 
        { 
            Name = "Original Name", 
            Email = "original@test.com" 
        });
        var updatedCustomer = new Customer 
        { 
            Name = "Updated Name", 
            Email = "updated@test.com" 
        };

        // Act
        var result = _controller.UpdateCustomer(customer!.Id, updatedCustomer);

        // Assert
        Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
        var okResult = result.Result as OkObjectResult;
        var returnedCustomer = okResult!.Value as Customer;
        Assert.That(returnedCustomer!.Name, Is.EqualTo("Updated Name"));
    }

    [Test]
    public void UpdateCustomer_WithInvalidId_ShouldReturn404()
    {
        // Arrange
        var updatedCustomer = new Customer 
        { 
            Name = "Updated Name", 
            Email = "updated@test.com" 
        };

        // Act
        var result = _controller.UpdateCustomer(99999, updatedCustomer);

        // Assert
        Assert.That(result.Result, Is.InstanceOf<NotFoundObjectResult>());
    }

    [Test]
    public void UpdateCustomer_WithNullCustomer_ShouldReturn400()
    {
        // Act
        var result = _controller.UpdateCustomer(1, null!);

        // Assert
        Assert.That(result.Result, Is.InstanceOf<BadRequestObjectResult>());
    }

    [Test]
    public void UpdateCustomer_WithInvalidIdValue_ShouldReturn400()
    {
        // Arrange
        var customer = new Customer { Name = "Test", Email = "test@test.com" };

        // Act
        var result = _controller.UpdateCustomer(0, customer);

        // Assert
        Assert.That(result.Result, Is.InstanceOf<BadRequestObjectResult>());
    }

    #endregion

    #region DELETE Tests

    [Test]
    public void DeleteCustomer_WithValidId_ShouldReturn204NoContent()
    {
        // Arrange
        var customer = AppDB.AddCustomerWithAutoId(new Customer 
        { 
            Name = "Test Customer", 
            Email = "test@test.com" 
        });

        // Act
        var result = _controller.DeleteCustomer(customer!.Id);

        // Assert
        Assert.That(result, Is.InstanceOf<NoContentResult>());
        Assert.That(AppDB.FindCustomer(customer.Id), Is.Null);
    }

    [Test]
    public void DeleteCustomer_WithInvalidId_ShouldReturn404()
    {
        // Act
        var result = _controller.DeleteCustomer(99999);

        // Assert
        Assert.That(result, Is.InstanceOf<NotFoundObjectResult>());
    }

    [Test]
    public void DeleteCustomer_ShouldCascadeDeleteInvoicesAndPhones()
    {
        // Arrange
        var customer = AppDB.AddCustomerWithAutoId(new Customer 
        { 
            Name = "Test Customer", 
            Email = "test@test.com" 
        });
        AppDB.AddInvoiceWithAutoId(new Invoice 
        { 
            CustomerId = customer!.Id, 
            InvoiceNumber = "INV-TEST-001", 
            Amount = 100, 
            InvoiceDate = DateTime.Now 
        });
        AppDB.AddPhoneNumberWithAutoId(new PhoneNumber 
        { 
            CustomerId = customer.Id, 
            Number = "555-1234", 
            Type = "Mobile" 
        });

        // Act
        var result = _controller.DeleteCustomer(customer.Id);

        // Assert
        Assert.That(result, Is.InstanceOf<NoContentResult>());
        Assert.That(AppDB.GetInvoicesByCustomer(customer.Id), Is.Empty);
        Assert.That(AppDB.GetPhoneNumbersByCustomer(customer.Id), Is.Empty);
    }

    #endregion
}
