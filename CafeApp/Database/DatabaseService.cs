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
        public List<string> GetEmployeesList()
        {
            var employees = new List<string>();
            try
            {
                using (var conn = GetConnection())
                {
                    string query = @"SELECT surname, name, patronymic
                                    FROM users 
                                    WHERE employment_status = true 
                                    ORDER BY surname, name";
                    
                    using (var command = new NpgsqlCommand(query, conn))
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string surname = reader.GetString(0);
                            string name = reader.GetString(1);
                            string patronymic = reader.IsDBNull(2) ? "" : reader.GetString(2);
                            string employeeInfo = $"{surname} {name} {patronymic}";
                            employees.Add(employeeInfo);
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
        public List<string> GetOrdersSimpleInfo()
        {
            var orders = new List<string>();
            try
            {
                using (var conn = GetConnection())
                {
                    string query = @"SELECT 
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
                            int tableId = reader.GetInt32(0);
                            DateTime createdAt = reader.GetDateTime(1);
                            string status = reader.GetString(2);
                            string orderInfo = $"Стол №{tableId} - {createdAt:yyyy-MM-dd HH:mm} - {status}";
                            orders.Add(orderInfo);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string filePath = @"A:\Инженерно-техническая поддержка сопровождения ИС\debug.log";
                string errorMessage = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - DB ERROR (GetOrdersSimpleInfo): {ex.Message}\n";
                File.AppendAllText(filePath, errorMessage);
            }
            return orders;
        }
        public List<string> GetShiftsList()
        {
            var shifts = new List<string>();
            try
            {
                using (var conn = GetConnection())
                {
                    string query = @"SELECT shift_id, shift_date, start_time, end_time
                            FROM shift ORDER BY shift_date DESC, start_time DESC";
            
                    using (var command = new NpgsqlCommand(query, conn))
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int shiftId = reader.GetInt32(0);
                            DateTime shiftDate = reader.GetDateTime(1);
                            TimeSpan startTime = reader.GetTimeSpan(2);
                            TimeSpan endTime = reader.GetTimeSpan(3);
                            string shiftInfo = $"Смена {shiftId} - {shiftDate:yyyy-MM-dd} ({startTime:hh\\:mm} - {endTime:hh\\:mm})";
                            shifts.Add(shiftInfo);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string filePath = @"A:\Инженерно-техническая поддержка сопровождения ИС\debug.log";
                string errorMessage = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - DB ERROR (GetShiftsList): {ex.Message}\n";
                File.AppendAllText(filePath, errorMessage);
                Console.WriteLine($"Ошибка получения списка смен: {ex.Message}");
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
                            (order_id, menu_item_id, quantity, price) 
                            VALUES (@orderId, @menuItemId, @quantity, @price)";foreach (var item in orderItems)
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
                    string query = "SELECT menu_item_id FROM menu_item WHERE name = @name";
                    
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
    }
}