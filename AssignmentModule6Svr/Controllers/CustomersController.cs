using Microsoft.AspNetCore.Mvc;
using AssignmentModule6Svr.Classes;
using AssignmentModule6Svr.Attributes;

namespace AssignmentModule6Svr.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CustomersController : ControllerBase
{
    private readonly ILogger<CustomersController> _logger;
    public CustomersController(ILogger<CustomersController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Get all customers
    /// </summary>
    /// <returns>List of all customers</returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult<List<Customer>> GetAllCustomers()
    {
        _logger.LogDebug("GetAllCustomers: Retrieving all customers");
        var customers = AppDB.GetCustomerList();
        _logger.LogDebug("GetAllCustomers: Returned {Count} customers", customers.Count);
        return Ok(customers);
    }

    /// <summary>
    /// Get a specific customer by ID
    /// </summary>
    /// <param name="id">Customer ID</param>
    /// <returns>Customer details</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ProblemDetails))]
    public ActionResult<Customer> GetCustomer(long id)
    {
        _logger.LogDebug("GetCustomer: Retrieving customer with ID {CustomerId}", id);
        var customer = AppDB.FindCustomer(id);

        if (customer == null)
        {
            _logger.LogDebug("GetCustomer: Customer with ID {CustomerId} not found", id);
            
            var notFoundProblem = new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Customer Not Found",
                Detail = $"Customer with ID {id} does not exist in the system.",
                Instance = $"{HttpContext.Request.Method} {HttpContext.Request.Path}",
                Extensions =
                {
                    ["traceId"] = HttpContext.TraceIdentifier,
                    ["customerId"] = id,
                    ["suggestion"] = "Verify the customer ID is correct or use GET /api/customers to list all customers"
                }
            };
            
            return NotFound(notFoundProblem);
        }

        // Populate phone numbers and invoices
        customer.PhoneNumbers = AppDB.GetPhoneNumbersByCustomer(id);
        customer.Invoices = AppDB.GetInvoicesByCustomer(id);

        _logger.LogDebug("GetCustomer: Successfully retrieved customer {CustomerId} with {PhoneCount} phone numbers and {InvoiceCount} invoices", 
            id, customer.PhoneNumbers?.Count ?? 0, customer.Invoices?.Count ?? 0);
        return Ok(customer);
    }

    /// <summary>
    /// Create a new customer
    /// </summary>
    /// <param name="customer">Customer object</param>
    /// <returns>Created customer</returns>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ValidationProblemDetails))]
    [ProducesResponseType(StatusCodes.Status409Conflict, Type = typeof(ProblemDetails))]
    public ActionResult<Customer> CreateCustomer([FromBody] Customer customer)
    {
        _logger.LogDebug("CreateCustomer: Attempting to create customer");

        if (customer == null)
        {
            _logger.LogDebug("CreateCustomer: Customer data is null");
            
            var validationProblem = new ValidationProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Invalid Customer Data",
                Detail = "Customer data is required. The request body cannot be null or empty.",
                Instance = $"{HttpContext.Request.Method} {HttpContext.Request.Path}"
            };
            validationProblem.Errors.Add("customer", new[] { "Customer object is required" });
            validationProblem.Extensions["traceId"] = HttpContext.TraceIdentifier;
            
            return BadRequest(validationProblem);
        }

