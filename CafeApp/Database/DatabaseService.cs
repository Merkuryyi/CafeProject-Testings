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
        public string logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "debug.log");
        public NpgsqlConnection GetConnection()
        {
            _connection = new NpgsqlConnection(_connectionString);
            _connection.Open();
            return _connection;
        }

        public (string? Role, int? UserId, string? FullName) AuthenticateUser(string username, string password)
        {
            try
            {
                using (var conn = GetConnection())
                {
                    string query = @"SELECT role, user_id, surname || ' ' || name || 
                           COALESCE(' ' || patronymic, '') as full_name 
                           FROM users 
                           WHERE username = @username AND password_ = @password 
                           AND employment_status = true";
            
                    using (var command = new NpgsqlCommand(query, conn))
                    {
                        command.Parameters.AddWithValue("@username", username);
                        command.Parameters.AddWithValue("@password", password);

                        using (var reader = command.ExecuteReader()) 
                        {
                            if (reader.Read())
                            {
                                string role = reader.GetString(0);     
                                int userId = reader.GetInt32(1);
                                string fullName = reader.GetString(2);
                                return (role, userId, fullName);
                            }
                            else
                            { return (null, null, null); }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                
                File.AppendAllText(logPath, $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - AUTH ERROR: {ex.Message}\n");
                return (null, null, null);
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
               
                string errorMessage = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - DB ERROR: {ex.Message}\n";
                File.AppendAllText(logPath, errorMessage);
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
               
                string errorMessage = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - DB ERROR (GetEmployeesList): {ex.Message}\n";
                File.AppendAllText(logPath, errorMessage);
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
                            FROM orders
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
                string errorMessage = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - DB ERROR (GetOrdersList): {ex.Message}\n";
                File.AppendAllText(logPath, errorMessage);
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
                string errorMessage = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - DB ERROR (GetOrderStatus): {ex.Message}\n";
                File.AppendAllText(logPath, errorMessage);
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
                   
                    DateTime currentDate = DateTime.Today;
                    string query = @"SELECT 
                                o.order_id,
                                o.table_id,
                                o.created_at,
                                o.status,
                                s.shift_id
                            FROM orders o
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
                string errorMessage = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - DB ERROR (GetCurrentShiftOrdersList): {ex.Message}\n";
                File.AppendAllText(logPath, errorMessage);
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
                string errorMessage = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - DB ERROR (GetShiftsList): {ex.Message}\n";
                File.AppendAllText(logPath, errorMessage);
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
                string errorMessage = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - DB ERROR (GetWaiterIdByName): {ex.Message}\n";
                File.AppendAllText(logPath, errorMessage);
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
                      
                        string orderQuery = @"
                            INSERT INTO orders 
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
                        
                        string orderItemQuery = @"
                            INSERT INTO order_item 
                            (order_id, menu_item_id, quantity) 
                            VALUES (@orderId, @menuItemId, @quantity)";

                        foreach (var item in orderItems)
                        {
                            using (var itemCommand = new NpgsqlCommand(orderItemQuery, conn, transaction))
                            {
                                itemCommand.Parameters.AddWithValue("@orderId", orderId);
                                itemCommand.Parameters.AddWithValue("@menuItemId", item.MenuItemId);
                                itemCommand.Parameters.AddWithValue("@quantity", item.Quantity);
                                itemCommand.ExecuteNonQuery();
                            }
                        }

                        transaction.Commit();
                        
                        string successMessage = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - Order created successfully: ID={orderId}, Table={tableId}, Items={orderItems.Count}\n";
                        File.AppendAllText(logPath, successMessage);
                        
                        return orderId;
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        string errorMessage = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - DB ERROR in transaction (CreateOrder): {ex.Message}\n{ex.StackTrace}\n";
                        File.AppendAllText(logPath, errorMessage);
                        throw;
                    }
                }
            }
            catch (Exception ex)
            {
                string errorMessage = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - DB ERROR (CreateOrder): {ex.Message}\n{ex.StackTrace}\n";
                File.AppendAllText(logPath, errorMessage);
                return -1;
            }
        }
        public int GetCurrentShiftId()
        {
            try
            {
                using (var conn = GetConnection())
                {
                    string query = @"
                SELECT shift_id FROM shift 
                WHERE shift_date = CURRENT_DATE 
                AND start_time <= CURRENT_TIME 
                AND end_time >= CURRENT_TIME 
                LIMIT 1";
            
                    using (var command = new NpgsqlCommand(query, conn))
                    {
                        var result = command.ExecuteScalar();
                        return result != null ? Convert.ToInt32(result) : -1;
                    }
                }
            }
            catch (Exception ex)
            {
                File.AppendAllText(logPath, 
                    $"Error getting current shift: {ex.Message}\n");
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
                string errorMessage = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - DB ERROR (GetMenuItemIdByName): {ex.Message}\n";
                File.AppendAllText(logPath, errorMessage);
                return -1;
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
                            o.customer_count,  -- Добавлен столбец customer_count
                            o.status,
                            o.created_at,
                            u.surname || ' ' || u.name || COALESCE(' ' || u.patronymic, '') as waiter_name
                        FROM orders o
                        JOIN users u ON o.waiter_id = u.user_id
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
                                orderInfo.CustomerCount = reader.GetInt32(3);  
                                orderInfo.Status = reader.GetString(4);
                                orderInfo.CreatedAt = reader.GetDateTime(5);
                                orderInfo.WaiterName = reader.GetString(6);
                                
                                File.AppendAllText(logPath, 
                                    $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - Order found: ID={orderInfo.OrderId}, Status={orderInfo.Status}, " +
                                    $"Waiter={orderInfo.WaiterName}, CustomerCount={orderInfo.CustomerCount}\n");
                            }
                            else
                            {
                                File.AppendAllText(logPath, 
                                    $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - No order found with ID: {orderId}\n");
                                return orderInfo;
                            }
                        }
                    }

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
                                
                                File.AppendAllText(logPath, 
                                    $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - Order item: {item.MenuItemName}, Quantity: {item.Quantity}\n");
                            }
                            File.AppendAllText(logPath, 
                                $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - Found {itemCount} order items\n");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string errorMessage = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - DB ERROR (GetOrderById): {ex.Message}\n{ex.StackTrace}\n";
                File.AppendAllText(logPath, errorMessage);
            }
            
            return orderInfo;
        }
        
        public bool UpdateOrder(int orderId, int tableId, int waiterId, string status, List<OrderItem> orderItems, int countCustomer)
        {
            try
            {
                using (var conn = GetConnection())
                {
                    using (var transaction = conn.BeginTransaction())
                    {
                        try
                        {
                             string orderQuery = @"
                                UPDATE orders 
                                SET table_id = @tableId, 
                                    waiter_id = @waiterId, 
                                    status = @status,
                                    created_at = @createdAt,
                                    customer_count = @countCustomer 
                                WHERE order_id = @orderId";
                            
                            using (var orderCommand = new NpgsqlCommand(orderQuery, conn, transaction))
                            {
                                orderCommand.Parameters.AddWithValue("@orderId", orderId);
                                orderCommand.Parameters.AddWithValue("@tableId", tableId);
                                orderCommand.Parameters.AddWithValue("@waiterId", waiterId);
                                orderCommand.Parameters.AddWithValue("@status", status);
                                orderCommand.Parameters.AddWithValue("@createdAt", DateTime.Now);
                                orderCommand.Parameters.AddWithValue("@countCustomer", countCustomer);
                                
                                int rowsAffected = orderCommand.ExecuteNonQuery();
                                
                                File.AppendAllText(logPath, 
                                    $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - Updated main order: ID={orderId}, Rows affected: {rowsAffected}\n");
                                
                                if (rowsAffected == 0)
                                {
                                    File.AppendAllText(logPath, 
                                        $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - WARNING: No rows affected in order update!\n");
                                }
                            }

                            string deleteItemsQuery = "DELETE FROM order_item WHERE order_id = @orderId";
                            using (var deleteCommand = new NpgsqlCommand(deleteItemsQuery, conn, transaction))
                            {
                                deleteCommand.Parameters.AddWithValue("@orderId", orderId);
                                int deletedRows = deleteCommand.ExecuteNonQuery();
                                
                                File.AppendAllText(logPath, 
                                    $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - Deleted old order items: {deletedRows} rows\n");
                            }

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
                                    
                                    File.AppendAllText(logPath, 
                                        $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - Inserted item: {item.MenuItemId}, rows affected: {itemRows}\n");
                                }
                            }
                            return true;
                        }
                        catch (Exception ex)
                        {
                            File.AppendAllText(logPath, 
                                $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - ERROR in update transaction: {ex.Message}\n{ex.StackTrace}\n");
                            throw;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                File.AppendAllText(logPath, 
                    $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - DB ERROR (UpdateOrder): {ex.Message}\n{ex.StackTrace}\n");
                return false;
            }
        }
        public string GetOrderPaymentType(int orderId)
        {
            try
            {
                using (var conn = GetConnection())
                {
                    string query = "SELECT payment_type FROM \"order\" WHERE order_id = @orderId";
            
                    using (var command = new NpgsqlCommand(query, conn))
                    {
                        command.Parameters.AddWithValue("@orderId", orderId);
                
                        var result = command.ExecuteScalar();
                        return result?.ToString() ?? "не указан";
                    }
                }
            }
            catch (Exception ex)
            {
                string errorMessage = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - DB ERROR (GetOrderPaymentType): {ex.Message}\n";
                File.AppendAllText(logPath, errorMessage);
                return "не указан";
            }
        }
        public int GetCurrentOrLatestShiftId()
        {
            try
            {
                using (var conn = GetConnection())
                {
                    string currentShiftQuery = @"
                        SELECT shift_id 
                        FROM shift 
                        WHERE shift_date = CURRENT_DATE 
                        AND start_time <= CURRENT_TIME 
                        AND end_time >= CURRENT_TIME 
                        LIMIT 1";
                    
                    using (var command = new NpgsqlCommand(currentShiftQuery, conn))
                    {
                        var result = command.ExecuteScalar();
                        if (result != null)
                        {
                            int currentShiftId = Convert.ToInt32(result);
                            File.AppendAllText(logPath, 
                                $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - Found current shift: ID={currentShiftId}\n");
                            return currentShiftId;
                        }
                    }
                    string latestShiftQuery = @"
                        SELECT shift_id 
                        FROM shift 
                        WHERE shift_date <= CURRENT_DATE
                        ORDER BY shift_date DESC, start_time DESC 
                        LIMIT 1";
                    
                    using (var command = new NpgsqlCommand(latestShiftQuery, conn))
                    {
                        var result = command.ExecuteScalar();
                        if (result != null)
                        {
                            int latestShiftId = Convert.ToInt32(result);
                            File.AppendAllText(logPath, 
                                $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - Current shift not found, using latest shift: ID={latestShiftId}\n");
                            return latestShiftId;
                        }
                    }
                    
                    File.AppendAllText(logPath, 
                        $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - WARNING: No shifts found in database\n");
                    return -1;
                }
            }
            catch (Exception ex)
            {
                File.AppendAllText(logPath, 
                    $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - ERROR in GetCurrentOrLatestShiftId: {ex.Message}\n{ex.StackTrace}\n");
                return -1;
            }
        }
        public ReceiptOrder GetReceiptOrderData(int orderId)
        {
            var receiptOrder = new ReceiptOrder();
            try
            {
                using (var conn = GetConnection())
                {
                    string orderQuery = @"
                        SELECT 
                            o.order_id,
                            o.created_at,
                            o.payment_type,
                            u.surname || ' ' || u.name || COALESCE(' ' || u.patronymic, '') as waiter_name
                        FROM orders o
                        JOIN users u ON o.waiter_id = u.user_id
                        WHERE o.order_id = @orderId";
                    
                    using (var orderCommand = new NpgsqlCommand(orderQuery, conn))
                    {
                        orderCommand.Parameters.AddWithValue("@orderId", orderId);
                        
                        using (var reader = orderCommand.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                receiptOrder.OrderId = reader.GetInt32(0);
                                receiptOrder.OrderDate = reader.GetDateTime(1);
                                receiptOrder.PaymentType = reader.IsDBNull(2) ? "не указан" : reader.GetString(2);
                                receiptOrder.WaiterName = reader.GetString(3);
                            }
                        }
                    }
                    string itemsQuery = @"
                        SELECT 
                            mi.name,
                            oi.quantity,
                            mi.price
                        FROM order_item oi
                        JOIN menu_item mi ON oi.menu_item_id = mi.item_id
                        WHERE oi.order_id = @orderId";
                    
                    using (var itemsCommand = new NpgsqlCommand(itemsQuery, conn))
                    {
                        itemsCommand.Parameters.AddWithValue("@orderId", orderId);
                        
                        using (var reader = itemsCommand.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var item = new ReceiptOrderItem
                                {
                                    DishName = reader.GetString(0),
                                    Quantity = reader.GetInt32(1),
                                    Price = reader.GetDecimal(2)
                                };
                                receiptOrder.Items.Add(item);
                                receiptOrder.TotalAmount += item.TotalPrice;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string errorMessage = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - DB ERROR (GetReceiptOrderData): {ex.Message}\n";
                File.AppendAllText(logPath, errorMessage);
            }
            
            return receiptOrder;
        }
        public bool UpdatePaymentOrder(int orderId, string paymentType)
        {
            try
            {
                using (var conn = GetConnection())
                {
                    using (var transaction = conn.BeginTransaction())
                    {
                        try
                        {
                            string orderQuery = @"
                                UPDATE orders 
                                SET payment_type = @paymentType 
                                WHERE order_id = @orderId";
                            
                            using (var orderCommand = new NpgsqlCommand(orderQuery, conn, transaction))
                            {
                                orderCommand.Parameters.AddWithValue("@orderId", orderId);
                                orderCommand.Parameters.AddWithValue("@paymentType", paymentType);
                                int rowsAffected = orderCommand.ExecuteNonQuery();
                                
                                File.AppendAllText(logPath, 
                                    $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - UpdatePaymentOrder: ID={orderId}, Type={paymentType}, Rows affected: {rowsAffected}\n");
                                
                                if (rowsAffected == 0)
                                {
                                    File.AppendAllText(logPath, 
                                        $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - WARNING: No rows affected in payment update!\n");
                                }
                            }
                            transaction.Commit();
                            
                            File.AppendAllText(logPath, 
                                $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - Payment update COMMITTED\n");
                            
                            return true;
                        }
                        catch (Exception ex)
                        {
                            File.AppendAllText(logPath, 
                                $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - ERROR in payment update transaction: {ex.Message}\n{ex.StackTrace}\n");
                            transaction.Rollback();
                            File.AppendAllText(logPath, 
                                $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - Payment update ROLLED BACK\n");
                            throw;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                File.AppendAllText(logPath, 
                    $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - DB ERROR (UpdatePaymentOrder): {ex.Message}\n{ex.StackTrace}\n");
                return false;
            }
        }
        public decimal GetMenuItemPriceByName(string menuItemName)
        {
            try
            {
                using var connection = GetConnection();
        
                var query = "SELECT price FROM menu_item WHERE name = @Name";
                using var command = new NpgsqlCommand(query, connection);
                command.Parameters.AddWithValue("@Name", menuItemName);
        
                var result = command.ExecuteScalar();
                return result != null ? Convert.ToDecimal(result) : 0;
            }
            catch (Exception ex)
            {
                string errorMessage = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - DB ERROR (GetMenuItemPriceByName): {ex.Message}\n";
                File.AppendAllText(logPath, errorMessage);
                return 0;
            }
        }
        public UserInfo GetEmployeeById(int employeeId)
        {
            var userInfo = new UserInfo();
            
            try
            {
                using (var conn = GetConnection())
                {
                    string query = @"
                        SELECT 
                            user_id,
                            username,
                            password_,
                            role,
                            name,
                            surname,
                            patronymic,
                            employment_status,
                            photo_link,
                            contract_scan_link
                        FROM users 
                        WHERE user_id = @employeeId";
                    
                    using (var command = new NpgsqlCommand(query, conn))
                    {
                        command.Parameters.AddWithValue("@employeeId", employeeId);
                        
                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                userInfo.UserId = reader.GetInt32(0);
                                userInfo.Username = reader.GetString(1);
                                userInfo.Password = reader.GetString(2);
                                userInfo.Role = reader.GetString(3);
                                userInfo.Name = reader.GetString(4);
                                userInfo.Surname = reader.GetString(5);
                                userInfo.Patronymic = reader.IsDBNull(6) ? null : reader.GetString(6);
                                userInfo.EmploymentStatus = reader.GetBoolean(7);
                                userInfo.PhotoLink = reader.IsDBNull(8) ? null : reader.GetString(8);
                                userInfo.ContractScanLink = reader.IsDBNull(9) ? null : reader.GetString(9);
                                
                                File.AppendAllText(logPath, 
                                    $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - Employee found: ID={userInfo.UserId}, " +
                                    $"Username={userInfo.Username}, Role={userInfo.Role}, Name={userInfo.Surname} {userInfo.Name}\n");
                            }
                            else
                            {
                                File.AppendAllText(logPath, 
                                    $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - No employee found with ID: {employeeId}\n");
                                return userInfo;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string errorMessage = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - DB ERROR (GetEmployeeById): {ex.Message}\n{ex.StackTrace}\n";
                File.AppendAllText(logPath, errorMessage);
            }
            
            return userInfo;
        }
        public bool UpdateEmployeeStatus(int employeeId, bool employmentStatus)
        {
            try
            {
                using (var conn = GetConnection())
                {
                    string query = @"
                UPDATE users 
                SET employment_status = @employmentStatus 
                WHERE user_id = @employeeId";
            
                    using (var command = new NpgsqlCommand(query, conn))
                    {
                        command.Parameters.AddWithValue("@employeeId", employeeId);
                        command.Parameters.AddWithValue("@employmentStatus", employmentStatus);
                
                        int rowsAffected = command.ExecuteNonQuery();
                        
                        string logMessage = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - Employee status updated: ID={employeeId}, " +
                                            $"New Status={(employmentStatus ? "работает" : "уволен")}, Rows affected: {rowsAffected}\n";
                        File.AppendAllText(logPath, logMessage);
                
                        return rowsAffected == 1;
                    }
                }
            }
            catch (Exception ex)
            {
                string errorMessage = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - DB ERROR (UpdateEmployeeStatus): {ex.Message}\n{ex.StackTrace}\n";
                File.AppendAllText(logPath, errorMessage);
                return false;
            }
        }
        public List<UserInfo> GetAllEmployeesExceptAdmins()
        {
            var employees = new List<UserInfo>();
            try
            {
                using (var conn = GetConnection())
                {
                    string query = @"SELECT 
                                user_id,
                                username,
                                password_,
                                role,
                                name,
                                surname,
                                patronymic,
                                employment_status,
                                photo_link,
                                contract_scan_link
                            FROM users 
                            WHERE employment_status = true 
                            AND role != 'администратор'
                            ORDER BY surname, name";
                    
                    using (var command = new NpgsqlCommand(query, conn))
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var userInfo = new UserInfo
                            {
                                UserId = reader.GetInt32(0),
                                Username = reader.GetString(1),
                                Password = reader.GetString(2),
                                Role = reader.GetString(3),
                                Name = reader.GetString(4),
                                Surname = reader.GetString(5),
                                Patronymic = reader.IsDBNull(6) ? null : reader.GetString(6),
                                EmploymentStatus = reader.GetBoolean(7),
                                PhotoLink = reader.IsDBNull(8) ? null : reader.GetString(8),
                                ContractScanLink = reader.IsDBNull(9) ? null : reader.GetString(9)
                            };
                            employees.Add(userInfo);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string errorMessage = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " - DB ERROR (GetAllEmployeesExceptAdmins): " + ex.Message + "\n";
                File.AppendAllText(logPath, errorMessage);
            }
            return employees;
        }
        public int CreateShift(DateTime shiftDate, TimeSpan startTime, TimeSpan endTime)
        {
            try
            {
                using (var conn = GetConnection())
                {
                    string query = @"
                        INSERT INTO shift (shift_date, start_time, end_time) 
                        VALUES (@shiftDate, @startTime, @endTime)
                        RETURNING shift_id";

                    using (var command = new NpgsqlCommand(query, conn))
                    {
                        command.Parameters.AddWithValue("@shiftDate", shiftDate);
                        command.Parameters.AddWithValue("@startTime", startTime);
                        command.Parameters.AddWithValue("@endTime", endTime);

                        var result = command.ExecuteScalar();
                        return result != null ? Convert.ToInt32(result) : -1;
                    }
                }
            }
            catch (Exception ex)
            {
                string errorMessage = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - DB ERROR (CreateShift): {ex.Message}\n";
                File.AppendAllText(logPath, errorMessage);
                return -1;
            }
        }


        public bool AddEmployeeToShift(int shiftId, int userId)
        {
            try
            {
                using (var conn = GetConnection())
                {
                    string query = @"
                        INSERT INTO shift_assignment (shift_id, user_id) 
                        VALUES (@shiftId, @userId)";

                    using (var command = new NpgsqlCommand(query, conn))
                    {
                        command.Parameters.AddWithValue("@shiftId", shiftId);
                        command.Parameters.AddWithValue("@userId", userId);

                        int rowsAffected = command.ExecuteNonQuery();
                        return rowsAffected == 1;
                    }
                }
            }
            catch (Exception ex)
            {
                string errorMessage = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - DB ERROR (AddEmployeeToShift): {ex.Message}\n";
                File.AppendAllText(logPath, errorMessage);
                return false;
            }
        }


       public ShiftInfo GetShiftById(int shiftId)
        {
            var shiftInfo = new ShiftInfo();
            
            try
            {
                using (var conn = GetConnection())
                {
                    string shiftQuery = @"
                        SELECT 
                            shift_id,
                            shift_date,
                            start_time,
                            end_time
                        FROM shift 
                        WHERE shift_id = @shiftId";
                    
                    using (var shiftCommand = new NpgsqlCommand(shiftQuery, conn))
                    {
                        shiftCommand.Parameters.AddWithValue("@shiftId", shiftId);
                        
                        using (var reader = shiftCommand.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                shiftInfo.ShiftId = reader.GetInt32(0);
                                shiftInfo.ShiftDate = reader.GetDateTime(1);
                                shiftInfo.StartTime = reader.GetTimeSpan(2);
                                shiftInfo.EndTime = reader.GetTimeSpan(3);
                            }
                        }
                    }
                    string employeesQuery = @"
                        SELECT 
                            u.user_id,
                            u.surname,
                            u.name,
                            u.patronymic,
                            u.role,
                            t.table_number
                        FROM shift_assignment sa
                        JOIN users u ON sa.user_id = u.user_id
                        LEFT JOIN table_assignment ta ON sa.shift_id = ta.shift_id AND sa.user_id = ta.user_id
                        LEFT JOIN table_cafe t ON ta.table_id = t.table_id
                        WHERE sa.shift_id = @shiftId
                        ORDER BY u.surname, u.name";
                    
                    using (var employeesCommand = new NpgsqlCommand(employeesQuery, conn))
                    {
                        employeesCommand.Parameters.AddWithValue("@shiftId", shiftId);
                        
                        using (var reader = employeesCommand.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var employee = new ShiftEmployeeInfo
                                {
                                    UserId = reader.GetInt32(0),
                                    Surname = reader.GetString(1),
                                    Name = reader.GetString(2),
                                    Patronymic = reader.IsDBNull(3) ? null : reader.GetString(3),
                                    Role = reader.GetString(4),
                                    TableNumber = reader.IsDBNull(5) ? null : reader.GetInt32(5).ToString()
                                };
                                shiftInfo.Employees.Add(employee);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string errorMessage = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - DB ERROR (GetShiftById): {ex.Message}\n";
                File.AppendAllText(logPath, errorMessage);
            }
            
            return shiftInfo;
        }
        public bool AssignTableToWaiter(int shiftId, int waiterId, int tableNumber, DateTime assignmentDate)
        {
            try
            {
                using (var conn = GetConnection())
                {
                    string checkTableQuery = "SELECT table_id FROM \"table\" WHERE table_number = @tableNumber";
                    int tableId = -1;
                    
                    using (var checkCommand = new NpgsqlCommand(checkTableQuery, conn))
                    {
                        checkCommand.Parameters.AddWithValue("@tableNumber", tableNumber);
                        var result = checkCommand.ExecuteScalar();
                        if (result == null)
                        {
                            string createTableQuery = @"
                                INSERT INTO table_cafe (table_number, capacity) 
                                VALUES (@tableNumber, 4)
                                RETURNING table_id";
                            
                            using (var createCommand = new NpgsqlCommand(createTableQuery, conn))
                            {
                                createCommand.Parameters.AddWithValue("@tableNumber", tableNumber);
                                tableId = Convert.ToInt32(createCommand.ExecuteScalar());
                                
                                File.AppendAllText(logPath, 
                                    DateTime.Now.ToString() + " - Created new table: ID=" + tableId.ToString() + 
                                    ", Number=" + tableNumber.ToString() + "\n");
                            }
                        }
                        else
                        {
                            tableId = Convert.ToInt32(result);
                        }
                    }

                    string assignQuery = @"
                        INSERT INTO table_assignment (table_id, user_id, shift_id, assignment_date) 
                        VALUES (@tableId, @waiterId, @shiftId, @assignmentDate)";
                    
                    using (var assignCommand = new NpgsqlCommand(assignQuery, conn))
                    {
                        assignCommand.Parameters.AddWithValue("@tableId", tableId);
                        assignCommand.Parameters.AddWithValue("@waiterId", waiterId);
                        assignCommand.Parameters.AddWithValue("@shiftId", shiftId);
                        assignCommand.Parameters.AddWithValue("@assignmentDate", assignmentDate);

                        int rowsAffected = assignCommand.ExecuteNonQuery();
                        
                        File.AppendAllText(logPath, 
                            DateTime.Now.ToString() + " - Table assignment: Table=" + tableNumber.ToString() + 
                            ", Waiter=" + waiterId.ToString() + ", Shift=" + shiftId.ToString() + 
                            ", Rows affected=" + rowsAffected.ToString() + "\n");
                        
                        return rowsAffected == 1;
                    }
                }
            }
            catch (Exception ex)
            {
                string errorMessage = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - DB ERROR (AssignTableToWaiter): {ex.Message}\n{ex.StackTrace}\n";
                File.AppendAllText(logPath, errorMessage);
                return false;
            }
        }
        public bool IsShiftExists(DateTime shiftDate, TimeSpan startTime, TimeSpan endTime)
        {
            try
            {
                using (var conn = GetConnection())
                {
                    string query = @"
                        SELECT COUNT(*) 
                        FROM shift 
                        WHERE shift_date = @shiftDate 
                        AND start_time = @startTime 
                        AND end_time = @endTime";
                    
                    using (var command = new NpgsqlCommand(query, conn))
                    {
                        command.Parameters.AddWithValue("@shiftDate", shiftDate);
                        command.Parameters.AddWithValue("@startTime", startTime);
                        command.Parameters.AddWithValue("@endTime", endTime);

                        long count = (long)command.ExecuteScalar();
                        return count > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                string errorMessage = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - DB ERROR (IsShiftExists): {ex.Message}\n";
                File.AppendAllText(logPath, errorMessage);
                return false;
            }
        }
        public int GetEmployeeIdByName(string fullName)
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
                    string patronymic = nameParts.Length > 2 ? nameParts[2] : "";
                    
                    string query = @"SELECT user_id FROM users 
                                   WHERE surname = @surname AND name = @name 
                                   AND (patronymic = @patronymic OR patronymic IS NULL)
                                   AND employment_status = true";
                    
                    using (var command = new NpgsqlCommand(query, conn))
                    {
                        command.Parameters.AddWithValue("@surname", surname);
                        command.Parameters.AddWithValue("@name", name);
                        command.Parameters.AddWithValue("@patronymic", 
                            string.IsNullOrEmpty(patronymic) ? (object)DBNull.Value : patronymic);
                        
                        var result = command.ExecuteScalar();
                        return result != null ? Convert.ToInt32(result) : -1;
                    }
                }
            }
            catch (Exception ex)
            {
                string errorMessage = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - DB ERROR (GetEmployeeIdByName): {ex.Message}\n";
                File.AppendAllText(logPath, errorMessage);
                return -1;
            }
        }
        public bool UpdateShift(int shiftId, DateTime shiftDate, TimeSpan startTime, TimeSpan endTime)
        {
            try
            {
                using (var conn = GetConnection())
                {
                    string query = @"
                        UPDATE shift 
                        SET shift_date = @shiftDate, 
                            start_time = @startTime, 
                            end_time = @endTime
                        WHERE shift_id = @shiftId";

                    using (var command = new NpgsqlCommand(query, conn))
                    {
                        command.Parameters.AddWithValue("@shiftId", shiftId);
                        command.Parameters.AddWithValue("@shiftDate", shiftDate);
                        command.Parameters.AddWithValue("@startTime", startTime);
                        command.Parameters.AddWithValue("@endTime", endTime);

                        int rowsAffected = command.ExecuteNonQuery();
                        return rowsAffected == 1;
                    }
                }
            }
            catch (Exception ex)
            {
                string errorMessage = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - DB ERROR (UpdateShift): {ex.Message}\n";
                File.AppendAllText(logPath, errorMessage);
                return false;
            }
        }

        public bool ClearShiftEmployees(int shiftId)
        {
            try
            {
                using (var conn = GetConnection())
                {
                    string query = "DELETE FROM shift_assignment WHERE shift_id = @shiftId";
                    
                    using (var command = new NpgsqlCommand(query, conn))
                    {
                        command.Parameters.AddWithValue("@shiftId", shiftId);
                        int rowsAffected = command.ExecuteNonQuery();
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                string errorMessage = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - DB ERROR (ClearShiftEmployees): {ex.Message}\n";
                File.AppendAllText(logPath, errorMessage);
                return false;
            }
        }

        public bool ClearWaiterTableAssignments(int shiftId, int waiterId)
        {
            try
            {
                using (var conn = GetConnection())
                {
                    string query = @"
                        DELETE FROM table_assignment 
                        WHERE shift_id = @shiftId AND user_id = @waiterId";
                    
                    using (var command = new NpgsqlCommand(query, conn))
                    {
                        command.Parameters.AddWithValue("@shiftId", shiftId);
                        command.Parameters.AddWithValue("@waiterId", waiterId);
                        int rowsAffected = command.ExecuteNonQuery();
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                string errorMessage = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - DB ERROR (ClearWaiterTableAssignments): {ex.Message}\n";
                File.AppendAllText(logPath, errorMessage);
                return false;
            }
        }
        public bool DeleteShift(int shiftId)
        {
            try
            {
                using (var conn = GetConnection())
                using (var transaction = conn.BeginTransaction())
                {
                    try
                    {
                        string deleteEmployeesQuery = "DELETE FROM shift_employee WHERE shift_id = @shiftId";
                        using (var deleteCommand = new NpgsqlCommand(deleteEmployeesQuery, conn, transaction))
                        {
                            deleteCommand.Parameters.AddWithValue("@shiftId", shiftId);
                            deleteCommand.ExecuteNonQuery();
                        }

                        string deleteShiftQuery = "DELETE FROM shift WHERE shift_id = @shiftId";
                        using (var shiftCommand = new NpgsqlCommand(deleteShiftQuery, conn, transaction))
                        {
                            shiftCommand.Parameters.AddWithValue("@shiftId", shiftId);
                            int rowsAffected = shiftCommand.ExecuteNonQuery();
                            
                            transaction.Commit();
                            return rowsAffected == 1;
                        }
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
            catch (Exception ex)
            {
                string errorMessage = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - DB ERROR (DeleteShift): {ex.Message}\n";
                File.AppendAllText(logPath, errorMessage);
                return false;
            }
            
        }
        public List<ListItem> GetWaiterOrders(int waiterId, int shiftId = -1)
        {
            var orders = new List<ListItem>();
            try
            {
                using (var conn = GetConnection())
                {
                    string query;
                    NpgsqlCommand command;

                    if (shiftId > 0)
                    {
                        query = @"
                            SELECT 
                                o.order_id,
                                o.table_id,
                                o.created_at,
                                o.status,
                                o.customer_count,
                                COALESCE((
                                    SELECT SUM(oi.quantity * mi.price)
                                    FROM order_item oi
                                    JOIN menu_item mi ON oi.menu_item_id = mi.item_id
                                    WHERE oi.order_id = o.order_id
                                ), 0) as total_amount
                            FROM orders o
                            WHERE o.waiter_id = @waiterId 
                            AND o.shift_id = @shiftId
                            ORDER BY o.created_at DESC";
                        
                        command = new NpgsqlCommand(query, conn);
                        command.Parameters.AddWithValue("@waiterId", waiterId);
                        command.Parameters.AddWithValue("@shiftId", shiftId);
                    }
                    else
                    {
                        query = @"
                            SELECT 
                                o.order_id,
                                o.table_id,
                                o.created_at,
                                o.status,
                                o.customer_count,
                                s.shift_date,
                                COALESCE((
                                    SELECT SUM(oi.quantity * mi.price)
                                    FROM order_item oi
                                    JOIN menu_item mi ON oi.menu_item_id = mi.item_id
                                    WHERE oi.order_id = o.order_id
                                ), 0) as total_amount
                            FROM orders o
                            JOIN shift s ON o.shift_id = s.shift_id
                            WHERE o.waiter_id = @waiterId
                            ORDER BY o.created_at DESC";
                        
                        command = new NpgsqlCommand(query, conn);
                        command.Parameters.AddWithValue("@waiterId", waiterId);
                    }

                    using (command)
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int orderId = reader.GetInt32(0);
                            int tableId = reader.GetInt32(1);
                            DateTime createdAt = reader.GetDateTime(2);
                            string status = reader.GetString(3);
                            int customerCount = reader.GetInt32(4);
                            decimal totalAmount = reader.GetDecimal(5);
                            
                            string displayText;
                            if (shiftId > 0)
                            {
                                displayText = $"Стол №{tableId} - {createdAt:HH:mm} - {status} - {customerCount} чел. - {totalAmount:C}";
                            }
                            else
                            {
                                DateTime shiftDate = reader.GetDateTime(5);
                                displayText = $"Стол №{tableId} - {shiftDate:yyyy-MM-dd} {createdAt:HH:mm} - {status} - {customerCount} чел. - {totalAmount:C}";
                            }

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
                string errorMessage = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - DB ERROR (GetWaiterOrders): {ex.Message}\n";
                File.AppendAllText(logPath, errorMessage);
            }
            return orders;
        }
        public int? GetWaiterTableForCurrentShift(int waiterId)
        {
            try
            {
                using (var conn = GetConnection())
                {
                    int currentShiftId = GetCurrentOrLatestShiftId();
                    if (currentShiftId == -1)
                    {
                        File.AppendAllText(logPath, 
                            $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - No current shift found for waiter {waiterId}\n");
                        return null;
                    }

                    string query = @"
                        SELECT t.table_number
                        FROM table_assignment ta
                        JOIN table_cafe t ON ta.table_id = t.table_id
                        WHERE ta.user_id = @waiterId 
                        AND ta.shift_id = @shiftId
                        LIMIT 1";

                    using (var command = new NpgsqlCommand(query, conn))
                    {
                        command.Parameters.AddWithValue("@waiterId", waiterId);
                        command.Parameters.AddWithValue("@shiftId", currentShiftId);

                        var result = command.ExecuteScalar();
                        if (result != null)
                        {
                            int tableNumber = Convert.ToInt32(result);
                            File.AppendAllText(logPath, 
                                $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - Found table {tableNumber} for waiter {waiterId} in shift {currentShiftId}\n");
                            return tableNumber;
                        }
                        else
                        {
                            File.AppendAllText(logPath, 
                                $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - No table assigned to waiter {waiterId} in shift {currentShiftId}\n");
                            return null;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string errorMessage = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - DB ERROR (GetWaiterTableForCurrentShift): {ex.Message}\n";
                File.AppendAllText(logPath, errorMessage);
                return null;
            }
        }
         public List<OrderReportData> GetOrdersReceivedReport(int shiftId)
         {
             var orders = new List<OrderReportData>();
             try
             {
                 using (var conn = GetConnection())
                 {
                     string query = @"
                         SELECT 
                             o.order_id,
                             o.created_at,
                             o.table_id,
                             u.surname || ' ' || u.name || COALESCE(' ' || u.patronymic, '') as waiter_name,
                             o.status,
                             COALESCE(o.payment_type, 'не указан') as payment_type,
                             o.customer_count,
                             COALESCE((
                                 SELECT SUM(oi.quantity * mi.price)
                                 FROM order_item oi
                                 JOIN menu_item mi ON oi.menu_item_id = mi.item_id
                                 WHERE oi.order_id = o.order_id
                             ), 0) as total_amount
                         FROM orders o
                         JOIN users u ON o.waiter_id = u.user_id
                         WHERE o.shift_id = @shiftId
                         ORDER BY o.created_at";
 
                     using (var command = new NpgsqlCommand(query, conn))
                     {
                         command.Parameters.AddWithValue("@shiftId", shiftId);
                         
                         using (var reader = command.ExecuteReader())
                         {
                             while (reader.Read())
                             {
                                 var order = new OrderReportData
                                 {
                                     OrderId = reader.GetInt32(0),
                                     OrderDate = reader.GetDateTime(1),
                                     TableId = reader.GetInt32(2),
                                     WaiterName = reader.GetString(3),
                                     Status = reader.GetString(4),
                                     PaymentType = reader.GetString(5),
                                     CustomerCount = reader.GetInt32(6),
                                     TotalAmount = reader.GetDecimal(7)
                                 };
                                 order.Items = GetOrderItemsForReport(order.OrderId);
                                 orders.Add(order);
                             }
                         }
                     }
                 }
             }
             catch (Exception ex)
             {
                 string errorMessage = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - DB ERROR (GetOrdersReceivedReport): {ex.Message}\n";
                 File.AppendAllText(logPath, errorMessage);
             }
             return orders;
         }
 
         public List<OrderReportData> GetPaidOrdersReport(int shiftId = -1)
         {
             var orders = new List<OrderReportData>();
             try
             {
                 using (var conn = GetConnection())
                 {
                     string query;
                     NpgsqlCommand command;
 
                     if (shiftId > 0)
                     {
                         query = @"
                             SELECT 
                                 o.order_id,
                                 o.created_at,
                                 o.table_id,
                                 u.surname || ' ' || u.name || COALESCE(' ' || u.patronymic, '') as waiter_name,
                                 o.status,
                                 COALESCE(o.payment_type, 'не указан') as payment_type,
                                 o.customer_count,
                                 COALESCE((
                                     SELECT SUM(oi.quantity * mi.price)
                                     FROM order_item oi
                                     JOIN menu_item mi ON oi.menu_item_id = mi.item_id
                                     WHERE oi.order_id = o.order_id
                                 ), 0) as total_amount
                             FROM orders o
                             JOIN users u ON o.waiter_id = u.user_id
                             WHERE o.shift_id = @shiftId AND o.status = 'оплачен'
                             ORDER BY o.created_at";
                         
                         command = new NpgsqlCommand(query, conn);
                         command.Parameters.AddWithValue("@shiftId", shiftId);
                     }
                     else
                     {
                         int currentShiftId = GetCurrentOrLatestShiftId();
                         
                         query = @"
                             SELECT 
                                 o.order_id,
                                 o.created_at,
                                 o.table_id,
                                 u.surname || ' ' || u.name || COALESCE(' ' || u.patronymic, '') as waiter_name,
                                 o.status,
                                 COALESCE(o.payment_type, 'не указан') as payment_type,
                                 o.customer_count,
                                 COALESCE((
                                     SELECT SUM(oi.quantity * mi.price)
                                     FROM order_item oi
                                     JOIN menu_item mi ON oi.menu_item_id = mi.item_id
                                     WHERE oi.order_id = o.order_id
                                 ), 0) as total_amount
                             FROM orders o
                             JOIN users u ON o.waiter_id = u.user_id
                             WHERE o.shift_id = @shiftId AND o.status = 'оплачен'
                             ORDER BY o.created_at";
                         
                         command = new NpgsqlCommand(query, conn);
                         command.Parameters.AddWithValue("@shiftId", currentShiftId);
                     }
 
                     using (command)
                     using (var reader = command.ExecuteReader())
                     {
                         while (reader.Read())
                         {
                             var order = new OrderReportData
                             {
                                 OrderId = reader.GetInt32(0),
                                 OrderDate = reader.GetDateTime(1),
                                 TableId = reader.GetInt32(2),
                                 WaiterName = reader.GetString(3),
                                 Status = reader.GetString(4),
                                 PaymentType = reader.GetString(5),
                                 CustomerCount = reader.GetInt32(6),
                                 TotalAmount = reader.GetDecimal(7)
                             };
                             
                             order.Items = GetOrderItemsForReport(order.OrderId);
                             orders.Add(order);
                         }
                     }
                 }
             }
             catch (Exception ex)
             {
                 string errorMessage = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - DB ERROR (GetPaidOrdersReport): {ex.Message}\n";
                 File.AppendAllText(logPath, errorMessage);
             }
             return orders;
         }

            private List<OrderItemReport> GetOrderItemsForReport(int orderId)
            {
                var items = new List<OrderItemReport>();
                try
                {
                    using (var conn = GetConnection())
                    {
                        string query = @"
                            SELECT 
                                mi.name,
                                oi.quantity,
                                mi.price
                            FROM order_item oi
                            JOIN menu_item mi ON oi.menu_item_id = mi.item_id
                            WHERE oi.order_id = @orderId";
                        
                        using (var command = new NpgsqlCommand(query, conn))
                        {
                            command.Parameters.AddWithValue("@orderId", orderId);
                            
                            using (var reader = command.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    var item = new OrderItemReport
                                    {
                                        DishName = reader.GetString(0),
                                        Quantity = reader.GetInt32(1),
                                        Price = reader.GetDecimal(2)
                                    };
                                    items.Add(item);
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    string errorMessage = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - DB ERROR (GetOrderItemsForReport): {ex.Message}\n";
                    File.AppendAllText(logPath, errorMessage);
                }
                return items;
            }
    }
}