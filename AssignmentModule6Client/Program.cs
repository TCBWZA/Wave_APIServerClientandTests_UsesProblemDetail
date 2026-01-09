using AssignmentModule6Client;
using AssignmentModule6Client.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Text.Json;

// Build configuration
var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "Production"}.json", optional: true)
    .Build();

// Setup dependency injection
var services = new ServiceCollection();

// Configure logging
services.AddLogging(builder =>
{
    builder.AddConfiguration(configuration.GetSection("Logging"));
    builder.AddConsole();
    builder.AddDebug();
});

// Register services
services.AddSingleton<IConfiguration>(configuration);

var serviceProvider = services.BuildServiceProvider();
var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();

// Read configuration
var baseUrl = configuration["ApiSettings:BaseUrl"] ?? "http://localhost:7136";
var apiToken = configuration["ApiSettings:ApiToken"] ?? "ThisIsANewToken";
var enableDetailedLogging = configuration.GetValue<bool>("AppSettings:EnableDetailedLogging", true);
var showFullStackTrace = configuration.GetValue<bool>("AppSettings:ShowFullStackTrace", false);

logger.LogInformation("==================================================");
logger.LogInformation("  AssignmentModule6 API Client - Example Usage");
logger.LogInformation("==================================================");
logger.LogInformation("Configuration loaded:");
logger.LogInformation("  Base URL: {BaseUrl}", baseUrl);
logger.LogInformation("  API Token: {TokenPreview}", string.IsNullOrEmpty(apiToken) ? "NOT SET" : $"{apiToken[..4]}***");
logger.LogInformation("  Detailed Logging: {DetailedLogging}", enableDetailedLogging);

Console.WriteLine("\n==================================================");
Console.WriteLine("  AssignmentModule6 API Client - Example Usage");
Console.WriteLine("==================================================");
Console.WriteLine();

// Initialize the API client with logger
var apiClientLogger = loggerFactory.CreateLogger<ApiClient>();
using var apiClient = new ApiClient(baseUrl, apiToken, apiClientLogger);

