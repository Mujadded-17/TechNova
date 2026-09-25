# TechNova Project Summary

## Overview

TechNova is a comprehensive web platform that connects **Startups** and **Investors** in a unified ecosystem. Built with ASP.NET Core Razor Pages, the platform enables startups to showcase their business ideas and attract investments, while empowering investors to discover promising startup opportunities and manage their investment portfolios.

---

## Technology Stack

### Backend
- **Framework**: ASP.NET Core (.NET 10)
- **Language**: C# 14.0
- **Database**: SQL Server (LocalDB)
- **ORM**: Entity Framework Core
- **Authentication**: Cookie-based authentication with claims
- **Email**: SMTP (Gmail configured, dev mode writes to disk)

### Frontend
- **View Engine**: Razor Pages (.cshtml)
- **Styling**: Bootstrap 5 + Custom CSS (tokens-based design system)
- **JavaScript**: jQuery & vanilla JS with AJAX
- **Layout Pattern**: Master page (_Layout.cshtml) with components

### Key Libraries
- QuestPDF (Investment PDF generation)
- Entity Framework Core (Data access & migrations)
- ASP.NET Core Identity components (Password hashing)
- jQuery Validation (Client-side form validation)

---

## Database Architecture

### Core Entities

#### User Models
- **Startup**: Company profile with funding requirements, business details, media
- **Founder**: Team members associated with a startup
- **Investor**: Individual investor profiles with investment preferences
- **Admin**: Platform administrators with verification and billing oversight

#### Business Models
- **InvestmentRequest**: Investment pitch from an investor to a startup
- **StartupInvestmentOpportunity**: Funding opportunities posted by startups
- **PitchDeck**: Stored presentation documents for startups
- **FavoriteStartup**: Investor's bookmarked startups

#### Communication
- **Message**: Direct messaging between startups and investors
- **Post**: Social media feed posts (Facebook-style)
- **Photo**: Images in posts
- **Video**: Videos in posts

#### Subscription & Billing
- **Subscription**: Active investor subscriptions (trial, active, pending, expired, cancelled)
- **SubscriptionPlan**: Tier definitions (Basic, Premium, Enterprise)
- **Payment**: Transaction records

#### Hardware Integration
- **NfcCardRequest**: Digital business cards with NFC + Stripe payment tracking

---

## Key Features

### 1. Authentication & Authorization
- **Dual-role login**: Startups and Investors register separately
- **Email verification**: Required for account activation
- **Password hashing**: Using ASP.NET Core PasswordHasher
- **Cookie authentication**: 30-day sliding expiration
- **Filters**: `RequiresSignedInAttribute`, `RequiresSubscriptionAttribute`

### 2. Investor Features
- **Discover Dashboard**: Browse and filter startups by industry, stage, location
- **Favorites**: Save and manage favorite startups
- **Investment Requests**: Send investment proposals with custom amounts
- **Subscription Plans**: Free trial + paid access to premium startups
- **Messaging**: Direct communication with startup founders
- **Profile Management**: Investment preferences, portfolio info
- **Profile Verification**: KYC verification by admins

### 3. Startup Features
- **Company Profiles**: Detailed business information with media
- **Investment Opportunities**: Post funding rounds with custom terms
- **Pitch Decks**: Upload and manage presentation documents
- **Founder Management**: Add team members to company profile
- **Social Media Feed**: Post updates, photos, and videos (Facebook-style)
- **Investment Requests**: Review and accept investor proposals
- **Messaging**: Communicate with interested investors
- **PDF Downloads**: Generate investment agreements and confirmations

### 4. Admin Dashboard
- **User Management**: Approve/reject startup registrations, manage investor KYC
- **Profile Verification**: Review and verify investor and startup profiles
- **Billing Oversight**: Monitor subscription revenue and plan distribution
- **NFC Card Admin**: Process physical smart business card orders
- **Reports**: Analytics on platform activity

### 5. Subscription & Billing System
- **Plans**: Basic (free trial), Premium, Enterprise tiers
- **Manual Bank Transfer**: Payment method with confirmation workflow
- **Stripe Integration**: (Configured for NFC card payments)
- **Status Tracking**: Trialing → Active → PendingPayment → Expired/Cancelled
- **Access Control**: Features gated behind subscription status

### 6. Social Media Features
- **Facebook-Style Dashboard**: Modern timeline feed for startups
- **Post Management**: Create, edit, delete feed posts (soft delete)
- **Media Upload**: Photo and video attachments with validation
- **File Limits**: 10MB photos, 100MB videos
- **Private Dashboard**: Startup's own content hub
- **Public Profile**: Investor view of startup's social presence

