# TechNova API Reference

Complete REST API documentation for all TechNova endpoints.

## Authentication

All endpoints require authentication via cookie unless marked as **public**.

**Headers**:
- `RequestVerificationToken` - CSRF token (for POST/PUT/DELETE)
- `Content-Type: application/json`

---

## Account Endpoints

### POST /Account/Login
Login with email and password.

**Request Body**:
```json
{
  "email": "user@example.com",
  "password": "password123",
  "userType": "Startup" // or "Investor"
}
```

**Response**: Redirect to dashboard on success

---

### POST /Account/RegisterStartup
Register a new startup account.

**Request Body**:
```json
{
  "email": "founder@startup.com",
  "password": "secure123",
  "companyName": "TechCorp",
  "description": "AI-powered solutions"
}
```

**Response**: Redirect to verification pending page

---

### POST /Account/RegisterInvestor
Register a new investor account.

**Request Body**:
```json
{
  "email": "investor@vc.com",
  "password": "secure123",
  "name": "Jane Investor",
  "investmentRange": "100000"
}
```

**Response**: Redirect to verification pending page

---

### GET /Account/VerifyEmail
Confirm email via token (sent in link).

**Query Parameters**:
- `token` - 30-minute GUID token

**Response**: Redirect to login on success

---

### GET/POST /Account/StartupProfile
Get or update startup profile.

**GET Response**:
```json
{
  "startupID": 1,
  "companyName": "TechCorp",
  "email": "founder@startup.com",
  "description": "AI-powered solutions",
  "website": "https://techcorp.com",
  "fundingRequired": 500000,
  "amountRaised": 150000,
  "businessStage": "Series A",
  "industry": "Software"
}
```

**POST Request**:
```json
{
  "companyName": "Updated Name",
  "description": "Updated description",
  "website": "https://new-website.com"
}
```

---

## Investor Endpoints

### GET /Investor/Dashboard
Get investor dashboard with statistics.

**Response**:
```json
{
  "investorID": 5,
  "name": "Jane Investor",
  "totalInvestments": 750000,
  "activeRequests": 3,
  "favoriteStartups": 12,
  "subscription": {
	"status": "Active",
	"currentPeriodEnd": "2025-01-15"
  }
}
```

---

### GET /Investor/Discover
Browse startups (paginated).

**Query Parameters**:
- `page` (default: 1)
- `pageSize` (default: 10)
- `industry` - Filter by industry
- `stage` - Filter by business stage (Seed, Series A, etc.)
- `location` - Filter by location

**Response**:
```json
{
  "startups": [
	{
	  "startupID": 1,
	  "companyName": "TechCorp",
	  "description": "AI platform",
	  "fundingRequired": 500000,
	  "amountRaised": 150000,
	  "industry": "Software",
	  "location": "San Francisco"
	}
  ],
  "totalCount": 42,
  "page": 1,
  "pageSize": 10
}
```

---

### GET /Investor/Favorites
Get investor's favorite startups.

**Response**: Same as Discover

---

### POST /Investor/SendInvestmentRequest
Submit investment proposal to startup.

**Request Body**:
```json
{
  "startupID": 1,
  "investmentAmount": 100000
}
```

**Response**:
```json
{
  "requestID": 5,
  "startupID": 1,
  "investmentAmount": 100000,
  "status": "Pending",
  "requestDate": "2025-01-10T12:00:00Z"
}
```

---

### GET /Investor/MyRequests
Get all investment requests sent by investor.

**Query Parameters**:
- `status` - Filter by status (Pending, Accepted, Rejected)

**Response**:
```json
{
  "requests": [
	{
	  "requestID": 5,
	  "startup": { "companyName": "TechCorp" },
	  "investmentAmount": 100000,
	  "status": "Pending",
	  "requestDate": "2025-01-10"
	}
  ]
}
```

---

## Startup Endpoints

### GET /Startup/Dashboard
Get startup private dashboard.

**Response**:
```json
{
  "startupID": 1,
  "companyName": "TechCorp",
  "fundingRequired": 500000,
  "amountRaised": 150000,
  "posts": [
	{
	  "postID": 1,
	  "content": "Exciting update!",
	  "createdAt": "2025-01-10T12:00:00Z",
	  "photos": [ { "photoID": 1, "imagePath": "/startup-media/photo_1.jpg" } ]
	}
  ]
}
```

---

### POST /Startup/CreatePost
Create a new feed post.

**Request Body**:
```json
{
  "content": "We just launched our MVP! 🚀",
  "imageFile": null  // Optional file upload
}
```

**Response**:
```json
{
  "postID": 1,
  "content": "We just launched our MVP! 🚀",
  "createdAt": "2025-01-10T12:00:00Z"
}
```

---

### POST /Startup/UploadPhoto
Upload photo to post.

**Content-Type**: `multipart/form-data`

**Form Fields**:
- `postID` - Post ID
- `file` - Image file (.jpg, .png, .webp, max 10MB)
- `caption` - Optional caption

**Response**:
```json
{
  "photoID": 1,
  "imagePath": "/startup-media/photo_1.jpg",
  "caption": "Product screenshot"
}
```

---

### POST /Startup/CreateOpportunity
Post new funding opportunity.