try
{
    // ====================================================================
    // Example 1: Get All Customers and Loop Through Details
    // ====================================================================
    logger.LogInformation("Starting Example 1: Get All Customers");
    Console.WriteLine("\n--- Example 1: Get All Customers and Display Details ---\n");
    
    var customers = await apiClient.GetFromJsonAsync<List<Customer>>("/api/Customers");
    
    if (customers != null && customers.Any())
    {
        logger.LogInformation("Retrieved {Count} customers from API", customers.Count);
        Console.WriteLine($"Total Customers: {customers.Count}\n");
        
        foreach (var customer in customers.Take(3))
        {
            logger.LogDebug("Processing customer ID: {CustomerId}", customer.Id);
            
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Customer ID: {customer.Id}");
            Console.WriteLine($"  Name: {customer.Name}");
            Console.WriteLine($"  Email: {customer.Email}");
            Console.WriteLine($"  Balance: ${customer.Balance:F2}");
            Console.ResetColor();
            
            if (customer.Invoices != null && customer.Invoices.Any())
            {
                Console.WriteLine($"  Invoices ({customer.Invoices.Count}):");
                foreach (var invoice in customer.Invoices)
                {
                    Console.WriteLine($"    - Invoice #{invoice.InvoiceNumber}: ${invoice.Amount:F2} (Date: {invoice.InvoiceDate:yyyy-MM-dd})");
                }
            }
            
            if (customer.PhoneNumbers != null && customer.PhoneNumbers.Any())
            {
                Console.WriteLine($"  Phone Numbers ({customer.PhoneNumbers.Count}):");
                foreach (var phone in customer.PhoneNumbers)
                {
                    Console.WriteLine($"    - {phone.Type}: {phone.Number}");
                }
            }
            
            Console.WriteLine();
        }
        
        logger.LogInformation("Example 1 completed successfully");
    }
    else
    {
        logger.LogWarning("No customers found in the API response");
    }

    // ====================================================================
    // Example 2: Create a New Customer with Invoices and Phone Numbers
    // ====================================================================
    logger.LogInformation("Starting Example 2: Create New Customer");
    Console.WriteLine("\n--- Example 2: Create New Customer ---\n");
    
    var newCustomer = new Customer
    {
        Name = "TechCorp Solutions Ltd",
        Email = "contact@techcorp.com",
        Invoices = new List<Invoice>
        {
            new Invoice
            {
                InvoiceNumber = "INV-TECH001",
                InvoiceDate = DateTime.Now.AddDays(-30),
                Amount = 1500.00M
            },
            new Invoice
            {
                InvoiceNumber = "INV-TECH002",
                InvoiceDate = DateTime.Now.AddDays(-15),
                Amount = 2750.50M
            },
            new Invoice
            {
                InvoiceNumber = "INV-TECH003",
                InvoiceDate = DateTime.Now.AddDays(-5),
                Amount = 899.99M
            }
        },
        PhoneNumbers = new List<PhoneNumber>
        {
            new PhoneNumber { Type = "Mobile", Number = "+44 7700 900123" },
            new PhoneNumber { Type = "Work", Number = "+44 20 7946 0958" },
            new PhoneNumber { Type = "DirectDial", Number = "+44 20 7946 0959" }
        }
    };

    var createdCustomer = await apiClient.PostAsync<Customer, Customer>("/api/Customers", newCustomer);
    
    if (createdCustomer != null)
    {
        logger.LogInformation("Customer created successfully with ID: {CustomerId}", createdCustomer.Id);
        
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"✓ Customer created successfully!");
        Console.WriteLine($"  ID: {createdCustomer.Id}");
        Console.WriteLine($"  Name: {createdCustomer.Name}");
        Console.WriteLine($"  Balance: ${createdCustomer.Balance:F2}");
        Console.WriteLine($"  Invoices: {createdCustomer.Invoices?.Count ?? 0}");
        Console.WriteLine($"  Phone Numbers: {createdCustomer.PhoneNumbers?.Count ?? 0}");
        Console.ResetColor();
        
        logger.LogInformation("Example 2 completed successfully");
    }

    // ====================================================================
    // Example 3: Try to Create Duplicate Customer (Error Handling)
    // ====================================================================
    logger.LogInformation("Starting Example 3: Attempt Duplicate Customer Creation");
    Console.WriteLine("\n--- Example 3: Attempt to Create Duplicate Customer (by Name/Email) ---\n");
    
    try
    {
        // Try to create a customer with the same name and email
        var duplicateCustomer = new Customer
        {
            Name = "TechCorp Solutions Ltd",  // Same name as Example 2
            Email = "contact@techcorp.com",    // Same email as Example 2
            Invoices = new List<Invoice>
            {
                new Invoice
                {
                    InvoiceNumber = $"INV-DUP{DateTime.Now.Ticks}",  // Different invoice number
                    InvoiceDate = DateTime.Now,
                    Amount = 500.00M
                }
            },
            PhoneNumbers = new List<PhoneNumber>
            {
                new PhoneNumber { Type = "Mobile", Number = "+44 7700 111222" }
            }
        };
        
        await apiClient.PostAsync<Customer, Customer>("/api/Customers", duplicateCustomer);
        logger.LogWarning("Duplicate customer creation succeeded unexpectedly");
        Console.WriteLine("Customer created (unexpected - duplicate should fail)");
    }
    catch (HttpRequestException ex)
    {
        logger.LogInformation("Expected error caught: {Message}, StatusCode: {StatusCode}", ex.Message, ex.StatusCode);
        
        var problemDetail = await ParseProblemDetails(ex.Message);
        LogProblemDetails(logger, problemDetail, "Duplicate Customer Creation", "Expected");
        
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"✓ Expected error caught!");
        Console.WriteLine($"  Status: {ex.StatusCode}");
        Console.WriteLine($"  Message: {ex.Message}");
        Console.WriteLine($"  Reason: Duplicate customer name/email detected (409 Conflict)");
        Console.ResetColor();
    }

    // ====================================================================
    // Example 4: Delete Customer ID 10
    // ====================================================================
    logger.LogInformation("Starting Example 4: Delete Customer ID 10");
    Console.WriteLine("\n--- Example 4: Delete Customer ID 10 ---\n");
    
    try
    {
        var deleted = await apiClient.DeleteAsync("/api/Customers/10");
        
        if (deleted)
        {
            logger.LogInformation("Customer ID 10 deleted successfully");
            
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("✓ Customer ID 10 deleted successfully!");
            Console.WriteLine("  All associated invoices and phone numbers also removed (cascade delete)");
            Console.ResetColor();
        }
        
        logger.LogInformation("Example 4 completed successfully");
    }
    catch (HttpRequestException ex)
    {
        logger.LogWarning("Delete operation failed: {Message}, StatusCode: {StatusCode}", ex.Message, ex.StatusCode);
        
        var problemDetail = await ParseProblemDetails(ex.Message);
        LogProblemDetails(logger, problemDetail, "Delete Customer", "Failed");
        
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"Delete operation failed:");
        Console.WriteLine($"  Status: {ex.StatusCode}");
        Console.WriteLine($"  Message: {ex.Message}");
        Console.ResetColor();
    }

    // ====================================================================
    // Example 5: Try to Delete Same Customer Again (Error Handling)
    // ====================================================================
    logger.LogInformation("Starting Example 5: Attempt to Delete Customer ID 10 Again");
    Console.WriteLine("\n--- Example 5: Attempt to Delete Customer ID 10 Again ---\n");
    
    try
    {
        var deleted = await apiClient.DeleteAsync("/api/Customers/10");
        
        if (deleted)
        {
            logger.LogWarning("Customer ID 10 deleted again unexpectedly");
            Console.WriteLine("Customer deleted (unexpected)");
        }
    }
    catch (HttpRequestException ex)
    {
        logger.LogInformation("Expected error caught: {Message}, StatusCode: {StatusCode}", ex.Message, ex.StatusCode);
        
        var problemDetail = await ParseProblemDetails(ex.Message);
        LogProblemDetails(logger, problemDetail, "Delete Non-Existing Customer", "Expected");
        
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"✓ Expected error caught!");
        Console.WriteLine($"  Status: {ex.StatusCode}");
        Console.WriteLine($"  Message: {ex.Message}");
        Console.WriteLine($"  Reason: Customer not found - already deleted (404 Not Found)");
        Console.ResetColor();
    }

    // ====================================================================
    // Example 6: Add Invoice to Existing Customer ID 5
    // ====================================================================
    logger.LogInformation("Starting Example 6: Add Invoice to Customer ID 5");
    Console.WriteLine("\n--- Example 6: Add Invoice to Customer ID 5 ---\n");
    
    try
    {
        var newInvoice = new Invoice
        {
            CustomerId = 5,
            InvoiceNumber = $"INV-NEW{DateTime.Now.Ticks}",
            InvoiceDate = DateTime.Now,
            Amount = 3500.00M
        };

        var createdInvoice = await apiClient.PostAsync<Invoice, Invoice>("/api/Invoices", newInvoice);
        
        if (createdInvoice != null)
        {
            logger.LogInformation("Invoice created successfully: {InvoiceNumber} for Customer ID: {CustomerId}", 
                createdInvoice.InvoiceNumber, createdInvoice.CustomerId);
            
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"✓ Invoice created successfully!");
            Console.WriteLine($"  Invoice Number: {createdInvoice.InvoiceNumber}");
            Console.WriteLine($"  Customer ID: {createdInvoice.CustomerId}");
            Console.WriteLine($"  Amount: ${createdInvoice.Amount:F2}");
            Console.WriteLine($"  Global Invoice ID: {createdInvoice.Id}");
            Console.ResetColor();
        }
        
        logger.LogInformation("Example 6 completed successfully");
    }
    catch (HttpRequestException ex)
    {
        logger.LogError(ex, "Failed to add invoice to Customer ID 5");
        
        var problemDetail = await ParseProblemDetails(ex.Message);
        LogProblemDetails(logger, problemDetail, "Add Invoice", "Failed");
        
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"✗ Failed to add invoice!");
        Console.WriteLine($"  Status: {ex.StatusCode}");
        Console.WriteLine($"  Message: {ex.Message}");
        Console.ResetColor();
    }

    // ====================================================================
    // Example 7: Add Invoice to Non-Existing Customer ID 99 (Error Handling)
    // ====================================================================
    logger.LogInformation("Starting Example 7: Attempt to Add Invoice to Non-Existing Customer ID 99");
    Console.WriteLine("\n--- Example 7: Attempt to Add Invoice to Non-Existing Customer ID 99 ---\n");
    
    try
    {
        var invalidInvoice = new Invoice
        {
            CustomerId = 99,
            InvoiceNumber = $"INV-INVALID{DateTime.Now.Ticks}",
            InvoiceDate = DateTime.Now,
            Amount = 1000.00M
        };

        await apiClient.PostAsync<Invoice, Invoice>("/api/Invoices", invalidInvoice);
        logger.LogWarning("Invoice created for non-existing customer unexpectedly");
        Console.WriteLine("Invoice created (unexpected)");
    }
    catch (HttpRequestException ex)
    {
        logger.LogInformation("Expected error caught: {Message}, StatusCode: {StatusCode}", ex.Message, ex.StatusCode);
        
        var problemDetail = await ParseProblemDetails(ex.Message);
        LogProblemDetails(logger, problemDetail, "Add Invoice to Non-Existing Customer", "Expected");
        
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"✓ Expected error caught!");
        Console.WriteLine($"  Status: {ex.StatusCode}");
        Console.WriteLine($"  Message: {ex.Message}");
        Console.WriteLine($"  Reason: Customer ID 99 does not exist (400 Bad Request or 404 Not Found)");
        Console.ResetColor();
    }

    // ====================================================================
    // Example 7a: Add Invoice with Invalid Invoice Number (Error Handling)
    // ====================================================================
    logger.LogInformation("Starting Example 7a: Attempt to Add Invoice with Invalid Invoice Number");
    Console.WriteLine("\n--- Example 7a: Attempt to Add Invoice with Invalid Invoice Number to Customer ID 5 ---\n");
    
    try
    {
        // Try to create an invoice with an invoice number that doesn't start with "INV"
        var invalidFormatInvoice = new Invoice
        {
            CustomerId = 5,
            InvoiceNumber = "ORDER-12345",  // Invalid: doesn't start with "INV"
            InvoiceDate = DateTime.Now,
            Amount = 750.00M
        };

        await apiClient.PostAsync<Invoice, Invoice>("/api/Invoices", invalidFormatInvoice);
        logger.LogWarning("Invoice with invalid format created unexpectedly");
        Console.WriteLine("Invoice created (unexpected - invalid format should fail)");
    }
    catch (HttpRequestException ex)
    {
        logger.LogInformation("Expected error caught: {Message}, StatusCode: {StatusCode}", ex.Message, ex.StatusCode);
        
        var problemDetail = await ParseProblemDetails(ex.Message);
        LogProblemDetails(logger, problemDetail, "Add Invoice with Invalid Format", "Expected");
        
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"✓ Expected error caught!");
        Console.WriteLine($"  Status: {ex.StatusCode}");
        Console.WriteLine($"  Title: {problemDetail?.Title ?? "Validation Error"}");
        Console.WriteLine($"  Detail: {problemDetail?.Detail ?? ex.Message}");
        Console.ResetColor();
        
        // Display validation errors using the new helper
        DisplayValidationErrors(problemDetail);
    }

    // Try another invalid scenario - empty invoice number
    logger.LogInformation("Starting Example 7b: Attempt to Add Invoice with Empty Invoice Number");
    Console.WriteLine("\n--- Example 7b: Attempt to Add Invoice with Empty Invoice Number to Customer ID 5 ---\n");
    
    try
    {
        var emptyInvoiceNumber = new Invoice
        {
            CustomerId = 5,
            InvoiceNumber = "",  // Invalid: empty invoice number
            InvoiceDate = DateTime.Now,
            Amount = 500.00M
        };

        await apiClient.PostAsync<Invoice, Invoice>("/api/Invoices", emptyInvoiceNumber);
        logger.LogWarning("Invoice with empty number created unexpectedly");
        Console.WriteLine("Invoice created (unexpected - empty invoice number should fail)");
    }
    catch (HttpRequestException ex)
    {
        logger.LogInformation("Expected error caught: {Message}, StatusCode: {StatusCode}", ex.Message, ex.StatusCode);
        
        var problemDetail = await ParseProblemDetails(ex.Message);
        LogProblemDetails(logger, problemDetail, "Add Invoice with Empty Number", "Expected");
        
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"✓ Expected error caught!");
        Console.WriteLine($"  Status: {ex.StatusCode}");
        Console.WriteLine($"  Title: {problemDetail?.Title ?? "Validation Error"}");
        Console.WriteLine($"  Detail: {problemDetail?.Detail ?? ex.Message}");
        Console.ResetColor();
        
        // Display validation errors using the new helper
        DisplayValidationErrors(problemDetail);
    }

    // ====================================================================
    // Example 7c: Multiple Validation Errors at Once
    // ====================================================================
    logger.LogInformation("Starting Example 7c: Attempt to Add Invoice with Multiple Validation Errors");
    Console.WriteLine("\n--- Example 7c: Multiple Validation Errors (Invalid Format + Invalid Customer) ---\n");
    
    try
    {
        // Create an invoice with MULTIPLE validation errors
        var multipleErrorsInvoice = new Invoice
        {
            CustomerId = 0,           // Invalid: must be > 0
            InvoiceNumber = "QUOTE-999",  // Invalid: doesn't start with "INV"
            InvoiceDate = DateTime.Now,
            Amount = 500.00M
        };

        await apiClient.PostAsync<Invoice, Invoice>("/api/Invoices", multipleErrorsInvoice);
        logger.LogWarning("Invoice with multiple errors created unexpectedly");
        Console.WriteLine("Invoice created (unexpected - multiple validation errors should fail)");
    }
    catch (HttpRequestException ex)
    {
        logger.LogInformation("Expected error caught: {Message}, StatusCode: {StatusCode}", ex.Message, ex.StatusCode);
        
        var problemDetail = await ParseProblemDetails(ex.Message);
        LogProblemDetails(logger, problemDetail, "Add Invoice with Multiple Errors", "Expected");
        
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"✓ Expected error caught!");
        Console.WriteLine($"  Status: {ex.StatusCode}");
        Console.WriteLine($"  Title: {problemDetail?.Title ?? "Validation Error"}");
        Console.WriteLine($"  Detail: {problemDetail?.Detail ?? ex.Message}");
        
        // Show count of validation errors
        if (problemDetail?.Errors != null)
        {
            Console.WriteLine($"  Total Fields with Errors: {problemDetail.Errors.Count}");
        }
        Console.ResetColor();
        
        // Display all validation errors
        DisplayValidationErrors(problemDetail);
        
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("\n  💡 Note: Multiple validation errors were caught and displayed above.");
        Console.ResetColor();
    }

    // ====================================================================
    // Summary
    // ====================================================================
    logger.LogInformation("All examples completed successfully");
    
    Console.WriteLine("\n==================================================");
    Console.WriteLine("  All Examples Completed Successfully!");
    Console.WriteLine("==================================================");
    Console.WriteLine("\n📊 Summary of Operations:");
    Console.WriteLine("  ✓ Retrieved and displayed customer data with nested collections");
    Console.WriteLine("  ✓ Created new customer with 3 invoices and 3 phone numbers");
    Console.WriteLine("  ✓ Demonstrated duplicate customer error handling (409 Conflict)");
    Console.WriteLine("  ✓ Successfully deleted customer with cascade deletion");
    Console.WriteLine("  ✓ Demonstrated not found error handling (404)");
    Console.WriteLine("  ✓ Added invoice to existing customer");
    Console.WriteLine("  ✓ Demonstrated invalid customer ID error handling");
    Console.WriteLine("  ✓ Demonstrated invalid invoice number format validation");
    Console.WriteLine("  ✓ Demonstrated empty invoice number validation");
    Console.WriteLine("  ✓ Demonstrated multiple validation errors at once");
    Console.WriteLine("\n🎯 All API client features working correctly!");
}
catch (Exception ex)
{
    logger.LogCritical(ex, "Unexpected critical error occurred in main program loop");
    
    var problemDetail = new ProblemDetails
    {
        Type = "UnexpectedError",
        Title = "Unexpected Error",
        Status = 500,
        Detail = ex.Message,
        Instance = "Program.Main"
    };
    
    LogProblemDetails(logger, problemDetail, "Critical Error", "Unexpected");
    
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine($"\n✗ Unexpected error occurred!");
    Console.WriteLine($"  Type: {ex.GetType().Name}");
    Console.WriteLine($"  Message: {ex.Message}");
    
    if (ex.InnerException != null)
    {
        Console.WriteLine($"  Inner Exception: {ex.InnerException.Message}");
        logger.LogError("Inner exception: {InnerException}", ex.InnerException.Message);
    }
    
    if (showFullStackTrace)
    {
        Console.WriteLine($"\nStack Trace:\n{ex.StackTrace}");
    }
    
    Console.ResetColor();
}
finally
{
    logger.LogInformation("Application shutting down");
}

