using Npgsql;
using CafeApp.Models.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Data;
using CafeApp.Database;

namespace CafeApp.Services
{
    public class DatabaseService : IDisposable
    {
        private readonly string _connectionString;
        private NpgsqlConnection? _connection;

        public DatabaseService(AppConfig config)
        {
            _connectionString = config.ConnectionString;
        }

        public async GetConnectionAsync()
        {
            if (_connection == null || _connection.State != ConnectionState.Open)
            {
                _connection = new NpgsqlConnection(_connectionString);
                await _connection.OpenAsync();
            }
            return _connection;
        }

        // Метод для проверки логина и пароля - возвращает true/false
        public async Task<bool> AuthenticateUserAsync(string username, string password)
        {
            using var connection = await GetConnectionAsync();
            using var cmd = new NpgsqlCommand("SELECT 1 FROM users WHERE username = @username AND password = @password", connection);
            
            cmd.Parameters.AddWithValue("@username", username);
            cmd.Parameters.AddWithValue("@password", password);

            var result = await cmd.ExecuteScalarAsync();
            return result != null; // true если пользователь найден, false если нет
        }

        // Альтернативный вариант - с COUNT
        public async Task<bool> ValidateUserAsync(string username, string password)
        {
            using var connection = await GetConnectionAsync();
            using var cmd = new NpgsqlCommand("SELECT COUNT(*) FROM users WHERE username = @username AND password = @password", connection);
            
            cmd.Parameters.AddWithValue("@username", username);
            cmd.Parameters.AddWithValue("@password", password);

            var result = await cmd.ExecuteScalarAsync();
            return Convert.ToInt32(result) > 0;
        }

        public async Task<List<User>> GetAllUsersAsync()
        {
            var users = new List<User>();
            
            using var connection = await GetConnectionAsync();
            using var cmd = new NpgsqlCommand("SELECT * FROM users", connection);
            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                var user = new User
                {
                    Id = reader.GetInt32("id"),
                    Username = reader.GetString("username"),
                    Password = reader.GetString("password"),
                    Role = reader.GetString("role"),
                    CreatedAt = reader.GetDateTime("created_at")
                };

                if (!reader.IsDBNull(reader.GetOrdinal("employee_id")))
                {
                    user.EmployeeId = reader.GetInt32("employee_id");
                }

                users.Add(user);
            }

            return users;
        }

        public void Dispose()
        {
            _connection?.Close();
            _connection?.Dispose();
        }
    }
}