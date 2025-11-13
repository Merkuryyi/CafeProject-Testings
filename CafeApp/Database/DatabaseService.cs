using Npgsql;
using System;
using System.Data;
using System.IO;
using System.Collections.Generic;

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
        
        public void Dispose()
        {
            _connection?.Close();
            _connection?.Dispose();
        }
    }
}