# PostLikes Table Missing - Migration Fix

## Problem
The application is trying to access the `PostLikes` table, but it doesn't exist in the database. The migration file exists (`20260926175751_AddPostInteractions.cs`) but hasn't been applied.

## Solution

### Option 1: Using Package Manager Console (Visual Studio)
1. Open **Package Manager Console** in Visual Studio
2. Run the following command:
   ```
   Update-Database
   ```

### Option 2: Using .NET CLI (Command Line)
1. Open a terminal/command prompt in the project directory
2. Run the following command:
   ```
   dotnet ef database update
   ```

### Option 3: Using Visual Studio CLI Package Manager Console (Most Reliable)
1. In Visual Studio, go to **Tools** → **NuGet Package Manager** → **Package Manager Console**
2. Ensure the **Default project** is set to `TechNova` (or your main project)
3. Run:
   ```
   Update-Database
   ```

## What This Does
The `Update-Database` command will:
- Apply all pending migrations to your SQL Server database
- Create the `PostLikes` table
- Create the `PostComments` table (also in the same migration)
- Create all necessary foreign key constraints and indexes

## Verification
After running the command:
1. The error should be resolved
2. The `GetPostLikes` method in `StartupController` will be able to query the database
3. You can verify in SQL Server that the `PostLikes` table now exists

## Additional Notes
- This migration is part of the `AddPostInteractions` migration
- Both `PostLikes` and `PostComments` tables will be created
- All foreign key relationships are properly configured
