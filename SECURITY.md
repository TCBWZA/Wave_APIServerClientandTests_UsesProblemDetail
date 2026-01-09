# Security Policy

## Supported Versions

Currently supported versions for security updates:

| Version | Supported          |
| ------- | ------------------ |
| 2.0.x   | :white_check_mark: |
| 1.0.x   | :x:                |

## Reporting a Vulnerability

If you discover a security vulnerability in this project, please report it privately:

### How to Report

1. **Do NOT create a public GitHub issue**
2. **Send details to**: [your-email@example.com] (replace with your contact)
3. **Include**:
   - Description of the vulnerability
   - Steps to reproduce
   - Potential impact
   - Suggested fix (if any)

### What to Expect

- **Initial Response**: Within 48 hours
- **Status Update**: Within 1 week
- **Fix Timeline**: Depends on severity
  - Critical: 1-7 days
  - High: 1-2 weeks
  - Medium: 2-4 weeks
  - Low: Best effort

### Security Best Practices

When using this project:

1. **Never commit secrets** to version control
   - Use `.gitignore` to exclude sensitive files
   - Use environment variables for production secrets
   - Use .NET Secret Manager for development

2. **Change default tokens** before deployment
   - Replace all "YOUR_*_HERE" placeholders
   - Use strong, unique tokens
   - Rotate tokens regularly

3. **Use HTTPS** in production
   - Never use HTTP for sensitive operations
   - Validate SSL certificates
   - Enable HSTS

4. **Keep dependencies updated**
   - Regularly update NuGet packages
   - Monitor for security advisories
   - Test updates before deploying

5. **Review API authentication**
   - Implement rate limiting
   - Use proper API key validation
   - Consider JWT tokens for production

### Known Security Considerations

#### Development Mode Only
- This project uses an in-memory database
- Sample data includes fake but realistic information
- Default API tokens are for development only

#### Production Deployment
Before deploying to production:
- [ ] Replace all default tokens
- [ ] Implement persistent database
- [ ] Enable HTTPS/TLS
- [ ] Configure CORS policies
- [ ] Set up rate limiting
- [ ] Enable audit logging
- [ ] Review and test authentication
- [ ] Scan for vulnerabilities

### Security Features

This project includes:
- API key authentication for DELETE operations
- RFC 7807 Problem Details (no sensitive data in errors)
- Structured logging (token masking)
- Configuration separation (secrets via environment)

### Out of Scope

The following are not considered security vulnerabilities:
- Issues in development/testing environments
- Features working as documented
- Vulnerabilities in dependencies (report to respective projects)

## Security Updates

Security updates will be:
- Published in release notes
- Announced in CHANGELOG
- Tagged with security labels
- Documented in commit messages

---

**Last Updated**: 2025
**Contact**: [your-email@example.com] (update this)
