namespace CafeApp.Models;
using System;
using System.Collections.Generic;

public class OrderReportData
{
    public int OrderId { get; set; }
    public DateTime OrderDate { get; set; }
    public int TableId { get; set; }
    public string WaiterName { get; set; } = "";
    public string Status { get; set; } = "";
    public string PaymentType { get; set; } = "";
    public int CustomerCount { get; set; }
    public decimal TotalAmount { get; set; }
    public List<OrderItemReport> Items { get; set; } = new List<OrderItemReport>();
}

public class OrderItemReport
{
    public string DishName { get; set; } = "";
    public int Quantity { get; set; }
    public decimal Price { get; set; }
    public decimal TotalPrice => Quantity * Price;
}