namespace CafeApp.Models;
using Npgsql;
using System;
using System.Collections.Generic;


public class OrderInfo
{
    public int OrderId { get; set; }
    public int TableId { get; set; }
    public int WaiterId { get; set; }
    public string WaiterName { get; set; } = "";
    public string Status { get; set; } = "";
    public int CustomerCount { get; set; } 
    public DateTime CreatedAt { get; set; }
    public List<OrderItemInfo> Items { get; set; } = new List<OrderItemInfo>();
}

public class OrderItemInfo
{
    public string MenuItemName { get; set; } = "";
    public int Quantity { get; set; }
    public decimal Price { get; set; }
}