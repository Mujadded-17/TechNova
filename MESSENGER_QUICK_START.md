# TechNova Messenger System - Quick Start Guide

## 🚀 Getting Started

### For Startups

#### Step 1: Access Message Investor
```
Dashboard → "💬 Message Investor" button
```

#### Step 2: Search for Investors
- Type investor name, company, or investment focus
- Browse profiles of verified investors
- View their investment range and areas of interest

#### Step 3: Send First Message
- Click "Send Message" button on investor profile
- Type your message (up to 4000 characters)
- Press Enter or click Send button
- Message appears immediately in the thread

#### Step 4: Check Conversations
```
Dashboard → Messages (or click message bell icon)
```
- View all active conversations
- See last message preview
- Unread message count appears as badge

---

### For Investors

#### Step 1: Access Messenger
```
Dashboard → Messages (in navigation)
```

#### Step 2: Start Conversation
- Click "Start a conversation" button
- Search for startup name or description
- Browse published startups

#### Step 3: Message Startup Founders
- Click "Message founders" on startup profile
- Type your message
- Press Enter or click Send
- Conversation appears in your thread

#### Step 4: Read Replies
- Open Messages page
- Click on startup conversation
- All previous messages visible
- Your unread messages auto-marked as read

---

## 💬 Messaging Features

### Message Types
| Feature | Description |
|---------|-------------|
| **Text Messages** | Up to 4000 characters per message |
| **Automatic Threading** | Messages grouped by date within conversation |
| **Timestamps** | Each message shows send time |
| **Read Status** | Messages marked read when recipient opens thread |
| **Unread Badges** | Shows count of unread messages per conversation |

### Message Notifications
- Unread count appears on Messages page
- Unread badge shows on conversation cards
- Last message preview in conversation list
- Sorted by most recent first

---

## 🔒 Security & Privacy

### Data Protection
- Only authorized users can send/receive messages
- Startups cannot read investor-to-investor messages
- Investors cannot read startup-to-startup messages
- Subscription required for investors to message

### Message Validation
- Messages max 4000 characters
- Automatic trimming of whitespace
- Both parties must exist in system
- CSRF token protection on all forms

### Privacy Controls
- No message history deletion (auditable)
- Thread-level isolation
- Role-based access control
- Counterparty validation

---

## 📊 Conversation Metrics

### Conversation List
```
📝 Investor Name              Last Message preview        12 hrs ago    [5]
```

**Fields Explained:**
- Avatar initials of counterparty
- Counterparty name (clickable)
- Last message preview text
- Timestamp of last message
- Unread count badge (if > 0)

---

## ⌨️ Keyboard Shortcuts

### In Message Thread
| Shortcut | Action |
|----------|--------|
| `Enter` | Send message |
| `Shift + Enter` | New line in message |
| `Escape` | Cancel (if available) |

### Active Development
- Auto-scroll to latest message
- Focus on message input by default
- Clear input after sending

---

## 🔄 Common Workflows

### Workflow 1: Startup Reaches Out First
```
Startup Dashboard
	↓
	Click "💬 Message Investor"
	↓
	Search: "Tech Investment" or investor name
	↓
	Click "Send Message" on profile
	↓
	Type message: "Hi! I'm interested in discussing..."
	↓
	Press Enter to send
	↓
	Conversation appears in Messages
```

### Workflow 2: Investor Initiates Contact
```
Investor Dashboard
	↓
	Click "Messages" in navigation
	↓
	Click "Start a conversation"
	↓
	Search: "AI Startup" or company name
	↓
	Click "Message founders"
	↓
	Type opening message
	↓
	Click Send button
	↓
	Startup receives notification
```

### Workflow 3: Ongoing Conversation
```
User opens Messages page
	↓
	Clicks on existing conversation
	↓
	Sees all previous messages
	↓
	Types reply in message input
	↓
	Presses Enter to send
	↓
	Conversation thread updates
	↓
	Other party receives message
	↓
	Message marked read when they open
```

---

## 🐛 Troubleshooting

### Problem: "No investors found"
**Solution:**
- Ensure investor is email verified
- Try broader search terms
- Check if investor is verified in system
- Refresh page and try again

