# TechNova Contributing Guide

Welcome to TechNova! This guide explains how to contribute to the project.

## Code of Conduct

- Be respectful and professional
- Focus on the code, not the person
- Help others learn and grow
- Report security issues privately

## Getting Started

1. **Fork** the repository on GitHub
2. **Clone** your fork locally
3. **Create** a feature branch (`git checkout -b feature/your-feature`)
4. **Make** your changes
5. **Commit** with clear messages
6. **Push** to your fork
7. **Open** a Pull Request

## Development Workflow

### Before You Code

- Check **Issues** for open tasks
- Comment on an issue to claim it
- Discuss major changes first

### Commit Message Convention

Use semantic prefixes:

```
feat:     A new feature
fix:      A bug fix
docs:     Documentation changes
style:    Code style (formatting, missing semicolons, etc.)
refactor: Code refactoring without feature changes
perf:     Performance improvements
test:     Adding or updating tests
chore:    Build process, dependencies, tooling
```

**Examples**:
```
feat: Add social media feed for startups
fix: Resolve email verification token expiry bug
docs: Update API reference
test: Add unit tests for SubscriptionService
refactor: Extract validation logic to helper method
```

### Pull Request Process

1. **Update** your branch with latest main
   ```powershell
   git fetch upstream
   git rebase upstream/main
   ```

2. **Run tests** locally
   ```powershell
   dotnet test
   ```

3. **Build** project
   ```powershell
   dotnet build
   ```

4. **Push** your changes
   ```powershell
   git push origin feature/your-feature
   ```

5. **Open PR** on GitHub with:
   - Clear title: `feat: Add [feature name]`
   - Description of changes
   - Related issue: `Closes #123`
   - Screenshots (if UI changes)

6. **Address** reviewer feedback
   - Make requested changes
   - Re-push without force
   - Mark conversation as resolved

7. **Squash & Merge** when approved

## Code Style Guide

### C# Conventions

```csharp
// ✅ GOOD: Clear, descriptive names
public async Task<Subscription> GetActiveSubscriptionAsync(int investorId)
{
	var subscription = await _context.Subscriptions
		.Where(s => s.InvestorID == investorId && s.Status == "Active")
		.FirstOrDefaultAsync();
	return subscription;
}

// ❌ BAD: Unclear abbreviations
public async Task<Sub> GetSubs(int id)
{
	return await ctx.Subs.Where(s => s.ID == id).FirstOrDefaultAsync();
}
```

### Naming
- **Classes**: PascalCase (`InvestmentRequest`, `SubscriptionService`)
- **Methods**: PascalCase (`CreatePost`, `VerifyEmail`)
- **Properties**: PascalCase (`StartupID`, `InvestmentAmount`)
- **Local Variables**: camelCase (`investorId`, `emailAddress`)
- **Constants**: UPPER_SNAKE_CASE (`MAX_FILE_SIZE`, `DEFAULT_PAGE_SIZE`)
- **Private Fields**: _camelCase (`_context`, `_logger`)

### Method Guidelines

```csharp
// ✅ GOOD: Single responsibility
public bool IsSubscriptionActive(Subscription sub)
{
	return sub.Status == SubscriptionStatus.Active || 
		   sub.Status == SubscriptionStatus.Trialing;
}

// ✅ GOOD: Clear return type and parameters
public async Task<InvestmentRequest> AcceptInvestmentAsync(
	int requestId, int startupId, CancellationToken cancellationToken = default)
{
	// Implementation
}

// ❌ BAD: Too many responsibilities
public void ProcessEverything(int id) { }
```

### Async/Await

```csharp
// ✅ GOOD: Async all the way
public async Task<List<Startup>> GetStartupsAsync()
{
	return await _context.Startups
		.ToListAsync();
}

// ❌ BAD: Mixing async/sync
public async Task<List<Startup>> GetStartupsAsync()
{
	return _context.Startups.ToList(); // Blocks the thread!
}
```

### Error Handling

```csharp
// ✅ GOOD: Specific exception handling
public async Task VerifyEmailAsync(string token)
{
	if (string.IsNullOrWhiteSpace(token))
		throw new ArgumentException("Token cannot be empty", nameof(token));

	var verification = await _context.EmailVerifications
		.FirstOrDefaultAsync(v => v.Token == token);

	if (verification == null)
		throw new InvalidOperationException("Token not found or expired");

	verification.IsVerified = true;
	await _context.SaveChangesAsync();
}

// ❌ BAD: Catching too broadly
try { /* code */ }
catch (Exception ex) { /* swallow */ }
```

