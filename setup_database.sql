-- Create the database
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'MarinaMagazinOdezdi')
BEGIN
    CREATE DATABASE MarinaMagazinOdezdi;
END
GO

-- Use the newly created database
USE MarinaMagazinOdezdi;
GO

-- 1. Users Table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Users' and xtype='U')
BEGIN
    CREATE TABLE Users (
        UserId INT PRIMARY KEY IDENTITY(1,1),
        Email NVARCHAR(255) NOT NULL UNIQUE,
        PasswordHash NVARCHAR(MAX) NOT NULL,
        FirstName NVARCHAR(255),
        LastName NVARCHAR(255),
        Role NVARCHAR(50) NOT NULL DEFAULT 'Customer' CHECK (Role IN ('Customer', 'Admin'))
    );
END
GO

-- 2. Categories Table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Categories' and xtype='U')
BEGIN
    CREATE TABLE Categories (
        CategoryId INT PRIMARY KEY IDENTITY(1,1),
        CategoryName NVARCHAR(255) NOT NULL
    );
END
GO

-- 3. Products Table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Products' and xtype='U')
BEGIN
    CREATE TABLE Products (
        ProductId INT PRIMARY KEY IDENTITY(1,1),
        Name NVARCHAR(255) NOT NULL,
        Description NVARCHAR(MAX),
        Price DECIMAL(10, 2) NOT NULL,
        CategoryId INT FOREIGN KEY REFERENCES Categories(CategoryId),
        StockQuantity INT NOT NULL
    );
END
GO

-- 4. Favorites Table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Favorites' and xtype='U')
BEGIN
    CREATE TABLE Favorites (
        FavoriteId INT PRIMARY KEY IDENTITY(1,1),
        UserId INT FOREIGN KEY REFERENCES Users(UserId),
        ProductId INT FOREIGN KEY REFERENCES Products(ProductId),
        CONSTRAINT UQ_User_Product_Favorite UNIQUE (UserId, ProductId)
    );
END
GO

-- 5. CartItems Table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='CartItems' and xtype='U')
BEGIN
    CREATE TABLE CartItems (
        CartItemId INT PRIMARY KEY IDENTITY(1,1),
        UserId INT FOREIGN KEY REFERENCES Users(UserId),
        ProductId INT FOREIGN KEY REFERENCES Products(ProductId),
        Quantity INT NOT NULL,
        CONSTRAINT UQ_User_Product_Cart UNIQUE (UserId, ProductId)
    );
END
GO

-- 6. Orders Table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Orders' and xtype='U')
BEGIN
    CREATE TABLE Orders (
        OrderId INT PRIMARY KEY IDENTITY(1,1),
        UserId INT FOREIGN KEY REFERENCES Users(UserId),
        OrderDate DATETIME NOT NULL DEFAULT GETDATE(),
        TotalAmount DECIMAL(10, 2) NOT NULL
    );
END
GO

-- 7. OrderItems Table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='OrderItems' and xtype='U')
BEGIN
    CREATE TABLE OrderItems (
        OrderItemId INT PRIMARY KEY IDENTITY(1,1),
        OrderId INT FOREIGN KEY REFERENCES Orders(OrderId),
        ProductId INT FOREIGN KEY REFERENCES Products(ProductId),
        Quantity INT NOT NULL,
        UnitPrice DECIMAL(10, 2) NOT NULL
    );
END
GO

PRINT 'Database and tables created successfully.';
GO
