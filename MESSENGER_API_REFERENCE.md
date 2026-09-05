# TechNova Messenger System - API Endpoints & Integration Guide

## REST API Endpoints

### Message Management

#### 1. Get Conversation List
```http
GET /Message/Index
Authorization: Bearer {token}
Roles: Startup, Investor
```

**Response:**
```json
[
  {
	"startupId": 5,
	"investorId": 12,
	"lastMessage": "Thanks for reaching out! Let's discuss...",
	"lastAt": "2025-01-15T10:30:00Z",
	"counterpartyId": 12,
	"counterpartyName": "John Smith",
	"unread": 3
  }
]
```

**Use Cases:**
- Display all active conversations
- Show unread count per conversation
- Sort by most recent
- Count total unread messages

---

#### 2. Get Conversation Thread
```http
GET /Message/Thread/{investorId}
Authorization: Bearer {token}
Roles: Startup, Investor
```

**Query Parameters:**
- `id` (required): Counterparty ID (Investor ID if Startup, Startup ID if Investor)

**Response:**
```json
[
  {
	"messageID": 1,
	"startupID": 5,
	"investorID": 12,
	"senderType": "Startup",
	"content": "Hi! We're looking for Series A funding...",
	"timestamp": "2025-01-15T10:00:00Z",
	"isRead": true
  },
  {
	"messageID": 2,
	"startupID": 5,
	"investorID": 12,
	"senderType": "Investor",
	"content": "Sounds interesting! Tell me more about your traction.",
	"timestamp": "2025-01-15T10:30:00Z",
	"isRead": false
  }
]
```

**Side Effects:**
- Auto-marks unread messages as read
- Groups messages by StartupID + InvestorID
- Orders chronologically

---

#### 3. Send Message
```http
POST /Message/Send
Content-Type: application/x-www-form-urlencoded
Authorization: Bearer {token}
Roles: Startup, Investor
```

**Request Body:**
```
id={counterpartyId}&content={messageText}&__RequestVerificationToken={token}
```

**Parameters:**
- `id` (required): Counterparty ID (integer)
- `content` (required): Message text (max 4000 chars)
- `__RequestVerificationToken` (required): CSRF token

**Response:**
- **Success**: Redirect to Thread view with new message
- **Error**: Redirect to Thread with error message in TempData

**Examples:**

Startup messaging Investor:
```
POST /Message/Send
id=42&content=Hi! We'd love to discuss investment&__RequestVerificationToken=...
```

Investor messaging Startup:
```
POST /Message/Send
id=5&content=Your product looks great. Can we schedule a call?&__RequestVerificationToken=...
```

**Message Creation Logic:**
```csharp
// Creates new message automatically if thread doesn't exist
var message = new Message 
{
	StartupID = startupId,
	InvestorID = investorId,
	SenderType = isStartup ? "Startup" : "Investor",
	Content = content.Trim(),
	Timestamp = DateTime.UtcNow,
	IsRead = false
};

// Validates:
// - Both StartupID and InvestorID exist
// - Content length <= 4000
// - Investor has active subscription
// - CSRF token valid
```

---

### Startup Investor Search

#### 4. Search Investors (Startup Action)
```http
GET /Startup/MessageInvestor?q={searchTerm}
Authorization: Bearer {token}
Roles: Startup
```

**Query Parameters:**
- `q` (optional): Search query string

**Response:**
```json
[
  {
	"investorID": 42,
	"name": "Jane Doe",
	"companyName": "Tech Ventures LLC",
	"bio": "Early-stage tech investor focusing on AI...",
	"investedIndustries": "AI, FinTech, SaaS",
	"investmentRange": 50000,
	"emailVerified": true
  }
]
```

**Search Behavior:**
- Maximum 50 results returned
- Filters by:
  - Investor name (case-insensitive)
  - Company name
  - Bio/description
  - Investment industries
- Only shows verified investors
- Ignores leading/trailing whitespace

---

#### 5. Search Startups (Investor Action)
```http
GET /Message/New?q={searchTerm}
Authorization: Bearer {token}
Roles: Investor
```

