# TechNova Setup & Development Guide

## Prerequisites

- **Visual Studio 2022+** or **VS Code**
- **.NET 10 SDK** (or later)
- **SQL Server 2019+** or **SQL Server Express LocalDB**
- **Git**

## Initial Setup

### 1. Clone Repository

```powershell
git clone https://github.com/Mujadded-17/TechNova.git
cd TechNova
```

### 2. Restore Dependencies

```powershell
dotnet restore
```

### 3. Setup Database

```powershell
# Navigate to project directory
cd TechNova

# Apply migrations (auto-seeds admin + plans in dev mode)
dotnet ef database update
```

**Connection String** (in `appsettings.json`):
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=TechNovaDb;Trusted_Connection=True;"
}
```

### 4. Configure Email (Optional for Dev)

**Development Mode**: Emails written to `App_Data/sent-emails/` with clickable links.

**Production SMTP** (set via environment variables or user secrets):
```powershell
# Using dotnet user-secrets
dotnet user-secrets set "Email:Smtp:Host" "smtp.gmail.com"
dotnet user-secrets set "Email:Smtp:Port" "587"
dotnet user-secrets set "Email:Smtp:User" "your-email@gmail.com"
dotnet user-secrets set "Email:Smtp:Password" "your-app-password"
dotnet user-secrets set "Email:FromAddress" "noreply@technova.app"
dotnet user-secrets set "Email:FromName" "Tech Nova"
```

### 5. Run Application

```powershell
dotnet run
```

Navigate to: `https://localhost:7068`

## Development Seeding

On first run in development mode, the app auto-seeds:

1. **Admin User** (via `AdminSeeder.cs`)
   - Email: `admin@technova.local`
   - Password: Check `appsettings.json` for dev credentials

2. **Subscription Plans** (via `PlanSeeder.cs`)
   - Basic (free tier)
   - Premium ($29/month)
   - Enterprise ($99/month)

## File Structure Guide

```
TechNova/
├── Controllers/          # HTTP request handlers (12 files)
├── Models/               # Data models (22 classes)
├── Views/                # Razor templates (50+ .cshtml files)
├── Services/             # Business logic (7 services)
├── Data/
│   ├── ApplicationDbContext.cs    # EF Core configuration
│   ├── Migrations/                # Database versioning
│   ├── AdminSeeder.cs             # Dev data
│   └── PlanSeeder.cs              # Subscription tiers
├── Filters/              # Authorization attributes
├── wwwroot/              # Static files
│   ├── css/              # Stylesheets (8 files)
│   ├── js/               # JavaScript (5 files)
│   ├── lib/              # Bootstrap, jQuery
│   └── startup-media/    # User uploads
├── Program.cs            # Startup configuration
├── appsettings.json      # Environment-specific config
└── TechNova.csproj       # Project file
```

## Database Migrations

### View Migration History

```powershell
dotnet ef migrations list
```

### Create New Migration

```powershell
# Make model changes, then:
dotnet ef migrations add DescriptiveNameHere

# Review generated migration file
# Apply to database
dotnet ef database update
```

### Rollback Migration

```powershell
# Go back one migration
dotnet ef database update PreviousMigrationName

# Delete latest migration (not applied yet)
dotnet ef migrations remove
```

## Debugging

### Enable Debug Logging

In `appsettings.json`:
```json
"Logging": {
  "LogLevel": {
	"Default": "Debug",
	"Microsoft.EntityFrameworkCore": "Debug"
  }
}
```

### Visual Studio Debugger

1. Set breakpoint (click line number)
2. Press `F5` or click **Debug → Start Debugging**
3. Application pauses at breakpoint
4. Inspect variables in **Watch**, **Locals** windows
5. Continue with `F10` (step over) or `F5` (continue)

### View Generated SQL

```csharp
// In a service
var sql = context.InvestmentRequests.ToQueryString();
Console.WriteLine(sql);
```

## Common Tasks

### Add New Page/View

```powershell
# 1. Create controller action
# File: Controllers/YourController.cs
public IActionResult Index() => View();

# 2. Create view
# File: Views/Your/Index.cshtml
@page
<h1>Your Page</h1>

# 3. Add route in Program.cs (if needed)
```

### Add New Database Table

```powershell
# 1. Create model class
# File: Models/YourModel.cs
public class YourModel { 
  public int Id { get; set; }
  public string Name { get; set; }
}

# 2. Add DbSet to ApplicationDbContext
public DbSet<YourModel> YourModels { get; set; }

# 3. Create migration
dotnet ef migrations add AddYourModel

# 4. Apply to database
dotnet ef database update
```

### Upload File with Validation

```csharp
// In controller
if (File.ContentLength > 10_000_000) // 10MB
  return BadRequest("File too large");

if (!IsValidImageType(File.ContentType))
  return BadRequest("Invalid file type");

var filename = Path.GetRandomFileName();
var filePath = Path.Combine("wwwroot/startup-media", filename);
File.SaveAsAsync(filePath);
```

## Testing

### Run All Tests

```powershell
dotnet test
```

### Run Specific Test Class

```powershell
dotnet test --filter MyTestClass
```

### Run Single Test

```powershell
dotnet test --filter MyTestClass.MyTestMethod
```

## Performance Tips

1. **Use .Include()** to avoid N+1 queries
2. **Async/Await** for all I/O operations
3. **Pagination** for large result sets
4. **Cache** frequently accessed data
5. **Index** foreign keys in database

## Troubleshooting

| Issue | Solution |
|-------|----------|
| `InvalidOperationException: No service` | Verify service is registered in `Program.cs` |
| `DbContext already disposed` | Use `using` or ensure DI injection |
| `Foreign key violation` | Check cascade delete settings in `OnModelCreating` |
| `Email not sending` | Verify SMTP credentials in user secrets |
| `Static assets 404` | Ensure `MapStaticAssets()` called in Program.cs |
| `Cookie authentication not working` | Check `AddAuthentication()` middleware order |

## IDE Recommendations

### Visual Studio 2022
- **Extensions**: Productivity Power Tools, GitHub Copilot
- **Tools → Options**: Code cleanup before save

### VS Code
- **Extensions**: C#, C# Dev Kit, REST Client
- **.vscode/settings.json**: Editor formatting

## Git Workflow

```powershell
# Create feature branch
git checkout -b feature/my-feature

# Make changes, commit
git add .
git commit -m "feat: description"

# Push to GitHub
git push origin feature/my-feature

# Create Pull Request on GitHub
# Request review → Merge → Delete branch
```

## Environment Variables (Production)

Set these on your hosting platform:
- `ASPNETCORE_ENVIRONMENT=Production`
- `ConnectionStrings__DefaultConnection=...`
- `Email:Smtp:Host`, `Email:Smtp:Port`, `Email:Smtp:User`, `Email:Smtp:Password`
- `Stripe:SecretKey`, `Stripe:PublishableKey`
- `ASPNETCORE_URLS=http://+:80;https://+:443`

## Next Steps

1. ✅ Clone repo and run locally
2. ✅ Explore the codebase using this guide
3. ✅ Create a feature branch for your work
4. ✅ Make changes and test locally
5. ✅ Push and create a PR
