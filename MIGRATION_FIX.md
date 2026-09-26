# Database Migration Fix

## Issue
The error "Invalid object name 'PostLikes'" occurs because the database is missing the `PostLikes` table that was defined in the migration `20260926175751_AddPostInteractions.cs`.

## Root Cause
The migration file exists in the codebase but has not been applied to the database yet. The code attempts to query a table that doesn't exist.

## Solution
Apply all pending Entity Framework Core migrations to synchronize the database schema with the code.

### Option 1: Using Package Manager Console (Visual Studio)
```powershell
Update-Database
```

### Option 2: Using .NET CLI
```bash
dotnet ef database update
```

### Option 3: Using Entity Framework Core Tools
```bash
dotnet ef database update --context ApplicationDbContext
```

## Expected Result
After running the migration update command:
- The `PostLikes` table will be created with columns: PostLikeID, PostID, InvestorID, CreatedAt
- The `PostComments` table will also be created
- Foreign key relationships will be established
- The API endpoint `GetPostLikes` will work correctly

## Verification
To verify the migration was applied successfully:
1. Check that the tables exist in SQL Server Management Studio or your database tool
2. Try calling the `/api/startup/post-likes/{postId}` endpoint again
3. Confirm it returns the expected JSON response without errors

## Additional Notes
- Ensure your connection string in `appsettings.json` is correct
- Make sure you have appropriate permissions to modify the database
- Consider backing up your database before applying migrations to production