### Database Queries

```csharp
// ✅ GOOD: Explicit includes, no N+1
var startups = await _context.Startups
	.Include(s => s.Founders)
	.Include(s => s.InvestmentRequests)
	.Where(s => s.BusinessStage == "Series A")
	.ToListAsync();

// ❌ BAD: N+1 query problem
var startups = _context.Startups.ToList();
foreach (var s in startups)
{
	s.Founders = _context.Founders
		.Where(f => f.StartupID == s.StartupID)
		.ToList(); // Query per startup!
}
```

## File Organization

Add new files following existing patterns:

```
Models/          → Add `YourModel.cs`
Controllers/     → Add `YourController.cs`
Services/        → Add `YourService.cs` + `IYourService.cs` (if shared)
Views/Your/      → Add `Index.cshtml`, etc.
Filters/         → Add `YourFilter.cs`
```

## Testing Guidelines

### Unit Tests
- Test single method in isolation
- Mock dependencies
- Use meaningful test names

```csharp
[TestClass]
public class SubscriptionServiceTests
{
	[TestMethod]
	public async Task GetCurrentAsync_WithActiveSubscription_ReturnsSubscription()
	{
		// Arrange
		var investorId = 1;
		var mockContext = new Mock<ApplicationDbContext>();
		var subscription = new Subscription { Status = "Active" };
		mockContext.Setup(c => c.Subscriptions).Returns(subscriptions);

		var service = new SubscriptionService(mockContext.Object);

		// Act
		var result = await service.GetCurrentAsync(investorId);

		// Assert
		Assert.IsNotNull(result);
		Assert.AreEqual("Active", result.Status);
	}
}
```

### Integration Tests
- Test full workflow with real database
- Verify controller actions end-to-end

## Documentation Standards

- **Inline Comments**: Only for _why_, not _what_
- **XML Doc Comments**: For public methods
- **README Sections**: Features, setup, architecture, API

```csharp
// ✅ GOOD: Explains business logic
// Subscriptions in "Trialing" or "Active" state grant full access.
// Other states are locked down (dev features gated by this check).
public bool GrantsAccess(string status) =>
	status == SubscriptionStatus.Trialing ||
	status == SubscriptionStatus.Active;

// ❌ BAD: States the obvious
// Get the subscription
public Subscription GetSub(int id) => _context.Subs.Find(id);
```

## Security Checklist

Before submitting PR, verify:

- [ ] No hardcoded credentials
- [ ] Passwords hashed (never plain text)
- [ ] Authorization checks on protected endpoints
- [ ] CSRF token on forms
- [ ] SQL injection prevented (parameterized queries)
- [ ] File uploads validated (type + size)
- [ ] User input sanitized
- [ ] Sensitive data not logged
- [ ] HTTPS enforced in production
- [ ] Dependencies up-to-date (no known CVEs)

## Performance Checklist

- [ ] Database queries optimized (no N+1)
- [ ] Large result sets paginated
- [ ] Async/await used correctly
- [ ] Static assets minified/cached
- [ ] No blocking operations in async code

## Common Issues & Solutions

### Issue: Tests fail locally but pass on CI

**Solution**: 
- Ensure database is clean: `dotnet ef database drop && dotnet ef database update`
- Check for timezone-dependent tests
- Verify all dependencies installed

### Issue: Cannot push to main branch

**Solution**:
- You shouldn't push directly; use feature branches
- Open a PR instead
- Admins only merge via PR reviews

### Issue: Migration conflicts

**Solution**:
```powershell
# Remove your local migration
dotnet ef migrations remove

# Pull latest from main
git pull origin main

# Reapply your changes
dotnet ef migrations add YourFeature
```

## Release Process

1. **Bump version** in `.csproj`
2. **Update CHANGELOG** with changes
3. **Merge** to main via PR
4. **Tag** release: `git tag v1.2.3`
5. **Push tag**: `git push origin v1.2.3`
6. **Deploy** to production

## Questions?

- **Open Issue**: For bugs or feature requests
- **Discussions Tab**: For questions and ideas
- **Email**: Contact maintainers directly

## Thank You! 🎉

Your contributions make TechNova better. We appreciate your time and effort!
