using Microsoft.AspNetCore.Mvc;
using AssignmentModule6Svr.Classes;
using AssignmentModule6Svr.Attributes;

namespace AssignmentModule6Svr.Controllers;

[ApiController]
[Route("api/[controller]")]
public class InvoicesController : ControllerBase
{
    private readonly ILogger<InvoicesController> _logger;

    public InvoicesController(ILogger<InvoicesController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Get all Invoices
    /// </summary>
    /// <returns>List of all Invoices</returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult<List<Invoice>> GetAllInvoices()
    {
        _logger.LogDebug("GetAllInvoices: Retrieving all invoices");
        var invoices = AppDB.GetInvoiceList();
        _logger.LogDebug("GetAllInvoices: Returned {Count} invoices", invoices.Count);
        return Ok(invoices);
    }

    /// <summary>
    /// Get a specific invoice by customer ID and invoice number
    /// </summary>
    /// <param name="customerId">Customer ID</param>
    /// <param name="invoiceNumber">Invoice number</param>
    /// <returns>Invoice details</returns>
    [HttpGet("{customerId}/{invoiceNumber}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ProblemDetails))]
    public ActionResult<Invoice> GetInvoice(long customerId, string invoiceNumber)
    {
        _logger.LogDebug("GetInvoice: Retrieving invoice {InvoiceNumber} for customer {CustomerId}", invoiceNumber, customerId);
        var invoice = AppDB.FindInvoice(invoiceNumber);
        
        if (invoice == null)
        {
            _logger.LogDebug("GetInvoice: Invoice {InvoiceNumber} for customer {CustomerId} not found", invoiceNumber, customerId);
            
            var notFoundProblem = new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Invoice Not Found",
                Detail = $"Invoice with number '{invoiceNumber}' for customer {customerId} does not exist.",
                Instance = $"{HttpContext.Request.Method} {HttpContext.Request.Path}",
                Extensions =
                {
                    ["traceId"] = HttpContext.TraceIdentifier,
                    ["customerId"] = customerId,
                    ["invoiceNumber"] = invoiceNumber,
                    ["suggestion"] = "Verify the invoice number and customer ID are correct"
                }
            };
            
            return NotFound(notFoundProblem);
        }

        _logger.LogDebug("GetInvoice: Successfully retrieved invoice {InvoiceNumber}", invoiceNumber);
        return Ok(invoice);
    }

    /// <summary>
    /// Get all Invoices for a specific customer
    /// </summary>
    /// <param name="customerId">Customer ID</param>
    /// <returns>List of Invoices for the customer</returns>
    [HttpGet("customer/{customerId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ProblemDetails))]
    public ActionResult<List<Invoice>> GetInvoicesByCustomer(long customerId)
    {
        _logger.LogDebug("GetInvoicesByCustomer: Retrieving invoices for customer {CustomerId}", customerId);
        var customer = AppDB.FindCustomer(customerId);
        
        if (customer == null)
        {
            _logger.LogDebug("GetInvoicesByCustomer: Customer with ID {CustomerId} not found", customerId);
            
            var notFoundProblem = new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Customer Not Found",
                Detail = $"Customer with ID {customerId} does not exist. Cannot retrieve invoices for non-existent customer.",
                Instance = $"{HttpContext.Request.Method} {HttpContext.Request.Path}",
                Extensions =
                {
                    ["traceId"] = HttpContext.TraceIdentifier,
                    ["customerId"] = customerId,
                    ["suggestion"] = "Verify the customer ID is correct or create the customer first"
                }
            };
            
            return NotFound(notFoundProblem);
        }

        var invoices = AppDB.GetInvoicesByCustomer(customerId);
        _logger.LogDebug("GetInvoicesByCustomer: Retrieved {Count} invoices for customer {CustomerId}", invoices.Count, customerId);
        return Ok(invoices);
    }

    /// <summary>
    /// Create a new invoice
    /// </summary>
    /// <param name="invoice">Invoice object (invoiceNumber must be provided and start with 'INV')</param>
    /// <returns>Created invoice</returns>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ValidationProblemDetails))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ProblemDetails))]
    [ProducesResponseType(StatusCodes.Status409Conflict, Type = typeof(ProblemDetails))]
    public ActionResult<Invoice> CreateInvoice([FromBody] Invoice invoice)
    {
        _logger.LogDebug("CreateInvoice: Attempting to create invoice {InvoiceNumber} for customer {CustomerId}", 
            invoice?.InvoiceNumber, invoice?.CustomerId);

        if (invoice == null)
        {
            _logger.LogDebug("CreateInvoice: Invoice data is null");
            
            var validationProblem = new ValidationProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Invalid Invoice Data",
                Detail = "Invoice data is required. The request body cannot be null or empty.",
                Instance = $"{HttpContext.Request.Method} {HttpContext.Request.Path}"
            };
            validationProblem.Errors.Add("invoice", new[] { "Invoice object is required" });
            validationProblem.Extensions["traceId"] = HttpContext.TraceIdentifier;
            
            return BadRequest(validationProblem);
        }

        if (!ModelState.IsValid)
        {
            var errors = ModelState
                .Where(x => x.Value?.Errors.Count > 0)
                .ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value?.Errors.Select(e => e.ErrorMessage).ToArray() ?? Array.Empty<string>()
                );
            
            // Log validation failure with detailed information
            _logger.LogWarning("========================================");
            _logger.LogWarning("VALIDATION FAILURE");
            _logger.LogWarning("========================================");
            _logger.LogWarning("Endpoint: {Method} {Path}", HttpContext.Request.Method, HttpContext.Request.Path);
            _logger.LogWarning("TraceId: {TraceId}", HttpContext.TraceIdentifier);
            _logger.LogWarning("Invoice Number: {InvoiceNumber}", invoice.InvoiceNumber ?? "(null)");
            _logger.LogWarning("Customer ID: {CustomerId}", invoice.CustomerId);
            _logger.LogWarning("Total Fields with Errors: {ErrorCount}", errors.Count);
            _logger.LogWarning("----------------------------------------");
            
            foreach (var error in errors)
            {
                _logger.LogWarning("Field: {FieldName}", error.Key);
                foreach (var message in error.Value)
                {
                    _logger.LogWarning("  • {ErrorMessage}", message);
                }
            }
            
            _logger.LogWarning("========================================");
            
            var problemDetails = new ValidationProblemDetails(errors)
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Validation Failed",
                Detail = "One or more validation errors occurred. Please review the 'errors' property for details.",
                Instance = $"{HttpContext.Request.Method} {HttpContext.Request.Path}"
            };
            problemDetails.Extensions["traceId"] = HttpContext.TraceIdentifier;
            problemDetails.Extensions["errorCount"] = errors.Count;
            problemDetails.Extensions["invoiceNumber"] = invoice.InvoiceNumber;
            problemDetails.Extensions["customerId"] = invoice.CustomerId;
            
            return BadRequest(problemDetails);
        }

        if (invoice.CustomerId <= 0)
        {
            _logger.LogWarning("========================================");
            _logger.LogWarning("VALIDATION FAILURE - Invalid Customer ID");
            _logger.LogWarning("========================================");
            _logger.LogWarning("Endpoint: {Method} {Path}", HttpContext.Request.Method, HttpContext.Request.Path);
            _logger.LogWarning("TraceId: {TraceId}", HttpContext.TraceIdentifier);
            _logger.LogWarning("Customer ID: {CustomerId} (must be > 0)", invoice.CustomerId);
            _logger.LogWarning("Invoice Number: {InvoiceNumber}", invoice.InvoiceNumber ?? "(null)");
            _logger.LogWarning("========================================");
            
            var validationProblem = new ValidationProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Invalid Customer ID",
                Detail = $"Customer ID must be greater than 0. Provided value: {invoice.CustomerId}",
                Instance = $"{HttpContext.Request.Method} {HttpContext.Request.Path}"
            };
            validationProblem.Errors.Add("customerId", new[] { "Customer ID must be greater than 0" });
            validationProblem.Extensions["traceId"] = HttpContext.TraceIdentifier;
            validationProblem.Extensions["customerId"] = invoice.CustomerId;
            
            return BadRequest(validationProblem);
        }