Console.WriteLine("\nPress any key to exit...");
Console.ReadKey();

// Helper method to parse problem details from error message
static async Task<ProblemDetails?> ParseProblemDetails(string errorMessage)
{
    try
    {
        var startIndex = errorMessage.IndexOf('{');
        if (startIndex >= 0)
        {
            var jsonPart = errorMessage[startIndex..];
            var endIndex = jsonPart.LastIndexOf('}');
            if (endIndex >= 0)
            {
                jsonPart = jsonPart[..(endIndex + 1)];
                return JsonSerializer.Deserialize<ProblemDetails>(jsonPart, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
        }
    }
    catch
    {
        // If parsing fails, return null
    }
    
    return null;
}

// Helper method to display validation errors to console
static void DisplayValidationErrors(ProblemDetails? problemDetails)
{
    if (problemDetails?.Errors != null && problemDetails.Errors.Any())
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"\n  📋 Validation Errors ({problemDetails.Errors.Count} field{(problemDetails.Errors.Count > 1 ? "s" : "")} failed):");
        Console.ResetColor();
        
        foreach (var error in problemDetails.Errors)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"    ❌ Field: {error.Key}");
            Console.ResetColor();
            
            foreach (var message in error.Value)
            {
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine($"       • {message}");
                Console.ResetColor();
            }
        }
    }
}

