// Models/Order.cs
using System;
using System.Collections.Generic;

namespace CafeApp.Models
{
    public class Order
    {
        public int OrderId { get; set; }
        public int TableId { get; set; }
        public int WaiterId { get; set; }
        public int ShiftId { get; set; }
        public int CustomerCount { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public List<OrderItem> OrderItems { get; set; } = new();
        public User? Waiter { get; set; }
        public CafeTable? Table { get; set; }
        public Shift? Shift { get; set; }

        // Метод для преобразования в данные для таблицы
        public List<string> ToTableRow()
        {
            return new List<string>
            {
                OrderId.ToString(),
                $"Стол {Table?.TableNumber}",
                $"{Waiter?.Surname} {Waiter?.Name}",
                Status,
                CreatedAt.ToString("dd.MM.yyyy HH:mm"),
                CustomerCount.ToString()
            };
        }
    }
}