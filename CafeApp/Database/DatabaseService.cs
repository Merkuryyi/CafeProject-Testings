using Npgsql;
using CafeApp.Database;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO; // Добавьте этот using

namespace CafeApp.Database
{
    public class DatabaseService : IDisposable
    {
        private NpgsqlConnection? _connection;
        private string _connectionString = "Host=localhost;Username=postgres;Password=6645;Database=Cafe";

        public NpgsqlConnection GetConnection()
        {
            _connection = new NpgsqlConnection(_connectionString);
            _connection.Open();
            return _connection;
        }

        public bool AuthenticateUser(string username, string password)
        {
            try
            {
                using (var conn = GetConnection())
                {
                    string query = @"SELECT * FROM ""users"" WHERE username = @username AND password_ = @password AND employment_status = true";
                    using (var command = new NpgsqlCommand(query, conn))
                    {
                        command.Parameters.AddWithValue("@username", username);
                        command.Parameters.AddWithValue("@password", password);
                        
                        // ИСПРАВЛЕННАЯ СТРОКА ЛОГИРОВАНИЯ:
                        string filePath = @"A:\Инженерно-техническая поддержка сопровождения ИС\debug.log";
                        string logMessage = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - DB query: username='{username}', password='{password}'\n";
                        File.AppendAllText(filePath, logMessage);

                        using (var reader = command.ExecuteReader()) 
                        {
                            bool hasRows = reader.HasRows;
                            
                            // Логируем результат запроса
                            string resultMessage = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - DB result: {hasRows}\n";
                            File.AppendAllText(filePath, resultMessage);
                            
                            return hasRows;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string filePath = @"A:\Инженерно-техническая поддержка сопровождения ИС\debug.log";
                string errorMessage = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - DB ERROR: {ex.Message}\n";
                File.AppendAllText(filePath, errorMessage);
                
                Console.WriteLine($"Ошибка аутентификации: {ex.Message}");
                return false;
            }
        }

        public void Dispose()
        {
            _connection?.Close();
            _connection?.Dispose();
        }
    }
}