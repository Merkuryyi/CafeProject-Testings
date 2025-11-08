using Npgsql;
using System;
using System.Data;
using System.IO;

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

        public string? AuthenticateUser(string username, string password)
        {
            try
            {
                using (var conn = GetConnection())
                {
                    // ИСПРАВЛЕН ЗАПРОС: таблица называется "user", а не "users"
                    string query = @"SELECT role FROM ""users"" WHERE username = @username AND password_ = @password AND employment_status = true";
                    using (var command = new NpgsqlCommand(query, conn))
                    {
                        command.Parameters.AddWithValue("@username", username);
                        command.Parameters.AddWithValue("@password", password);
                        
                        string filePath = @"A:\Инженерно-техническая поддержка сопровождения ИС\debug.log";
                        string logMessage = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - DB query: username='{username}', password='{password}'\n";
                        File.AppendAllText(filePath, logMessage);

                        using (var reader = command.ExecuteReader()) 
                        {
                            if (reader.Read())
                            {
                                string role = reader.GetString(0); // Получаем роль из первого столбца
                                
                                string resultMessage = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - DB result: role='{role}'\n";
                                File.AppendAllText(filePath, resultMessage);
                                
                                return role;
                            }
                            else
                            {
                                string resultMessage = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - DB result: user not found\n";
                                File.AppendAllText(filePath, resultMessage);
                                return null;
                            }
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
                return null;
            }
        }

        public void Dispose()
        {
            _connection?.Close();
            _connection?.Dispose();
        }
    }
}