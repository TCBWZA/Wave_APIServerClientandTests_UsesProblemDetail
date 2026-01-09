/// <summary>
/// Defines standard Problem Details type URIs for consistent error responses across the API.
/// These URIs follow RFC 7807 Problem Details specification.
/// </summary>
public static class ProblemDetailsTypes
{
    /// <summary>
    /// Problem type URI for resource not found errors (404)
    /// </summary>
    public const string ResourceNotFound = "https://yourapi.com/problems/resource-not-found";
    
    /// <summary>
    /// Problem type URI for validation failures (400)
    /// </summary>
    public const string ValidationFailed = "https://yourapi.com/problems/validation-failed";
    
    /// <summary>
    /// Problem type URI for duplicate resource conflicts (409)
    /// </summary>
    public const string DuplicateResource = "https://yourapi.com/problems/duplicate-resource";
    
    /// <summary>
    /// Problem type URI for unauthorized access attempts (401)
    /// </summary>
    public const string Unauthorized = "https://yourapi.com/problems/unauthorized";
}