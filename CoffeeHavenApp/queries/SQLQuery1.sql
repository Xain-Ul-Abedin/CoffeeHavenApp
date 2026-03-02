/* COFFEE HAVEN - DATABASE REFINEMENT SCRIPT 
   This script upgrades the existing schema to support Enterprise Logic.
*/

-- 1. Support for Soft Deletes and Inventory in Menu
-- We use separate blocks to ensure columns are created before they are accessed
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('MenuItems') AND name = 'IsActive')
BEGIN
    ALTER TABLE MenuItems ADD IsActive BIT DEFAULT 1;
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('MenuItems') AND name = 'StockQuantity')
BEGIN
    ALTER TABLE MenuItems ADD StockQuantity INT DEFAULT 100;
END

-- Use dynamic SQL for the updates to avoid "Invalid Column Name" compilation errors
-- This ensures the script doesn't fail if the columns were JUST created in the same batch
EXEC sp_executesql N'UPDATE MenuItems SET IsActive = 1 WHERE IsActive IS NULL';
EXEC sp_executesql N'UPDATE MenuItems SET StockQuantity = 100 WHERE StockQuantity IS NULL';

-- 2. Verify Core Tables for Order Flow
-- If these don't exist from your Lab 04, they are created now.

IF OBJECT_ID('Orders', 'U') IS NULL
BEGIN
    CREATE TABLE Orders (
        OrderID INT PRIMARY KEY IDENTITY(1,1),
        UserID INT NOT NULL,
        OrderDate DATETIME DEFAULT GETDATE(),
        TotalAmount DECIMAL(10,2),
        Status NVARCHAR(50) DEFAULT 'Pending'
    );
END

IF OBJECT_ID('OrderItems', 'U') IS NULL
BEGIN
    CREATE TABLE OrderItems (
        OrderItemID INT PRIMARY KEY IDENTITY(1,1),
        OrderID INT FOREIGN KEY REFERENCES Orders(OrderID),
        ItemID INT FOREIGN KEY REFERENCES MenuItems(ItemID),
        Quantity INT,
        PriceAtTime DECIMAL(10,2) -- Crucial for price history
    );
END