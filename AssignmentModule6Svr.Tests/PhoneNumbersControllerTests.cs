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
public class PhoneNumbersControllerTests
{
    private PhoneNumbersController _controller;
    private Mock<ILogger<PhoneNumbersController>> _mockLogger;
    private Customer _testCustomer;

    [SetUp]
    public void Setup()
    {
        AppDB.ClearDatabase();
        _mockLogger = new Mock<ILogger<PhoneNumbersController>>();
        _controller = new PhoneNumbersController(_mockLogger.Object);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
        
        // Create a test customer for use in tests
        _testCustomer = AppDB.AddCustomerWithAutoId(new Customer 
        { 
            Name = "Test Customer", 
            Email = "test@test.com" 
        })!;
    }

    [TearDown]
    public void TearDown()
    {
        AppDB.ClearDatabase();
    }

    #region GET Tests

    [Test]
    public void GetAllPhoneNumbers_ShouldReturnOkWithPhoneNumberList()
    {
        // Arrange
        AppDB.AddPhoneNumberWithAutoId(new PhoneNumber 
        { 
            CustomerId = _testCustomer.Id, 
            Number = "555-1111", 
            Type = "Mobile" 
        });
        AppDB.AddPhoneNumberWithAutoId(new PhoneNumber 
        { 
            CustomerId = _testCustomer.Id, 
            Number = "555-2222", 
            Type = "Work" 
        });

        // Act
        var result = _controller.GetAllPhoneNumbers();

        // Assert
        Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
        var okResult = result.Result as OkObjectResult;
        var phoneNumbers = okResult!.Value as List<PhoneNumber>;
        Assert.That(phoneNumbers, Has.Count.EqualTo(2));
    }

    [Test]
    public void GetPhoneNumber_WithValidId_ShouldReturnOkWithPhoneNumber()
    {
        // Arrange
        var phone = AppDB.AddPhoneNumberWithAutoId(new PhoneNumber 
        { 
            CustomerId = _testCustomer.Id, 
            Number = "555-TEST", 
            Type = "Mobile" 
        });

        // Act
        var result = _controller.GetPhoneNumber(phone!.Id);

        // Assert
        Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
        var okResult = result.Result as OkObjectResult;
        var returnedPhone = okResult!.Value as PhoneNumber;
        Assert.That(returnedPhone!.Number, Is.EqualTo("555-TEST"));
    }

    [Test]
    public void GetPhoneNumber_WithInvalidId_ShouldReturn404()
    {
        // Act
        var result = _controller.GetPhoneNumber(99999);

        // Assert
        Assert.That(result.Result, Is.InstanceOf<NotFoundObjectResult>());
    }

    [Test]
    public void GetPhoneNumbersByCustomer_WithValidCustomerId_ShouldReturnOkWithPhoneNumbers()
    {
        // Arrange
        AppDB.AddPhoneNumberWithAutoId(new PhoneNumber 
        { 
            CustomerId = _testCustomer.Id, 
            Number = "555-1111", 
            Type = "Mobile" 
        });
        AppDB.AddPhoneNumberWithAutoId(new PhoneNumber 
        { 
            CustomerId = _testCustomer.Id, 
            Number = "555-2222", 
            Type = "Work" 
        });

        // Act
        var result = _controller.GetPhoneNumbersByCustomer(_testCustomer.Id);

        // Assert
        Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
        var okResult = result.Result as OkObjectResult;
        var phoneNumbers = okResult!.Value as List<PhoneNumber>;
        Assert.That(phoneNumbers, Has.Count.EqualTo(2));
    }

    [Test]
    public void GetPhoneNumbersByCustomer_WithInvalidCustomerId_ShouldReturn404()
    {
        // Act
        var result = _controller.GetPhoneNumbersByCustomer(99999);

        // Assert
        Assert.That(result.Result, Is.InstanceOf<NotFoundObjectResult>());
    }

    #endregion

    #region POST Tests