        var customer = AppDB.FindCustomer(invoice.CustomerId);
        if (customer == null)
        {
            _logger.LogWarning("========================================");
            _logger.LogWarning("VALIDATION FAILURE - Customer Not Found");
            _logger.LogWarning("========================================");
            _logger.LogWarning("Endpoint: {Method} {Path}", HttpContext.Request.Method, HttpContext.Request.Path);
            _logger.LogWarning("TraceId: {TraceId}", HttpContext.TraceIdentifier);
            _logger.LogWarning("Customer ID: {CustomerId} (does not exist)", invoice.CustomerId);
            _logger.LogWarning("Invoice Number: {InvoiceNumber}", invoice.InvoiceNumber ?? "(null)");
            _logger.LogWarning("========================================");
            
            var validationProblem = new ValidationProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Customer Not Found",
                Detail = $"Customer with ID {invoice.CustomerId} does not exist. Please provide a valid customer ID.",
                Instance = $"{HttpContext.Request.Method} {HttpContext.Request.Path}"
            };
            validationProblem.Errors.Add("customerId", new[] { $"Customer with ID {invoice.CustomerId} not found" });
            validationProblem.Extensions["traceId"] = HttpContext.TraceIdentifier;
            validationProblem.Extensions["customerId"] = invoice.CustomerId;
            
