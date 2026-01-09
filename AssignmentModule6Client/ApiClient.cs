using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using AssignmentModule6Client.Models;
using Microsoft.Extensions.Logging;

namespace AssignmentModule6Client
{
    /// <summary>
    /// API client for communicating with the AssignmentModule6Svr API.
    /// Provides methods for GET, POST, and DELETE operations with error handling and logging.
    /// </summary>
    public class ApiClient : IDisposable
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;
        private readonly string? _apiToken;
        private readonly JsonSerializerOptions _jsonOptions;
        private readonly ILogger<ApiClient>? _logger;

        /// <summary>
        /// Initializes a new instance of the ApiClient class.
        /// </summary>
        /// <param name="baseUrl">The base URL of the API (e.g., "https://localhost:7136")</param>
        /// <param name="apiToken">Optional API token for authentication (required for DELETE operations)</param>
        /// <param name="logger">Optional logger for structured logging</param>
        public ApiClient(string baseUrl, string? apiToken = null, ILogger<ApiClient>? logger = null)
        {
            _baseUrl = baseUrl.TrimEnd('/');
            _apiToken = apiToken;
            _logger = logger;
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(_baseUrl)
            };

            // Configure JSON serialization options
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                WriteIndented = true
            };

            // Set default headers
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            _logger?.LogInformation("ApiClient initialized with base URL: {BaseUrl}", _baseUrl);
        }

        /// <summary>
        /// Performs an asynchronous GET request to the specified endpoint.
        /// </summary>
        /// <param name="endpoint">The API endpoint (e.g., "/api/Customers")</param>
        /// <returns>The response content as a string</returns>
        public async Task<string> GetAsync(string endpoint)
        {
            try
            {
                _logger?.LogDebug("GET Request: {BaseUrl}{Endpoint}", _baseUrl, endpoint);
                var response = await _httpClient.GetAsync(endpoint);
                
                await LogResponse(response);
                
                if (!response.IsSuccessStatusCode)
                {
                    await HandleErrorResponse(response, "GET", endpoint);
                }

                var content = await response.Content.ReadAsStringAsync();
                return content;
            }
            catch (HttpRequestException ex)
            {
                _logger?.LogError(ex, "Network error during GET {Endpoint}", endpoint);
                throw;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Unexpected error during GET {Endpoint}", endpoint);
                throw;
            }
        }

        /// <summary>
        /// Performs an asynchronous GET request and deserializes the response to the specified type.
        /// </summary>
        /// <typeparam name="T">The type to deserialize the response to</typeparam>
        /// <param name="endpoint">The API endpoint</param>
        /// <returns>The deserialized object of type T</returns>
        public async Task<T?> GetFromJsonAsync<T>(string endpoint)
        {
            try
            {
                _logger?.LogDebug("GET Request (JSON): {BaseUrl}{Endpoint}", _baseUrl, endpoint);
                var response = await _httpClient.GetAsync(endpoint);
                
                await LogResponse(response);
                
                if (!response.IsSuccessStatusCode)
                {
                    await HandleErrorResponse(response, "GET", endpoint);
                }

                var content = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<T>(content, _jsonOptions);
                _logger?.LogDebug("Successfully deserialized response to type {TypeName}", typeof(T).Name);
                return result;
            }
            catch (HttpRequestException ex)
            {
                _logger?.LogError(ex, "Network error during GET {Endpoint}", endpoint);
                throw;
            }
            catch (JsonException ex)
            {
                _logger?.LogError(ex, "JSON deserialization error for endpoint {Endpoint}", endpoint);
                throw;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Unexpected error during GET {Endpoint}", endpoint);
                throw;
            }
        }

        /// <summary>
        /// Performs an asynchronous POST request with JSON content.
        /// </summary>
        /// <typeparam name="T">The type of object to send</typeparam>
        /// <param name="endpoint">The API endpoint</param>
        /// <param name="data">The data to send in the request body</param>
        /// <returns>The response content as a string</returns>
        public async Task<string> PostAsync<T>(string endpoint, T data)
        {
            try
            {
                _logger?.LogDebug("POST Request: {BaseUrl}{Endpoint}", _baseUrl, endpoint);
                
                var json = JsonSerializer.Serialize(data, _jsonOptions);
                _logger?.LogTrace("Request Body: {Json}", json);
                
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync(endpoint, content);
                
                await LogResponse(response);
                
                if (!response.IsSuccessStatusCode)
                {
                    await HandleErrorResponse(response, "POST", endpoint);
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                return responseContent;
            }
            catch (HttpRequestException ex)
            {
                _logger?.LogError(ex, "Network error during POST {Endpoint}", endpoint);
                throw;
            }
            catch (JsonException ex)
            {
                _logger?.LogError(ex, "JSON serialization error for endpoint {Endpoint}", endpoint);
                throw;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Unexpected error during POST {Endpoint}", endpoint);
                throw;
            }
        }

        /// <summary>
        /// Performs an asynchronous POST request and deserializes the response to the specified type.
        /// </summary>
        /// <typeparam name="TRequest">The type of object to send</typeparam>
        /// <typeparam name="TResponse">The type to deserialize the response to</typeparam>
        /// <param name="endpoint">The API endpoint</param>
        /// <param name="data">The data to send in the request body</param>
        /// <returns>The deserialized response object</returns>
        public async Task<TResponse?> PostAsync<TRequest, TResponse>(string endpoint, TRequest data)
        {
            try
            {
                var responseContent = await PostAsync(endpoint, data);
                var result = JsonSerializer.Deserialize<TResponse>(responseContent, _jsonOptions);
                _logger?.LogDebug("Successfully deserialized response to type {TypeName}", typeof(TResponse).Name);
                return result;
            }
            catch (JsonException ex)
            {
                _logger?.LogError(ex, "JSON deserialization error for endpoint {Endpoint}", endpoint);
                throw;
            }
        }

        /// <summary>
        /// Performs an asynchronous DELETE request.
        /// Requires API token to be set.
        /// </summary>
        /// <param name="endpoint">The API endpoint</param>
        /// <returns>True if successful, false otherwise</returns>
        public async Task<bool> DeleteAsync(string endpoint)
        {
            try
            {
                _logger?.LogDebug("DELETE Request: {BaseUrl}{Endpoint}", _baseUrl, endpoint);
                
                // Create a new request message to add the API key header
                var request = new HttpRequestMessage(HttpMethod.Delete, endpoint);
                
                if (!string.IsNullOrEmpty(_apiToken))
                {
                    request.Headers.Add("X-API-Key", _apiToken);
                    _logger?.LogTrace("Added X-API-Key header");
                }
                else
                {
                    _logger?.LogWarning("No API token provided for DELETE request to {Endpoint}", endpoint);
                }

                var response = await _httpClient.SendAsync(request);
                
                await LogResponse(response);
                
                if (!response.IsSuccessStatusCode)
                {
                    await HandleErrorResponse(response, "DELETE", endpoint);
                    return false;
                }

                return true;
            }
            catch (HttpRequestException ex)
            {
                _logger?.LogError(ex, "Network error during DELETE {Endpoint}", endpoint);
                throw;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Unexpected error during DELETE {Endpoint}", endpoint);
                throw;
            }
        }

        /// <summary>
        /// Handles error responses and throws appropriate exceptions.
        /// </summary>
        private async Task HandleErrorResponse(HttpResponseMessage response, string method, string endpoint)
        {
            var content = await response.Content.ReadAsStringAsync();
            
            try
            {
                // Try to parse as ProblemDetails
                var problemDetails = JsonSerializer.Deserialize<ProblemDetails>(content, _jsonOptions);
                if (problemDetails != null)
                {
                    // Log basic problem details
                    _logger?.LogError(
                        "API Error - Method: {Method}, Endpoint: {Endpoint}, Status: {Status}, Title: {Title}, Detail: {Detail}, TraceId: {TraceId}",
                        method, endpoint, problemDetails.Status, problemDetails.Title, problemDetails.Detail, problemDetails.TraceId);
                    
                    // Log validation errors if present
                    if (problemDetails.Errors != null && problemDetails.Errors.Any())
                    {
                        _logger?.LogError("Validation Errors ({ErrorCount} fields):", problemDetails.Errors.Count);
                        foreach (var error in problemDetails.Errors)
                        {
                            _logger?.LogError("  • Field '{Field}': {Errors}", error.Key, string.Join("; ", error.Value));
                        }
                    }
                    
                    throw new HttpRequestException($"{method} {endpoint} failed: {problemDetails.Title} - {problemDetails.Detail}", null, response.StatusCode);
                }
            }
            catch (JsonException)
            {
                // Not a ProblemDetails response, use raw content
                _logger?.LogError("HTTP {StatusCode} {ReasonPhrase}: {Content}", 
                    (int)response.StatusCode, response.ReasonPhrase, content);
            }

            throw new HttpRequestException($"{method} {endpoint} failed with status code {(int)response.StatusCode}: {response.ReasonPhrase}", null, response.StatusCode);
        }

        /// <summary>
        /// Logs the HTTP response details.
        /// </summary>
        private async Task LogResponse(HttpResponseMessage response)
        {
            var content = await response.Content.ReadAsStringAsync();
            var statusCode = (int)response.StatusCode;
            
            if (response.IsSuccessStatusCode)
            {
                _logger?.LogInformation("Response: {StatusCode} {ReasonPhrase}", statusCode, response.ReasonPhrase);
                
                if (!string.IsNullOrWhiteSpace(content) && content.Length < 500)
                {
                    _logger?.LogTrace("Response Body: {Content}", content);
                }
                else if (content.Length >= 500)
                {
                    _logger?.LogTrace("Response Body: [Large response - {Length} characters]", content.Length);
                }
            }
            else
            {
                _logger?.LogWarning("Response: {StatusCode} {ReasonPhrase}", statusCode, response.ReasonPhrase);
                if (!string.IsNullOrWhiteSpace(content))
                {
                    _logger?.LogWarning("Error Response: {Content}", content);
                }
            }
        }

        /// <summary>
        /// Disposes the HTTP client resources.
        /// </summary>
        public void Dispose()
        {
            _httpClient?.Dispose();
            _logger?.LogDebug("ApiClient disposed");
        }
    }
}
