/* COFFEE HAVEN - ROLE-BASED ACCESS CONTROL MIGRATION
   Adds a Role column to the Users table to support Admin/Customer workflows.
   Run this script ONCE against your CoffeeHavenDB.
*/

-- 1. Add Role column if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Users') AND name = 'Role')
BEGIN
    ALTER TABLE Users ADD Role NVARCHAR(20) DEFAULT 'Customer';
END

-- 2. Backfill existing rows that have NULL role
EXEC sp_executesql N'UPDATE Users SET Role = ''Customer'' WHERE Role IS NULL';

-- 3. Promote the seed admin user (from SQLQuery2) to Admin
EXEC sp_executesql N'UPDATE Users SET Role = ''Admin'' WHERE Email = ''zain@coffeehaven.com''';

-- Verify
SELECT UserID, FullName, Email, Role FROM Users;