        // Check for duplicate customer by name or email
        if (AppDB.IsDuplicateCustomer(customer.Name, customer.Email))
        {
            var existingByName = AppDB.FindCustomerByName(customer.Name ?? "");
            var existingByEmail = AppDB.FindCustomerByEmail(customer.Email ?? "");
            
            var duplicateFields = new List<string>();
            var conflictDetails = new List<string>();
            
            if (existingByName != null)
            {
                duplicateFields.Add("name");
                conflictDetails.Add($"Customer with name '{customer.Name}' already exists (ID: {existingByName.Id})");
            }
            
            if (existingByEmail != null && (existingByName == null || existingByEmail.Id != existingByName.Id))
            {
                duplicateFields.Add("email");
                conflictDetails.Add($"Customer with email '{customer.Email}' already exists (ID: {existingByEmail.Id})");
            }
            
            // Log detailed validation failure
            _logger.LogWarning("========================================");
            _logger.LogWarning("VALIDATION FAILURE - Duplicate Customer");
            _logger.LogWarning("========================================");
            _logger.LogWarning("Endpoint: {Method} {Path}", HttpContext.Request.Method, HttpContext.Request.Path);
            _logger.LogWarning("TraceId: {TraceId}", HttpContext.TraceIdentifier);
            _logger.LogWarning("Duplicate Fields: {Fields}", string.Join(", ", duplicateFields));
            _logger.LogWarning("Provided Name: {Name}", customer.Name ?? "(null)");
            _logger.LogWarning("Provided Email: {Email}", customer.Email ?? "(null)");
            _logger.LogWarning("Existing Customer ID: {ExistingId}", existingByName?.Id ?? existingByEmail?.Id ?? 0);
            _logger.LogWarning("----------------------------------------");
            foreach (var detail in conflictDetails)
            {
                _logger.LogWarning("  • {Detail}", detail);
            }
            _logger.LogWarning("========================================");
            
            var conflictProblem = new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = "Duplicate Customer",
                Detail = $"A customer with the provided {string.Join(" and/or ", duplicateFields)} already exists in the system. {string.Join(" ", conflictDetails)}",
                Instance = $"{HttpContext.Request.Method} {HttpContext.Request.Path}",
                Extensions =
                {
                    ["traceId"] = HttpContext.TraceIdentifier,
                    ["duplicateFields"] = duplicateFields,
                    ["existingCustomerId"] = existingByName?.Id ?? existingByEmail?.Id ?? 0,
                    ["providedName"] = customer.Name ?? "",
                    ["providedEmail"] = customer.Email ?? "",
                    ["suggestion"] = "Use PUT /api/customers/{id} to update the existing customer or provide a different name/email"
                }
            };
            
