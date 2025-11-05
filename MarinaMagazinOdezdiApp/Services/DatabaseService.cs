using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using MarinaMagazinOdezdiApp.Models;
using MarinaMagazinOdezdiApp.Utils;

namespace MarinaMagazinOdezdiApp.Services
{
    public class DatabaseService
    {
        private readonly string _connectionString;

        public DatabaseService(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<bool> TestConnectionAsync()
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    return true;
                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show($"Database connection failed: {ex.Message}", "Connection Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        public async Task<User> RegisterUserAsync(string email, string password, string firstName, string lastName, string role)
        {
            var cleanEmail = email?.Trim();
            var cleanFirstName = firstName?.Trim();
            var cleanLastName = lastName?.Trim();

            if (string.IsNullOrEmpty(cleanEmail) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(cleanFirstName) || string.IsNullOrEmpty(cleanLastName))
            {
                throw new ArgumentException("Email, password, first name, and last name cannot be empty.");
            }

            string hashedPassword = PasswordHasher.HashPassword(password);

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                string sql = "INSERT INTO Users (Email, PasswordHash, FirstName, LastName, Role) VALUES (@Email, @PasswordHash, @FirstName, @LastName, @Role); SELECT SCOPE_IDENTITY();";
                
                using (var command = new SqlCommand(sql, connection))
                {
                    command.Parameters.Add("@Email", System.Data.SqlDbType.NVarChar, 255).Value = cleanEmail;
                    command.Parameters.Add("@PasswordHash", System.Data.SqlDbType.NVarChar, -1).Value = hashedPassword;
                    command.Parameters.Add("@FirstName", System.Data.SqlDbType.NVarChar, 255).Value = cleanFirstName;
                    command.Parameters.Add("@LastName", System.Data.SqlDbType.NVarChar, 255).Value = cleanLastName;
                    command.Parameters.Add("@Role", System.Data.SqlDbType.NVarChar, 50).Value = role;

                    try
                    {
                        var result = await command.ExecuteScalarAsync();
                        var userId = Convert.ToInt32(result);
                        
                        return new User
                        {
                            UserId = userId,
                            Email = cleanEmail,
                            FirstName = cleanFirstName,
                            LastName = cleanLastName,
                            Role = role
                        };
                    }
                    catch (SqlException ex)
                    {
                        if (ex.Number == 2627 || ex.Number == 2601)
                        {
                            throw new InvalidOperationException($"Пользователь с таким Email '{cleanEmail}' уже существует.");
                        }
                        else
                        {
                            throw;
                        }
                    }
                }
            }
        }

        public async Task<User> LoginUserAsync(string email, string password)
        {
            email = email?.Trim();
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                string sql = "SELECT UserId, Email, PasswordHash, FirstName, LastName, Role FROM Users WHERE LOWER(Email) = LOWER(@Email);";
                
                using (var command = new SqlCommand(sql, connection))
                {
                    command.Parameters.Add("@Email", System.Data.SqlDbType.NVarChar, 255).Value = email;
                    
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            string storedHash = reader["PasswordHash"].ToString();
                            if (PasswordHasher.VerifyPassword(password, storedHash))
                            {
                                return new User
                                {
                                    UserId = reader.GetInt32(reader.GetOrdinal("UserId")),
                                    Email = reader["Email"].ToString(),
                                    FirstName = reader["FirstName"].ToString(),
                                    LastName = reader["LastName"].ToString(),
                                    Role = reader["Role"].ToString()
                                };
                            }
                        }
                    }
                }
            }
            return null; // Login failed
        }

        public async Task<List<User>> GetUsersAsync()
        {
            var users = new List<User>();
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                string sql = "SELECT UserId, Email, FirstName, LastName, Role FROM Users;";
                using (var command = new SqlCommand(sql, connection))
                {
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            users.Add(new User
                            {
                                UserId = reader.GetInt32(reader.GetOrdinal("UserId")),
                                Email = reader.GetString(reader.GetOrdinal("Email")),
                                FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                                LastName = reader.GetString(reader.GetOrdinal("LastName")),
                                Role = reader.GetString(reader.GetOrdinal("Role"))
                            });
                        }
                    }
                }
            }
            return users;
        }

        public async Task UpdateUserAsync(User user)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                string sql = "UPDATE Users SET Email = @Email, FirstName = @FirstName, LastName = @LastName, Role = @Role WHERE UserId = @UserId;";
                using (var command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@UserId", user.UserId);
                    command.Parameters.AddWithValue("@Email", user.Email);
                    command.Parameters.AddWithValue("@FirstName", user.FirstName);
                    command.Parameters.AddWithValue("@LastName", user.LastName);
                    command.Parameters.AddWithValue("@Role", user.Role);
                    await command.ExecuteNonQueryAsync();
                }
            }
        }

        public async Task DeleteUserAsync(int userId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                // Before deleting a user, we need to handle related data in other tables
                // to avoid foreign key constraint violations.

                // 1. Delete from CartItems
                string deleteCartSql = "DELETE FROM CartItems WHERE UserId = @UserId;";
                using (var command = new SqlCommand(deleteCartSql, connection))
                {
                    command.Parameters.AddWithValue("@UserId", userId);
                    await command.ExecuteNonQueryAsync();
                }

                // 2. Delete from Favorites
                string deleteFavoritesSql = "DELETE FROM Favorites WHERE UserId = @UserId;";
                using (var command = new SqlCommand(deleteFavoritesSql, connection))
                {
                    command.Parameters.AddWithValue("@UserId", userId);
                    await command.ExecuteNonQueryAsync();
                }

                // 3. Handle Orders. A common approach is not to delete orders, but to anonymize the user
                // or prevent user deletion if they have orders. For simplicity, we'll prevent deletion.
                string checkOrdersSql = "SELECT COUNT(*) FROM Orders WHERE UserId = @UserId;";
                using (var command = new SqlCommand(checkOrdersSql, connection))
                {
                    command.Parameters.AddWithValue("@UserId", userId);
                    var orderCount = (int)await command.ExecuteScalarAsync();
                    if (orderCount > 0)
                    {
                        throw new InvalidOperationException("Нельзя удалить пользователя, так как у него есть заказы. Рассмотрите возможность деактивации пользователя.");
                    }
                }

                // 4. Finally, delete the user
                string deleteUserSql = "DELETE FROM Users WHERE UserId = @UserId;";
                using (var command = new SqlCommand(deleteUserSql, connection))
                {
                    command.Parameters.AddWithValue("@UserId", userId);
                    await command.ExecuteNonQueryAsync();
                }
            }
        }

        public async Task<List<Order>> GetOrdersForUserAsync(int userId)
        {
            var orders = new List<Order>();
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                string sql = "SELECT OrderId, OrderDate, TotalAmount, ShippingCity, ShippingStreet, ShippingHouseNumber FROM Orders WHERE UserId = @UserId ORDER BY OrderDate DESC;";
                using (var command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@UserId", userId);
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            orders.Add(new Order
                            {
                                OrderId = reader.GetInt32(reader.GetOrdinal("OrderId")),
                                UserId = userId,
                                OrderDate = reader.GetDateTime(reader.GetOrdinal("OrderDate")),
                                TotalAmount = reader.GetDecimal(reader.GetOrdinal("TotalAmount")),
                                ShippingCity = reader["ShippingCity"] as string,
                                ShippingStreet = reader["ShippingStreet"] as string,
                                ShippingHouseNumber = reader["ShippingHouseNumber"] as string,
                                OrderItems = new List<OrderItem>() // Initialize empty list
                            });
                        }
                    }
                }

                // For each order, get the order items
                foreach (var order in orders)
                {
                    string itemsSql = @"SELECT oi.OrderItemId, oi.Quantity, oi.UnitPrice, 
                                             p.ProductId, p.Name, p.Description
                                      FROM OrderItems oi
                                      JOIN Products p ON oi.ProductId = p.ProductId
                                      WHERE oi.OrderId = @OrderId;";
                    using (var itemCommand = new SqlCommand(itemsSql, connection))
                    {
                        itemCommand.Parameters.AddWithValue("@OrderId", order.OrderId);
                        using (var itemReader = await itemCommand.ExecuteReaderAsync())
                        {
                            while (await itemReader.ReadAsync())
                            {
                                order.OrderItems.Add(new OrderItem
                                {
                                    OrderItemId = itemReader.GetInt32(itemReader.GetOrdinal("OrderItemId")),
                                    OrderId = order.OrderId,
                                    ProductId = itemReader.GetInt32(itemReader.GetOrdinal("ProductId")),
                                    Quantity = itemReader.GetInt32(itemReader.GetOrdinal("Quantity")),
                                    UnitPrice = itemReader.GetDecimal(itemReader.GetOrdinal("UnitPrice")),
                                    Product = new Product
                                    {
                                        ProductId = itemReader.GetInt32(itemReader.GetOrdinal("ProductId")),
                                        Name = itemReader.GetString(itemReader.GetOrdinal("Name")),
                                        Description = itemReader.GetString(itemReader.GetOrdinal("Description"))
                                    }
                                });
                            }
                        }
                    }
                }
            }
            return orders;
        }


        public async Task<List<Product>> GetProductsAsync()
        {
            var products = new List<Product>();
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                string sql = "SELECT ProductId, Name, Description, Price, CategoryId, StockQuantity FROM Products;";
                
                using (var command = new SqlCommand(sql, connection))
                {
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            products.Add(new Product
                            {
                                ProductId = reader.GetInt32(reader.GetOrdinal("ProductId")),
                                Name = reader["Name"].ToString(),
                                Description = reader["Description"].ToString(),
                                Price = reader.GetDecimal(reader.GetOrdinal("Price")),
                                CategoryId = reader.GetInt32(reader.GetOrdinal("CategoryId")),
                                StockQuantity = reader.GetInt32(reader.GetOrdinal("StockQuantity"))
                            });
                        }
                    }
                }
            }
            return products;
        }

        public async Task<List<Category>> GetCategoriesAsync()
        {
            var categories = new List<Category>();
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                string sql = "SELECT CategoryId, CategoryName FROM Categories ORDER BY CategoryName;";
                
                using (var command = new SqlCommand(sql, connection))
                {
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            categories.Add(new Category
                            {
                                CategoryId = reader.GetInt32(reader.GetOrdinal("CategoryId")),
                                CategoryName = reader.GetString(reader.GetOrdinal("CategoryName"))
                            });
                        }
                    }
                }
            }
            return categories;
        }
        public async Task AddToCartAsync(int userId, int productId, int quantity)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                // Check if item already in cart
                string checkCartSql = "SELECT Quantity FROM CartItems WHERE UserId = @UserId AND ProductId = @ProductId;";
                using (var checkCartCommand = new SqlCommand(checkCartSql, connection))
                {
                    checkCartCommand.Parameters.AddWithValue("@UserId", userId);
                    checkCartCommand.Parameters.AddWithValue("@ProductId", productId);
                    var existingQuantity = (int?)await checkCartCommand.ExecuteScalarAsync();

                    if (existingQuantity != null)
                    {
                        // Item exists, update quantity
                        string updateSql = "UPDATE CartItems SET Quantity = Quantity + @Quantity WHERE UserId = @UserId AND ProductId = @ProductId;";
                        using (var updateCommand = new SqlCommand(updateSql, connection))
                        {
                            updateCommand.Parameters.AddWithValue("@Quantity", quantity);
                            updateCommand.Parameters.AddWithValue("@UserId", userId);
                            updateCommand.Parameters.AddWithValue("@ProductId", productId);
                            await updateCommand.ExecuteNonQueryAsync();
                        }
                    }
                    else
                    {
                        // Item does not exist, insert new
                        string insertSql = "INSERT INTO CartItems (UserId, ProductId, Quantity) VALUES (@UserId, @ProductId, @Quantity);";
                        using (var insertCommand = new SqlCommand(insertSql, connection))
                        {
                            insertCommand.Parameters.AddWithValue("@UserId", userId);
                            insertCommand.Parameters.AddWithValue("@ProductId", productId);
                            insertCommand.Parameters.AddWithValue("@Quantity", quantity);
                            await insertCommand.ExecuteNonQueryAsync();
                        }
                    }
                }
            }
        }
        public async Task<List<CartItem>> GetCartItemsAsync(int userId)
        {
            var cartItems = new List<CartItem>();
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                string sql = @"SELECT 
                                ci.CartItemId, 
                                ci.UserId, 
                                ci.ProductId, 
                                ci.Quantity, 
                                p.Name, 
                                p.Price 
                            FROM CartItems ci
                            JOIN Products p ON ci.ProductId = p.ProductId
                            WHERE ci.UserId = @UserId;";

                using (var command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@UserId", userId);
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            cartItems.Add(new CartItem
                            {
                                CartItemId = reader.GetInt32(reader.GetOrdinal("CartItemId")),
                                UserId = reader.GetInt32(reader.GetOrdinal("UserId")),
                                ProductId = reader.GetInt32(reader.GetOrdinal("ProductId")),
                                Quantity = reader.GetInt32(reader.GetOrdinal("Quantity")),
                                Product = new Product
                                {
                                    ProductId = reader.GetInt32(reader.GetOrdinal("ProductId")),
                                    Name = reader.GetString(reader.GetOrdinal("Name")),
                                    Price = reader.GetDecimal(reader.GetOrdinal("Price"))
                                }
                            });
                        }
                    }
                }
            }
            return cartItems;
        }

        public async Task UpdateCartItemQuantityAsync(int cartItemId, int newQuantity)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                string sql = "UPDATE CartItems SET Quantity = @NewQuantity WHERE CartItemId = @CartItemId;";
                using (var command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@NewQuantity", newQuantity);
                    command.Parameters.AddWithValue("@CartItemId", cartItemId);
                    await command.ExecuteNonQueryAsync();
                }
            }
        }

        public async Task RemoveFromCartAsync(int cartItemId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                string sql = "DELETE FROM CartItems WHERE CartItemId = @CartItemId;";
                using (var command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@CartItemId", cartItemId);
                    await command.ExecuteNonQueryAsync();
                }
            }
        }
        public async Task PlaceOrderAsync(int userId, List<CartItem> cartItems, string city, string street, string houseNumber)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // 1. Calculate total amount
                        decimal totalAmount = cartItems.Sum(item => item.Quantity * item.Product.Price);

                        // 2. Create a new Order
                        string insertOrderSql = "INSERT INTO Orders (UserId, OrderDate, TotalAmount, ShippingCity, ShippingStreet, ShippingHouseNumber) VALUES (@UserId, GETDATE(), @TotalAmount, @ShippingCity, @ShippingStreet, @ShippingHouseNumber); SELECT SCOPE_IDENTITY();";
                        int orderId;
                        using (var orderCommand = new SqlCommand(insertOrderSql, connection, transaction))
                        {
                            orderCommand.Parameters.AddWithValue("@UserId", userId);
                            orderCommand.Parameters.AddWithValue("@TotalAmount", totalAmount);
                            orderCommand.Parameters.AddWithValue("@ShippingCity", city);
                            orderCommand.Parameters.AddWithValue("@ShippingStreet", street);
                            orderCommand.Parameters.AddWithValue("@ShippingHouseNumber", houseNumber);
                            orderId = Convert.ToInt32(await orderCommand.ExecuteScalarAsync());
                        }

                        // 3. Create OrderItems and update stock
                        foreach (var item in cartItems)
                        {
                            // Check stock one more time within the transaction
                            string checkStockSql = "SELECT StockQuantity FROM Products WHERE ProductId = @ProductId;";
                            using (var stockCommand = new SqlCommand(checkStockSql, connection, transaction))
                            {
                                stockCommand.Parameters.AddWithValue("@ProductId", item.ProductId);
                                var stockQuantity = (int)await stockCommand.ExecuteScalarAsync();
                                if (stockQuantity < item.Quantity)
                                {
                                    throw new InvalidOperationException($"Недостаточно товара на складе для '{item.Product.Name}'.");
                                }
                            }

                            // Insert into OrderItems
                            string insertOrderItemSql = "INSERT INTO OrderItems (OrderId, ProductId, Quantity, UnitPrice) VALUES (@OrderId, @ProductId, @Quantity, @UnitPrice);";
                            using (var orderItemCommand = new SqlCommand(insertOrderItemSql, connection, transaction))
                            {
                                orderItemCommand.Parameters.AddWithValue("@OrderId", orderId);
                                orderItemCommand.Parameters.AddWithValue("@ProductId", item.ProductId);
                                orderItemCommand.Parameters.AddWithValue("@Quantity", item.Quantity);
                                orderItemCommand.Parameters.AddWithValue("@UnitPrice", item.Product.Price);
                                await orderItemCommand.ExecuteNonQueryAsync();
                            }

                            // Update product stock
                            string updateStockSql = "UPDATE Products SET StockQuantity = StockQuantity - @Quantity WHERE ProductId = @ProductId;";
                            using (var updateStockCommand = new SqlCommand(updateStockSql, connection, transaction))
                            {
                                updateStockCommand.Parameters.AddWithValue("@Quantity", item.Quantity);
                                updateStockCommand.Parameters.AddWithValue("@ProductId", item.ProductId);
                                await updateStockCommand.ExecuteNonQueryAsync();
                            }
                        }

                        // 4. Clear the user's cart
                        string clearCartSql = "DELETE FROM CartItems WHERE UserId = @UserId;";
                        using (var clearCartCommand = new SqlCommand(clearCartSql, connection, transaction))
                        {
                            clearCartCommand.Parameters.AddWithValue("@UserId", userId);
                            await clearCartCommand.ExecuteNonQueryAsync();
                        }

                        // 5. If all successful, commit the transaction
                        transaction.Commit();
                    }
                    catch (Exception)
                    {
                        // If anything fails, roll back the entire transaction
                        transaction.Rollback();
                        throw; // Re-throw the exception to be handled by the ViewModel
                    }
                }
            }
        }
        public async Task AddProductAsync(Product product)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                string sql = "INSERT INTO Products (Name, Description, Price, CategoryId, StockQuantity) VALUES (@Name, @Description, @Price, @CategoryId, @StockQuantity);";
                using (var command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@Name", product.Name);
                    command.Parameters.AddWithValue("@Description", product.Description);
                    command.Parameters.AddWithValue("@Price", product.Price);
                    command.Parameters.AddWithValue("@CategoryId", product.CategoryId);
                    command.Parameters.AddWithValue("@StockQuantity", product.StockQuantity);
                    await command.ExecuteNonQueryAsync();
                }
            }
        }

        public async Task UpdateProductAsync(Product product)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                string sql = "UPDATE Products SET Name = @Name, Description = @Description, Price = @Price, CategoryId = @CategoryId, StockQuantity = @StockQuantity WHERE ProductId = @ProductId;";
                using (var command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@ProductId", product.ProductId);
                    command.Parameters.AddWithValue("@Name", product.Name);
                    command.Parameters.AddWithValue("@Description", product.Description);
                    command.Parameters.AddWithValue("@Price", product.Price);
                    command.Parameters.AddWithValue("@CategoryId", product.CategoryId);
                    command.Parameters.AddWithValue("@StockQuantity", product.StockQuantity);
                    await command.ExecuteNonQueryAsync();
                }
            }
        }

        public async Task DeleteProductAsync(int productId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                // First, delete related records in OrderItems, CartItems, Favorites to avoid foreign key violations
                string deleteFavoritesSql = "DELETE FROM Favorites WHERE ProductId = @ProductId;";
                using (var command = new SqlCommand(deleteFavoritesSql, connection))
                {
                    command.Parameters.AddWithValue("@ProductId", productId);
                    await command.ExecuteNonQueryAsync();
                }

                string deleteCartItemsSql = "DELETE FROM CartItems WHERE ProductId = @ProductId;";
                using (var command = new SqlCommand(deleteCartItemsSql, connection))
                {
                    command.Parameters.AddWithValue("@ProductId", productId);
                    await command.ExecuteNonQueryAsync();
                }

                // We should handle OrderItems carefully. Deleting them might not be the best business logic.
                // For now, we will prevent deletion if the product is in any order.
                string checkOrdersSql = "SELECT COUNT(*) FROM OrderItems WHERE ProductId = @ProductId;";
                using (var command = new SqlCommand(checkOrdersSql, connection))
                {
                    command.Parameters.AddWithValue("@ProductId", productId);
                    var orderCount = (int)await command.ExecuteScalarAsync();
                    if (orderCount > 0)
                    {
                        throw new InvalidOperationException("Нельзя удалить товар, так как он уже присутствует в заказах.");
                    }
                }

                string deleteProductSql = "DELETE FROM Products WHERE ProductId = @ProductId;";
                using (var command = new SqlCommand(deleteProductSql, connection))
                {
                    command.Parameters.AddWithValue("@ProductId", productId);
                    await command.ExecuteNonQueryAsync();
                }
            }
        }

        public async Task AddCategoryAsync(Category category)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                string sql = "INSERT INTO Categories (CategoryName) VALUES (@CategoryName);";
                using (var command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@CategoryName", category.CategoryName);
                    await command.ExecuteNonQueryAsync();
                }
            }
        }

        public async Task UpdateCategoryAsync(Category category)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                string sql = "UPDATE Categories SET CategoryName = @CategoryName WHERE CategoryId = @CategoryId;";
                using (var command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@CategoryId", category.CategoryId);
                    command.Parameters.AddWithValue("@CategoryName", category.CategoryName);
                    await command.ExecuteNonQueryAsync();
                }
            }
        }

        public async Task DeleteCategoryAsync(int categoryId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                // Check if any products are using this category
                string checkProductsSql = "SELECT COUNT(*) FROM Products WHERE CategoryId = @CategoryId;";
                using (var command = new SqlCommand(checkProductsSql, connection))
                {
                    command.Parameters.AddWithValue("@CategoryId", categoryId);
                    var productCount = (int)await command.ExecuteScalarAsync();
                    if (productCount > 0)
                    {
                        throw new InvalidOperationException("Нельзя удалить категорию, так как она используется товарами.");
                    }
                }

                string deleteCategorySql = "DELETE FROM Categories WHERE CategoryId = @CategoryId;";
                using (var command = new SqlCommand(deleteCategorySql, connection))
                {
                    command.Parameters.AddWithValue("@CategoryId", categoryId);
                    await command.ExecuteNonQueryAsync();
                }
            }
        }
    }
}