### Problem: "Message won't send"
**Solution:**
- Check message length (max 4000 chars)
- Ensure you have an active subscription (investors)
- Verify both parties exist in system
- Check for CSRF token in form

### Problem: "Can't see old messages"
**Solution:**
- Open correct conversation thread
- Scroll up to see earlier messages
- Check that counterparty exists
- Clear browser cache

### Problem: "Unread count not updating"
**Solution:**
- Open the conversation thread
- Refresh page after viewing
- Check that messages are from other party
- Verify IsRead flag status in database

---

## 📈 Performance Tips

### For Smooth Messaging
1. **Keep conversations concise** - Shorter messages load faster
2. **Search specificity** - More specific searches return fewer results
3. **Archive old threads** - (Coming soon) Clean up old conversations
4. **Use web version** - Better performance than mobile for long threads

### Message Limits
- **Per message:** 4000 characters
- **Search results:** 50 max per page
- **Message retrieval:** All messages in thread
- **Conversation list:** All active conversations

---

## 🎯 Best Practices

### For Startup Founders
✅ **DO**
- Be professional and courteous
- Include specific details about your startup
- Ask clear questions
- Respond within 24 hours
- Keep messages concise

❌ **DON'T**
- Send spam or mass messages
- Include personal sensitive information
- Send messages in ALL CAPS
- Send repeated messages without response

### For Investors
✅ **DO**
- Provide detailed feedback
- Ask clarifying questions
- Share your investment criteria
- Set expectations for follow-up
- Be respectful of founders' time

❌ **DON'T**
- Make unsolicited promises
- Share other founders' information
- Provide advice outside your expertise
- Schedule calls without confirmation

---

## 🔔 Notification Settings

### Current Features (MVP)
- ✅ Unread message badges
- ✅ Last message preview in list
- ✅ Auto-scroll in thread
- ⏳ Email notifications (coming soon)
- ⏳ Push notifications (coming soon)
- ⏳ Typing indicators (coming soon)

---

## 📱 Mobile Experience

### Responsive Design
- ✅ Mobile-optimized thread view
- ✅ Touch-friendly message input
- ✅ Swipe-friendly conversation list
- ✅ Bottom message input (sticky)
- ✅ Full keyboard support

### Best Mobile Practices
1. Open Messages from dashboard
2. Tap conversation to open thread
3. Type in message box at bottom
4. Tap Send or press Enter
5. Scroll up for conversation history

---

## 🚀 Advanced Features (Roadmap)

### Phase 2 Features
- [ ] Real-time message delivery (SignalR)
- [ ] Typing indicators ("User is typing...")
- [ ] Message reactions (emoji)
- [ ] Read receipts
- [ ] File attachments
- [ ] Image gallery in threads

### Phase 3 Features
- [ ] Voice/video calls
- [ ] Message search
- [ ] Conversation archiving
- [ ] Message pinning
- [ ] Conversation templates
- [ ] Scheduled messages

### Phase 4 Features
- [ ] AI-suggested responses
- [ ] Meeting scheduling integration
- [ ] Document sharing
- [ ] Screen sharing
- [ ] Message encryption

---

## 📞 Support

### Getting Help
- **System Errors**: Check the troubleshooting guide above
- **Account Issues**: Contact admin support
- **Feature Requests**: Submit through dashboard feedback
- **Bug Reports**: Email support@technova.com

### Documentation
- Full system guide: `MESSENGER_SYSTEM_GUIDE.md`
- Database schema: Check ApplicationDbContext
- Controller code: `/Controllers/MessageController.cs`
- View templates: `/Views/Message/` and `/Views/Startup/`

---

## ✨ Pro Tips

### Maximize Your Messaging
1. **First impression matters** - Write thoughtful first messages
2. **Include relevant details** - Help the recipient understand context
3. **Ask for specific things** - Be clear about what you want
4. **Follow-up strategically** - Wait for response before re-messaging
5. **Keep it professional** - This is business communication

### Build Relationships
1. Start with genuine interest
2. Research before reaching out
3. Personalize your messages
4. Provide value in conversations
5. Schedule follow-up calls
6. Maintain the conversation over time

---

Last Updated: 2025
TechNova Messenger System v1.0
