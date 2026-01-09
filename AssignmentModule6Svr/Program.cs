using AssignmentModule6Svr;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Diagnostics;
using Scalar.AspNetCore;

namespace AssignmentModule6Svr
{
    /// <summary>
    /// The main entry point for the AssignmentModule6 API application.
    /// Configures services, middleware, and initializes the in-memory database with sample data.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Application entry point. Configures and runs the web application.
        /// Sets up logging, controllers, OpenAPI documentation, exception handling, and sample data.
        /// </summary>
        public static void Main()
        {
            var builder = WebApplication.CreateBuilder();

            // Add services to the container.

            // Configure logging - Console sink, enable Debug for endpoints and detailed diagnostics
            builder.Logging.ClearProviders();
            builder.Logging.AddConsole();
            builder.Logging.SetMinimumLevel(LogLevel.Debug);

            builder.Services.AddControllers();
            
            // Add Problem Details support (RFC 7807)
            builder.Services.AddProblemDetails(options =>
            {
                options.CustomizeProblemDetails = context =>
                {
                    context.ProblemDetails.Instance = $"{context.HttpContext.Request.Method} {context.HttpContext.Request.Path}";
                    context.ProblemDetails.Extensions["traceId"] = context.HttpContext.TraceIdentifier;
                    context.ProblemDetails.Extensions["timestamp"] = DateTime.UtcNow.ToString("o");
                };
            });
            
            // Add OpenAPI support (required for Scalar)
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddOpenApi();
            
            // Add Health Checks
            builder.Services.AddHealthChecks();

            var app = builder.Build();

            var logger = app.Services.GetRequiredService<ILogger<Program>>();

            // Log the API token being used (for debugging purposes only - remove in production)
            var apiToken = builder.Configuration.GetValue<string>("ApiToken:StaticToken");
            logger.LogInformation("API Token loaded from configuration: {TokenPreview}", 
                string.IsNullOrEmpty(apiToken) ? "NOT FOUND" : $"{apiToken[..4]}***");

            // Add middleware to disable caching for all API responses
            app.Use(async (context, next) =>
            {
                context.Response.Headers.CacheControl = "no-cache, no-store, must-revalidate";
                context.Response.Headers.Pragma = "no-cache";
                context.Response.Headers.Expires = "0";
                await next();
            });

            // Global exception logging middleware (logs unhandled exceptions)
            app.Use(async (context, next) =>
            {
                try
                {
                    await next();
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Unhandled exception processing request {Method} {Path}", context.Request?.Method, context.Request?.Path);
                    throw;
                }
            });

            // Configure global exception handler to return Problem Details format
            app.UseExceptionHandler(errorApp =>
            {
                errorApp.Run(async context =>
                {
                    var exceptionHandler = context.Features.Get<IExceptionHandlerPathFeature>();
                    var exception = exceptionHandler?.Error;
                    
                    var problemDetails = new ProblemDetails
                    {
                        Status = StatusCodes.Status500InternalServerError,
                        Title = "An error occurred",
                        Detail = exception?.Message,
                        Instance = context.Request.Path,
                        Extensions =
                        {
                            ["traceId"] = context.TraceIdentifier
                        }
                    };
                    
                    await context.Response.WriteAsJsonAsync(problemDetails);
                });
            });

            // Initialize in-memory database on application startup.
            // Adjust the numbers below as needed (or replace with configuration values).
            bool customersCreated = AppDB.CreateCustomers(true, 50);
            if (customersCreated)
            {
                var customers = AppDB.GetCustomerList();
                AppDB.CreateInvoices(customers);
                AppDB.CreatePhoneNumbers(customers);

                logger.LogInformation($"AppDB initialized: {AppDB.GetCustomerCount()} customers, {AppDB.GetInvoiceCount()} Invoices, {AppDB.GetPhoneNumberCount()} Telephone Numbers");
            }
            else
            {
                logger.LogWarning("AppDB.CreateCustomers returned false. No customers were created on startup.");
            }
            
            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                // Map OpenAPI endpoint
                app.MapOpenApi();
                
                // Enable Scalar - Modern API Documentation with C# examples only
                app.MapScalarApiReference(options =>
                {
                    options
                        .WithTitle("Customer Management API")
                        .WithTheme(ScalarTheme.Purple)
                        .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient)
                        .AddPreferredSecuritySchemes("ApiKey");
                });

                logger.LogInformation("API Documentation available at:");
                logger.LogInformation("  - Scalar UI (C# HttpClient & RestSharp examples): /scalar/v1");
                logger.LogInformation("  - OpenAPI Spec: /openapi/v1.json");
                logger.LogInformation($"  - API Key: {(string.IsNullOrEmpty(apiToken) ? "NOT CONFIGURED" : "Configured")}");
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.MapControllers();
            app.MapHealthChecks("/health");

            app.Run();
        }
    }

}