**Request Body**:
```json
{
  "title": "Series A Funding Round",
  "description": "Raising $2M Series A",
  "fundingAmount": 2000000,
  "equityOffered": 15,
  "deadline": "2025-03-31T23:59:59Z"
}
```

**Response**:
```json
{
  "opportunityID": 1,
  "title": "Series A Funding Round",
  "status": "Active",
  "createdAt": "2025-01-10"
}
```

---

### GET /Startup/InvestmentRequests
Get investment requests from investors.

**Query Parameters**:
- `status` - Filter by status (Pending, Accepted, Rejected)

**Response**:
```json
{
  "requests": [
	{
	  "requestID": 5,
	  "investor": { "name": "Jane Investor" },
	  "investmentAmount": 100000,
	  "status": "Pending",
	  "requestDate": "2025-01-10"
	}
  ]
}
```

---

### POST /Startup/AcceptInvestment
Accept investor proposal.

**Request Body**:
```json
{
  "requestID": 5,
  "accept": true  // false to reject
}
```

**Response**: Investment agreement PDF path

---

## Messaging Endpoints

### GET /Message/
Get all message threads.

**Query Parameters**:
- `page` (default: 1)

**Response**:
```json
{
  "threads": [
	{
	  "counterpartyName": "Jane Investor",
	  "lastMessage": "When can we meet?",
	  "lastMessageTime": "2025-01-10T12:00:00Z",
	  "unreadCount": 2
	}
  ]
}
```

---

### POST /Message/New
Send new message.

**Request Body**:
```json
{
  "counterpartyID": 5,
  "counterpartyType": "Investor",  // or "Startup"
  "content": "Hello! Interested in your platform."
}
```

**Response**:
```json
{
  "messageID": 10,
  "content": "Hello! Interested in your platform.",
  "timestamp": "2025-01-10T12:00:00Z"
}
```

---

### GET /Message/Thread/:id
Get conversation thread.

**Response**:
```json
{
  "messages": [
	{
	  "messageID": 10,
	  "senderType": "Startup",
	  "content": "Hello!",
	  "timestamp": "2025-01-10T12:00:00Z",
	  "isRead": true
	}
  ]
}
```

---

## Billing Endpoints

### GET /Billing/Plans
Get subscription plans (public).

**Response**:
```json
{
  "plans": [
	{
	  "planID": 1,
	  "name": "Basic",
	  "pricePerMonth": 0,
	  "features": [ "Browse startups", "Limited messages" ]
	},
	{
	  "planID": 2,
	  "name": "Premium",
	  "pricePerMonth": 29,
	  "features": [ "Unlimited browsing", "Full messaging" ]
	}
  ]
}
```

---

### POST /Billing/Checkout
Initiate purchase flow.

**Request Body**:
```json
{
  "planID": 2,
  "paymentMethod": "stripe"  // or "bank_transfer"
}
```

**Response**:
```json
{
  "sessionID": "sess_12345",
  "redirectUrl": "https://checkout.stripe.com/..."
}
```

---

### GET /Billing/Manage
Get investor subscription details.

**Response**:
```json
{
  "currentSubscription": {
	"subscriptionID": 1,
	"plan": { "name": "Premium" },
	"status": "Active",
	"currentPeriodEnd": "2025-02-10",
	"nextPaymentAmount": 29
  }
}
```

---

## Admin Endpoints

### GET /Admin/Dashboard
Admin overview.

**Response**:
```json
{
  "totalUsers": 150,
  "totalStartups": 35,
  "totalInvestors": 115,
  "monthlyRevenue": 2900,
  "pendingVerifications": 5
}
```

---

### GET /Admin/ProfileVerification
Get pending profile verifications.

**Response**:
```json
{
  "pendingInvestors": [
	{
	  "investorID": 5,
	  "name": "Jane Investor",
	  "submittedAt": "2025-01-08",
	  "status": "PendingReview"
	}
  ]
}
```

---

### POST /Admin/ApproveInvestor/:id
Approve investor KYC.

**Response**: Confirmation

---

## NFC Card Endpoints

### POST /NfcCard/RequestCard
Order NFC smart business card.

**Request Body**:
```json
{
  "quantity": 100,
  "designPreset": "simple"  // or "premium"
}
```

**Response**: Stripe checkout session

---

### GET /Card/Profile/:nfcToken
Public profile via NFC link (no auth required).

**Response**: HTML profile page

---

## Error Responses

All errors return appropriate HTTP status with message:

```json
{
  "error": "Unauthorized",
  "message": "You must be logged in to access this resource."
}
```

**Status Codes**:
- `200` - Success
- `201` - Created
- `400` - Bad Request (validation error)
- `401` - Unauthorized (auth required)
- `403` - Forbidden (access denied)
- `404` - Not Found
- `409` - Conflict (duplicate)
- `422` - Unprocessable Entity (business logic error)
- `500` - Server Error

---

## Rate Limiting

Currently: None (implement if needed at scale)

## Pagination

All list endpoints support:
- `page` - Page number (1-indexed)
- `pageSize` - Items per page (default 10, max 100)

Response includes:
```json
{
  "data": [...],
  "totalCount": 150,
  "page": 1,
  "pageSize": 10
}
```