// Helper method to log problem details in structured format
static void LogProblemDetails(ILogger logger, ProblemDetails? problemDetails, string operation, string severity)
{
    if (problemDetails != null)
    {
        if (severity == "Expected")
        {
            logger.LogInformation(
                "Problem Details - Operation: {Operation}, Type: {Type}, Status: {Status}, Title: {Title}, Detail: {Detail}, Instance: {Instance}",
                operation, problemDetails.Type, problemDetails.Status, problemDetails.Title, 
                problemDetails.Detail, problemDetails.Instance);
        }
        else if (severity == "Failed")
        {
            logger.LogWarning(
                "Problem Details - Operation: {Operation}, Type: {Type}, Status: {Status}, Title: {Title}, Detail: {Detail}, Instance: {Instance}",
                operation, problemDetails.Type, problemDetails.Status, problemDetails.Title, 
                problemDetails.Detail, problemDetails.Instance);
        }
        else
        {
            logger.LogError(
                "Problem Details - Operation: {Operation}, Type: {Type}, Status: {Status}, Title: {Title}, Detail: {Detail}, Instance: {Instance}",
                operation, problemDetails.Type, problemDetails.Status, problemDetails.Title, 
                problemDetails.Detail, problemDetails.Instance);
        }
        
        // Log validation errors if present
        if (problemDetails.Errors != null && problemDetails.Errors.Any())
        {
            logger.LogWarning("Validation Errors ({ErrorCount} fields):", problemDetails.Errors.Count);
            foreach (var error in problemDetails.Errors)
            {
                logger.LogWarning("  Field: {Field}, Errors: {Errors}", error.Key, string.Join(", ", error.Value));
            }
        }
    }
}
