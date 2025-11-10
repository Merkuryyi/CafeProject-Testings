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
                Console.WriteLine($"Ошибка аутентификации: {ex.Message}");
                return null;
            }
        }
		public bool RegisterUser(string username, string password, string name, 
                 string surname, string patronymic, string role)
		{
    		try
    		{
        		using (var conn = GetConnection())
        		{
            		// INSERT запрос для добавления пользователя
            		string query = @"INSERT INTO ""user"" 
                           (username, password_, role, name, surname, patronymic, employment_status) 
                           VALUES (@username, @password, @role, @name, @surname, @patronymic, TRUE)";
            
            		using (var command = new NpgsqlCommand(query, conn))
            		{
                		command.Parameters.AddWithValue("@username", username);
                		command.Parameters.AddWithValue("@password", password);
               			command.Parameters.AddWithValue("@role", role);
                		command.Parameters.AddWithValue("@name", name);
                		command.Parameters.AddWithValue("@surname", surname);
                		command.Parameters.AddWithValue("@patronymic", patronymic);

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
        
        		Console.WriteLine($"Ошибка регистрации: {ex.Message}");
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