    [Test]
    public void CreatePhoneNumber_WithValidData_ShouldReturn201Created()
    {
        // Arrange
        var phoneNumber = new PhoneNumber 
        { 
            CustomerId = _testCustomer.Id, 
            Number = "(555) 123-4567", 
            Type = "Mobile" 
        };

        // Act
        var result = _controller.CreatePhoneNumber(phoneNumber);

        // Assert
        Assert.That(result.Result, Is.InstanceOf<CreatedAtActionResult>());
        var createdResult = result.Result as CreatedAtActionResult;
        var createdPhone = createdResult!.Value as PhoneNumber;
        Assert.That(createdPhone!.Id, Is.GreaterThan(0));
        Assert.That(createdPhone.Number, Is.EqualTo("(555) 123-4567"));
    }

    [Test]
    public void CreatePhoneNumber_WithNullPhoneNumber_ShouldReturn400()
    {
        // Act
        var result = _controller.CreatePhoneNumber(null!);

        // Assert
        Assert.That(result.Result, Is.InstanceOf<BadRequestObjectResult>());
    }

    [Test]
    public void CreatePhoneNumber_WithInvalidCustomerId_ShouldReturn404()
    {
        // Arrange
        var phoneNumber = new PhoneNumber 
        { 
            CustomerId = 99999, 
            Number = "555-9999", 
            Type = "Mobile" 
        };

        // Act
        var result = _controller.CreatePhoneNumber(phoneNumber);

        // Assert
        Assert.That(result.Result, Is.InstanceOf<NotFoundObjectResult>());
    }

    [Test]
    public void CreatePhoneNumber_WithInvalidCustomerIdValue_ShouldReturn400()
    {
        // Arrange
        var phoneNumber = new PhoneNumber 
        { 
            CustomerId = 0, 
            Number = "555-0000", 
            Type = "Mobile" 
        };

        // Act
        var result = _controller.CreatePhoneNumber(phoneNumber);

        // Assert
        Assert.That(result.Result, Is.InstanceOf<BadRequestObjectResult>());
    }

    [Test]
    public void CreatePhoneNumber_ShouldIgnoreProvidedId()
    {
        // Arrange
        var phoneNumber = new PhoneNumber 
        { 
            Id = 9999, 
            CustomerId = _testCustomer.Id, 
            Number = "555-TEST-ID", 
            Type = "Work" 
        };

        // Act
        var result = _controller.CreatePhoneNumber(phoneNumber);

        // Assert
        Assert.That(result.Result, Is.InstanceOf<CreatedAtActionResult>());
        var createdResult = result.Result as CreatedAtActionResult;
        var createdPhone = createdResult!.Value as PhoneNumber;
        Assert.That(createdPhone!.Id, Is.Not.EqualTo(9999)); // Should be auto-generated
    }

    [Test]
    public void CreatePhoneNumber_AllPhoneTypes_ShouldWork()
    {
        // Test all valid phone types: Mobile, Work, DirectDial
        var phoneTypes = new[] { "Mobile", "Work", "DirectDial" };

        foreach (var type in phoneTypes)
        {
            // Arrange
            var phoneNumber = new PhoneNumber 
            { 
                CustomerId = _testCustomer.Id, 
                Number = $"555-{type}", 
                Type = type 
            };

            // Act
            var result = _controller.CreatePhoneNumber(phoneNumber);

            // Assert
            Assert.That(result.Result, Is.InstanceOf<CreatedAtActionResult>(), $"Type '{type}' should be valid");
            var createdResult = result.Result as CreatedAtActionResult;
            var createdPhone = createdResult!.Value as PhoneNumber;
            Assert.That(createdPhone!.Type, Is.EqualTo(type));
        }
    }

    #endregion

    #region DELETE Tests

