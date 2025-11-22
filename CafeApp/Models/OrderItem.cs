using System;
using System.Collections.Generic;

namespace CafeApp.Models
{
    public class OrderItem
    {
        public int OrderItemId { get; set; }
        public int OrderId { get; set; }
        public int MenuItemId { get; set; }
        public int? Quantity { get; set; }
        public MenuItem? MenuItem { get; set; }
        public decimal Price { get; set; }
    }
}