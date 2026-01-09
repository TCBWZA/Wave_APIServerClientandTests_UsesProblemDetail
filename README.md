# AssignmentModule6 - Customer Management API Suite

A comprehensive .NET 10 solution for customer management consisting of a RESTful API server, client library, and comprehensive test suite.

## Projects

### AssignmentModule6Svr
RESTful API server for managing customers, invoices, and phone numbers.
- **Framework**: ASP.NET Core Web API (.NET 10)
- **Features**: CRUD operations, API key authentication, RFC 7807 Problem Details, Scalar API documentation
- **Database**: In-memory (for development/testing)
- **Tests**: 74 NUnit tests with ~85% coverage

### AssignmentModule6Client
Console application demonstrating API consumption with enterprise-grade features.
- **Framework**: .NET 10 Console Application
- **Version**: 2.0.0 - RFC 7807 Problem Details focused
- **Features**: Structured logging, configuration management, dependency injection, Problem Details parsing
- **Note**: Requires .NET 10 or later (not compatible with .NET 8)

### AssignmentModule6Svr.Tests
Comprehensive test suite for the API server.
- **Framework**: NUnit 4.2.2
- **Coverage**: AppDB, Controllers, Models
- **Tests**: 74 tests covering CRUD, validation, authentication, error handling

## Quick Start

### Prerequisites
- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) (Required)
- Visual Studio 2022+ or VS Code
- Git (for version control)

### Clone and Setup

```powershell
# Clone the repository
git clone <your-repository-url>
cd AssignmentModule6.Svr

# Restore packages
dotnet restore

# Build all projects
dotnet build

# Run tests
dotnet test
```

### Run the Server

```powershell
cd AssignmentModule6Svr
dotnet run

# API available at: https://localhost:7136
# API Documentation: https://localhost:7136/scalar/v1
```

### Run the Client

```powershell
# In a new terminal
cd AssignmentModule6Client
dotnet run
```

## Configuration

### API Server (AssignmentModule6Svr)

Edit `appsettings.json`:
```json
{
  "ApiToken": {
    "StaticToken": "YOUR_SECURE_TOKEN_HERE"
  }
}
```

### Client (AssignmentModule6Client)

Edit `appsettings.json`:
```json
{
  "ApiSettings": {
    "BaseUrl": "https://localhost:7136",
    "ApiToken": "YOUR_API_TOKEN_HERE"
  }
}
```

**Security Note**: Never commit sensitive tokens. Use environment variables or .NET Secret Manager for production.

## Documentation

Each project has detailed documentation:
- **Server**: `AssignmentModule6Svr/README.md`
- **Client**: `AssignmentModule6Client/README.md`
- **Tests**: `AssignmentModule6Svr.Tests/README.md`

## Features

### API Server
- Complete CRUD operations for Customers, Invoices, Phone Numbers
- Auto-generated IDs with global uniqueness
- API Key authentication for DELETE operations
- RFC 7807 Problem Details error responses
- Interactive Scalar API documentation
- Sample data generation with Bogus library

### Client v2.0
- RFC 7807 Problem Details parsing and logging
- Structured logging with Microsoft.Extensions.Logging
- Configuration management with appsettings.json
- Dependency injection support
- Intelligent error recovery strategies
- Requires .NET 10+

### Test Suite
- 74 comprehensive NUnit tests
- ~85% code coverage
- Tests for CRUD operations, validation, authentication
- Integration tests with WebApplicationFactory

## Technology Stack

- **.NET 10** - Required framework
- **ASP.NET Core** - Web API framework
- **System.Text.Json** - JSON serialization
- **NUnit** - Testing framework
- **FluentAssertions** - Test assertions
- **Moq** - Mocking framework
- **Bogus** - Fake data generation
- **Scalar** - API documentation UI

## Project Structure

```
AssignmentModule6.Svr/
|-- AssignmentModule6Svr/           # API Server
|   |-- Controllers/
|   |-- Classes/
|   |-- Docs/
|   +-- README.md
|
|-- AssignmentModule6Client/        # Client Application
|   |-- Models/
|   |-- Docs/
|   +-- README.md
|
|-- AssignmentModule6Svr.Tests/     # Test Suite
|   +-- README.md
|
|-- .gitignore
+-- README.md (this file)
```

## Testing

```powershell
# Run all tests
dotnet test

# Run with detailed output
dotnet test --verbosity normal

# Generate coverage report
dotnet test --collect:"XPlat Code Coverage"
```

## Building for Production

```powershell
# Build in Release mode
dotnet build -c Release

# Publish server
cd AssignmentModule6Svr
dotnet publish -c Release -o .\publish

# Publish client
cd AssignmentModule6Client
dotnet publish -c Release -o .\publish
```

## Environment Variables

Set environment-specific configuration:

**PowerShell:**
```powershell
$env:ASPNETCORE_ENVIRONMENT = "Production"
$env:ApiToken__StaticToken = "YOUR_PRODUCTION_TOKEN"
```

**Command Prompt:**
```cmd
set ASPNETCORE_ENVIRONMENT=Production
set ApiToken__StaticToken=YOUR_PRODUCTION_TOKEN
```

## Security Best Practices

1. **Never commit secrets**: Use `.gitignore` to exclude sensitive files
2. **Use environment variables**: For production secrets
3. **Use .NET Secret Manager**: For development secrets
4. **Change default tokens**: Before deploying to any environment
5. **Use HTTPS**: Always in production

```powershell
# Set up user secrets for development
dotnet user-secrets init --project AssignmentModule6Svr
dotnet user-secrets set "ApiToken:StaticToken" "YOUR_DEV_TOKEN" --project AssignmentModule6Svr
```

## Contributing

This is an educational project. For suggestions or issues:

1. Document the issue clearly
2. Provide steps to reproduce
3. Include expected vs actual behavior
4. Note your environment (.NET version, OS, etc.)

## License

This project is for educational purposes.

## Support

- **API Documentation**: Run server and visit `/scalar/v1`
- **Project Documentation**: See README files in each project folder
- **Detailed Guides**: Check `Docs/` folders in each project

## Version Information

- **Server Version**: 1.0.0
- **Client Version**: 2.0.0 (RFC 7807 Problem Details focused)
- **Test Coverage**: ~85%
- **Target Framework**: .NET 10
- **Status**: Production Ready

## Learning Resources

- [ASP.NET Core Documentation](https://docs.microsoft.com/aspnet/core)
- [.NET 10 Documentation](https://docs.microsoft.com/dotnet/core/whats-new/dotnet-10)
- [RFC 7807 Problem Details](https://tools.ietf.org/html/rfc7807)
- [NUnit Testing Framework](https://nunit.org/)

---

**Last Updated**: 2025  
**Platform**: Windows  
**Framework**: .NET 10  
**Build Status**: Passing
