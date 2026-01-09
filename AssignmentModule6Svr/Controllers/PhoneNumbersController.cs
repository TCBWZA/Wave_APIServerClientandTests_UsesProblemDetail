using Microsoft.AspNetCore.Mvc;
using AssignmentModule6Svr.Classes;
using AssignmentModule6Svr;
using AssignmentModule6Svr.Attributes;

namespace AssignmentModule6Svr.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PhoneNumbersController : ControllerBase
{
    private readonly ILogger<PhoneNumbersController> _logger;

    public PhoneNumbersController(ILogger<PhoneNumbersController> logger)
    {
        _logger = logger;
    }


    /// <summary>
    /// Get all phone numbers
    /// </summary>
    /// <returns>List of all phone numbers</returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult<List<PhoneNumber>> GetAllPhoneNumbers()
    {
        _logger.LogDebug("GetAllPhoneNumbers: Retrieving all phone numbers");
        var phoneNumbers = AppDB.GetPhoneNumbersList();
        _logger.LogDebug("GetAllPhoneNumbers: Returned {Count} phone numbers", phoneNumbers.Count);
        return Ok(phoneNumbers);
    }

    /// <summary>
    /// Get a specific phone number by ID
    /// </summary>
    /// <param name="id">Phone number ID</param>
    /// <returns>Phone number details</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ProblemDetails))]
    public ActionResult<PhoneNumber> GetPhoneNumber(long id)
    {
        _logger.LogDebug("GetPhoneNumber: Retrieving phone number with ID {PhoneNumberId}", id);
        var phoneNumber = AppDB.FindPhoneNumber(id);
        
        if (phoneNumber == null)
        {
            _logger.LogDebug("GetPhoneNumber: Phone number with ID {PhoneNumberId} not found", id);
            
            var notFoundProblem = new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Phone Number Not Found",
                Detail = $"Phone number with ID {id} does not exist in the system.",
                Instance = $"{HttpContext.Request.Method} {HttpContext.Request.Path}",
                Extensions =
                {
                    ["traceId"] = HttpContext.TraceIdentifier,
                    ["phoneNumberId"] = id,
                    ["suggestion"] = "Verify the phone number ID is correct or use GET /api/phonenumbers to list all phone numbers"
                }
            };
            
            return NotFound(notFoundProblem);
        }

