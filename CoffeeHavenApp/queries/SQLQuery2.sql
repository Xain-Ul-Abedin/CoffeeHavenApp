/* COFFEE HAVEN - ENTERPRISE DATABASE SCHEMA 
   This script ensures all tables exist and are updated to support 
   Product Management, Orders, Inventory, and User Loyalty.
*/

-- 1. CATEGORIES TABLE (Base for Menu items)
IF OBJECT_ID('Categories', 'U') IS NULL
BEGIN
    CREATE TABLE Categories (
        CategoryID INT PRIMARY KEY IDENTITY(1,1),
        CategoryName NVARCHAR(100) NOT NULL,
        Description NVARCHAR(MAX)
    );
    -- Seed initial categories if empty
    INSERT INTO Categories (CategoryName) VALUES ('Hot Coffee'), ('Cold Coffee'), ('Bakery');
END

-- 2. MENU ITEMS TABLE (Enhanced with Soft Deletes and Inventory)
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('MenuItems') AND name = 'IsActive')
BEGIN
    ALTER TABLE MenuItems ADD IsActive BIT DEFAULT 1;
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('MenuItems') AND name = 'StockQuantity')
BEGIN
    ALTER TABLE MenuItems ADD StockQuantity INT DEFAULT 100;
END

-- Ensure columns are usable immediately in this batch
EXEC sp_executesql N'UPDATE MenuItems SET IsActive = 1 WHERE IsActive IS NULL';
EXEC sp_executesql N'UPDATE MenuItems SET StockQuantity = 100 WHERE StockQuantity IS NULL';

-- 3. USERS TABLE (Supports UserDAL and Loyalty Logic)
IF OBJECT_ID('Users', 'U') IS NULL
BEGIN
    CREATE TABLE Users (
        UserID INT PRIMARY KEY IDENTITY(1,1),
        FullName NVARCHAR(200) NOT NULL,
        Email NVARCHAR(200) UNIQUE,
        LoyaltyPoints INT DEFAULT 0,
        CreatedAt DATETIME DEFAULT GETDATE()
    );
END
ELSE
BEGIN
    -- If the table exists, check if the LoyaltyPoints column is missing and add it
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Users') AND name = 'LoyaltyPoints')
    BEGIN
        ALTER TABLE Users ADD LoyaltyPoints INT DEFAULT 0;
    END
END

-- Seed an admin/test user
-- FIX: We check if PasswordHash exists to avoid "Cannot insert NULL" errors
IF NOT EXISTS (SELECT * FROM Users WHERE Email = 'zain@coffeehaven.com')
BEGIN
    IF COL_LENGTH('Users', 'PasswordHash') IS NOT NULL
    BEGIN
        -- If your table has a mandatory PasswordHash, we provide a default one
        EXEC sp_executesql N'INSERT INTO Users (FullName, Email, LoyaltyPoints, PasswordHash) VALUES (''Zain-Ul-Abedin'', ''zain@coffeehaven.com'', 50, ''default_hash_123'')';
    END
    ELSE
    BEGIN
        -- Standard insert if no password column exists
        EXEC sp_executesql N'INSERT INTO Users (FullName, Email, LoyaltyPoints) VALUES (''Zain-Ul-Abedin'', ''zain@coffeehaven.com'', 50)';
    END
END

-- 4. ORDERS TABLE (Supports OrderDAL)
IF OBJECT_ID('Orders', 'U') IS NULL
BEGIN
    CREATE TABLE Orders (
        OrderID INT PRIMARY KEY IDENTITY(1,1),
        UserID INT NOT NULL FOREIGN KEY REFERENCES Users(UserID),
        OrderDate DATETIME DEFAULT GETDATE(),
        TotalAmount DECIMAL(10,2) DEFAULT 0.00,
        Status NVARCHAR(50) DEFAULT 'Completed'
    );
END

-- 5. ORDER ITEMS TABLE (Transactional details)
IF OBJECT_ID('OrderItems', 'U') IS NULL
BEGIN
    CREATE TABLE OrderItems (
        OrderItemID INT PRIMARY KEY IDENTITY(1,1),
        OrderID INT NOT NULL FOREIGN KEY REFERENCES Orders(OrderID),
        ItemID INT NOT NULL FOREIGN KEY REFERENCES MenuItems(ItemID),
        Quantity INT NOT NULL,
        PriceAtTime DECIMAL(10,2) NOT NULL -- Snapshots price for historical accuracy
    );
END