            return Conflict(conflictProblem);
        }

        // Always use auto-ID generation (ignore any provided ID)
        var createdCustomer = AppDB.AddCustomerWithAutoId(customer);
        
        if (createdCustomer == null)
        {
            _logger.LogDebug("CreateCustomer: Failed to create customer with auto-generated ID");
            
            var failureProblem = new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Failed to Create Customer",
                Detail = "Failed to create customer due to internal validation. Ensure all required fields are provided and valid.",
                Instance = $"{HttpContext.Request.Method} {HttpContext.Request.Path}",
                Extensions =
                {
                    ["traceId"] = HttpContext.TraceIdentifier
                }
            };
            
            return BadRequest(failureProblem);
        }

        _logger.LogInformation("CreateCustomer: Successfully created customer with ID {CustomerId}, Name: {Name}, Email: {Email}", 
            createdCustomer.Id, createdCustomer.Name, createdCustomer.Email);
        return CreatedAtAction(nameof(GetCustomer), new { id = createdCustomer.Id }, createdCustomer);
    }

    /// <summary>
    /// Update an existing customer
    /// </summary>
    /// <param name="id">Customer ID</param>
    /// <param name="customer">Updated customer object</param>
    /// <returns>Updated customer</returns>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ValidationProblemDetails))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ProblemDetails))]
    [ProducesResponseType(StatusCodes.Status409Conflict, Type = typeof(ProblemDetails))]
    public ActionResult<Customer> UpdateCustomer(long id, [FromBody] Customer customer)
    {
        _logger.LogDebug("UpdateCustomer: Attempting to update customer with ID {CustomerId}", id);

        if (customer == null)
        {
            _logger.LogDebug("UpdateCustomer: Customer data is null");
            
            var validationProblem = new ValidationProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Invalid Customer Data",
                Detail = "Customer data is required. The request body cannot be null or empty.",
                Instance = $"{HttpContext.Request.Method} {HttpContext.Request.Path}"
            };
            validationProblem.Errors.Add("customer", new[] { "Customer object is required" });
            validationProblem.Extensions["traceId"] = HttpContext.TraceIdentifier;
            validationProblem.Extensions["customerId"] = id;
            
            return BadRequest(validationProblem);
        }

        if (id <= 0)
        {
            _logger.LogDebug("UpdateCustomer: Invalid customer ID {CustomerId}", id);
            
            var validationProblem = new ValidationProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Invalid Customer ID",
                Detail = $"Customer ID must be greater than 0. Provided value: {id}",
                Instance = $"{HttpContext.Request.Method} {HttpContext.Request.Path}"
            };
            validationProblem.Errors.Add("id", new[] { "Customer ID must be greater than 0" });
            validationProblem.Extensions["traceId"] = HttpContext.TraceIdentifier;
            validationProblem.Extensions["customerId"] = id;
            
            return BadRequest(validationProblem);
        }

        var existingCustomer = AppDB.FindCustomer(id);
        if (existingCustomer == null)
        {
            _logger.LogDebug("UpdateCustomer: Customer with ID {CustomerId} not found", id);
            
            var notFoundProblem = new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Customer Not Found",
                Detail = $"Customer with ID {id} does not exist. Cannot update non-existent customer.",
                Instance = $"{HttpContext.Request.Method} {HttpContext.Request.Path}",
                Extensions =
                {
                    ["traceId"] = HttpContext.TraceIdentifier,
                    ["customerId"] = id,
                    ["suggestion"] = "Use POST to create a new customer or verify the customer ID is correct"
                }
            };
            
            return NotFound(notFoundProblem);
        }

        // Check for duplicate customer by name or email (excluding current customer)
        if (AppDB.IsDuplicateCustomer(customer.Name, customer.Email, id))
        {
            var existingByName = AppDB.FindCustomerByName(customer.Name ?? "");
            var existingByEmail = AppDB.FindCustomerByEmail(customer.Email ?? "");
            
            var duplicateFields = new List<string>();
            var conflictDetails = new List<string>();
            
            if (existingByName != null && existingByName.Id != id)
            {
                duplicateFields.Add("name");
                conflictDetails.Add($"Customer with name '{customer.Name}' already exists (ID: {existingByName.Id})");
            }
            
            if (existingByEmail != null && existingByEmail.Id != id && 
                (existingByName == null || existingByEmail.Id != existingByName.Id))
            {
                duplicateFields.Add("email");
                conflictDetails.Add($"Customer with email '{customer.Email}' already exists (ID: {existingByEmail.Id})");
            }
            
            // Log detailed validation failure
            _logger.LogWarning("========================================");
            _logger.LogWarning("VALIDATION FAILURE - Duplicate Customer (Update)");
            _logger.LogWarning("========================================");
            _logger.LogWarning("Endpoint: {Method} {Path}", HttpContext.Request.Method, HttpContext.Request.Path);
            _logger.LogWarning("TraceId: {TraceId}", HttpContext.TraceIdentifier);
            _logger.LogWarning("Customer ID being updated: {CustomerId}", id);
            _logger.LogWarning("Duplicate Fields: {Fields}", string.Join(", ", duplicateFields));
            _logger.LogWarning("Provided Name: {Name}", customer.Name ?? "(null)");
            _logger.LogWarning("Provided Email: {Email}", customer.Email ?? "(null)");
            _logger.LogWarning("Conflicting Customer ID: {ConflictingId}", existingByName?.Id ?? existingByEmail?.Id ?? 0);
            _logger.LogWarning("----------------------------------------");
            foreach (var detail in conflictDetails)
            {
                _logger.LogWarning("  • {Detail}", detail);
            }
            _logger.LogWarning("========================================");
            
            var conflictProblem = new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = "Duplicate Customer",
                Detail = $"Cannot update customer {id}. A different customer with the provided {string.Join(" and/or ", duplicateFields)} already exists. {string.Join(" ", conflictDetails)}",
                Instance = $"{HttpContext.Request.Method} {HttpContext.Request.Path}",
                Extensions =
                {
                    ["traceId"] = HttpContext.TraceIdentifier,
                    ["customerId"] = id,
                    ["duplicateFields"] = duplicateFields,
                    ["conflictingCustomerId"] = existingByName?.Id ?? existingByEmail?.Id ?? 0,
                    ["providedName"] = customer.Name ?? "",
                    ["providedEmail"] = customer.Email ?? "",
                    ["suggestion"] = "Provide a different name/email that doesn't conflict with existing customers"
                }
            };
            
            return Conflict(conflictProblem);
        }

        var success = AppDB.UpdateCustomer(id, customer);
        
        if (!success)
        {
            _logger.LogDebug("UpdateCustomer: Failed to update customer with ID {CustomerId}", id);
            
            var failureProblem = new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "Failed to Update Customer",
                Detail = $"An error occurred while attempting to update customer {id}.",
                Instance = $"{HttpContext.Request.Method} {HttpContext.Request.Path}",
                Extensions =
                {
                    ["traceId"] = HttpContext.TraceIdentifier,
                    ["customerId"] = id
                }
            };
            
            return StatusCode(StatusCodes.Status500InternalServerError, failureProblem);
        }

        // Retrieve the updated customer with all related data
        var updatedCustomer = AppDB.FindCustomer(id);
        if (updatedCustomer != null)
        {
            updatedCustomer.PhoneNumbers = AppDB.GetPhoneNumbersByCustomer(id);
            updatedCustomer.Invoices = AppDB.GetInvoicesByCustomer(id);
        }

        _logger.LogInformation("UpdateCustomer: Successfully updated customer with ID {CustomerId}, Name: {Name}, Email: {Email}", 
            id, updatedCustomer?.Name, updatedCustomer?.Email);
        return Ok(updatedCustomer);
    }

    /// <summary>
    /// Delete a customer by ID (Requires API Key authentication)
    /// </summary>
    /// <param name="id">Customer ID</param>
    /// <returns>No content on success</returns>
    [HttpDelete("{id}")]
    [ApiKeyAuth]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ProblemDetails))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ProblemDetails))]
    public ActionResult DeleteCustomer(long id)
    {
        _logger.LogDebug("DeleteCustomer: Attempting to delete customer with ID {CustomerId}", id);

        var customer = AppDB.FindCustomer(id);
        
        if (customer == null)
        {
            _logger.LogDebug("DeleteCustomer: Customer with ID {CustomerId} not found", id);
            
            var notFoundProblem = new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Customer Not Found",
                Detail = $"Customer with ID {id} does not exist or has already been deleted.",
                Instance = $"{HttpContext.Request.Method} {HttpContext.Request.Path}",
                Extensions =
                {
                    ["traceId"] = HttpContext.TraceIdentifier,
                    ["customerId"] = id
                }
            };
            
            return NotFound(notFoundProblem);
        }

        var invoiceCount = customer.Invoices?.Count ?? 0;
        var phoneCount = customer.PhoneNumbers?.Count ?? 0;

        var success = AppDB.RemoveCustomer(id);
        
        if (!success)
        {
            _logger.LogDebug("DeleteCustomer: Failed to delete customer with ID {CustomerId}", id);
            
            var failureProblem = new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "Failed to Delete Customer",
                Detail = $"An error occurred while attempting to delete customer {id}. The customer exists but could not be removed.",
                Instance = $"{HttpContext.Request.Method} {HttpContext.Request.Path}",
                Extensions =
                {
                    ["traceId"] = HttpContext.TraceIdentifier,
                    ["customerId"] = id,
                    ["customerName"] = customer.Name
                }
            };
            
            return StatusCode(StatusCodes.Status500InternalServerError, failureProblem);
        }

        _logger.LogDebug("DeleteCustomer: Successfully deleted customer with ID {CustomerId} along with {InvoiceCount} invoices and {PhoneCount} phone numbers", 
            id, invoiceCount, phoneCount);
        return NoContent();
    }

    /// <summary>
    /// Get all Invoices for a specific customer
    /// </summary>
    /// <param name="id">Customer ID</param>
    /// <returns>List of Invoices for the customer</returns>
    [HttpGet("{id}/Invoices")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ProblemDetails))]
    public ActionResult<List<Invoice>> GetCustomerInvoices(long id)
    {
        _logger.LogDebug("GetCustomerInvoices: Retrieving invoices for customer {CustomerId}", id);

        var customer = AppDB.FindCustomer(id);
        
        if (customer == null)
        {
            _logger.LogDebug("GetCustomerInvoices: Customer with ID {CustomerId} not found", id);
            
            var notFoundProblem = new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Customer Not Found",
                Detail = $"Customer with ID {id} does not exist. Cannot retrieve invoices for non-existent customer.",
                Instance = $"{HttpContext.Request.Method} {HttpContext.Request.Path}",
                Extensions =
                {
                    ["traceId"] = HttpContext.TraceIdentifier,
                    ["customerId"] = id,
                    ["suggestion"] = "Verify the customer ID is correct or use GET /api/customers to list all customers"
                }
            };
            
            return NotFound(notFoundProblem);
        }

        var invoices = AppDB.GetInvoicesByCustomer(id);
        _logger.LogDebug("GetCustomerInvoices: Retrieved {Count} invoices for customer {CustomerId}", invoices.Count, id);
        return Ok(invoices);
    }

}