### 7. NFC Smart Business Cards
- **Digital Profiles**: QR-encoded URL written to NFC chip
- **Tiered Pricing**: Orders through Stripe checkout
- **Admin Approval**: Payment verification before issuance
- **Status Workflow**: Pending → Paid → Approved → Issued

### 8. Messaging System
- **Thread-based Conversations**: Between startups and investors
- **Unread Tracking**: Mark messages as read; navbar badge shows count
- **Real-time Updates**: View component displays unread count
- **Timestamp Logging**: All messages timestamped (UTC)

---

## Project Structure

### Controllers (12 total)
| Controller | Purpose |
|-----------|---------|
| **AccountController** | Login, registration, email verification, profile CRUD |
| **HomeController** | Landing page, privacy policy |
| **InvestorController** | Investor dashboard, discover, favorites, requests |
| **StartupController** | Startup dashboard, profile, opportunities, investments |
| **ExploreController** | Public endpoints for browsing profiles |
| **MessageController** | Messaging API: send, thread, unread count |
| **PitchDeckController** | Pitch deck upload, storage, download |
| **NfcCardController** | User-facing NFC card checkout flow |
| **AdminNfcCardController** | Admin NFC card request processing |
| **BillingController** | Subscription plans, checkout, manual payments |
| **CardController** | Public NFC card profile viewing |
| **AdminController** | Admin dashboard, user verification, reports |

### Services (7 total)
| Service | Purpose |
|---------|---------|
| **IEmailSender** (SmtpEmailSender / DevEmailSender) | Email delivery via SMTP or local file |
| **EmailVerificationService** | Token generation and validation for email verification |
| **SubscriptionService** | Subscription state machine and access control logic |
| **InvestmentPdfService** | Generate PDF investment agreements |
| **PitchDeckStorage** | Manage pitch deck file I/O |
| **IStartupMediaService** (StartupMediaService) | Post/photo/video data access layer |
| **IMediaUploadService** (MediaUploadService) | File upload validation and storage |

### Data Layer
- **ApplicationDbContext**: 25+ DbSet definitions with fluent API relationships
- **Migrations**: 10 migration files tracking schema evolution
- **Seeders**: `AdminSeeder` (dev-only admin), `PlanSeeder` (subscription tiers)

### Models (22 total)
**Core**: Startup, Founder, Investor, Admin  
**Business**: InvestmentRequest, StartupInvestmentOpportunity, PitchDeck, FavoriteStartup  
**Social**: Post, Photo, Video  
**Messaging**: Message  
**Billing**: Subscription, SubscriptionPlan, Payment  
**Hardware**: NfcCardRequest  
**View Models**: RegisterStartup, RegisterInvestor, ErrorViewModel, PagerModel

### Views (50+ .cshtml files)
**Layouts**: _Layout.cshtml, _AuthLayout.cshtml, _LandingLayout.cshtml  
**Shared**: _Pager, _PaymentPill, _StatusPill, _SubscriptionPill, _AdminTabs, _ValidationScriptsPartial  
**Account**: Login, Register, VerifyEmail, Profile, AccessDenied  
**Admin**: Dashboard, Investors, Startups, Billing, Reports, ProfileVerification, NfcCard  
**Investor**: Dashboard, Discover, Favorites, Profile, MyRequests  
**Startup**: Dashboard, Profile, EditProfile, Opportunities, InvestmentRequests  
**Billing**: Plans, Checkout, Manage  
**Message**: Index (thread list), New, Thread (conversation)  
**Other**: Home, Explore (public browse), NfcCard, Card (public profile)

### Static Assets
- **CSS**: 8 primary stylesheets
  - `app.css` – Modern app-wide design
  - `facebook-style.css` – 1000+ lines for social feed
  - `auth.css` – Authentication pages
  - `landing.css` – Homepage
  - `profile.css` – Profile pages
  - `account.css` – Account settings
  - `tokens.css` – Design system variables
  - Bootstrap 5 (minified)

- **JavaScript**: 5 custom scripts + jQuery libraries
  - `site.js` – Global utilities
  - `auth.js` – Authentication flows
  - `landing.js` – Homepage interactivity
  - `startup-dashboard.js` – Facebook-style feed management
  - `velaris.js` – Component library

