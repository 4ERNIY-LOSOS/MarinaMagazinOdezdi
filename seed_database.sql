-- Clear existing data from tables in the correct order to respect foreign key constraints
-- Use DELETE instead of TRUNCATE because TRUNCATE cannot be used on tables referenced by a FOREIGN KEY
DELETE FROM OrderItems;
DELETE FROM Orders;
DELETE FROM CartItems;
DELETE FROM Favorites;
DELETE FROM Products;
DELETE FROM Categories;
DELETE FROM Users;
GO

-- Insert a default Admin user
-- Password is 'admin'
INSERT INTO Users (Email, PasswordHash, FirstName, LastName, Role) VALUES
('admin@shop.com', '$2a$11$1dxrOWMLg.K5OH7ktbxoeeYvYY92bbZ7nfSvGwK5GxcSLOp7/FqpW', 'Admin', 'User', 'Admin');
GO

-- Reset identity columns so the IDs start from 1 again
DBCC CHECKIDENT ('OrderItems', RESEED, 0);
DBCC CHECKIDENT ('Orders', RESEED, 0);
DBCC CHECKIDENT ('CartItems', RESEED, 0);
DBCC CHECKIDENT ('Favorites', RESEED, 0);
DBCC CHECKIDENT ('Products', RESEED, 0);
DBCC CHECKIDENT ('Categories', RESEED, 0);
DBCC CHECKIDENT ('Users', RESEED, 0);
GO

-- Insert Categories
INSERT INTO Categories (CategoryName) VALUES
(N'Футболки'),       -- T-shirts
(N'Джинсы'),        -- Jeans
(N'Платья'),        -- Dresses
(N'Обувь'),         -- Shoes
(N'Аксессуары');    -- Accessories
GO

-- Insert Products
-- T-shirts (Category 1)
INSERT INTO Products (Name, Description, Price, CategoryId, StockQuantity) VALUES
(N'Классическая белая футболка', N'Хлопковая футболка унисекс, идеально подходит для повседневной носки.', 1200.00, 1, 150),
(N'Черная футболка с принтом', N'Стильная черная футболка с графическим принтом на груди.', 1500.00, 1, 100),
(N'Футболка-поло', N'Элегантная футболка-поло синего цвета.', 2200.00, 1, 80);

-- Jeans (Category 2)
INSERT INTO Products (Name, Description, Price, CategoryId, StockQuantity) VALUES
(N'Синие джинсы скинни', N'Узкие джинсы из эластичного денима.', 4500.00, 2, 120),
(N'Прямые черные джинсы', N'Классические прямые джинсы черного цвета.', 4800.00, 2, 90),
(N'Рваные джинсы бойфренда', N'Свободные джинсы с декоративными потертостями.', 5200.00, 2, 70);

-- Dresses (Category 3)
INSERT INTO Products (Name, Description, Price, CategoryId, StockQuantity) VALUES
(N'Летнее платье в цветочек', N'Легкое платье из вискозы с цветочным принтом.', 6000.00, 3, 60),
(N'Маленькое черное платье', N'Элегантное коктейльное платье, которое должно быть в каждом гардеробе.', 7500.00, 3, 40);

-- Shoes (Category 4)
INSERT INTO Products (Name, Description, Price, CategoryId, StockQuantity) VALUES
(N'Белые кеды', N'Классические кожаные кеды на каждый день.', 8000.00, 4, 100),
(N'Черные туфли-лодочки', N'Элегантные туфли на высоком каблуке.', 9500.00, 4, 50);

-- Accessories (Category 5)
INSERT INTO Products (Name, Description, Price, CategoryId, StockQuantity) VALUES
(N'Кожаный ремень', N'Классический ремень из натуральной кожи.', 3000.00, 5, 80),
(N'Солнцезащитные очки "Авиатор"', N'Стильные очки в металлической оправе.', 4200.00, 5, 60);
GO

PRINT 'Database has been cleaned and seeded with test data.';
GO
-- Added comment to force git to recognize the file as modified.