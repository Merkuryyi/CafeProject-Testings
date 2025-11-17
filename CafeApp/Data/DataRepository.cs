using CafeApp.Models;
using Npgsql;
using System.Collections.ObjectModel;
using CafeApp.Database;
using System.Collections.Generic;
using System;
using System.Linq;

namespace CafeApp.Data
{
    public class DataRepository : IDisposable
    {
        private readonly DatabaseService _databaseService;
        private readonly ObservableCollection<User> _employees = new();
        private readonly ObservableCollection<Order> _orders = new();
        private readonly ObservableCollection<Shift> _shifts = new();
        private readonly ObservableCollection<MenuItem> _menuItems = new();
        private readonly ObservableCollection<CafeTable> _tables = new(); // Изменено на CafeTable

        public ReadOnlyObservableCollection<User> Employees => new(_employees);
        public ReadOnlyObservableCollection<Order> Orders => new(_orders);
        public ReadOnlyObservableCollection<Shift> Shifts => new(_shifts);
        public ReadOnlyObservableCollection<MenuItem> MenuItems => new(_menuItems);
        public ReadOnlyObservableCollection<CafeTable> Tables => new(_tables); // Изменено на CafeTable

        public DataRepository(DatabaseService databaseService)
        {
            _databaseService = databaseService;
            LoadAllData();
        }

        public void LoadAllData()
        {
            LoadEmployees();
            LoadShifts();
            LoadOrders();
            LoadMenuItems();
            LoadTables();
        }

        private void LoadEmployees()
        {
            _employees.Clear();
            try
            {
                using var conn = _databaseService.GetConnection();
                string query = @"SELECT user_id, username, password_, role, name, surname, patronymic, 
                                employment_status, photo_link, contract_scan_link 
                                FROM users WHERE employment_status = true";
                
                using var command = new NpgsqlCommand(query, conn);
                using var reader = command.ExecuteReader();
                
                while (reader.Read())
                {
                    var employee = new User
                    {
                        UserId = reader.GetInt32(0),
                        Username = reader.GetString(1),
                        Password = reader.GetString(2),
                        Role = reader.GetString(3),
                        Name = reader.GetString(4),
                        Surname = reader.GetString(5),
                        Patronymic = reader.IsDBNull(6) ? null : reader.GetString(6),
                        EmploymentStatus = reader.GetBoolean(7),
                        PhotoLink = reader.IsDBNull(8) ? null : reader.GetString(8),
                        ContractScanLink = reader.IsDBNull(9) ? null : reader.GetString(9)
                    };
                    _employees.Add(employee);
                }
            }
            catch (Exception ex)
            {
                // Логирование ошибки
            }
        }

        private void LoadShifts()
        {
            _shifts.Clear();
            try
            {
                using var conn = _databaseService.GetConnection();
                
                // Загружаем основные данные о сменах
                string shiftQuery = @"SELECT shift_id, shift_date, start_time, end_time FROM shift 
                                    ORDER BY shift_date DESC, start_time DESC";
                
                using var shiftCommand = new NpgsqlCommand(shiftQuery, conn);
                using var shiftReader = shiftCommand.ExecuteReader();
                
                var shifts = new List<Shift>();
                while (shiftReader.Read())
                {
                    var shift = new Shift
                    {
                        ShiftId = shiftReader.GetInt32(0),
                        ShiftDate = shiftReader.GetDateTime(1),
                        StartTime = shiftReader.GetTimeSpan(2),
                        EndTime = shiftReader.GetTimeSpan(3)
                    };
                    shifts.Add(shift);
                }
                shiftReader.Close();

                // Загружаем назначения сотрудников на смены
                foreach (var shift in shifts)
                {
                    string assignmentQuery = @"SELECT u.user_id, u.username, u.name, u.surname, u.patronymic, u.role
                                             FROM shift_assignment sa
                                             JOIN users u ON sa.user_id = u.user_id
                                             WHERE sa.shift_id = @shiftId";
                    
                    using var assignmentCommand = new NpgsqlCommand(assignmentQuery, conn);
                    assignmentCommand.Parameters.AddWithValue("@shiftId", shift.ShiftId);
                    
                    using var assignmentReader = assignmentCommand.ExecuteReader();
                    while (assignmentReader.Read())
                    {
                        var user = new User
                        {
                            UserId = assignmentReader.GetInt32(0),
                            Username = assignmentReader.GetString(1),
                            Name = assignmentReader.GetString(2),
                            Surname = assignmentReader.GetString(3),
                            Patronymic = assignmentReader.IsDBNull(4) ? null : assignmentReader.GetString(4),
                            Role = assignmentReader.GetString(5)
                        };
                        shift.AssignedUsers.Add(user);
                    }
                    assignmentReader.Close();
                    
                    _shifts.Add(shift);
                }
            }
            catch (Exception ex)
            {
                // Логирование ошибки
            }
        }