- **Libraries**: Bootstrap, jQuery, jQuery Validation

---

## Authentication & Access Control

### Login Flow
1. User selects role (Startup/Investor) on login page
2. Credentials validated against stored PasswordHash
3. Claims-based cookie created with role, ID, email
4. Sliding expiration resets on each request (30 days)

### Authorization
- **Filters**:
  - `RequiresSignedInAttribute`: Redirect to login if not authenticated
  - `RequiresSubscriptionAttribute`: Check investor subscription status
- **Route Guards**: 
  - Admins: Full access to `/Admin/*`
  - Startups: Read/write `/Startup/*`, read-only `/Explore/*`
  - Investors: Read/write `/Investor/*`, read-only `/Explore/*`

### Email Verification
- **Token Generation**: 30-minute GUID tokens
- **Re-send**: Throttled to prevent spam
- **Dev Mode**: Emails written to `App_Data/sent-emails/`, links remain clickable

---

## Database Relationships

```
Startup (1) ──→ (many) Founder
Startup (1) ──→ (many) PitchDeck
Startup (1) ──→ (many) InvestmentRequest ←─ (1) Investor
Startup (1) ──→ (many) StartupInvestmentOpportunity
Startup (1) ──→ (many) Message ←─ (1) Investor
Startup (1) ──→ (many) Post
Post (1) ──→ (many) Photo
Post (1) ──→ (many) Video
Investor (1) ──→ (many) Subscription
Investor (1) ──→ (many) FavoriteStartup ─→ Startup
```

---

## Business Logic Highlights

### Subscription Model
- **Statuses**: Trialing, Active, PendingPayment, Expired, Cancelled
- **Access**: Only `Trialing` and `Active` grant feature access
- **Expiry Logic**: Automatically marks expired subscriptions on retrieval
- **One Per Investor**: Investor transitions between statuses in a single active row

### Investment Workflow
1. Investor submits `InvestmentRequest` with amount and terms
2. Startup reviews request on dashboard
3. Startup accepts/rejects via modal
4. PDF generated on acceptance (audit trail)
5. Both parties can download PDF agreement

### Soft Deletes (Posts/Media)
- Posts marked `IsDeleted = true`, `DeletedAt = timestamp`
- Feed queries filter out deleted posts
- Preserves audit trail for investor visibility

### File Upload Validation
- **Photos**: .jpg, .jpeg, .png, .webp; max 10MB
- **Videos**: .mp4, .webm, .mov; max 100MB
- **Pitch Decks**: .pdf, .pptx; configurable limit
- **Stored**: `wwwroot/startup-media/` with sanitized names

---

## API Endpoints (Key Routes)

### Account
- `GET/POST /Account/Login` – Authentication
- `GET/POST /Account/RegisterStartup` – Startup registration
- `GET/POST /Account/RegisterInvestor` – Investor registration
- `GET /Account/VerifyEmail?token={token}` – Email confirmation
- `GET/POST /Account/StartupProfile` – Startup profile management
- `GET/POST /Account/InvestorProfile` – Investor profile management

### Investor
- `GET /Investor/Dashboard` – Overview, statistics
- `GET /Investor/Discover` – Browse startups with filters
- `GET /Investor/Favorites` – Bookmarked startups
- `GET /Investor/MyRequests` – Sent investment requests
- `POST /Investor/SendInvestmentRequest` – Create request
- `GET/POST /Investor/EditProfile` – Profile updates

### Startup
- `GET /Startup/Dashboard` – Private dashboard with feed
- `GET/POST /Startup/EditProfile` – Company info
- `GET /Startup/Opportunities` – List funding rounds
- `GET/POST /Startup/CreateOpportunity` – Post new opportunity
- `GET /Startup/InvestmentRequests` – Incoming investor requests
- `POST /Startup/AcceptInvestment` – Accept investor proposal
- `POST /Startup/CreatePost` – Feed post creation (AJAX)
- `POST /Startup/UploadPhoto` – Media upload (AJAX)

### Messaging
- `GET /Message/` – Thread list
- `GET/POST /Message/New` – Start conversation
- `GET /Message/Thread/{id}` – Thread view

### Admin
- `GET /Admin/Dashboard` – Overview
- `GET /Admin/Investors` – User management
- `GET /Admin/ProfileVerification` – KYC verification queue
- `GET /Admin/Billing` – Subscription reports
- `GET /Admin/NfcCard` – Smart card order processing

