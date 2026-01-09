using AssignmentModule6Svr;
using AssignmentModule6Svr.Classes;
using NUnit.Framework;

namespace AssignmentModule6Svr.Tests;

[TestFixture]
public class AppDBTests
{
    [SetUp]
    public void Setup()
    {
        // Clear database before each test
        AppDB.ClearDatabase();
    }

    [TearDown]
    public void TearDown()
    {
        // Clean up after each test
        AppDB.ClearDatabase();
    }

    #region Customer Tests

    [Test]
    public void AddCustomerWithAutoId_ShouldGenerateUniqueId()
    {
        // Arrange
        var customer = new Customer { Name = "Test Customer", Email = "test@example.com" };

        // Act
        var result = AppDB.AddCustomerWithAutoId(customer);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Id, Is.GreaterThan(0));
        Assert.That(result.Name, Is.EqualTo("Test Customer"));
        Assert.That(result.Email, Is.EqualTo("test@example.com"));
    }

    [Test]
    public void AddCustomerWithAutoId_ShouldIncrementIdsGlobally()
    {
        // Arrange
        var customer1 = new Customer { Name = "Customer 1", Email = "customer1@example.com" };
        var customer2 = new Customer { Name = "Customer 2", Email = "customer2@example.com" };

        // Act
        var result1 = AppDB.AddCustomerWithAutoId(customer1);
        var result2 = AppDB.AddCustomerWithAutoId(customer2);

        // Assert
        Assert.That(result1!.Id, Is.LessThan(result2!.Id));
        Assert.That(result2.Id - result1.Id, Is.EqualTo(1));
    }

    [Test]
    public void FindCustomer_WithValidId_ShouldReturnCustomer()
    {
        // Arrange
        var customer = new Customer { Name = "Test Customer", Email = "test@example.com" };
        var created = AppDB.AddCustomerWithAutoId(customer);

        // Act
        var found = AppDB.FindCustomer(created!.Id);

        // Assert
        Assert.That(found, Is.Not.Null);
        Assert.That(found!.Id, Is.EqualTo(created.Id));
        Assert.That(found.Name, Is.EqualTo("Test Customer"));
    }

    [Test]
    public void FindCustomer_WithInvalidId_ShouldReturnNull()
    {
        // Act
        var found = AppDB.FindCustomer(99999);

        // Assert
        Assert.That(found, Is.Null);
    }

    [Test]
    public void RemoveCustomer_ShouldDeleteCustomerAndRelatedData()
    {
        // Arrange
        var customer = new Customer { Name = "Test Customer", Email = "test@example.com" };
        var created = AppDB.AddCustomerWithAutoId(customer);
        
        var invoice = new Invoice 
        { 
            CustomerId = created!.Id, 
            InvoiceNumber = "INV-TEST-001", 
            Amount = 100, 
            InvoiceDate = DateTime.Now 
        };
        AppDB.AddInvoiceWithAutoId(invoice);

        var phone = new PhoneNumber 
        { 
            CustomerId = created.Id, 
            Number = "555-1234", 
            Type = "Mobile" 
        };
        AppDB.AddPhoneNumberWithAutoId(phone);

        // Act
        var result = AppDB.RemoveCustomer(created.Id);

        // Assert
        Assert.That(result, Is.True);
        Assert.That(AppDB.FindCustomer(created.Id), Is.Null);
        Assert.That(AppDB.GetInvoicesByCustomer(created.Id), Is.Empty);
        Assert.That(AppDB.GetPhoneNumbersByCustomer(created.Id), Is.Empty);
    }

    [Test]
    public void GetCustomerList_ShouldReturnAllCustomersWithRelatedData()
    {
        // Arrange
        var customer1 = new Customer { Name = "Customer 1", Email = "c1@example.com" };
        var customer2 = new Customer { Name = "Customer 2", Email = "c2@example.com" };
        
        var created1 = AppDB.AddCustomerWithAutoId(customer1);
        var created2 = AppDB.AddCustomerWithAutoId(customer2);

        // Act
        var customers = AppDB.GetCustomerList();

        // Assert
        Assert.That(customers, Has.Count.EqualTo(2));
        Assert.That(customers, Has.Some.Matches<Customer>(c => c.Id == created1!.Id));
        Assert.That(customers, Has.Some.Matches<Customer>(c => c.Id == created2!.Id));
    }

    [Test]
    public void UpdateCustomer_ShouldUpdateCustomerProperties()
    {
        // Arrange
        var customer = new Customer { Name = "Original Name", Email = "original@example.com" };
        var created = AppDB.AddCustomerWithAutoId(customer);
        
        var updated = new Customer { Name = "Updated Name", Email = "updated@example.com" };

        // Act
        var result = AppDB.UpdateCustomer(created!.Id, updated);

        // Assert
        Assert.That(result, Is.True);
        var found = AppDB.FindCustomer(created.Id);
        Assert.That(found!.Name, Is.EqualTo("Updated Name"));
        Assert.That(found.Email, Is.EqualTo("updated@example.com"));
    }

    #endregion

    #region Invoice Tests

    [Test]
    public void AddInvoiceWithAutoId_ShouldGenerateUniqueIdGlobally()
    {
        // Arrange
        var customer = new Customer { Name = "Test Customer", Email = "test@example.com" };
        var created = AppDB.AddCustomerWithAutoId(customer);
        
        var invoice1 = new Invoice 
        { 
            CustomerId = created!.Id, 
            InvoiceNumber = "INV-001", 
            Amount = 100, 
            InvoiceDate = DateTime.Now 
        };
        var invoice2 = new Invoice 
        { 
            CustomerId = created.Id, 
            InvoiceNumber = "INV-002", 
            Amount = 200, 
            InvoiceDate = DateTime.Now 
        };

        // Act
        var result1 = AppDB.AddInvoiceWithAutoId(invoice1);
        var result2 = AppDB.AddInvoiceWithAutoId(invoice2);

        // Assert
        Assert.That(result1, Is.Not.Null);
        Assert.That(result2, Is.Not.Null);
        Assert.That(result1!.Id, Is.GreaterThan(0));
        Assert.That(result2!.Id, Is.GreaterThan(result1.Id));
        Assert.That(result2.Id - result1.Id, Is.EqualTo(1));
    }

    [Test]
    public void AddInvoiceWithAutoId_WithoutInvoiceNumber_ShouldReturnNull()
    {
        // Arrange
        var customer = new Customer { Name = "Test Customer", Email = "test@example.com" };
        var created = AppDB.AddCustomerWithAutoId(customer);
        
        var invoice = new Invoice 
        { 
            CustomerId = created!.Id, 
            InvoiceNumber = "", 
            Amount = 100, 
            InvoiceDate = DateTime.Now 
        };

        // Act
        var result = AppDB.AddInvoiceWithAutoId(invoice);

        // Assert
        Assert.That(result, Is.Null);
    }

    [Test]
    public void FindInvoice_ByInvoiceNumber_ShouldReturnInvoice()
    {
        // Arrange
        var customer = new Customer { Name = "Test Customer", Email = "test@example.com" };
        var created = AppDB.AddCustomerWithAutoId(customer);
        
        var invoice = new Invoice 
        { 
            CustomerId = created!.Id, 
            InvoiceNumber = "INV-TEST-123", 
            Amount = 500, 
            InvoiceDate = DateTime.Now 
        };
        var createdInvoice = AppDB.AddInvoiceWithAutoId(invoice);

        // Act
        var found = AppDB.FindInvoice("INV-TEST-123");

        // Assert
        Assert.That(found, Is.Not.Null);
        Assert.That(found!.Id, Is.EqualTo(createdInvoice!.Id));
        Assert.That(found.InvoiceNumber, Is.EqualTo("INV-TEST-123"));
        Assert.That(found.Amount, Is.EqualTo(500));
    }

    [Test]
    public void FindInvoice_ById_ShouldReturnInvoice()
    {
        // Arrange
        var customer = new Customer { Name = "Test Customer", Email = "test@example.com" };
        var created = AppDB.AddCustomerWithAutoId(customer);
        
        var invoice = new Invoice 
        { 
            CustomerId = created!.Id, 
            InvoiceNumber = "INV-TEST-456", 
            Amount = 750, 
            InvoiceDate = DateTime.Now 
        };
        var createdInvoice = AppDB.AddInvoiceWithAutoId(invoice);

        // Act
        var found = AppDB.FindInvoice(createdInvoice!.Id);

        // Assert
        Assert.That(found, Is.Not.Null);
        Assert.That(found!.InvoiceNumber, Is.EqualTo("INV-TEST-456"));
        Assert.That(found.Amount, Is.EqualTo(750));
    }

    [Test]
    public void GetInvoicesByCustomer_ShouldReturnOnlyCustomerInvoices()
    {
        // Arrange
        var customer1 = new Customer { Name = "Customer 1", Email = "c1@example.com" };
        var customer2 = new Customer { Name = "Customer 2", Email = "c2@example.com" };
        
        var created1 = AppDB.AddCustomerWithAutoId(customer1);
        var created2 = AppDB.AddCustomerWithAutoId(customer2);
        
        AppDB.AddInvoiceWithAutoId(new Invoice 
        { 
            CustomerId = created1!.Id, 
            InvoiceNumber = "INV-C1-001", 
            Amount = 100, 
            InvoiceDate = DateTime.Now 
        });
        AppDB.AddInvoiceWithAutoId(new Invoice 
        { 
            CustomerId = created1.Id, 
            InvoiceNumber = "INV-C1-002", 
            Amount = 200, 
            InvoiceDate = DateTime.Now 
        });
        AppDB.AddInvoiceWithAutoId(new Invoice 
        { 
            CustomerId = created2!.Id, 
            InvoiceNumber = "INV-C2-001", 
            Amount = 300, 
            InvoiceDate = DateTime.Now 
        });

        // Act
        var invoices = AppDB.GetInvoicesByCustomer(created1.Id);

        // Assert
        Assert.That(invoices, Has.Count.EqualTo(2));
        Assert.That(invoices, Has.All.Matches<Invoice>(i => i.CustomerId == created1.Id));
    }

    [Test]
    public void RemoveInvoice_ByInvoiceNumber_ShouldDeleteInvoice()
    {
        // Arrange
        var customer = new Customer { Name = "Test Customer", Email = "test@example.com" };
        var created = AppDB.AddCustomerWithAutoId(customer);
        
        var invoice = new Invoice 
        { 
            CustomerId = created!.Id, 
            InvoiceNumber = "INV-DELETE-TEST", 
            Amount = 100, 
            InvoiceDate = DateTime.Now 
        };
        AppDB.AddInvoiceWithAutoId(invoice);

        // Act
        var result = AppDB.RemoveInvoice("INV-DELETE-TEST");

        // Assert
        Assert.That(result, Is.True);
        Assert.That(AppDB.FindInvoice("INV-DELETE-TEST"), Is.Null);
    }

    [Test]
    public void RemoveInvoicesbyCustomer_ShouldDeleteAllCustomerInvoices()
    {
        // Arrange
        var customer = new Customer { Name = "Test Customer", Email = "test@example.com" };
        var created = AppDB.AddCustomerWithAutoId(customer);
        
        AppDB.AddInvoiceWithAutoId(new Invoice 
        { 
            CustomerId = created!.Id, 
            InvoiceNumber = "INV-001", 
            Amount = 100, 
            InvoiceDate = DateTime.Now 
        });
        AppDB.AddInvoiceWithAutoId(new Invoice 
        { 
            CustomerId = created.Id, 
            InvoiceNumber = "INV-002", 
            Amount = 200, 
            InvoiceDate = DateTime.Now 
        });

        // Act
        var result = AppDB.RemoveInvoicesbyCustomer(created.Id);

        // Assert
        Assert.That(result, Is.True);
        Assert.That(AppDB.GetInvoicesByCustomer(created.Id), Is.Empty);
    }

    #endregion

    #region Phone Number Tests

    [Test]
    public void AddPhoneNumberWithAutoId_ShouldGenerateUniqueIdGlobally()
    {
        // Arrange
        var customer = new Customer { Name = "Test Customer", Email = "test@example.com" };
        var created = AppDB.AddCustomerWithAutoId(customer);
        
        var phone1 = new PhoneNumber { CustomerId = created!.Id, Number = "555-1111", Type = "Mobile" };
        var phone2 = new PhoneNumber { CustomerId = created.Id, Number = "555-2222", Type = "Work" };

        // Act
        var result1 = AppDB.AddPhoneNumberWithAutoId(phone1);
        var result2 = AppDB.AddPhoneNumberWithAutoId(phone2);

        // Assert
        Assert.That(result1, Is.Not.Null);
        Assert.That(result2, Is.Not.Null);
        Assert.That(result1!.Id, Is.GreaterThan(0));
        Assert.That(result2!.Id, Is.GreaterThan(result1.Id));
        Assert.That(result2.Id - result1.Id, Is.EqualTo(1));
    }

    [Test]
    public void FindPhoneNumber_WithValidId_ShouldReturnPhoneNumber()
    {
        // Arrange
        var customer = new Customer { Name = "Test Customer", Email = "test@example.com" };
        var created = AppDB.AddCustomerWithAutoId(customer);
        
        var phone = new PhoneNumber { CustomerId = created!.Id, Number = "555-9999", Type = "DirectDial" };
        var createdPhone = AppDB.AddPhoneNumberWithAutoId(phone);

        // Act
        var found = AppDB.FindPhoneNumber(createdPhone!.Id);

        // Assert
        Assert.That(found, Is.Not.Null);
        Assert.That(found!.Number, Is.EqualTo("555-9999"));
        Assert.That(found.Type, Is.EqualTo("DirectDial"));
    }

    [Test]
    public void GetPhoneNumbersByCustomer_ShouldReturnOnlyCustomerPhones()
    {
        // Arrange
        var customer1 = new Customer { Name = "Customer 1", Email = "c1@example.com" };
        var customer2 = new Customer { Name = "Customer 2", Email = "c2@example.com" };
        
        var created1 = AppDB.AddCustomerWithAutoId(customer1);
        var created2 = AppDB.AddCustomerWithAutoId(customer2);
        
        AppDB.AddPhoneNumberWithAutoId(new PhoneNumber { CustomerId = created1!.Id, Number = "555-1111", Type = "Mobile" });
        AppDB.AddPhoneNumberWithAutoId(new PhoneNumber { CustomerId = created1.Id, Number = "555-2222", Type = "Work" });
        AppDB.AddPhoneNumberWithAutoId(new PhoneNumber { CustomerId = created2!.Id, Number = "555-3333", Type = "Mobile" });

        // Act
        var phones = AppDB.GetPhoneNumbersByCustomer(created1.Id);

        // Assert
        Assert.That(phones, Has.Count.EqualTo(2));
        Assert.That(phones, Has.All.Matches<PhoneNumber>(p => p.CustomerId == created1.Id));
    }

    [Test]
    public void RemovePhoneNumber_ShouldDeletePhoneNumber()
    {
        // Arrange
        var customer = new Customer { Name = "Test Customer", Email = "test@example.com" };
        var created = AppDB.AddCustomerWithAutoId(customer);
        
        var phone = new PhoneNumber { CustomerId = created!.Id, Number = "555-DELETE", Type = "Mobile" };
        var createdPhone = AppDB.AddPhoneNumberWithAutoId(phone);

        // Act
        var result = AppDB.RemovePhoneNumber(createdPhone!.Id);

        // Assert
        Assert.That(result, Is.True);
        Assert.That(AppDB.FindPhoneNumber(createdPhone.Id), Is.Null);
    }

    [Test]
    public void RemovePhonesbyCustomer_ShouldDeleteAllCustomerPhones()
    {
        // Arrange
        var customer = new Customer { Name = "Test Customer", Email = "test@example.com" };
        var created = AppDB.AddCustomerWithAutoId(customer);
        
        AppDB.AddPhoneNumberWithAutoId(new PhoneNumber { CustomerId = created!.Id, Number = "555-1111", Type = "Mobile" });
        AppDB.AddPhoneNumberWithAutoId(new PhoneNumber { CustomerId = created.Id, Number = "555-2222", Type = "Work" });

        // Act
        var result = AppDB.RemovePhonesbyCustomer(created.Id);

        // Assert
        Assert.That(result, Is.True);
        Assert.That(AppDB.GetPhoneNumbersByCustomer(created.Id), Is.Empty);
    }

    [Test]
    public void UpdatePhoneNumber_ShouldUpdatePhoneProperties()
    {
        // Arrange
        var customer = new Customer { Name = "Test Customer", Email = "test@example.com" };
        var created = AppDB.AddCustomerWithAutoId(customer);
        
        var phone = new PhoneNumber { CustomerId = created!.Id, Number = "555-ORIGINAL", Type = "Mobile" };
        var createdPhone = AppDB.AddPhoneNumberWithAutoId(phone);
        
        var updated = new PhoneNumber { Number = "555-UPDATED", Type = "Work" };

        // Act
        var result = AppDB.UpdatePhoneNumber(createdPhone!.Id, updated);

        // Assert
        Assert.That(result, Is.True);
        var found = AppDB.FindPhoneNumber(createdPhone.Id);
        Assert.That(found!.Number, Is.EqualTo("555-UPDATED"));
        Assert.That(found.Type, Is.EqualTo("Work"));
    }

    #endregion

    #region Global ID Tests

    [Test]
    public void InvoiceIds_ShouldBeGloballyUnique_AcrossAllCustomers()
    {
        // Arrange
        var customer1 = new Customer { Name = "Customer 1", Email = "c1@example.com" };
        var customer2 = new Customer { Name = "Customer 2", Email = "c2@example.com" };
        
        var created1 = AppDB.AddCustomerWithAutoId(customer1);
        var created2 = AppDB.AddCustomerWithAutoId(customer2);

        // Act - Create invoices for different customers
        var invoice1 = AppDB.AddInvoiceWithAutoId(new Invoice 
        { 
            CustomerId = created1!.Id, 
            InvoiceNumber = "INV-C1-001", 
            Amount = 100, 
            InvoiceDate = DateTime.Now 
        });
        var invoice2 = AppDB.AddInvoiceWithAutoId(new Invoice 
        { 
            CustomerId = created2!.Id, 
            InvoiceNumber = "INV-C2-001", 
            Amount = 200, 
            InvoiceDate = DateTime.Now 
        });
        var invoice3 = AppDB.AddInvoiceWithAutoId(new Invoice 
        { 
            CustomerId = created1.Id, 
            InvoiceNumber = "INV-C1-002", 
            Amount = 300, 
            InvoiceDate = DateTime.Now 
        });

        // Assert - IDs should be sequential globally
        Assert.That(invoice2!.Id, Is.EqualTo(invoice1!.Id + 1));
        Assert.That(invoice3!.Id, Is.EqualTo(invoice2.Id + 1));
    }

    [Test]
    public void PhoneNumberIds_ShouldBeGloballyUnique_AcrossAllCustomers()
    {
        // Arrange
        var customer1 = new Customer { Name = "Customer 1", Email = "c1@example.com" };
        var customer2 = new Customer { Name = "Customer 2", Email = "c2@example.com" };
        
        var created1 = AppDB.AddCustomerWithAutoId(customer1);
        var created2 = AppDB.AddCustomerWithAutoId(customer2);

        // Act - Create phone numbers for different customers
        var phone1 = AppDB.AddPhoneNumberWithAutoId(new PhoneNumber 
        { 
            CustomerId = created1!.Id, 
            Number = "555-1111", 
            Type = "Mobile" 
        });
        var phone2 = AppDB.AddPhoneNumberWithAutoId(new PhoneNumber 
        { 
            CustomerId = created2!.Id, 
            Number = "555-2222", 
            Type = "Work" 
        });
        var phone3 = AppDB.AddPhoneNumberWithAutoId(new PhoneNumber 
        { 
            CustomerId = created1.Id, 
            Number = "555-3333", 
            Type = "DirectDial" 
        });

        // Assert - IDs should be sequential globally
        Assert.That(phone2!.Id, Is.EqualTo(phone1!.Id + 1));
        Assert.That(phone3!.Id, Is.EqualTo(phone2.Id + 1));
    }

    #endregion
}