        private void LoadOrders()
        {
            _orders.Clear();
            try
            {
                using var conn = _databaseService.GetConnection();
                
                // Загружаем основные данные о заказах
                string orderQuery = @"SELECT o.order_id, o.table_id, o.waiter_id, o.shift_id, 
                                    o.customer_count, o.status, o.created_at,
                                    u.name, u.surname, u.patronymic,
                                    t.table_number
                                    FROM ""order"" o
                                    JOIN users u ON o.waiter_id = u.user_id
                                    JOIN ""table"" t ON o.table_id = t.table_id
                                    ORDER BY o.created_at DESC";
                
                using var orderCommand = new NpgsqlCommand(orderQuery, conn);
                using var orderReader = orderCommand.ExecuteReader();
                
                var orders = new List<Order>();
                while (orderReader.Read())
                {
                    var order = new Order
                    {
                        OrderId = orderReader.GetInt32(0),
                        TableId = orderReader.GetInt32(1),
                        WaiterId = orderReader.GetInt32(2),
                        ShiftId = orderReader.GetInt32(3),
                        CustomerCount = orderReader.GetInt32(4),
                        Status = orderReader.GetString(5),
                        CreatedAt = orderReader.GetDateTime(6),
                        Waiter = new User
                        {
                            Name = orderReader.GetString(7),
                            Surname = orderReader.GetString(8),
                            Patronymic = orderReader.IsDBNull(9) ? null : orderReader.GetString(9)
                        },
                        Table = new CafeTable // Изменено на CafeTable
                        {
                            TableNumber = orderReader.GetInt32(10)
                        }
                    };
                    orders.Add(order);
                }
                orderReader.Close();

                // Загружаем состав заказов
                foreach (var order in orders)
                {
                    string itemsQuery = @"SELECT oi.order_item_id, oi.menu_item_id, oi.quantity,
                                        mi.name, mi.price, mi.category
                                        FROM order_item oi
                                        JOIN menu_item mi ON oi.menu_item_id = mi.item_id
                                        WHERE oi.order_id = @orderId";
                    
                    using var itemsCommand = new NpgsqlCommand(itemsQuery, conn);
                    itemsCommand.Parameters.AddWithValue("@orderId", order.OrderId);
                    
                    using var itemsReader = itemsCommand.ExecuteReader();
                    while (itemsReader.Read())
                    {
                        var orderItem = new OrderItem
                        {
                            OrderItemId = itemsReader.GetInt32(0),
                            MenuItemId = itemsReader.GetInt32(1),
                            Quantity = itemsReader.GetInt32(2),
                            MenuItem = new MenuItem
                            {
                                Name = itemsReader.GetString(3),
                                Price = itemsReader.GetDecimal(4),
                                Category = itemsReader.GetString(5)
                            }
                        };
                        order.OrderItems.Add(orderItem);
                    }
                    itemsReader.Close();
                    
                    _orders.Add(order);
                }
            }
            catch (Exception ex)
            {
                // Логирование ошибки
            }
        }

        private void LoadMenuItems()
        {
            _menuItems.Clear();
            try
            {
                using var conn = _databaseService.GetConnection();
                string query = "SELECT item_id, name, price, category FROM menu_item ORDER BY category, name";
                
                using var command = new NpgsqlCommand(query, conn);
                using var reader = command.ExecuteReader();
                
                while (reader.Read())
                {
                    var menuItem = new MenuItem
                    {
                        ItemId = reader.GetInt32(0),
                        Name = reader.GetString(1),
                        Price = reader.GetDecimal(2),
                        Category = reader.GetString(3)
                    };
                    _menuItems.Add(menuItem);
                }
            }
            catch (Exception ex)
            {
                // Логирование ошибки
            }
        }

        private void LoadTables()
        {
            _tables.Clear();
            try
            {
                using var conn = _databaseService.GetConnection();
                string query = "SELECT table_id, table_number, capacity FROM \"table\" ORDER BY table_number";
                
                using var command = new NpgsqlCommand(query, conn);
                using var reader = command.ExecuteReader();
                
                while (reader.Read())
                {
                    var table = new CafeTable // Изменено на CafeTable
                    {
                        TableId = reader.GetInt32(0),
                        TableNumber = reader.GetInt32(1),
                        Capacity = reader.GetInt32(2)
                    };
                    _tables.Add(table);
                }
            }
            catch (Exception ex)
            {
                // Логирование ошибки
            }
        }

        // Методы для получения конкретных объектов
        public User? GetEmployeeById(int userId) => _employees.FirstOrDefault(e => e.UserId == userId);
        public Order? GetOrderById(int orderId) => _orders.FirstOrDefault(o => o.OrderId == orderId);
        public Shift? GetShiftById(int shiftId) => _shifts.FirstOrDefault(s => s.ShiftId == shiftId);

        public void RefreshData()
        {
            LoadAllData();
        }

        public void Dispose()
        {
            // Очистка ресурсов
        }
    }
}