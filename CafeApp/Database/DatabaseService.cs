using Npgsql;
using System;
using System.Data;
using System.IO;
using System.Collections.Generic;
using CafeApp.Models;

namespace CafeApp.Database
{
    public class DatabaseService : IDisposable
    {
        private NpgsqlConnection? _connection;
        private string _connectionString = "Host=localhost;Username=postgres;Password=6645;Database=Cafe;Include Error Detail=true";

        public NpgsqlConnection GetConnection()
        {
            _connection = new NpgsqlConnection(_connectionString);
            _connection.Open();
            return _connection;
        }

        public string? AuthenticateUser(string username, string password)
        {
            try
            {
                using (var conn = GetConnection())
                {
                    string query = @"SELECT role FROM users WHERE username = @username AND password_ = @password AND employment_status = true";
                    using (var command = new NpgsqlCommand(query, conn))
                    {
                        command.Parameters.AddWithValue("@username", username);
                        command.Parameters.AddWithValue("@password", password);

                        using (var reader = command.ExecuteReader()) 
                        {
                            if (reader.Read())
                            {
                                string role = reader.GetString(0);     
                                return role;
                            }
                            else
                            {
                                return null;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public bool RegisterUser(string username, string password, string name, 
                 string surname, string patronymic, string role, string photoLink, string contractScanLink)
        {
            try
            {
                using (var conn = GetConnection())
                {
                    string query = @"INSERT INTO users
                           (username, password_, role, name, surname, patronymic, employment_status, photo_link, contract_scan_link) 
                           VALUES (@username, @password, @role, @name, @surname, @patronymic, TRUE, @photoLink, @contractScanLink)";
        
                    using (var command = new NpgsqlCommand(query, conn))
                    {
                        command.Parameters.AddWithValue("@username", username);
                        command.Parameters.AddWithValue("@password", password);
                        command.Parameters.AddWithValue("@role", role.ToLower());
                        command.Parameters.AddWithValue("@name", name);
                        command.Parameters.AddWithValue("@surname", surname);
                        command.Parameters.AddWithValue("@patronymic", patronymic);
                        command.Parameters.AddWithValue("@photoLink", photoLink);
                        command.Parameters.AddWithValue("@contractScanLink", contractScanLink);

                        int rowsAffected = command.ExecuteNonQuery();
  
                        return rowsAffected == 1;
                    }
                }
            }
            catch (Exception ex)
            {
                string filePath = @"A:\Инженерно-техническая поддержка сопровождения ИС\debug.log";
                string errorMessage = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - DB ERROR: {ex.Message}\n";
                File.AppendAllText(filePath, errorMessage);
                return false;
            }
        }
        public List<ListItem> GetEmployeesList()
        {
            var employees = new List<ListItem>();
            try
            {
                using (var conn = GetConnection())
                {
                    string query = @"SELECT 
                                user_id,
                                surname, 
                                name, 
                                patronymic,
                                role
                            FROM users 
                            WHERE employment_status = true 
                            ORDER BY surname, name";
            
                    using (var command = new NpgsqlCommand(query, conn))
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int userId = reader.GetInt32(0);
                            string surname = reader.GetString(1);
                            string name = reader.GetString(2);
                            string patronymic = reader.IsDBNull(3) ? "" : reader.GetString(3);
                            string role = reader.GetString(4);
                    
                            string fullName = $"{surname} {name} {patronymic}".Trim();
                            string displayText = $"{fullName} - {role}";
                    
                            employees.Add(new ListItem
                            {
                                Id = userId,
                                DisplayText = displayText
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string filePath = @"A:\Инженерно-техническая поддержка сопровождения ИС\debug.log";
                string errorMessage = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - DB ERROR (GetEmployeesList): {ex.Message}\n";
                File.AppendAllText(filePath, errorMessage);
            }
            return employees;
        }
        public List<ListItem> GetOrdersList()
        {
            var orders = new List<ListItem>();
            try
            {
                using (var conn = GetConnection())
                {
                    string query = @"SELECT 
                                order_id,
                                table_id,
                                created_at,
                                status
                            FROM ""order""
                            ORDER BY created_at DESC";
            
                    using (var command = new NpgsqlCommand(query, conn))
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int orderId = reader.GetInt32(0);
                            int tableId = reader.GetInt32(1);
                            DateTime createdAt = reader.GetDateTime(2);
                            string status = reader.GetString(3);
                    
                            string displayText = $"Стол №{tableId} - {createdAt:yyyy-MM-dd HH:mm} - {status}";
                    
                            orders.Add(new ListItem
                            {
                                Id = orderId,
                                DisplayText = displayText
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string filePath = @"A:\Инженерно-техническая поддержка сопровождения ИС\debug.log";
                string errorMessage = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - DB ERROR (GetOrdersList): {ex.Message}\n";
                File.AppendAllText(filePath, errorMessage);
            }
            return orders;
        }
        public string GetOrderStatus(int orderId)
        {
            try
            {
                using (var conn = GetConnection())
                {
                    string query = "SELECT status FROM \"order\" WHERE order_id = @orderId";
            
                    using (var command = new NpgsqlCommand(query, conn))
                    {
                        command.Parameters.AddWithValue("@orderId", orderId);
                
                        var result = command.ExecuteScalar();
                        return result?.ToString() ?? "";
                    }
                }
            }
            catch (Exception ex)
            {
                string filePath = @"A:\Инженерно-техническая поддержка сопровождения ИС\debug.log";
                string errorMessage = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - DB ERROR (GetOrderStatus): {ex.Message}\n";
                File.AppendAllText(filePath, errorMessage);
                return "";
            }
        }
        public List<ListItem> GetCurrentShiftOrdersList()
        {
            var orders = new List<ListItem>();
            try
            {
                using (var conn = GetConnection())
                {
                    // Получаем текущую дату
                    DateTime currentDate = DateTime.Today;
                    
                    // Находим текущую смену (предполагаем, что смена сегодняшняя)
                    string query = @"SELECT 
                                o.order_id,
                                o.table_id,
                                o.created_at,
                                o.status,
                                s.shift_id
                            FROM ""order"" o
                            JOIN shift s ON o.shift_id = s.shift_id
                            WHERE s.shift_date = @currentDate
                            ORDER BY o.created_at DESC";
                    
                    using (var command = new NpgsqlCommand(query, conn))
                    {
                        command.Parameters.AddWithValue("@currentDate", currentDate);
                        
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                int orderId = reader.GetInt32(0);
                                int tableId = reader.GetInt32(1);
                                DateTime createdAt = reader.GetDateTime(2);
                                string status = reader.GetString(3);
                                int shiftId = reader.GetInt32(4);
                        
                                string displayText = $"Стол №{tableId} - {createdAt:HH:mm} - {status}";
                        
                                orders.Add(new ListItem
                                {
                                    Id = orderId,
                                    DisplayText = displayText
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string filePath = @"A:\Инженерно-техническая поддержка сопровождения ИС\debug.log";
                string errorMessage = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - DB ERROR (GetCurrentShiftOrdersList): {ex.Message}\n";
                File.AppendAllText(filePath, errorMessage);
            }
            return orders;
        }
        public List<ListItem> GetShiftsList()
        {
            var shifts = new List<ListItem>();
            try
            {
                using (var conn = GetConnection())
                {
                    string query = @"SELECT 
                                shift_id, 
                                shift_date, 
                                start_time, 
                                end_time
                            FROM shift 
                            ORDER BY shift_date DESC, start_time DESC";
            
                    using (var command = new NpgsqlCommand(query, conn))
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int shiftId = reader.GetInt32(0);
                            DateTime shiftDate = reader.GetDateTime(1);
                            TimeSpan startTime = reader.GetTimeSpan(2);
                            TimeSpan endTime = reader.GetTimeSpan(3);
                    
                            string displayText = $"Смена {shiftId} - {shiftDate:yyyy-MM-dd} ({startTime:hh\\:mm} - {endTime:hh\\:mm})";
                    
                            shifts.Add(new ListItem
                            {
                                Id = shiftId,
                                DisplayText = displayText
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string filePath = @"A:\Инженерно-техническая поддержка сопровождения ИС\debug.log";
                string errorMessage = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - DB ERROR (GetShiftsList): {ex.Message}\n";
                File.AppendAllText(filePath, errorMessage);
            }
            return shifts;
        }
        public int GetWaiterIdByName(string fullName)
        {
            try
            {
                using (var conn = GetConnection())
                {
                    string[] nameParts = fullName.Split(' ');
                    if (nameParts.Length < 2)
                        return -1;

                    string surname = nameParts[0];
                    string name = nameParts[1];
                    
                    string query = @"SELECT user_id FROM users 
                                   WHERE surname = @surname AND name = @name 
                                   AND role = 'официант' AND employment_status = true";
                    
                    using (var command = new NpgsqlCommand(query, conn))
                    {
                        command.Parameters.AddWithValue("@surname", surname);
                        command.Parameters.AddWithValue("@name", name);
                        
                        var result = command.ExecuteScalar();
                        return result != null ? Convert.ToInt32(result) : -1;
                    }
                }
            }
            catch (Exception ex)
            {
                string filePath = @"A:\Инженерно-техническая поддержка сопровождения ИС\debug.log";
                string errorMessage = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - DB ERROR (GetWaiterIdByName): {ex.Message}\n";
                File.AppendAllText(filePath, errorMessage);
                return -1;
            }
        }
        public int CreateOrder(int tableId, int waiterId, int shiftId, int customerCount, string status, List<OrderItem> orderItems)
        {
            try
            {
                using (var conn = GetConnection())
                using (var transaction = conn.BeginTransaction())
                {
                    try
                    {
                        // 1. Вставляем основной заказ
                        string orderQuery = @"
                            INSERT INTO ""order"" 
                            (table_id, waiter_id, shift_id, customer_count, status, created_at) 
                            VALUES (@tableId, @waiterId, @shiftId, @customerCount, @status, @createdAt)
                            RETURNING order_id";

                        int orderId;
                        using (var orderCommand = new NpgsqlCommand(orderQuery, conn, transaction))
                        {
                            orderCommand.Parameters.AddWithValue("@tableId", tableId);
                            orderCommand.Parameters.AddWithValue("@waiterId", waiterId);
                            orderCommand.Parameters.AddWithValue("@shiftId", shiftId);
                            orderCommand.Parameters.AddWithValue("@customerCount", customerCount);
                            orderCommand.Parameters.AddWithValue("@status", status);
                            orderCommand.Parameters.AddWithValue("@createdAt", DateTime.Now);

                            orderId = Convert.ToInt32(orderCommand.ExecuteScalar());
                        }

                        // 2. Вставляем позиции заказа
                        string orderItemQuery = @"
                            INSERT INTO order_item 
                            (order_id, menu_item_id, quantity) 
                            VALUES (@orderId, @menuItemId, @quantity)";foreach (var item in orderItems)
                        {
                            using (var itemCommand = new NpgsqlCommand(orderItemQuery, conn, transaction))
                            {
                                itemCommand.Parameters.AddWithValue("@orderId", orderId);
                                itemCommand.Parameters.AddWithValue("@menuItemId", item.MenuItemId);
                                itemCommand.Parameters.AddWithValue("@quantity", item.Quantity);
                                itemCommand.Parameters.AddWithValue("@price", item.Price);

                                itemCommand.ExecuteNonQuery();
                            }
                        }

                        transaction.Commit();
                        return orderId;
                    }
                    catch (Exception)
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
            catch (Exception ex)
            {
                string filePath = @"A:\Инженерно-техническая поддержка сопровождения ИС\debug.log";
                string errorMessage = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - DB ERROR (CreateOrder): {ex.Message}\n";
                File.AppendAllText(filePath, errorMessage);
                return -1;
            }
        }
        public int GetMenuItemIdByName(string menuItemName)
        {
            try
            {
                using (var conn = GetConnection())
                {
                    string query = "SELECT item_id FROM menu_item WHERE name = @name";
                    
                    using (var command = new NpgsqlCommand(query, conn))
                    {
                        command.Parameters.AddWithValue("@name", menuItemName);
                        
                        var result = command.ExecuteScalar();
                        return result != null ? Convert.ToInt32(result) : -1;
                    }
                }
            }
            catch (Exception ex)
            {
                string filePath = @"A:\Инженерно-техническая поддержка сопровождения ИС\debug.log";
                string errorMessage = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - DB ERROR (GetMenuItemIdByName): {ex.Message}\n";
                File.AppendAllText(filePath, errorMessage);
                return -1;
            }
        }
        public decimal GetMenuItemPrice(int menuItemId)
        {
            try
            {
                using (var conn = GetConnection())
                {
                    string query = "SELECT price FROM menu_item WHERE menu_item_id = @id";
                    
                    using (var command = new NpgsqlCommand(query, conn))
                    {
                        command.Parameters.AddWithValue("@id", menuItemId);
                        
                        var result = command.ExecuteScalar();
                        return result != null ? Convert.ToDecimal(result) : 0;
                    }
                }
            }
            catch (Exception ex)
            {
                string filePath = @"A:\Инженерно-техническая поддержка сопровождения ИС\debug.log";
                string errorMessage = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - DB ERROR (GetMenuItemPrice): {ex.Message}\n";
                File.AppendAllText(filePath, errorMessage);
                return 0;
            }
        }

        
        public void Dispose()
        {
            _connection?.Close();
            _connection?.Dispose();
        }
        public OrderInfo GetOrderById(int orderId)
        {
            var orderInfo = new OrderInfo();
            
            try
            {
                using (var conn = GetConnection())
                {
                    string orderQuery = @"
                        SELECT 
                            o.order_id,
                            o.table_id,
                            o.waiter_id,
                            o.status,
                            o.created_at,
                            u.surname || ' ' || u.name || COALESCE(' ' || u.patronymic, '') as waiter_name
                        FROM ""order"" o
                        JOIN ""users"" u ON o.waiter_id = u.user_id
                        WHERE o.order_id = @orderId";
                    
                    using (var orderCommand = new NpgsqlCommand(orderQuery, conn))
                    {
                        orderCommand.Parameters.AddWithValue("@orderId", orderId);
                        
                        using (var reader = orderCommand.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                orderInfo.OrderId = reader.GetInt32(0);
                                orderInfo.TableId = reader.GetInt32(1);
                                orderInfo.WaiterId = reader.GetInt32(2);
                                orderInfo.Status = reader.GetString(3);
                                orderInfo.CreatedAt = reader.GetDateTime(4);
                                orderInfo.WaiterName = reader.GetString(5);
                                
                                File.AppendAllText("A:/Инженерно-техническая поддержка сопровождения ИС/debug.log", 
                                    $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - Order found: ID={orderInfo.OrderId}, Status={orderInfo.Status}, Waiter={orderInfo.WaiterName}\n");
                            }
                            else
                            {
                                File.AppendAllText("A:/Инженерно-техническая поддержка сопровождения ИС/debug.log", 
                                    $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - No order found with ID: {orderId}\n");
                                return orderInfo;
                            }
                        }
                    }

                    // Получаем позиции заказа - ВАРИАНТ 1: Без цены
                    string itemsQuery = @"
                        SELECT 
                            mi.name,
                            oi.quantity
                        FROM order_item oi
                        JOIN menu_item mi ON oi.menu_item_id = mi.item_id
                        WHERE oi.order_id = @orderId";
                    
                    using (var itemsCommand = new NpgsqlCommand(itemsQuery, conn))
                    {
                        itemsCommand.Parameters.AddWithValue("@orderId", orderId);
                        
                        using (var reader = itemsCommand.ExecuteReader())
                        {
                            int itemCount = 0;
                            while (reader.Read())
                            {
                                var item = new OrderItemInfo
                                {
                                    MenuItemName = reader.GetString(0),
                                    Quantity = reader.GetInt32(1)
                                };
                                orderInfo.Items.Add(item);
                                itemCount++;
                                
                                File.AppendAllText("A:/Инженерно-техническая поддержка сопровождения ИС/debug.log", 
                                    $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - Order item: {item.MenuItemName}, Quantity: {item.Quantity}\n");
                            }
                            File.AppendAllText("A:/Инженерно-техническая поддержка сопровождения ИС/debug.log", 
                                $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - Found {itemCount} order items\n");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string filePath = @"A:/Инженерно-техническая поддержка сопровождения ИС/debug.log";
                string errorMessage = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - DB ERROR (GetOrderById): {ex.Message}\n{ex.StackTrace}\n";
                File.AppendAllText(filePath, errorMessage);
            }
            
            return orderInfo;
        }
        public bool UpdateOrder(int orderId, int tableId, int waiterId, string status, List<OrderItem> orderItems)
        {
            try
            {
                using (var conn = GetConnection())
                {
                    File.AppendAllText("A:/Инженерно-техническая поддержка сопровождения ИС/debug.log", 
                        $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - UpdateOrder started. Connection state: {conn.State}\n");
                    
                    using (var transaction = conn.BeginTransaction())
                    {
                        try
                        {
                            // 1. Обновляем основной заказ
                            string orderQuery = @"
                                UPDATE ""order"" 
                                SET table_id = @tableId, 
                                    waiter_id = @waiterId, 
                                    status = @status
                                WHERE order_id = @orderId";
                            
                            using (var orderCommand = new NpgsqlCommand(orderQuery, conn, transaction))
                            {
                                orderCommand.Parameters.AddWithValue("@orderId", orderId);
                                orderCommand.Parameters.AddWithValue("@tableId", tableId);
                                orderCommand.Parameters.AddWithValue("@waiterId", waiterId);
                                orderCommand.Parameters.AddWithValue("@status", status);
                                
                                int rowsAffected = orderCommand.ExecuteNonQuery();
                                
                                File.AppendAllText("A:/Инженерно-техническая поддержка сопровождения ИС/debug.log", 
                                    $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - Updated main order: ID={orderId}, Rows affected: {rowsAffected}\n");
                                
                                if (rowsAffected == 0)
                                {
                                    File.AppendAllText("A:/Инженерно-техническая поддержка сопровождения ИС/debug.log", 
                                        $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - WARNING: No rows affected in order update!\n");
                                }
                            }

                            // 2. Удаляем старые позиции заказа
                            string deleteItemsQuery = "DELETE FROM order_item WHERE order_id = @orderId";
                            using (var deleteCommand = new NpgsqlCommand(deleteItemsQuery, conn, transaction))
                            {
                                deleteCommand.Parameters.AddWithValue("@orderId", orderId);
                                int deletedRows = deleteCommand.ExecuteNonQuery();
                                
                                File.AppendAllText("A:/Инженерно-техническая поддержка сопровождения ИС/debug.log", 
                                    $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - Deleted old order items: {deletedRows} rows\n");
                            }

                            // 3. Добавляем новые позиции заказа
                            string insertItemQuery = @"
                                INSERT INTO order_item 
                                (order_id, menu_item_id, quantity) 
                                VALUES (@orderId, @menuItemId, @quantity)";
                            
                            int itemsCount = 0;
                            foreach (var item in orderItems)
                            {
                                using (var itemCommand = new NpgsqlCommand(insertItemQuery, conn, transaction))
                                {
                                    itemCommand.Parameters.AddWithValue("@orderId", orderId);
                                    itemCommand.Parameters.AddWithValue("@menuItemId", item.MenuItemId);
                                    itemCommand.Parameters.AddWithValue("@quantity", item.Quantity);

                                    int itemRows = itemCommand.ExecuteNonQuery();
                                    itemsCount++;
                                    
                                    File.AppendAllText("A:/Инженерно-техническая поддержка сопровождения ИС/debug.log", 
                                        $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - Inserted item: {item.MenuItemId}, rows affected: {itemRows}\n");
                                }
                            }

                            // Явно коммитим транзакцию
                            transaction.Commit();
                            
                            File.AppendAllText("A:/Инженерно-техническая поддержка сопровождения ИС/debug.log", 
                                $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - TRANSACTION COMMITTED. Order ID: {orderId}, Items added: {itemsCount}\n");
                            
                            // Проверяем изменения сразу после коммита
                            VerifyChangesInDatabase(orderId, conn);
                            
                            return true;
                        }
                        catch (Exception ex)
                        {
                            File.AppendAllText("A:/Инженерно-техническая поддержка сопровождения ИС/debug.log", 
                                $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - ERROR in update transaction: {ex.Message}\n{ex.StackTrace}\n");
                            transaction.Rollback();
                            File.AppendAllText("A:/Инженерно-техническая поддержка сопровождения ИС/debug.log", 
                                $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - TRANSACTION ROLLED BACK\n");
                            throw;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                File.AppendAllText("A:/Инженерно-техническая поддержка сопровождения ИС/debug.log", 
                    $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - DB ERROR (UpdateOrder): {ex.Message}\n{ex.StackTrace}\n");
                return false;
            }
        }

        // Метод для проверки изменений
        private void VerifyChangesInDatabase(int orderId, NpgsqlConnection conn)
        {
            try
            {
                // Проверяем обновленный заказ
                string checkOrderQuery = "SELECT table_id, waiter_id, status FROM \"order\" WHERE order_id = @orderId";
                using (var checkCommand = new NpgsqlCommand(checkOrderQuery, conn))
                {
                    checkCommand.Parameters.AddWithValue("@orderId", orderId);
                    using (var reader = checkCommand.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            File.AppendAllText("A:/Инженерно-техническая поддержка сопровождения ИС/debug.log", 
                                $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - VERIFY ORDER: Table={reader.GetInt32(0)}, Waiter={reader.GetInt32(1)}, Status={reader.GetString(2)}\n");
                        }
                        else
                        {
                            File.AppendAllText("A:/Инженерно-техническая поддержка сопровождения ИС/debug.log", 
                                $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - VERIFY ORDER: Order not found!\n");
                        }
                    }
                }

                // Проверяем позиции заказа
                string checkItemsQuery = @"
                    SELECT COUNT(*) FROM order_item 
                    WHERE order_id = @orderId";
                using (var checkCommand = new NpgsqlCommand(checkItemsQuery, conn))
                {
                    checkCommand.Parameters.AddWithValue("@orderId", orderId);
                    int itemCount = Convert.ToInt32(checkCommand.ExecuteScalar());
                    
                    File.AppendAllText("A:/Инженерно-техническая поддержка сопровождения ИС/debug.log", 
                        $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - VERIFY ITEMS: {itemCount} items in database\n");
                }
            }
            catch (Exception ex)
            {
                File.AppendAllText("A:/Инженерно-техническая поддержка сопровождения ИС/debug.log", 
                    $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - ERROR in verification: {ex.Message}\n");
            }
        }
    }
    public class OrderInfo
    {
        public int OrderId { get; set; }
        public int TableId { get; set; }
        public int WaiterId { get; set; }
        public string WaiterName { get; set; } = "";
        public string Status { get; set; } = "";
        public DateTime CreatedAt { get; set; }
        public List<OrderItemInfo> Items { get; set; } = new List<OrderItemInfo>();
    }

    public class OrderItemInfo
    {
        public string MenuItemName { get; set; } = "";
        public int Quantity { get; set; }
        public decimal Price { get; set; }
    }
}