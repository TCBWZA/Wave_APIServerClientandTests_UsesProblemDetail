# Contributing to AssignmentModule6

Thank you for considering contributing to this project! This is an educational project, but we welcome improvements and suggestions.

## How to Contribute

### Reporting Issues

If you find a bug or have a suggestion:

1. **Check existing issues** to avoid duplicates
2. **Create a new issue** with a descriptive title
3. **Provide details**:
   - Steps to reproduce the problem
   - Expected behavior
   - Actual behavior
   - Environment details (.NET version, OS, etc.)
   - Screenshots if applicable

### Submitting Changes

1. **Fork the repository**
2. **Create a feature branch**:
   ```powershell
   git checkout -b feature/your-feature-name
   ```
3. **Make your changes**:
   - Follow existing code style
   - Add or update tests as needed
   - Update documentation
4. **Test your changes**:
   ```powershell
   dotnet build
   dotnet test
   ```
5. **Commit your changes**:
   ```powershell
   git add .
   git commit -m "Description of your changes"
   ```
6. **Push to your fork**:
   ```powershell
   git push origin feature/your-feature-name
   ```
7. **Create a Pull Request**

## Development Guidelines

### Code Style

- Follow C# naming conventions (PascalCase for public members)
- Use meaningful variable and method names
- Add XML documentation comments for public APIs
- Keep methods focused and concise
- Use async/await for I/O operations

### Testing

- Write tests for new features
- Maintain or improve code coverage (target: >80%)
- Use descriptive test names: `MethodName_Scenario_ExpectedBehavior`
- Follow AAA pattern (Arrange-Act-Assert)

### Documentation

- Update README files when adding features
- Add code comments for complex logic
- Include examples in documentation
- Keep documentation synchronized with code

### Commit Messages

Use clear and descriptive commit messages:

- **Good**: "Add Problem Details parsing to ApiClient"
- **Good**: "Fix null reference in customer validation"
- **Bad**: "Update code"
- **Bad**: "Fix bug"

Format:
```
<type>: <description>

[optional body]

[optional footer]
```

Types:
- **feat**: New feature
- **fix**: Bug fix
- **docs**: Documentation changes
- **test**: Adding or updating tests
- **refactor**: Code refactoring
- **style**: Formatting changes
- **perf**: Performance improvements

### Security

- Never commit sensitive information (passwords, API keys, tokens)
- Use configuration files and environment variables for secrets
- Review `.gitignore` to ensure secrets are excluded
- Report security vulnerabilities privately

## Project Structure

```
AssignmentModule6.Svr/
|-- AssignmentModule6Svr/           # API Server
|-- AssignmentModule6Client/        # Client Application  
|-- AssignmentModule6Svr.Tests/     # Test Suite
```

## Building and Testing

```powershell
# Restore dependencies
dotnet restore

# Build all projects
dotnet build

# Run all tests
dotnet test

# Run specific test project
dotnet test AssignmentModule6Svr.Tests/

# Build for release
dotnet build -c Release
```

## Code Review Process

1. All submissions require review
2. Reviewers will check:
   - Code quality and style
   - Test coverage
   - Documentation updates
   - Breaking changes
3. Address feedback promptly
4. Maintainers will merge approved PRs

## Questions?

If you have questions about contributing:
- Check existing documentation
- Review closed issues for similar questions
- Open a new issue with the "question" label

## License

By contributing, you agree that your contributions will be licensed under the same license as the project.

---

Thank you for contributing to AssignmentModule6!