**Query Parameters:**
- `q` (optional): Search query string

**Response:**
```json
[
  {
	"startupID": 5,
	"companyName": "TechNova AI",
	"industry": "Artificial Intelligence",
	"businessStage": "Series A",
	"description": "AI-powered analytics platform...",
	"location": "San Francisco, CA",
	"verificationStatus": "Verified"
  }
]
```

**Search Behavior:**
- Returns published startups only
- Maximum 50 results
- Filters by company name and description
- Shows verification status
- Excludes deleted startups

---

## Data Models

### Message Entity
```csharp
public class Message
{
	public int MessageID { get; set; }              // PK
	public int StartupID { get; set; }              // FK
	public int InvestorID { get; set; }             // FK
	public string SenderType { get; set; }          // "Startup" or "Investor"
	public string Content { get; set; }             // Max 4000 chars
	public DateTime Timestamp { get; set; }         // UTC time
	public bool IsRead { get; set; }                // False = unread

	// Navigation properties
	public Startup Startup { get; set; }
	public Investor Investor { get; set; }
}
```

### ConversationSummary (View Model)
```csharp
public class ConversationSummary
{
	public int StartupID { get; set; }
	public int InvestorID { get; set; }
	public string LastMessage { get; set; }
	public DateTime LastAt { get; set; }
	public int Unread { get; set; }

	// Added properties
	public int CounterpartyId { get; set; }
	public string CounterpartyName { get; set; }
}
```

---

## Database Queries

### Find existing conversation
```sql
SELECT * FROM Messages 
WHERE StartupID = @StartupID AND InvestorID = @InvestorID
ORDER BY Timestamp DESC
LIMIT 1;
```

### Get unread count for user
```sql
-- For Startup
SELECT COUNT(*) FROM Messages 
WHERE StartupID = @StartupID 
  AND InvestorID = @InvestorID 
  AND IsRead = 0 
  AND SenderType = 'Investor';

-- For Investor
SELECT COUNT(*) FROM Messages 
WHERE InvestorID = @InvestorID 
  AND StartupID = @StartupID 
  AND IsRead = 0 
  AND SenderType = 'Startup';
```

### Get conversation list with latest message
```sql
SELECT DISTINCT 
	m.StartupID,
	m.InvestorID,
	MAX(m.MessageID) as LatestMessageID
FROM Messages m
WHERE (m.StartupID = @UserID AND @UserType = 'Startup')
   OR (m.InvestorID = @UserID AND @UserType = 'Investor')
GROUP BY m.StartupID, m.InvestorID
ORDER BY MAX(m.Timestamp) DESC;
```

### Mark messages as read
```sql
UPDATE Messages 
SET IsRead = 1 
WHERE StartupID = @StartupID 
  AND InvestorID = @InvestorID 
  AND SenderType = @OtherPartyType 
  AND IsRead = 0;
```

---

## HTTP Status Codes

| Code | Scenario | Response |
|------|----------|----------|
| 200 | GET successful | JSON array of results |
| 302 | POST successful | Redirect to Thread view |
| 400 | Invalid parameters | Bad request |
| 401 | Not authenticated | Redirect to login |
| 403 | Insufficient subscription | Forbid |
| 404 | User/thread not found | Not found page |
| 500 | Server error | Error page |

---

## Authentication & Authorization

### Required Claims
```csharp
// Startup
User.IsInRole("Startup")
User.FindFirstValue(ClaimTypes.NameIdentifier) == startupId

// Investor  
User.IsInRole("Investor")
User.FindFirstValue(ClaimTypes.NameIdentifier) == investorId
```

### Authorization Attributes
```csharp
[Authorize(Roles = "Startup,Investor")]      // MessageController
[Authorize(Roles = "Startup")]               // StartupController.MessageInvestor
[RequiresSubscription]                       // Message.Send (investors)
```

---

## Error Handling

### Validation Errors

**Empty message:**
```
Error: "Write something before sending."
HTTP 302 → Redirect to Thread
```

**Message too long:**
```
Automatic: Truncate to 4000 chars
```

