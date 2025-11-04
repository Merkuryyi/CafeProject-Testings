using Npgsql;
using CafeApp.Models;
using System;
using System.Data;

namespace CafeApp.Services
{
    public class DatabaseService : IDisposable
    {
        private NpgsqlConnection? _connection;
        private readonly string _connectionString;

        public DatabaseService(AppConfig config)
        {
            _connectionString = config.ConnectionString;
        }

        public NpgsqlConnection GetConnection()
        {
            if (_connection == null || _connection.State != ConnectionState.Open)
            {
                _connection = new NpgsqlConnection(_connectionString);
                _connection.Open();
            }
            return _connection;
        }

        public bool AuthenticateUser(string username, string password)
        {
            try
            {
                using (var conn = AppConfig.GetSqlConnection())
                {
                    string query = @"SELECT * FROM users WHERE username = @username AND password = @password";
                    using (var command = new NpgsqlCommand(query, conn))
                    {
                        command.Parameters.AddWithValue("@username", username);
                        command.Parameters.AddWithValue("@password", password);
                
                        using (var reader = command.ExecuteReader()) 
                        {
                            // Если есть хотя бы одна запись - пользователь найден
                            if (reader.HasRows)
                            {
                                return true;
                            }
                            else
                            {
                                return false;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
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