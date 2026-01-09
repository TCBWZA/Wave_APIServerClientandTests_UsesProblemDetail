# GitHub Setup Guide

This guide will help you set up your GitHub repository for the AssignmentModule6 project.

## Prerequisites

- Git installed on your system
- GitHub account created
- .NET 10 SDK installed

## Step 1: Initialize Git Repository (If Not Already Done)

```powershell
# Navigate to your project root
cd D:\Users\tbw_\source\repos\AssignmentModule6.Svr

# Initialize git (if not already initialized)
git init

# Check git status
git status
```

## Step 2: Create GitHub Repository

1. Go to [GitHub](https://github.com)
2. Click the **+** icon in top right ? **New repository**
3. Fill in repository details:
   - **Repository name**: `AssignmentModule6.Svr` (or your preferred name)
   - **Description**: "Customer Management API Suite with .NET 10 - RESTful API, Client Library, and Test Suite"
   - **Visibility**: Choose Public or Private
   - **DO NOT** initialize with README, .gitignore, or license (we already have these)
4. Click **Create repository**

## Step 3: Configure Git User (If Not Already Done)

```powershell
# Set your name
git config --global user.name "Your Name"

# Set your email
git config --global user.email "your.email@example.com"

# Verify configuration
git config --global --list
```

## Step 4: Review Files Before Committing

```powershell
# Check what files will be staged
git status

# Review .gitignore to ensure sensitive files are excluded
# The .gitignore is already configured to exclude:
# - bin/ and obj/ folders
# - appsettings.Development.json and appsettings.Production.json
# - User secrets
# - Build artifacts
```

## Step 5: Stage and Commit Files

```powershell
# Add all files (respecting .gitignore)
git add .

# Check what's staged
git status

# Create initial commit
git commit -m "Initial commit: Customer Management API Suite with .NET 10"
```

## Step 6: Connect to GitHub Remote

Replace `<your-username>` with your GitHub username and `<repository-name>` with your repo name:

```powershell
# Add remote repository
git remote add origin https://github.com/<your-username>/<repository-name>.git

# Verify remote
git remote -v
```

Example:
```powershell
git remote add origin https://github.com/johndoe/AssignmentModule6.Svr.git
```

## Step 7: Push to GitHub

```powershell
# Push to main branch
git branch -M main
git push -u origin main
```

If you get authentication prompts:
- **HTTPS**: Use GitHub Personal Access Token (not password)
- **SSH**: Use SSH key (see SSH setup below)

## Step 8: Verify on GitHub

1. Go to your repository on GitHub
2. Verify all files are present
3. Check that README.md displays correctly
4. Verify .gitignore is working (no bin/obj folders)

## Step 9: Configure Repository Settings (Optional)

On GitHub, go to repository Settings:

### General
- Add topics/tags: `dotnet`, `csharp`, `rest-api`, `nunit`, `netcore`
- Add website URL if applicable

### Security
- Enable **Dependabot alerts**
- Enable **Code scanning**
- Review SECURITY.md file

### Actions
- Enable **GitHub Actions** (for CI/CD)
- Review `.github/workflows/dotnet.yml`

### Branches
- Set `main` as default branch
- Consider adding branch protection rules:
  - Require pull request reviews
  - Require status checks to pass
  - Require branches to be up to date

## Step 10: Create Development Branch (Recommended)

```powershell
# Create and switch to develop branch
git checkout -b develop

# Push develop branch
git push -u origin develop
```

## Setting Up SSH (Alternative to HTTPS)

### Generate SSH Key

```powershell
# Generate SSH key
ssh-keygen -t ed25519 -C "your.email@example.com"

# Start SSH agent
ssh-agent

# Add key to agent
ssh-add ~/.ssh/id_ed25519
```

### Add SSH Key to GitHub

1. Copy your public key:
   ```powershell
   cat ~/.ssh/id_ed25519.pub
   ```
2. Go to GitHub ? Settings ? SSH and GPG keys
3. Click **New SSH key**
4. Paste your public key
5. Save

### Change Remote to SSH

```powershell
# Change remote URL to SSH
git remote set-url origin git@github.com:<your-username>/<repository-name>.git

# Verify
git remote -v
```

## Using Personal Access Token (PAT) for HTTPS

### Create PAT

1. Go to GitHub ? Settings ? Developer settings ? Personal access tokens ? Tokens (classic)
2. Click **Generate new token (classic)**
3. Give it a name: "AssignmentModule6 Development"
4. Set expiration (recommend 90 days)
5. Select scopes:
   - `repo` (all)
   - `workflow`
6. Click **Generate token**
7. **COPY THE TOKEN** (you won't see it again!)

### Use PAT for Authentication

When prompted for password, use your PAT instead.

Or configure credential helper:

```powershell
# Windows
git config --global credential.helper wincred

# First push will prompt for credentials
# Username: your-github-username
# Password: your-personal-access-token
```

## Common Git Commands

```powershell
# Check status
git status

# View commit history
git log --oneline

# Create new branch
git checkout -b feature/new-feature

# Switch branches
git checkout main

# Pull latest changes
git pull

# Push changes
git push

# View differences
git diff

# Undo changes (before commit)
git checkout -- filename

# Undo last commit (keep changes)
git reset --soft HEAD~1
```

## .gitignore Highlights

The included `.gitignore` already excludes:
- Build outputs (`bin/`, `obj/`)
- Visual Studio files (`.vs/`, `*.user`)
- NuGet packages
- **Sensitive configuration files** (`appsettings.*.json` except base `appsettings.json`)
- User secrets
- Test results
- Coverage reports
- Temporary markdown files

## GitHub Actions CI/CD

The repository includes a GitHub Actions workflow (`.github/workflows/dotnet.yml`) that:
- Runs on push to `main` or `develop`
- Runs on pull requests
- Builds all projects
- Runs all tests
- Generates code coverage reports

First push will trigger the workflow automatically.

## Next Steps

After setup:
1. Create a `develop` branch for active development
2. Use feature branches for new features
3. Create pull requests for code review
4. Monitor GitHub Actions for build status
5. Keep dependencies updated
6. Review security alerts

## Troubleshooting

### "fatal: remote origin already exists"
```powershell
git remote remove origin
git remote add origin <your-repo-url>
```

### "Permission denied (publickey)"
- Check SSH key is added to GitHub
- Verify SSH agent is running
- Try HTTPS instead

### "Updates were rejected"
```powershell
# Pull first, then push
git pull origin main --rebase
git push origin main
```

### Large files warning
- Check `.gitignore` is working
- Remove large files from history if needed
- Use Git LFS for large binary files

## Resources

- [GitHub Documentation](https://docs.github.com)
- [Git Documentation](https://git-scm.com/doc)
- [GitHub Actions Documentation](https://docs.github.com/actions)
- [Git Cheat Sheet](https://education.github.com/git-cheat-sheet-education.pdf)

## Support

If you encounter issues:
1. Check error messages carefully
2. Review GitHub documentation
3. Check repository settings
4. Verify credentials are correct

---

**Ready to push?** Run:
```powershell
git add .
git commit -m "Your commit message"
git push origin main
```

Good luck! ??