**Missing counterparty:**
```
Error: "404 Not Found"
HTTP 404 → Not found page
```

**No subscription:**
```
Error: "403 Forbidden"
HTTP 403 → Forbid response
```

**CSRF token missing:**
```
Error: "400 Bad Request"
Validation error in form
```

---

## Rate Limiting (Recommended Implementation)

```csharp
// Suggested limits
- 100 messages per user per day
- 20 searches per user per hour
- 5 new conversations per hour
- No more than 1 message per 2 seconds

// Implementation with Redis/DistributedCache
var key = $"msg_{userId}_{action}";
var count = await cache.GetAsync(key);
if (count >= limit) return Forbid("Rate limit exceeded");
```

---

## Webhook Events (Future)

### Message Sent
```json
{
  "event": "message.sent",
  "timestamp": "2025-01-15T10:30:00Z",
  "data": {
	"messageId": 123,
	"startupId": 5,
	"investorId": 12,
	"senderType": "Startup",
	"content": "...",
	"messageLength": 45
  }
}
```

### Conversation Started
```json
{
  "event": "conversation.started",
  "timestamp": "2025-01-15T10:00:00Z",
  "data": {
	"startupId": 5,
	"investorId": 12,
	"initiator": "Startup",
	"firstMessageId": 1
  }
}
```

### Message Read
```json
{
  "event": "message.read",
  "timestamp": "2025-01-15T10:35:00Z",
  "data": {
	"messageId": 123,
	"reader": "Investor",
	"readAt": "2025-01-15T10:35:00Z"
  }
}
```

---

## Performance Considerations

### Query Optimization
```csharp
// ✅ GOOD: Efficient group query
var threads = messages
	.AsNoTracking()
	.GroupBy(m => new { m.StartupID, m.InvestorID })
	.Select(g => new { ... })
	.ToList();

// ❌ AVOID: N+1 query problem
foreach (var msg in messages)
{
	var startup = db.Startups.Find(msg.StartupID);
	var investor = db.Investors.Find(msg.InvestorID);
}
```

### Indexing Strategy
```sql
-- Add indexes for common queries
CREATE INDEX IX_Messages_StartupID ON Messages(StartupID);
CREATE INDEX IX_Messages_InvestorID ON Messages(InvestorID);
CREATE INDEX IX_Messages_Composite ON Messages(StartupID, InvestorID, Timestamp DESC);
CREATE INDEX IX_Messages_ReadStatus ON Messages(StartupID, InvestorID, IsRead);
```

### Caching
```csharp
// Cache investor/startup names during thread display
var cache = new Dictionary<int, string>();
foreach (var id in investorIds)
{
	cache[id] = await db.Investors.Select(i => new { i.InvestorID, i.Name })
		.FirstOrDefaultAsync();
}
```

---

## Testing Endpoints

### cURL Examples

**Get conversations:**
```bash
curl -X GET \
  http://localhost:5000/Message/Index \
  -H "Authorization: Bearer {token}"
```

**Get thread:**
```bash
curl -X GET \
  http://localhost:5000/Message/Thread/42 \
  -H "Authorization: Bearer {token}"
```

**Send message:**
```bash
curl -X POST \
  http://localhost:5000/Message/Send \
  -H "Content-Type: application/x-www-form-urlencoded" \
  -H "Authorization: Bearer {token}" \
  -d "id=42&content=Hello!&__RequestVerificationToken={token}"
```

**Search investors:**
```bash
curl -X GET \
  "http://localhost:5000/Startup/MessageInvestor?q=tech" \
  -H "Authorization: Bearer {token}"
```

---

## Integration Checklist

- [ ] Database migrations applied
- [ ] Message table exists with proper indexes
- [ ] User authentication configured
- [ ] Subscription validation working
- [ ] CSRF protection enabled
- [ ] Views created and accessible
- [ ] Controller actions routing correctly
- [ ] Search functionality working
- [ ] Unread message logic verified
- [ ] Read/unread status updating
- [ ] Error handling implemented
- [ ] Tested with real users

---

Last Updated: January 2025
TechNova API v1.0