    [Test]
    public void DeletePhoneNumber_WithValidId_ShouldReturn204NoContent()
    {
        // Arrange
        var phone = AppDB.AddPhoneNumberWithAutoId(new PhoneNumber 
        { 
            CustomerId = _testCustomer.Id, 
            Number = "555-DELETE", 
            Type = "Mobile" 
        });

        // Act
        var result = _controller.DeletePhoneNumber(phone!.Id);

        // Assert
        Assert.That(result, Is.InstanceOf<NoContentResult>());
        Assert.That(AppDB.FindPhoneNumber(phone.Id), Is.Null);
    }

    [Test]
    public void DeletePhoneNumber_WithInvalidId_ShouldReturn404()
    {
        // Act
        var result = _controller.DeletePhoneNumber(99999);

        // Assert
        Assert.That(result, Is.InstanceOf<NotFoundObjectResult>());
    }

    [Test]
    public void DeletePhoneNumber_WithInvalidIdValue_ShouldReturn400()
    {
        // Act
        var result = _controller.DeletePhoneNumber(0);

        // Assert
        Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
    }

    [Test]
    public void DeletePhoneNumbersByCustomer_WithValidCustomerId_ShouldReturn204NoContent()
    {
        // Arrange
        AppDB.AddPhoneNumberWithAutoId(new PhoneNumber 
        { 
            CustomerId = _testCustomer.Id, 
            Number = "555-1111", 
            Type = "Mobile" 
        });
        AppDB.AddPhoneNumberWithAutoId(new PhoneNumber 
        { 
            CustomerId = _testCustomer.Id, 
            Number = "555-2222", 
            Type = "Work" 
        });

        // Act
        var result = _controller.DeletePhoneNumbersByCustomer(_testCustomer.Id);

        // Assert
        Assert.That(result, Is.InstanceOf<NoContentResult>());
        Assert.That(AppDB.GetPhoneNumbersByCustomer(_testCustomer.Id), Is.Empty);
    }

    [Test]
    public void DeletePhoneNumbersByCustomer_WithInvalidCustomerId_ShouldReturn404()
    {
        // Act
        var result = _controller.DeletePhoneNumbersByCustomer(99999);

        // Assert
        Assert.That(result, Is.InstanceOf<NotFoundObjectResult>());
    }

    [Test]
    public void DeletePhoneNumbersByCustomer_WithInvalidIdValue_ShouldReturn400()
    {
        // Act
        var result = _controller.DeletePhoneNumbersByCustomer(0);

        // Assert
        Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
    }

    #endregion

    #region Global ID Tests

    [Test]
    public void CreatePhoneNumber_IdsShouldBeGloballyUnique_AcrossMultipleCustomers()
    {
        // Arrange
        var customer2 = AppDB.AddCustomerWithAutoId(new Customer 
        { 
            Name = "Customer 2", 
            Email = "customer2@test.com" 
        });

        var phone1 = new PhoneNumber 
        { 
            CustomerId = _testCustomer.Id, 
            Number = "555-1111", 
            Type = "Mobile" 
        };
        var phone2 = new PhoneNumber 
        { 
            CustomerId = customer2!.Id, 
            Number = "555-2222", 
            Type = "Work" 
        };
        var phone3 = new PhoneNumber 
        { 
            CustomerId = _testCustomer.Id, 
            Number = "555-3333", 
            Type = "DirectDial" 
        };

        // Act
        var result1 = _controller.CreatePhoneNumber(phone1);
        var result2 = _controller.CreatePhoneNumber(phone2);
        var result3 = _controller.CreatePhoneNumber(phone3);

        // Assert
        var created1 = (result1.Result as CreatedAtActionResult)!.Value as PhoneNumber;
        var created2 = (result2.Result as CreatedAtActionResult)!.Value as PhoneNumber;
        var created3 = (result3.Result as CreatedAtActionResult)!.Value as PhoneNumber;

        Assert.That(created2!.Id, Is.EqualTo(created1!.Id + 1));
        Assert.That(created3!.Id, Is.EqualTo(created2.Id + 1));
    }

    #endregion
}