### Public
- `GET /Explore/Startups` – Public startup directory
- `GET /Explore/Startup/{id}` – Startup profile with feed
- `GET /Card/Profile/{nfcToken}` – NFC card public link

---

## Configuration & Environment

### appsettings.json
```json
{
  "ConnectionStrings": {
	"DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=TechNovaDb;..."
  },
  "Logging": {
	"LogLevel": { "Default": "Information", "Microsoft.AspNetCore": "Warning" }
  },
  "Email": {
	"Smtp": { "Host": "smtp.gmail.com", "Port": 587, "UseSsl": true },
	"FromAddress": "mujaddedc@gmail.com"
  },
  "Stripe": { "SecretKey": "", "PublishableKey": "", "WebhookSecret": "" },
  "Billing": {
	"Currency": "USD",
	"PayeeName": "Tech Nova",
	"Instructions": "Transfer amount and quote reference. Access opens once confirmed."
  }
}
```

### Development Mode
- Admin seeding: Auto-creates admin user
- Plan seeding: Loads subscription tiers
- Email: Writes to disk instead of sending
- HTTPS: Redirected in production only

---

## Deployment Readiness

### Security
- ✅ Password hashing (ASP.NET Core Identity)
- ✅ CSRF protection (AntiforgeryToken middleware)
- ✅ Cookie security (HttpOnly, SameSite=Lax, SecurePolicy=SameAsRequest)
- ✅ HTTPS redirection (production)
- ✅ Claims-based authorization
- ✅ Email verification (prevents spam accounts)

### Performance
- ✅ Database indexing on foreign keys
- ✅ Pagination: 10–50 items per page
- ✅ Lazy loading controls (explicit includes)
- ✅ Soft deletes prevent cascading row deletions
- ✅ Static asset compression (CSS/JS minified)

### Scalability
- ✅ Stateless design (cookie authentication)
- ✅ Async/await throughout (no blocking I/O)
- ✅ DbContext injection per request
- ✅ Service layer abstraction
- ✅ Supports load balancing (no session affinity required)

---

## Important Files Reference

| File | Purpose |
|------|---------|
| `Program.cs` | Dependency injection, middleware pipeline, database seeding |
| `Data/ApplicationDbContext.cs` | EF Core configuration, 400+ lines of relationship definitions |
| `Services/SubscriptionService.cs` | Core access control and subscription logic (270 lines) |
| `Controllers/InvestorController.cs` | Investor workflows (discovery, favorites, requests) |
| `Controllers/StartupController.cs` | Startup workflows (profile, opportunities, social feed) |
| `Views/Startup/Dashboard.cshtml` | Facebook-style feed UI with create post form |
| `wwwroot/css/facebook-style.css` | 1000+ lines of modern CSS for social features |
| `wwwroot/js/startup-dashboard.js` | AJAX post/photo/video management |
| `Migrations/` | 10 migration files capturing schema versioning |

---

## Known Limitations & Enhancements

### Current
- Manual bank transfer payment (Stripe hooks ready but not implemented)
- Admin verification required for investor KYC
- Single file upload per post (queue support not yet added)
- No real-time notifications (polling model)
- Limited searching (basic contains filter)

### Roadmap
1. Stripe webhook implementation for instant billing
2. WebSocket messaging for real-time chat
3. Advanced search with Elasticsearch
4. Video transcoding service for uploads
5. Push notifications (email + app)
6. Portfolio analytics for investors
7. Automated compliance checks (AML/KYC)
8. Two-factor authentication
9. API clients (mobile app SDKs)
10. Batch operations (CSV import/export)

---

## Testing

The project includes:
- **Unit Tests**: Service layer validation (subscription, media, email)
- **Integration Tests**: Controller workflows (auth, CRUD, authorization)
- **Manual QA Checklist**: Facebook dashboard, messaging, billing flows

---

## Git Repository

- **Remote**: https://github.com/Mujadded-17/TechNova
- **Branch**: main
- **Local Path**: F:\New folder

---

## Project Metrics

- **Lines of Code**: ~15,000+
- **Database Tables**: 15+
- **Controllers**: 12
- **Views**: 50+
- **Models**: 22
- **Migrations**: 10
- **Services**: 7
- **Stylesheets**: 8

---

## Summary

TechNova is a **production-grade SaaS platform** connecting startups and investors with modern features including social media feeds, subscription billing, smart business cards, and comprehensive admin oversight. Built with ASP.NET Core best practices, it prioritizes security, scalability, and user experience while maintaining a clean, layered architecture.