        _logger.LogDebug("GetPhoneNumber: Successfully retrieved phone number {PhoneNumberId} for customer {CustomerId}", id, phoneNumber.CustomerId);
        return Ok(phoneNumber);
    }

    /// <summary>
    /// Get all phone numbers for a specific customer
    /// </summary>
    /// <param name="customerId">Customer ID</param>
    /// <returns>List of phone numbers for the customer</returns>
    [HttpGet("customer/{customerId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ProblemDetails))]
    public ActionResult<List<PhoneNumber>> GetPhoneNumbersByCustomer(long customerId)
    {
        _logger.LogDebug("GetPhoneNumbersByCustomer: Retrieving phone numbers for customer {CustomerId}", customerId);
        var customer = AppDB.FindCustomer(customerId);
        
        if (customer == null)
        {
            _logger.LogDebug("GetPhoneNumbersByCustomer: Customer with ID {CustomerId} not found", customerId);
            
            var notFoundProblem = new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Customer Not Found",
                Detail = $"Customer with ID {customerId} does not exist. Cannot retrieve phone numbers for non-existent customer.",
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

        var phoneNumbers = AppDB.GetPhoneNumbersByCustomer(customerId);
        _logger.LogDebug("GetPhoneNumbersByCustomer: Retrieved {Count} phone numbers for customer {CustomerId}", phoneNumbers.Count, customerId);
        return Ok(phoneNumbers);
    }

    /// <summary>
    /// Create a new phone number
    /// </summary>
    /// <param name="phoneNumber">Phone number object</param>
    /// <returns>Created phone number</returns>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ValidationProblemDetails))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ProblemDetails))]
    [ProducesResponseType(StatusCodes.Status409Conflict, Type = typeof(ProblemDetails))]
    public ActionResult<PhoneNumber> CreatePhoneNumber([FromBody] PhoneNumber phoneNumber)
    {
        _logger.LogDebug("CreatePhoneNumber: Attempting to create phone number for customer {CustomerId}", 
            phoneNumber?.CustomerId);

        if (phoneNumber == null)
        {
            _logger.LogDebug("CreatePhoneNumber: Phone number data is null");
            
            var validationProblem = new ValidationProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Invalid Phone Number Data",
                Detail = "Phone number data is required. The request body cannot be null or empty.",
                Instance = $"{HttpContext.Request.Method} {HttpContext.Request.Path}"
            };
            validationProblem.Errors.Add("phoneNumber", new[] { "Phone number object is required" });
            validationProblem.Extensions["traceId"] = HttpContext.TraceIdentifier;
            
            return BadRequest(validationProblem);
        }

        if (!ModelState.IsValid)
        {
            _logger.LogDebug("CreatePhoneNumber: Model state is invalid");
            
            var problemDetails = new ValidationProblemDetails(ModelState)
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Validation Failed",
                Detail = "One or more validation errors occurred. Please review the 'errors' property for details.",
                Instance = $"{HttpContext.Request.Method} {HttpContext.Request.Path}"
            };
            problemDetails.Extensions["traceId"] = HttpContext.TraceIdentifier;
            problemDetails.Extensions["errorCount"] = ModelState.ErrorCount;
            
            return BadRequest(problemDetails);
        }

        if (phoneNumber.CustomerId <= 0)
        {
            _logger.LogDebug("CreatePhoneNumber: Invalid customer ID {CustomerId}", phoneNumber.CustomerId);
            
            var validationProblem = new ValidationProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Invalid Customer ID",
                Detail = $"Customer ID must be greater than 0. Provided value: {phoneNumber.CustomerId}",
                Instance = $"{HttpContext.Request.Method} {HttpContext.Request.Path}"
            };
            validationProblem.Errors.Add("customerId", new[] { "Customer ID must be greater than 0" });
            validationProblem.Extensions["traceId"] = HttpContext.TraceIdentifier;
            validationProblem.Extensions["customerId"] = phoneNumber.CustomerId;
            
            return BadRequest(validationProblem);
        }

        var customer = AppDB.FindCustomer(phoneNumber.CustomerId);
        if (customer == null)
        {
            _logger.LogDebug("CreatePhoneNumber: Customer with ID {CustomerId} not found", phoneNumber.CustomerId);
            
            var validationProblem = new ValidationProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Customer Not Found",
                Detail = $"Customer with ID {phoneNumber.CustomerId} does not exist. Phone numbers must be associated with an existing customer.",
                Instance = $"{HttpContext.Request.Method} {HttpContext.Request.Path}"
            };
            validationProblem.Errors.Add("customerId", new[] { $"Customer with ID {phoneNumber.CustomerId} not found" });
            validationProblem.Extensions["traceId"] = HttpContext.TraceIdentifier;
            validationProblem.Extensions["customerId"] = phoneNumber.CustomerId;
            validationProblem.Extensions["suggestion"] = "Create the customer first or use a valid customer ID";
            
            return NotFound(validationProblem);
        }

        // Always use auto-ID generation (ignore any provided ID)
        var createdPhoneNumber = AppDB.AddPhoneNumberWithAutoId(phoneNumber);
        
        if (createdPhoneNumber == null)
        {
            _logger.LogDebug("CreatePhoneNumber: Failed to create phone number with auto-generated ID");
            
            var failureProblem = new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Failed to Create Phone Number",
                Detail = "Failed to create phone number due to internal validation. Ensure all required fields are provided and valid.",
                Instance = $"{HttpContext.Request.Method} {HttpContext.Request.Path}",
                Extensions =
                {
                    ["traceId"] = HttpContext.TraceIdentifier,
                    ["customerId"] = phoneNumber.CustomerId
                }
            };
            
            return BadRequest(failureProblem);
        }

        _logger.LogDebug("CreatePhoneNumber: Successfully created phone number {PhoneNumberId} (Type: {Type}, Number: {Number}) for customer {CustomerId} with auto-generated ID", 
            createdPhoneNumber.Id, createdPhoneNumber.Type, createdPhoneNumber.Number, createdPhoneNumber.CustomerId);
        return CreatedAtAction(nameof(GetPhoneNumber), new { id = createdPhoneNumber.Id }, createdPhoneNumber);
    }

    /// <summary>
    /// Delete all phone numbers for a specific customer (Requires API Key authentication)
    /// </summary>
    /// <param name="customerId">Customer ID</param>
    /// <returns>No content on success</returns>
    [HttpDelete("customer/{customerId}")]
    [ApiKeyAuth]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ProblemDetails))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ProblemDetails))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ValidationProblemDetails))]
    public ActionResult DeletePhoneNumbersByCustomer(long customerId)
    {
        _logger.LogDebug("DeletePhoneNumbersByCustomer: Attempting to delete all phone numbers for customer {CustomerId}", customerId);

        if (customerId <= 0)
        {
            _logger.LogDebug("DeletePhoneNumbersByCustomer: Invalid customer ID {CustomerId}", customerId);
            
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
            _logger.LogDebug("DeletePhoneNumbersByCustomer: Customer with ID {CustomerId} not found", customerId);
            
            var notFoundProblem = new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Customer Not Found",
                Detail = $"Customer with ID {customerId} does not exist. Cannot delete phone numbers for non-existent customer.",
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

        // Get phone numbers count before deletion for logging
        var phoneNumbers = AppDB.GetPhoneNumbersByCustomer(customerId);
        var phoneCount = phoneNumbers.Count;

        var success = AppDB.RemovePhonesbyCustomer(customerId);
        
        if (!success)
        {
            _logger.LogDebug("DeletePhoneNumbersByCustomer: Failed to delete phone numbers for customer {CustomerId}", customerId);
            
            var failureProblem = new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "Failed to Delete Phone Numbers",
                Detail = $"An error occurred while attempting to delete phone numbers for customer {customerId}.",
                Instance = $"{HttpContext.Request.Method} {HttpContext.Request.Path}",
                Extensions =
                {
                    ["traceId"] = HttpContext.TraceIdentifier,
                    ["customerId"] = customerId,
                    ["phoneNumberCount"] = phoneCount
                }
            };
            
            return StatusCode(StatusCodes.Status500InternalServerError, failureProblem);
        }

        _logger.LogDebug("DeletePhoneNumbersByCustomer: Successfully deleted {Count} phone numbers for customer {CustomerId}", phoneCount, customerId);
        return NoContent();
    }

    /// <summary>
    /// Delete a specific phone number by ID (Requires API Key authentication)
    /// </summary>
    /// <param name="id">Phone number ID</param>
    /// <returns>No content on success</returns>
    [HttpDelete("{id}")]
    [ApiKeyAuth]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ProblemDetails))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ProblemDetails))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ValidationProblemDetails))]
    public ActionResult DeletePhoneNumber(long id)
    {
        _logger.LogDebug("DeletePhoneNumber: Attempting to delete phone number with ID {PhoneNumberId}", id);

        if (id <= 0)
        {
            _logger.LogDebug("DeletePhoneNumber: Invalid phone number ID {PhoneNumberId}", id);
            
            var validationProblem = new ValidationProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Invalid Phone Number ID",
                Detail = $"Phone number ID must be greater than 0. Provided value: {id}",
                Instance = $"{HttpContext.Request.Method} {HttpContext.Request.Path}"
            };
            validationProblem.Errors.Add("id", new[] { "Phone number ID must be greater than 0" });
            validationProblem.Extensions["traceId"] = HttpContext.TraceIdentifier;
            validationProblem.Extensions["phoneNumberId"] = id;
            
            return BadRequest(validationProblem);
        }

        var phoneNumber = AppDB.FindPhoneNumber(id);
        if (phoneNumber == null)
        {
            _logger.LogDebug("DeletePhoneNumber: Phone number with ID {PhoneNumberId} not found", id);
            
            var notFoundProblem = new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Phone Number Not Found",
                Detail = $"Phone number with ID {id} does not exist or has already been deleted.",
                Instance = $"{HttpContext.Request.Method} {HttpContext.Request.Path}",
                Extensions =
                {
                    ["traceId"] = HttpContext.TraceIdentifier,
                    ["phoneNumberId"] = id
                }
            };
            
            return NotFound(notFoundProblem);
        }

        var success = AppDB.RemovePhoneNumber(id);
        
        if (!success)
        {
            _logger.LogDebug("DeletePhoneNumber: Failed to delete phone number with ID {PhoneNumberId}", id);
            
            var failureProblem = new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "Failed to Delete Phone Number",
                Detail = $"An error occurred while attempting to delete phone number {id}. The phone number exists but could not be removed.",
                Instance = $"{HttpContext.Request.Method} {HttpContext.Request.Path}",
                Extensions =
                {
                    ["traceId"] = HttpContext.TraceIdentifier,
                    ["phoneNumberId"] = id,
                    ["phoneNumber"] = phoneNumber.Number,
                    ["customerId"] = phoneNumber.CustomerId
                }
            };
            
            return StatusCode(StatusCodes.Status500InternalServerError, failureProblem);
        }

        _logger.LogDebug("DeletePhoneNumber: Successfully deleted phone number with ID {PhoneNumberId}", id);
        return NoContent();
    }
}