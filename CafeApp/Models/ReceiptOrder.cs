using System;
using System.Collections.Generic;

namespace CafeApp.Models
{
    public class ReceiptOrder
    {
        public int OrderId { get; set; }
        public DateTime OrderDate { get; set; }
        public string WaiterName { get; set; } = "";
        public string PaymentType { get; set; } = "";
        public int TableId { get; set; }
        public List<ReceiptOrderItem> Items { get; set; } = new List<ReceiptOrderItem>();
        public decimal TotalAmount { get; set; }
    }

    public class ReceiptOrderItem
    {
        public string DishName { get; set; } = "";
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public decimal TotalPrice => Quantity * Price;
    }
}