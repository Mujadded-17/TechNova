# TechNova Architecture Guide

## System Architecture

### Layered Architecture Pattern

```
┌─────────────────────────────────────────────┐
│         Presentation Layer                   │
│  (Razor Pages, Views, JavaScript, CSS)      │
└─────────────────────────────────────────────┘
					  ↓
┌─────────────────────────────────────────────┐
│      Controllers & HTTP Layer                │
│  (Account, Investor, Startup, Admin, etc.)  │
└─────────────────────────────────────────────┘
					  ↓
┌─────────────────────────────────────────────┐
│      Business Logic Layer                    │
│  (Services: Subscription, Email, Media)     │
└─────────────────────────────────────────────┘
					  ↓
┌─────────────────────────────────────────────┐
│      Data Access Layer                       │
│  (Entity Framework Core, DbContext)         │
└─────────────────────────────────────────────┘
					  ↓
┌─────────────────────────────────────────────┐
│      Database Layer                          │
│  (SQL Server with 15+ tables)                │
└─────────────────────────────────────────────┘
```

## Dependency Injection Container

All services registered in `Program.cs`:
- `AddDbContext<ApplicationDbContext>()` - Database
- `AddScoped<InvestmentPdfService>()` - PDF generation
- `AddScoped<SubscriptionService>()` - Subscription logic
- `AddScoped<IEmailSender>()` - Email (SMTP or Dev)
- `AddScoped<EmailVerificationService>()` - Verification tokens
- `AddScoped<PitchDeckStorage>()` - File storage
- `AddScoped<IStartupMediaService>()` - Media data access
- `AddScoped<IMediaUploadService>()` - File uploads

## Authentication & Authorization Flow

```
User Request
	↓
[Cookie Middleware] - Validates auth cookie
	↓
[Claims Identity] - Extracts user role, ID, email
	↓
[Controllers] - Apply [Authorize] or custom filters
	↓
[RequiresSignedIn] - Redirects if not authenticated
[RequiresSubscription] - Checks investor subscription status
	↓
Action Method Executes
```

## Request Pipeline

```
User Request
	↓
HTTPS Redirect (prod only)
	↓
Routing - Match pattern to controller/action
	↓
Authentication - Load claims from cookie
	↓
Authorization - Apply [Authorize] filters
	↓
Controller Action
	↓
Service Layer (business logic)
	↓
DbContext (data access)
	↓
SQL Server (persistence)
	↓
Response (View rendering / JSON)
```

## Entity Relationship Model

### Core Entities
- **Startup** (1) ↔ (many) **Founder**
- **Startup** (1) ↔ (many) **PitchDeck**
- **Startup** (1) ↔ (many) **Post**
- **Post** (1) ↔ (many) **Photo** / **Video**

### Investment Flow
- **Investor** (1) ↔ (many) **InvestmentRequest**
- **Startup** (1) ↔ (many) **StartupInvestmentOpportunity**
- **Startup** (1) ↔ (many) **InvestmentRequest**

### User Management
- **Investor** (1) ↔ (many) **Subscription**
- **Investor** (1) ↔ (many) **Message**
- **Startup** (1) ↔ (many) **Message**

### Billing
- **Investor** (1) ↔ (many) **Payment**
- **Subscription** → **SubscriptionPlan**

## Service Layer Responsibilities

### SubscriptionService
- Determines investor access level
- Manages subscription state transitions
- Validates active vs. expired subscriptions

### EmailVerificationService
- Generates verification tokens
- Validates token expiry (30 min)
- Prevents reuse of tokens

### MediaUploadService
- Validates file type (whitelist)
- Checks file size limits
- Generates secure filenames
- Stores in `wwwroot/startup-media/`

### StartupMediaService
- Queries posts with pagination
- Filters soft-deleted content
- Counts photos/videos per post
- Loads related entities

### InvestmentPdfService
- Generates PDF agreements
- Includes investment terms
- Timestamps and signatures

## Error Handling

```
Exception Thrown
	↓
[Exception Handling Middleware]
	↓
Development: Detailed error page
Production: Generic error view (/Home/Error)
	↓
Logged to ILogger
	↓
User Feedback
```

## Caching Strategy

- **Static Assets**: Browser cache (CSS, JS, images)
- **Database**: Default EF Core tracking (in-memory during request)
- **No Distributed Cache**: Stateless design supports scaling

## Security Boundaries

1. **Authentication**: Cookie validation
2. **Authorization**: Claims-based (role, ID)
3. **CSRF**: Anti-forgery tokens on forms
4. **SQL Injection**: Parameterized queries (EF Core)
5. **XSS**: Razor view encoding
6. **File Upload**: Type + size validation

## Deployment Architecture

```
Load Balancer
	↓
	├→ Web Server 1 (TechNova app)
	├→ Web Server 2 (TechNova app)
	└→ Web Server N (TechNova app)
	↓
	Shared SQL Server Database
	↓
	File Share (startup-media/)
```

