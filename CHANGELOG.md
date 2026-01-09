# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [2.0.0] - 2025-01-XX

### Added - Client v2.0
- **RFC 7807 Problem Details** parsing and structured logging
- **Microsoft.Extensions.Logging** for structured logging
- **Microsoft.Extensions.Configuration** for settings management
- **Dependency Injection** with IServiceCollection
- Environment-specific configuration support (Dev/Prod)
- Problem Details examples and error recovery strategies
- Comprehensive documentation for v2.0 features

### Changed
- **BREAKING**: Client now requires .NET 10 or later
- Upgraded all documentation to emphasize Problem Details
- Replaced bash commands with Windows PowerShell/CMD
- Configuration now uses appsettings.json instead of hardcoded values
- Logging now uses structured format with contextual data

### Security
- Removed all hardcoded API tokens from configuration
- Added token masking in logs (only first 4 characters shown)
- Configuration separation for sensitive data
- Audit trail for all API operations

## [1.0.0] - 2025-01-XX

### Added - Initial Release

#### Server (AssignmentModule6Svr)
- RESTful API for customers, invoices, and phone numbers
- Complete CRUD operations
- Auto-generated IDs with global uniqueness
- API key authentication for DELETE operations
- RFC 7807 Problem Details error responses
- Scalar API documentation UI
- In-memory database with sample data generation
- 74 comprehensive NUnit tests (~85% coverage)

#### Client (AssignmentModule6Client v1.0)
- Basic HTTP client for API consumption
- Type-safe JSON serialization
- Simple error handling
- Color-coded console output
- 7 example scenarios

#### Tests (AssignmentModule6Svr.Tests)
- NUnit test framework
- FluentAssertions for readable tests
- Moq for mocking
- Integration tests with WebApplicationFactory
- Tests for CRUD, validation, authentication

### Features
- Customer management with invoices and phone numbers
- Invoice number validation (must start with "INV")
- Duplicate invoice detection (409 Conflict)
- Cascade deletion (customer -> invoices -> phones)
- Global ID uniqueness for invoices and phone numbers
- Bogus library for realistic fake data
- XML documentation for all public APIs

### Documentation
- Comprehensive README files for each project
- Quick reference guides
- API documentation with Scalar
- Code examples and troubleshooting guides

---

## Version Guidelines

### Semantic Versioning
- **MAJOR**: Breaking changes (e.g., .NET version requirements)
- **MINOR**: New features (backward compatible)
- **PATCH**: Bug fixes (backward compatible)

### Version History
- v2.0.0: Client upgrade with Problem Details (requires .NET 10)
- v1.0.0: Initial release (.NET 8+ compatible)

---

[Unreleased]: https://github.com/yourusername/AssignmentModule6.Svr/compare/v2.0.0...HEAD
[2.0.0]: https://github.com/yourusername/AssignmentModule6.Svr/compare/v1.0.0...v2.0.0
[1.0.0]: https://github.com/yourusername/AssignmentModule6.Svr/releases/tag/v1.0.0
