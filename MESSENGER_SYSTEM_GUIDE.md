# TechNova Messenger System - Implementation Guide

## Overview
A complete bidirectional messaging system where **Startups** and **Investors** can communicate with each other, similar to Facebook Messenger or WhatsApp.

## Features Implemented

### 1. **Conversation Listing** (`/Message/Index`)
- Shows all active conversations for the current user
- Displays last message preview and timestamp
- Shows unread message count with badge
- Latest conversations appear first
- Different UI for Startups vs. Investors

### 2. **Message Threading** (`/Message/Thread/{investorId}`)
- Real-time conversation view
- Messages grouped by date
- Different styling for sent vs. received messages
- Auto-scroll to latest message
- Mark unread messages as read on thread open
- Time stamps for each message

### 3. **Send Message** (`POST /Message/Send`)
- Maximum 4000 characters per message
- Content trimming and validation
- Sender type tracking (Startup vs. Investor)
- Automatic thread creation if doesn't exist
- Requires subscription (for investors)

### 4. **Investor Search & Selection** (`/Startup/MessageInvestor`)
- Startups can search for investors by:
  - Name
  - Company Name
  - Bio/Description
  - Investment Industries
- View investor profiles before messaging
- Shows existing threads vs. new messages
- Returns up to 50 results

### 5. **Investor Conversation Initiation** (`/Message/New`)
- Investors can search for published startups
- Browse startup profiles
- Start conversations with founders
- Track existing conversations

## Database Schema

### Message Table
```sql
CREATE TABLE Messages (
	MessageID INT PRIMARY KEY IDENTITY,
	StartupID INT NOT NULL,
	InvestorID INT NOT NULL,
	SenderType VARCHAR(20) NOT NULL, -- "Startup" or "Investor"
	Content NVARCHAR(MAX) NOT NULL,
	Timestamp DATETIME2 DEFAULT GETUTCDATE(),
	IsRead BIT DEFAULT 0,
	FOREIGN KEY (StartupID) REFERENCES Startups(StartupID),
	FOREIGN KEY (InvestorID) REFERENCES Investors(InvestorID)
)
```

**Thread Identification**: A conversation is uniquely identified by the pair `(StartupID, InvestorID)` - no separate Conversation table needed.

## User Flows

### Flow 1: Investor Messages a Startup
1. Investor logs in → goes to Messages
2. Clicks "Start a conversation"
3. Searches for a startup
4. Clicks "Message founders"
5. Enters message and clicks Send
6. Message appears in conversation thread

### Flow 2: Startup Messages an Investor
1. Startup logs in → goes to Dashboard
2. Clicks "💬 Message Investor" button
3. Searches for investor
4. Clicks "Send Message"
5. Enters message and clicks Send
6. Message appears in conversation thread

### Flow 3: Continue Existing Conversation
1. User goes to Messages page
2. Clicks on existing conversation
3. Can view all previous messages
4. Replies appear with timestamp
5. Unread messages auto-mark as read

## Controllers & Actions

### MessageController
- `GET /Message/Index` - View all conversations
- `GET /Message/Thread/{id}` - View specific thread
- `POST /Message/Send` - Send a message
- `GET /Message/New` - Search startups to message (investors only)

### StartupController (New)
- `GET /Startup/MessageInvestor` - Search investors to message

## Views

### Message Views
- `Views/Message/Index.cshtml` - Conversation list
- `Views/Message/Thread.cshtml` - Conversation thread
- `Views/Message/New.cshtml` - Start conversation (investors only)

### Startup Views
- `Views/Startup/MessageInvestor.cshtml` - Find and message investors

## Security Features

1. **Role-based Access**: Only Startups and Investors can use messaging
2. **Thread Isolation**: Users can only access their own conversations
3. **Subscription Check**: Investors need active subscription to send messages
4. **Data Validation**: Messages max 4000 chars, content trimming
5. **CSRF Protection**: All POST actions require anti-forgery tokens
6. **Counterparty Validation**: Both parties must exist before allowing messages

## Key Implementation Details

### Unread Message Tracking
- Messages marked `IsRead=false` when sent
- Auto-marked `true` when recipient opens thread
- Unread count shows messages from other party only

### Message Grouping
- Messages grouped by date in thread view
- Date separator shown between different days
- Time displayed for each message

### Search Optimization
- Searches take up to 50 results
- Filters by name, company, industries
- Only shows verified investors
- Only shows published startups

### Responsive Design
- Mobile-friendly message layout
- Auto-scroll to latest message
- Optimized card-based design
- Touch-friendly buttons

## Database Relationships

```
Investor (1) ---< (Many) Messages (Many) >--- Startup (1)
```

**One-to-Many Relationships**:
- Investor → Messages: One investor can send many messages
- Startup → Messages: One startup can send many messages

## Message Queue & Notifications

Currently, the system stores messages in the database. For production enhancements:
- Add SignalR for real-time notifications
- Add message status indicators (sent, delivered, read)
- Add typing indicators
- Add push notifications

## Performance Considerations

1. **Indexing**: Create indexes on (StartupID, InvestorID) for fast lookups
2. **Pagination**: Implement for long conversations
3. **Caching**: Cache investor/startup names during thread display
4. **Timestamps**: Use UTC for consistency

## Testing Scenarios

1. **Create thread**: Startup finds investor → sends first message
2. **Reply flow**: Investor replies → startup sees notification
3. **Unread tracking**: Verify unread count increments/decrements
4. **Search**: Test investor search with partial names
5. **Permissions**: Verify users can't access other conversations
6. **Subscription**: Verify investors need subscription to message

## Future Enhancements

1. **Real-time Updates**: Add SignalR for live message delivery
2. **File Sharing**: Allow image/document sharing
3. **Message Reactions**: Add emoji reactions to messages
4. **Typing Indicators**: Show "User is typing..."
5. **Conversation Archiving**: Archive old threads
6. **Message Search**: Search within conversations
7. **Message Edit/Delete**: Allow editing/deleting sent messages
8. **Call Integration**: Add voice/video call buttons in threads
9. **Read Receipts**: Show when message was read
10. **Scheduled Messages**: Schedule messages to send later

## Troubleshooting

### Messages not saving
- Verify both StartupID and InvestorID exist
- Check message length (max 4000 chars)
- Verify investor has active subscription

### Threads not showing up
- Clear browser cache
- Verify correct role (Startup/Investor)
- Check thread ID in URL

### Search not working
- Use spaces in search terms
- Verify startups are published
- Verify investors are email verified

## Support & Documentation

For deployment and support:
1. Run database migrations for Message table
2. Ensure Startup and Investor tables are populated
3. Configure subscription filtering for investor messaging
4. Set up SMTP for optional email notifications
