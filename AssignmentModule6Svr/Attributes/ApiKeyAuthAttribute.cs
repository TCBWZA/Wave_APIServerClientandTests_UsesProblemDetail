using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace AssignmentModule6Svr.Attributes;

/// <summary>
/// Custom authorization attribute for API Key authentication on DELETE endpoints
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class ApiKeyAuthAttribute : Attribute, IAuthorizationFilter
{
    private const string API_KEY_HEADER_NAME = "X-API-Key";
    
    /// <summary>
    /// Validates the API key from the request header against the configured value.
    /// Returns 401 Unauthorized if the key is missing or invalid.
    /// </summary>
    /// <param name="context">The authorization filter context containing request information</param>
    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<ApiKeyAuthAttribute>>();
        var requestPath = context.HttpContext.Request.Path;
        var requestMethod = context.HttpContext.Request.Method;

        logger.LogDebug("ApiKeyAuth: Authorization check initiated for {Method} {Path}", requestMethod, requestPath);

        // Check if the API key header exists
        if (!context.HttpContext.Request.Headers.TryGetValue(API_KEY_HEADER_NAME, out var extractedApiKey))
        {
            logger.LogWarning("ApiKeyAuth: Missing API key header for {Method} {Path}", requestMethod, requestPath);
            context.Result = new UnauthorizedObjectResult(new ProblemDetails
            {
                Status = StatusCodes.Status401Unauthorized,
                Title = "API Key Missing",
                Detail = "API Key is missing. Please provide a valid API key in the X-API-Key header.",
                Instance = $"{requestMethod} {requestPath}",
                Extensions =
                {
                    ["traceId"] = context.HttpContext.TraceIdentifier,
                    ["requiredHeader"] = API_KEY_HEADER_NAME
                }
            });
            return;
        }

        // Get the valid API key from configuration (reading from appsettings.json)
        var configuration = context.HttpContext.RequestServices.GetRequiredService<IConfiguration>();
        var apiKey = configuration.GetValue<string>("ApiToken:StaticToken");

        // Validate the API key
        if (string.IsNullOrWhiteSpace(apiKey) || !apiKey.Equals(extractedApiKey))
        {
            logger.LogWarning("ApiKeyAuth: Invalid API key provided for {Method} {Path}", requestMethod, requestPath);
            context.Result = new UnauthorizedObjectResult(new ProblemDetails
            {
                Status = StatusCodes.Status401Unauthorized,
                Title = "Invalid API Key",
                Detail = "Invalid API Key. Access denied.",
                Instance = $"{requestMethod} {requestPath}",
                Extensions =
                {
                    ["traceId"] = context.HttpContext.TraceIdentifier,
                    ["requiredHeader"] = API_KEY_HEADER_NAME
                }
            });
            return;
        }

        logger.LogDebug("ApiKeyAuth: Authentication successful for {Method} {Path}", requestMethod, requestPath);
    }
}