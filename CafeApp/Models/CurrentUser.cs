using System;

namespace CafeApp.Models
{
    public static class CurrentUser
    {
        public static int Id { get; private set; }
        public static string Role { get; private set; } = "";
        public static string Username { get; private set; } = "";
        public static string FullName { get; private set; } = "";
        public static bool IsAuthenticated => Id > 0;

        public static void SetUser(int id, string role, string username, string fullName = "")
        {
            Id = id;
            Role = role ?? "";
            Username = username ?? "";
            FullName = fullName ?? "";
            
            // Логируем вход пользователя
            // string logPath = @"A:\Инженерно-техническая поддержка сопровождения ИС\debug.log";
           // File.AppendAllText(logPath, $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - USER LOGIN: ID={Id}, Role={Role}, Username={Username}\n");
        }

        public static void Clear()
        {
            Id = 0;
            Role = "";
            Username = "";
            FullName = "";
        }

        // Проверка прав доступа
        public static bool IsAdmin => Role?.ToLower() == "администратор";
        public static bool IsWaiter => Role?.ToLower() == "официант";
        public static bool IsCook => Role?.ToLower() == "повар";

        // Метод для проверки разрешений
        public static bool HasPermission(string requiredRole)
        {
            return Role?.ToLower() == requiredRole.ToLower();
        }

        // Метод для получения информации о пользователе
        public static string GetUserInfo()
        {
            return $"ID: {Id}, Роль: {Role}, Пользователь: {Username}";
        }
    }
}