            return NotFound(validationProblem);
        }

        // Check if invoice number already exists (must be unique within AppDB)
        var existingInvoice = AppDB.FindInvoice(invoice.InvoiceNumber);
        if (existingInvoice != null)
        {
            _logger.LogWarning("========================================");
            _logger.LogWarning("VALIDATION FAILURE - Duplicate Invoice Number");
            _logger.LogWarning("========================================");
            _logger.LogWarning("Endpoint: {Method} {Path}", HttpContext.Request.Method, HttpContext.Request.Path);
            _logger.LogWarning("TraceId: {TraceId}", HttpContext.TraceIdentifier);
            _logger.LogWarning("Invoice Number: {InvoiceNumber} (already exists)", invoice.InvoiceNumber);
            _logger.LogWarning("Existing Invoice ID: {ExistingInvoiceId}", existingInvoice.Id);
            _logger.LogWarning("Existing Customer ID: {ExistingCustomerId}", existingInvoice.CustomerId);
            _logger.LogWarning("Requested Customer ID: {RequestedCustomerId}", invoice.CustomerId);
            _logger.LogWarning("========================================");
            
            var conflictProblem = new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = "Invoice Number Already Exists",
                Detail = $"Invoice with number '{invoice.InvoiceNumber}' already exists. Invoice numbers must be unique across all customers.",
                Instance = $"{HttpContext.Request.Method} {HttpContext.Request.Path}",
                Extensions =
                {
                    ["traceId"] = HttpContext.TraceIdentifier,
                    ["invoiceNumber"] = invoice.InvoiceNumber,
                    ["existingInvoiceId"] = existingInvoice.Id,
                    ["existingCustomerId"] = existingInvoice.CustomerId
                }
            };
            
            return Conflict(conflictProblem);
        }

        // Use auto-ID generation (ignore any provided ID)
        // Invoice number is saved as received from user input
        var createdInvoice = AppDB.AddInvoiceWithAutoId(invoice);
        
        if (createdInvoice == null)
        {
            _logger.LogDebug("CreateInvoice: Failed to create invoice {InvoiceNumber} for customer {CustomerId}", invoice.InvoiceNumber, invoice.CustomerId);
            
            var failureProblem = new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Failed to Create Invoice",
                Detail = "Failed to create invoice due to internal validation. Ensure all required fields are provided and valid.",
                Instance = $"{HttpContext.Request.Method} {HttpContext.Request.Path}",
                Extensions =
                {
                    ["traceId"] = HttpContext.TraceIdentifier,
                    ["invoiceNumber"] = invoice.InvoiceNumber,
                    ["customerId"] = invoice.CustomerId
                }
            };
            
            return BadRequest(failureProblem);
        }

        _logger.LogDebug("CreateInvoice: Successfully created invoice {InvoiceId} with number {InvoiceNumber} for customer {CustomerId} with amount {Amount}", 
            createdInvoice.Id, createdInvoice.InvoiceNumber, createdInvoice.CustomerId, createdInvoice.Amount);
        return CreatedAtAction(nameof(GetInvoice), new { customerId = createdInvoice.CustomerId, invoiceNumber = createdInvoice.InvoiceNumber }, createdInvoice);
    }

    /// <summary>
    /// Delete a specific invoice by invoice number (Requires API Key authentication)
    /// </summary>
    /// <param name="invoiceNumber">Invoice number</param>
    /// <returns>No content on success</returns>
    [HttpDelete("{invoiceNumber}")]
    [ApiKeyAuth]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ProblemDetails))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ProblemDetails))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ProblemDetails))]
    public ActionResult DeleteInvoice(string invoiceNumber)
    {
        _logger.LogDebug("DeleteInvoice: Attempting to delete invoice {InvoiceNumber}", invoiceNumber);

        if (string.IsNullOrWhiteSpace(invoiceNumber))
        {
            _logger.LogDebug("DeleteInvoice: Invoice number is null or empty");
            
            var validationProblem = new ValidationProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Invalid Invoice Number",
                Detail = "Invoice number is required and cannot be null, empty, or whitespace.",
                Instance = $"{HttpContext.Request.Method} {HttpContext.Request.Path}"
            };
            validationProblem.Errors.Add("invoiceNumber", new[] { "Invoice number is required" });
            validationProblem.Extensions["traceId"] = HttpContext.TraceIdentifier;
            
            return BadRequest(validationProblem);
        }

        var invoice = AppDB.FindInvoice(invoiceNumber);
        if (invoice == null)
        {
            _logger.LogDebug("DeleteInvoice: Invoice {InvoiceNumber} not found", invoiceNumber);
            
            var notFoundProblem = new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Invoice Not Found",
                Detail = $"Invoice with number '{invoiceNumber}' does not exist or has already been deleted.",
                Instance = $"{HttpContext.Request.Method} {HttpContext.Request.Path}",
                Extensions =
                {
                    ["traceId"] = HttpContext.TraceIdentifier,
                    ["invoiceNumber"] = invoiceNumber
                }
            };
            
            return NotFound(notFoundProblem);
        }

        var success = AppDB.RemoveInvoice(invoiceNumber);
        
        if (!success)
        {
            _logger.LogDebug("DeleteInvoice: Failed to delete invoice {InvoiceNumber}", invoiceNumber);
            
            var failureProblem = new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "Failed to Delete Invoice",
                Detail = $"An error occurred while attempting to delete invoice '{invoiceNumber}'. The invoice exists but could not be removed.",
                Instance = $"{HttpContext.Request.Method} {HttpContext.Request.Path}",
                Extensions =
                {
                    ["traceId"] = HttpContext.TraceIdentifier,
                    ["invoiceNumber"] = invoiceNumber,
                    ["invoiceId"] = invoice.Id
                }
            };
            
            return StatusCode(StatusCodes.Status500InternalServerError, failureProblem);
        }

        _logger.LogDebug("DeleteInvoice: Successfully deleted invoice {InvoiceNumber}", invoiceNumber);
        return NoContent();
    }

    /// <summary>
    /// Delete all invoices for a specific customer (Requires API Key authentication)
    /// </summary>
    /// <param name="customerId">Customer ID</param>
    /// <returns>No content on success</returns>
    [HttpDelete("customer/{customerId}")]
    [ApiKeyAuth]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ProblemDetails))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ProblemDetails))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ValidationProblemDetails))]
    public ActionResult DeleteInvoicebyCustomer(long customerId)
    {
        _logger.LogDebug("DeleteInvoicebyCustomer: Attempting to delete all invoices for customer {CustomerId}", customerId);

        if (customerId <= 0)
        {
            _logger.LogDebug("DeleteInvoicebyCustomer: Invalid customer ID {CustomerId}", customerId);
            
            var validationProblem = new ValidationProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Invalid Customer ID",
                Detail = $"Customer ID must be greater than 0. Provided value: {customerId}",
                Instance = $"{HttpContext.Request.Method} {HttpContext.Request.Path}"
            };
            validationProblem.Errors.Add("customerId", new[] { "Customer ID must be greater than 0" });
            validationProblem.Extensions["traceId"] = HttpContext.TraceIdentifier;
            validationProblem.Extensions["customerId"] = customerId;
            
            return BadRequest(validationProblem);
        }

        // Check if customer exists first
        var customer = AppDB.FindCustomer(customerId);
        if (customer == null)
        {
            _logger.LogDebug("DeleteInvoicebyCustomer: Customer with ID {CustomerId} not found", customerId);
            
            var notFoundProblem = new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Customer Not Found",
                Detail = $"Customer with ID {customerId} does not exist. Cannot delete invoices for non-existent customer.",
                Instance = $"{HttpContext.Request.Method} {HttpContext.Request.Path}",
                Extensions =
                {
                    ["traceId"] = HttpContext.TraceIdentifier,
                    ["customerId"] = customerId,
                    ["suggestion"] = "Verify the customer ID is correct"
                }
            };
            
            return NotFound(notFoundProblem);
        }

        // Get invoice count before deletion for logging
        var invoices = AppDB.GetInvoicesByCustomer(customerId);
        var invoiceCount = invoices.Count;

        var success = AppDB.RemoveInvoicesbyCustomer(customerId);
        
        if (!success)
        {
            _logger.LogDebug("DeleteInvoicebyCustomer: Failed to delete invoices for customer {CustomerId}", customerId);
            
            var failureProblem = new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "Failed to Delete Invoices",
                Detail = $"An error occurred while attempting to delete invoices for customer {customerId}.",
                Instance = $"{HttpContext.Request.Method} {HttpContext.Request.Path}",
                Extensions =
                {
                    ["traceId"] = HttpContext.TraceIdentifier,
                    ["customerId"] = customerId,
                    ["invoiceCount"] = invoiceCount
                }
            };
            
            return StatusCode(StatusCodes.Status500InternalServerError, failureProblem);
        }

        _logger.LogDebug("DeleteInvoicebyCustomer: Successfully deleted {Count} invoices for customer {CustomerId}", invoiceCount, customerId);
        return NoContent();
    }
}